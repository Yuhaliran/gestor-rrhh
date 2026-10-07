// Hook PreToolUse de Antigravity CLI: impone los permisos de los roles tester y revisor
// (docs/AGENTES.md). Responde siempre una decisión: agy trata una respuesta vacía como "deny".
// En los archivos, lo permitido reproduce lo que agy hace por defecto: "allow" en el repositorio
// y en sus propias carpetas, "ask" en lo demás.
//
// Rol: variable de entorno RRHH_ROL, "tester" (por defecto) o "revisor". Con un valor
// desconocido se bloquea toda escritura en el repositorio. El revisor (y un rol desconocido)
// sólo puede ejecutar comandos de lectura, con aprobación. El tester ejecuta sin preguntar los
// comandos de cada tarea, no puede usar los que ya causaron problemas (scripts que editan
// archivos, PowerShell que escribe, borrar) y para el resto pregunta: así las aprobaciones que
// quedan son las que importan. De npm, sólo los scripts del frontend (frontend/package.json).
//
// Entrada (stdin):  { "toolCall": { "name": "...", "args": { ... } }, "artifactDirectoryPath": "...", ... }
// Salida (stdout):  { "decision": "allow" | "ask" | "deny", "reason": "..." }

import { appendFileSync } from 'node:fs';
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
const ESCRITURA_TESTER = [
  'tests/', 'postman/', 'docs/PLAN_PRUEBAS.md', 'docs/ERRORES_RECURRENTES.md',
  'frontend/tests/', 'docs/frontend/PLAN_PRUEBAS.md',
];

// Implementación que el tester no lee: sus pruebas son de caja negra. Del frontend lee
// contratos/, app.routes.ts y app.config.ts (docs/frontend/PLAN.md, «Agentes»).
const LECTURA_PROHIBIDA_TESTER = [
  'src/RRHH.Application/Servicios/', 'src/RRHH.Infrastructure/',
  'frontend/src/app/api/', 'frontend/src/app/servicios/', 'frontend/src/app/vistas/', 'frontend/src/app/componentes/',
];

// Campos de texto libre que agy agrega a las herramientas: describen la acción, no son rutas.
const CAMPOS_DESCRIPTIVOS = new Set(['toolAction', 'toolSummary']);

// Carpetas propias de agy fuera del repositorio, que agy ya permite por defecto.
const TEMPORALES = os.tmpdir();
const AYUDA_AGY = path.join(os.homedir(), '.gemini', 'antigravity-cli', 'builtin');

const GIT_PUSH = /\bgit\b.*\bpush\b/i;

