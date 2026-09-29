// Hook PreToolUse de Antigravity CLI: impone los permisos de los roles tester y revisor
// (docs/AGENTES.md). Responde siempre una decisión: agy trata una respuesta vacía como "deny".
// Para no conceder más de lo habitual, lo permitido reproduce lo que agy hace por defecto:
// "allow" en el repositorio y en sus propias carpetas, "ask" en lo demás y en los comandos.
//
// Rol: variable de entorno RRHH_ROL, "tester" (por defecto) o "revisor". Con un valor
// desconocido se bloquea toda escritura en el repositorio.
//
// Entrada (stdin):  { "toolCall": { "name": "...", "args": { ... } }, "artifactDirectoryPath": "...", ... }
// Salida (stdout):  { "decision": "allow" | "ask" | "deny", "reason": "..." }

import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const raiz = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const enWindows = process.platform === 'win32';
const rol = (process.env.RRHH_ROL ?? '').trim().toLowerCase() || 'tester';

const HERRAMIENTAS_ESCRITURA = new Set(['write_to_file', 'replace_file_content', 'multi_replace_file_content']);
const HERRAMIENTAS_LECTURA = new Set(['view_file', 'list_dir', 'find_by_name', 'grep_search']);
const HERRAMIENTAS_SUBAGENTES = new Set(['invoke_subagent', 'define_subagent', 'send_message', 'manage_subagents']);

// Lo único que el tester puede modificar (docs/agentes/tester.md). Las carpetas terminan en "/".
const ESCRITURA_TESTER = ['tests/', 'postman/', 'docs/PLAN_PRUEBAS.md', 'docs/ERRORES_RECURRENTES.md'];

// Implementación que el tester no lee: sus pruebas son de caja negra.
const LECTURA_PROHIBIDA_TESTER = ['src/RRHH.Application/Servicios/', 'src/RRHH.Infrastructure/'];

// Carpetas propias de agy fuera del repositorio, que agy ya permite por defecto.
const TEMPORALES = os.tmpdir();
const AYUDA_AGY = path.join(os.homedir(), '.gemini', 'antigravity-cli', 'builtin');

const GIT_PUSH = /\bgit\b.*\bpush\b/i;

const permite = { decision: 'allow' };
const pregunta = { decision: 'ask' };
const niega = (motivo) => ({ decision: 'deny', reason: motivo });

// Ruta absoluta (las relativas son del repositorio), o null si el valor no es una ruta.
function aAbsoluta(valor) {
  if (typeof valor !== 'string' || valor.trim() === '' || valor.includes('\n')) return null;
  try {
    return path.resolve(raiz, valor.startsWith('file:') ? fileURLToPath(valor) : valor);
  } catch {
    return null;
  }
}

// Ruta relativa a la carpeta, con "/", o null si no está dentro de ella.
function dentroDe(carpeta, absoluta) {
  if (typeof carpeta !== 'string' || carpeta === '') return null;
  const relativa = path.relative(carpeta, absoluta);
  if (relativa === '..' || relativa.startsWith('..' + path.sep) || path.isAbsolute(relativa)) return null;
  return relativa.split(path.sep).join('/');
}

// true si la ruta es un archivo de la lista o está dentro de una de sus carpetas.
function cubre(lista, ruta) {
  const comparable = (texto) => (enWindows ? texto.toLowerCase() : texto);
  const r = comparable(ruta);
  return lista.some((entrada) => {
    const e = comparable(entrada);
    return e.endsWith('/') ? r === e.slice(0, -1) || r.startsWith(e) : r === e;
  });
}

function motivoEscritura(ruta) {
  if (rol === 'revisor') return 'El revisor no modifica archivos: sólo informa hallazgos (docs/agentes/revisor.md).';
  if (rol !== 'tester') return `RRHH_ROL="${rol}" no es un rol válido (tester o revisor): se bloquea toda escritura.`;
  return `El tester sólo modifica tests/, postman/, docs/PLAN_PRUEBAS.md y docs/ERRORES_RECURRENTES.md `
    + `(docs/agentes/tester.md); "${ruta || '.'}" no está permitido.`;
}

function decidirEscritura(args, artefactos) {
  const destino = aAbsoluta(args.TargetFile);
  if (destino === null) return niega('No se pudo determinar el archivo a modificar; se bloquea por seguridad.');

  const ruta = dentroDe(raiz, destino);
  if (ruta !== null) {
    return rol === 'tester' && cubre(ESCRITURA_TESTER, ruta) ? permite : niega(motivoEscritura(ruta));
  }
  const propiaDeAgy = [artefactos, TEMPORALES].some((carpeta) => dentroDe(carpeta, destino) !== null);
  return propiaDeAgy ? permite : pregunta;
}

function decidirLectura(args, artefactos) {
  const rutas = Object.values(args).map(aAbsoluta).filter((ruta) => ruta !== null);

  if (rol !== 'revisor') {
    const prohibida = rutas
      .map((ruta) => dentroDe(raiz, ruta))
      .find((ruta) => ruta !== null && cubre(LECTURA_PROHIBIDA_TESTER, ruta));
    if (prohibida !== undefined) {
      return niega(`El tester no lee la implementación ("${prohibida}"): las pruebas son de caja negra `
        + '(docs/agentes/tester.md). Si el contrato no define algo, pedilo.');
    }
  }

  const permitidas = [raiz, artefactos, TEMPORALES, AYUDA_AGY];
  const todasPermitidas = rutas.every((ruta) => permitidas.some((carpeta) => dentroDe(carpeta, ruta) !== null));
  return todasPermitidas ? permite : pregunta;
}

function decidir(entrada) {
  const llamada = entrada?.toolCall ?? {};
  const herramienta = llamada.name;
  const args = typeof llamada.args === 'string' ? JSON.parse(llamada.args) : (llamada.args ?? {});
  const artefactos = entrada?.artifactDirectoryPath;

  if (herramienta === 'run_command') {
    return GIT_PUSH.test(String(args.CommandLine ?? ''))
      ? niega('git push lo hace sólo el responsable (AGENTS.md, «No hacer»).')
      : pregunta;
  }
  if (HERRAMIENTAS_SUBAGENTES.has(herramienta)) {
    return niega('Los subagentes no pasan por este hook y podrían saltear los permisos del rol: no se usan.');
  }
  if (HERRAMIENTAS_ESCRITURA.has(herramienta)) return decidirEscritura(args, artefactos);
  if (HERRAMIENTAS_LECTURA.has(herramienta)) return decidirLectura(args, artefactos);
  return pregunta;
}

let respuesta;
try {
  process.stdin.setEncoding('utf8');
  let texto = '';
  for await (const parte of process.stdin) texto += parte;
  respuesta = decidir(JSON.parse(texto));
} catch (error) {
  respuesta = niega(`El hook de permisos falló (${error.message}); se bloquea por seguridad.`);
}
process.stdout.write(JSON.stringify(respuesta));