// Comandos de sólo lectura para el revisor (docs/agentes/revisor.md: no modifica nada).
// Es una lista de lo permitido: un comando que no esté acá se bloquea.
const TERMINAL_LECTURA = new Set([
  'get-content', 'gc', 'cat', 'type', 'more', 'head', 'tail', 'select-string', 'sls', 'findstr', 'grep',
  'rg', 'get-childitem', 'gci', 'dir', 'ls', 'get-item', 'test-path', 'get-filehash', 'format-hex',
  'measure-object', 'measure', 'select-object', 'select', 'where-object', 'sort-object', 'format-table',
  'format-list', 'out-string', 'wc', 'sort', 'uniq', 'echo', 'write-output', 'write-host', 'get-command',
  'where',
]);
const GIT_LECTURA = new Set([
  'diff', 'log', 'show', 'status', 'blame', 'grep', 'ls-files', 'ls-tree', 'rev-parse', 'rev-list',
  'merge-base', 'show-ref', 'cat-file', 'shortlog', 'describe', 'branch',
]);
const GIT_BRANCH_MODIFICA = new Set([
  '-d', '-D', '--delete', '-m', '-M', '--move', '-c', '-C', '--copy', '-f', '--force', '-u',
  '--set-upstream-to', '--unset-upstream', '--edit-description',
]);
const DOTNET_LECTURA = new Set(['build', 'test', 'list', '--version', '--info', '--list-sdks', '--list-runtimes']);
// Aunque el comando principal sea de lectura, esto escribe o ejecuta código ($(...), script blocks).
const ESCRITURA_EN_COMANDO =
  /\b(Set-Content|Add-Content|Out-File|New-Item|Remove-Item|Copy-Item|Move-Item|Rename-Item|Clear-Content|Set-Item|Tee-Object|Invoke-Expression|Start-Process|Export-Csv|Export-Clixml|iex)\b|\[(System\.)?IO\./i;
const REDIRECCION = /(?:\d|\*)?>>?\s*(&\d|[^\s|;&]+)/g;
const DESTINO_INOFENSIVO = /^(\$null|nul|\/dev\/null|&\d)$/i;

// Comandos del tester. Intérpretes: con ellos editó archivos por fuera de este hook (incluso con
// código en base64) y rompió pruebas y codificaciones (E-013). Sólo se permite validar un JSON.
const INTERPRETES = new Set([
  'node', 'nodejs', 'npx', 'npm', 'python', 'python3', 'py', 'pwsh', 'powershell', 'cmd', 'bash', 'sh',
  'wsl', 'deno', 'bun', 'wscript', 'cscript',
]);
const VALIDAR_JSON = /^node(?:\.exe)?\s+-e\s+"JSON\.parse\(require\('fs'\)\.readFileSync\('([^'"]+)'\s*,\s*'utf-?8'\)\)\s*;?"$/i;
// npm, sólo para los scripts de frontend/package.json, que el tester no puede modificar.
// Cualquier otro uso de npm (o npx) sigue bloqueado.
const NPM_LECTURA = new Set(['test', 'run lint', 'run build', 'run format:check']);
const NPM_TESTER = new Set([...NPM_LECTURA, 'run format:pruebas']);   // formatea sólo frontend/tests/
const NPM_INSTALACION = new Set(['ci', 'install']);                   // sin argumentos: lo de package-lock.json
const MOTIVO_NPM = 'De npm, el tester sólo corre los scripts del frontend, en frontend/ (o con --prefix frontend): '
  + 'npm test, npm run lint, npm run build, npm run format:check, npm run format:pruebas y '
  + 'npm test -- --include <ruta de frontend/tests/>. npm ci y npm install, con aprobación.';
const BORRAR = new Set(['del', 'erase', 'rm', 'rmdir', 'rd', 'remove-item', 'ri', 'unlink']);
// Git que no muestra el contenido de los archivos
const GIT_RESUMEN_TESTER = new Set([
  'status', 'log', 'branch', 'rev-parse', 'rev-list', 'ls-files', 'show-ref', 'describe', 'shortlog', 'merge-base',
]);
// Opciones de diff y show que no muestran el contenido, o muestran sólo lo que el tester preparó
const DIFF_RESUMIDO = new Set(['--stat', '--shortstat', '--name-only', '--name-status', '--cached', '--staged']);
// Lo que el tester puede ver completo en un diff: lo suyo y el contrato
const DIFF_PERMITIDO_TESTER = [
  ...ESCRITURA_TESTER, 'docs/', 'src/RRHH.Contratos/', 'src/RRHH.Application/Interfaces/', 'frontend/src/app/contratos/',
];
// git commit que tomaría cambios ajenos o saltearía verificaciones (-a, --amend, -n)
const COMMIT_PROHIBIDO = /^(--all|--amend|--no-verify|-[^-]*[an][^-]*)$/;
const ADD_PROHIBIDO = new Set(['-A', '--all', '-u', '--update', '-f', '--force', '.', '*', ':/']);

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
  return `El tester sólo modifica tests/, postman/, frontend/tests/, docs/PLAN_PRUEBAS.md, `
    + `docs/frontend/PLAN_PRUEBAS.md y docs/ERRORES_RECURRENTES.md (docs/agentes/tester.md); `
    + `"${ruta || '.'}" no está permitido.`;
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
  const rutas = Object.entries(args)
    .filter(([campo]) => !CAMPOS_DESCRIPTIVOS.has(campo))
    .map(([, valor]) => aAbsoluta(valor))
    .filter((ruta) => ruta !== null);

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

// Partes de una línea de comandos separadas por |, ;, &&, || o saltos de línea (fuera de comillas).
function segmentos(linea) {
  const partes = [];
  let actual = '';
  let comilla = null;
  for (let i = 0; i < linea.length; i++) {
    const c = linea[i];
    if (comilla) {
      actual += c;
      if (c === comilla) comilla = null;
    } else if (c === '"' || c === "'") {
      comilla = c;
      actual += c;
    } else if (c === '|' || c === ';' || c === '\n' || (c === '&' && linea[i + 1] === '&')) {
      partes.push(actual);
      actual = '';
      if (linea[i + 1] === c) i++;
    } else {
      actual += c;
    }
  }
  partes.push(actual);
  return partes.map((p) => p.trim()).filter((p) => p !== '');
}

const palabras = (segmento) => [...segmento.matchAll(/"([^"]*)"|'([^']*)'|(\S+)/g)].map((m) => m[1] ?? m[2] ?? m[3]);

function gitDeLectura(args) {
  let i = 0;
  while (args[i]?.startsWith('-')) {
    if (args[i] === '--no-pager') i += 1;
    else if (args[i] === '-C') i += 2;
    else return false;                       // otras opciones globales (como -c) pueden ejecutar programas
  }
  const [subcomando, ...resto] = args.slice(i);
  if (!GIT_LECTURA.has(subcomando) || resto.some((a) => a.startsWith('--output'))) return false;
  return subcomando !== 'branch' || resto.every((a) => a.startsWith('-') && !GIT_BRANCH_MODIFICA.has(a));
}

function dotnetDeLectura([subcomando, ...resto]) {
  if (DOTNET_LECTURA.has(subcomando)) return true;
  if (subcomando === 'format') return resto.includes('--verify-no-changes');
  if (subcomando === 'ef') {
    return (resto[0] === 'migrations' && ['list', 'has-pending-model-changes'].includes(resto[1]))
      || (resto[0] === 'dbcontext' && ['info', 'list'].includes(resto[1]));
  }
  return false;
}

function comandoDeLectura(segmento, cwd) {
  if (esNpm(segmento)) return npmDelFrontend(palabras(segmento).slice(1), cwd, NPM_LECTURA) === 'permite';
  const [programa = '', ...args] = palabras(segmento);
  const nombre = programa.toLowerCase().replace(/\.exe$/, '');
  if (nombre === 'git') return gitDeLectura(args);
  if (nombre === 'dotnet') return dotnetDeLectura(args);
  return TERMINAL_LECTURA.has(nombre);
}

// Motivo por el que el comando no es de sólo lectura, o null si lo es.
function motivoComandoNoLectura(linea, cwd) {
  const sinComillas = linea.replace(/"[^"]*"|'[^']*'/g, '""');
  const redireccion = [...sinComillas.matchAll(REDIRECCION)].find((m) => !DESTINO_INOFENSIVO.test(m[1]));
  if (redireccion) return `redirige la salida a «${redireccion[1]}»`;
  if (ESCRITURA_EN_COMANDO.test(linea)) return 'usa un comando que escribe archivos o ejecuta código';
  const noPermitido = segmentos(linea).find((s) => !comandoDeLectura(s, cwd));
  return noPermitido === undefined ? null : `«${noPermitido}» no está entre los comandos de lectura`;
}

// Ruta de un argumento, relativa al repositorio y con "/", o null si está fuera de él.
function rutaDelRepositorio(argumento, cwd) {
  const base = typeof cwd === 'string' && cwd !== '' ? cwd : raiz;
  try {
    return dentroDe(raiz, path.resolve(base, argumento));
  } catch {
    return null;
  }
}

const todasEn = (lista, rutas, cwd) =>
  rutas.length > 0 && rutas.every((r) => {
    const ruta = rutaDelRepositorio(r, cwd);
    return ruta !== null && cubre(lista, ruta);
  });

// npm por su nombre, sin ruta: una ruta podría apuntar a un npm.cmd escrito por el tester.
const esNpm = (segmento) => /^npm(\.cmd)?$/i.test(palabras(segmento)[0] ?? '');

// npm en frontend/ (con la terminal ahí o con --prefix frontend): "permite" si es uno de los
// scripts, "pregunta" si instala lo que fija package-lock.json, null si es cualquier otra cosa.
function npmDelFrontend(args, cwd, scripts) {
  const base = typeof cwd === 'string' && cwd !== '' ? path.resolve(raiz, cwd) : raiz;
  // Desde una carpeta del tester, cmd.exe tomaría primero un npm.cmd que él haya escrito ahí
  if (cubre(ESCRITURA_TESTER, dentroDe(raiz, base) ?? '')) return null;
  let carpeta = base;
  // 2>&1 o > $null no forman parte del script; una redirección a un archivo se bloquea aparte
  let resto = args.filter((a) => !/^(\d|\*)?>>?(&\d|\$null|nul)$/i.test(a));
  if (resto[0] === '--prefix' && resto.length > 1) {
    carpeta = path.resolve(base, resto[1]);
    resto = resto.slice(2);
  }
  const relativa = dentroDe(raiz, carpeta);
  if (relativa === null || (enWindows ? relativa.toLowerCase() : relativa) !== 'frontend') return null;

  const separador = resto.indexOf('--');
  const comando = (separador === -1 ? resto : resto.slice(0, separador)).join(' ');
  const extra = separador === -1 ? [] : resto.slice(separador + 1);
  if (extra.length === 0) {
    if (scripts.has(comando)) return 'permite';
    return NPM_INSTALACION.has(comando) && scripts === NPM_TESTER ? 'pregunta' : null;
  }
  // npm test -- --include <ruta>: ng test toma las rutas desde frontend/src; sólo pruebas del tester
  if (comando !== 'test' || !scripts.has('test')) return null;
  const rutas = [];
  for (let i = 0; i < extra.length; i += 2) {
    if (extra[i] !== '--include' || extra[i + 1] === undefined) return null;
    rutas.push(extra[i + 1]);
  }
  return todasEn(['frontend/tests/'], rutas, path.join(carpeta, 'src')) ? 'permite' : null;
}

// git diff, show o log -p: sin preguntar si es un resumen, o si todas las rutas son del tester o
// del contrato (hace falta al menos una: sin rutas mostraría también la implementación).
function diffDelTester(resto, cwd) {
  if (resto.some((a) => a.startsWith('--output') || a === '--ext-diff')) return false;
  if (resto.some((a) => DIFF_RESUMIDO.has(a))) return true;
  const separador = resto.indexOf('--');
  const antes = separador === -1 ? resto : resto.slice(0, separador);
  const rutas = [
    ...antes.filter((a) => !a.startsWith('-') && /[/\\:]/.test(a)).map((a) => a.slice(a.lastIndexOf(':') + 1)),
    ...(separador === -1 ? [] : resto.slice(separador + 1)),
  ];
  return todasEn(DIFF_PERMITIDO_TESTER, rutas, cwd);
}

// git grep: sin preguntar si todas las rutas son del tester o del contrato (sin rutas buscaría
// también en la implementación). -O abre los resultados con otro programa.
function grepDelTester(resto, cwd) {
  if (resto.some((a) => a.startsWith('-O') || a.startsWith('--open-files-in-pager'))) return false;
  const separador = resto.indexOf('--');
  const rutas = separador === -1
    ? resto.filter((a) => !a.startsWith('-') && /[/\\]/.test(a))
    : resto.slice(separador + 1);
  return todasEn(DIFF_PERMITIDO_TESTER, rutas, cwd);
}

function gitDelTester(args, cwd) {
  const inicio = args[0] === '--no-pager' ? 1 : 0;
  const [subcomando, ...resto] = args.slice(inicio);
  if (subcomando === 'add') {
    const rutas = resto.filter((a) => a !== '--' && !a.startsWith('-'));
    return !resto.some((a) => ADD_PROHIBIDO.has(a)) && todasEn(ESCRITURA_TESTER, rutas, cwd);
  }
  if (subcomando === 'commit') return !resto.some((a) => COMMIT_PROHIBIDO.test(a));
  if (subcomando === 'diff' || subcomando === 'show') return diffDelTester(resto, cwd);
  if (subcomando === 'grep') return grepDelTester(resto, cwd);
  if (subcomando === 'log' && resto.some((a) => ['-p', '-u', '--patch'].includes(a))) return diffDelTester(resto, cwd);
  if (subcomando === 'branch') return gitDeLectura(args.slice(inicio));
  return GIT_RESUMEN_TESTER.has(subcomando) && !resto.some((a) => a.startsWith('--output'));
}

// dotnet format sólo sobre archivos del tester: dotnet format [whitespace|style] --include <rutas>
function formatDelTester(args, cwd) {
  const rutas = [];
  let enInclude = false;
  for (const a of args.slice(1)) {
    if (a === '--include') enInclude = true;
    else if (a === '--no-restore' || (rutas.length === 0 && !enInclude && ['whitespace', 'style'].includes(a))) enInclude = false;
    else if (enInclude && !a.startsWith('-')) rutas.push(a);
    else return false;
  }
  return todasEn(ESCRITURA_TESTER, rutas, cwd);
}

function permitidoAlTester(segmento, cwd) {
  if (VALIDAR_JSON.test(segmento)) {
    return todasEn(ESCRITURA_TESTER, [segmento.match(VALIDAR_JSON)[1]], cwd);
  }
  if (esNpm(segmento)) return npmDelFrontend(palabras(segmento).slice(1), cwd, NPM_TESTER) === 'permite';
  const [programa = '', ...args] = palabras(segmento);
  const nombre = programa.toLowerCase().replace(/\.exe$/, '');
  if (nombre === 'git') return gitDelTester(args, cwd);
  if (nombre === 'dotnet') return dotnetDeLectura(args) || (args[0] === 'format' && formatDelTester(args, cwd));
  return TERMINAL_LECTURA.has(nombre);
}

// Programa de un segmento, también detrás de los operadores de llamada de PowerShell (& y .).
// Sin extensión: npm.cmd o node.bat no esquivan la lista de intérpretes.
function programaDe(segmento) {
  const [primera = '', segunda = ''] = palabras(segmento);
  const programa = primera === '&' || primera === '.' ? segunda : primera;
  return path.basename(programa.replace(/\\/g, '/')).toLowerCase().replace(/\.(exe|cmd|bat)$/, '');
}

function decidirComandoTester(linea, cwd) {
  const normalizada = linea.replace(/\\/g, '/').toLowerCase();
  const prohibida = LECTURA_PROHIBIDA_TESTER.find((ruta) => normalizada.includes(ruta.toLowerCase().replace(/\/$/, '')));
  if (prohibida !== undefined) {
    return niega(`El tester no lee la implementación ("${prohibida}"): las pruebas son de caja negra `
      + '(docs/agentes/tester.md). Si el contrato no define algo, pedilo.');
  }
  for (const segmento of segmentos(linea)) {
    if (esNpm(segmento)) {
      if (npmDelFrontend(palabras(segmento).slice(1), cwd, NPM_TESTER) === null) return niega(MOTIVO_NPM);
      continue;
    }
    const programa = programaDe(segmento);
    if (INTERPRETES.has(programa) && !VALIDAR_JSON.test(segmento)) {
      return niega('El tester no ejecuta scripts (node -e, python -c, código en base64…): con ellos se editaron '
        + 'archivos por fuera de los permisos y se rompieron pruebas (E-013). Editá sólo con la herramienta de '
        + "edición. Para validar un JSON: node -e \"JSON.parse(require('fs').readFileSync('<archivo>','utf8'))\".");
    }
    if (BORRAR.has(programa)) return niega('El tester no borra archivos: lo decide el responsable.');
  }
  const sinComillas = linea.replace(/"[^"]*"|'[^']*'/g, '""');
  const redireccion = [...sinComillas.matchAll(REDIRECCION)].some((m) => !DESTINO_INOFENSIVO.test(m[1]));
  if (redireccion || ESCRITURA_EN_COMANDO.test(linea)) {
    return niega('El tester no escribe archivos con comandos ni redirecciones: PowerShell rompe la codificación '
      + '(E-013). Usá la herramienta de edición.');
  }
  return segmentos(linea).every((s) => permitidoAlTester(s, cwd)) ? permite : pregunta;
}

function decidirComando(args) {
  const linea = String(args.CommandLine ?? '');
  if (GIT_PUSH.test(linea)) return niega('git push lo hace sólo el responsable (AGENTS.md, «No hacer»).');
  if (rol === 'tester') return decidirComandoTester(linea, args.Cwd);
  const motivo = motivoComandoNoLectura(linea, args.Cwd);
  return motivo === null
    ? pregunta
    : niega(`${rol === 'revisor' ? 'El revisor' : `El rol «${rol}»`} sólo usa comandos de lectura (docs/agentes/revisor.md): ${motivo}. `
      + 'Para leer el diff, por partes: git diff main...HEAD -- <ruta>.');
}

function decidirHerramienta(herramienta, args, artefactos) {
  if (herramienta === 'run_command') return decidirComando(args);
  if (HERRAMIENTAS_SUBAGENTES.has(herramienta)) {
    return niega('Los subagentes no pasan por este hook y podrían saltear los permisos del rol: no se usan.');
  }
  if (HERRAMIENTAS_ESCRITURA.has(herramienta)) return decidirEscritura(args, artefactos);
  if (HERRAMIENTAS_LECTURA.has(herramienta)) return decidirLectura(args, artefactos);
  return pregunta;
}

// Permisos exactos de una llamada, con el formato de agy: command(...), write_file(...), read_file(...)
function permisosDe(herramienta, args) {
  if (herramienta === 'run_command') return [`command(${String(args.CommandLine ?? '')})`];
  if (HERRAMIENTAS_ESCRITURA.has(herramienta)) {
    const destino = aAbsoluta(args.TargetFile);
    return destino === null ? [] : [`write_file(${destino})`];
  }
  if (HERRAMIENTAS_LECTURA.has(herramienta)) {
    return Object.entries(args)
      .filter(([campo]) => !CAMPOS_DESCRIPTIVOS.has(campo))
      .map(([, valor]) => aAbsoluta(valor))
      .filter((ruta) => ruta !== null)
      .map((ruta) => `read_file(${ruta})`);
  }
  return [];
}

// agy no toma un "allow" del hook como aprobación: sólo no pregunta si el permiso está concedido.
// Por eso un "allow" lleva el permiso exacto de la llamada (permissionOverrides, temporal). Y el
// "ask" del tester es "force_ask": pregunta siempre, aunque haya un permiso recordado en la
// configuración de agy (algunos recordados eran scripts que editaban archivos, E-015).
function decidir(entrada) {
  const llamada = entrada?.toolCall ?? {};
  const args = typeof llamada.args === 'string' ? JSON.parse(llamada.args) : (llamada.args ?? {});
  const respuesta = decidirHerramienta(llamada.name, args, entrada?.artifactDirectoryPath);
  if (respuesta.decision === 'allow') {
    const permisos = permisosDe(llamada.name, args);
    return permisos.length > 0 ? { ...respuesta, permissionOverrides: permisos } : respuesta;
  }
  if (respuesta.decision === 'ask' && rol === 'tester') return { ...respuesta, decision: 'force_ask' };
  return respuesta;
}

// Registro de cada decisión en la carpeta temporal, para revisar qué se permitió, qué se
// preguntó y qué se bloqueó (y confirmar que agy ejecuta el hook).
function registrar(entrada, respuesta) {
  try {
    const llamada = entrada?.toolCall ?? {};
    const args = typeof llamada.args === 'string' ? JSON.parse(llamada.args) : (llamada.args ?? {});
    const detalle = args.CommandLine ?? args.TargetFile ?? args.AbsolutePath ?? args.DirectoryPath ?? '';
    const linea = `${new Date().toISOString()} ${rol} ${llamada.name ?? '?'} ${respuesta.decision} ${detalle}`
      .replace(/\s+/g, ' ');
    appendFileSync(path.join(os.tmpdir(), 'rrhh-permisos.log'), `${linea}\n`);
  } catch {
    // el registro nunca cambia la decisión
  }
}

let respuesta;
let entrada;
try {
  process.stdin.setEncoding('utf8');
  let texto = '';
  for await (const parte of process.stdin) texto += parte;
  entrada = JSON.parse(texto);
  respuesta = decidir(entrada);
} catch (error) {
  respuesta = niega(`El hook de permisos falló (${error.message}); se bloquea por seguridad.`);
}
registrar(entrada, respuesta);
process.stdout.write(JSON.stringify(respuesta));
