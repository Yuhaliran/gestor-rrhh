# Preparación del entorno (Windows)

Verificado en septiembre de 2026. Todo desde PowerShell.

## Base de desarrollo
```powershell
winget install Git.Git
winget install Microsoft.DotNet.SDK.10
winget install OpenJS.NodeJS.LTS
winget install Microsoft.VisualStudioCode
winget install Postman.Postman
```
Abrir una terminal nueva y:
```powershell
dotnet tool install --global dotnet-ef
npm install -g newman
code --install-extension ms-dotnettools.csdevkit
```
Editor: VS Code con C# Dev Kit. Visual Studio 2022 no es compatible con .NET 10 y reescribe mal
los lock files (E-010); si se prefiere Visual Studio, usar la versión 2026.

SDK: `global.json` fija .NET 10 (`10.0.100` o superior, con `rollForward: latestFeature` y sin
versiones preliminares). El repo usa el SDK 10 más nuevo instalado y nunca uno de otra versión
mayor, aunque un instalador (por ejemplo, el de Visual Studio) agregue un SDK más nuevo.

SQL Server: instalar **SQL Server Express o LocalDB** desde el instalador oficial de Microsoft.
No hace falta SSMS: las migraciones crean la base.

## Agentes
```powershell
irm https://claude.ai/install.ps1 | iex                 # Claude Code (requiere plan Pro, Max, Team, Enterprise o Console)
irm https://antigravity.google/cli/install.ps1 | iex    # Antigravity CLI (inicio de sesión con Google)
```
Primer inicio: `claude` y `agy` dentro de la carpeta del proyecto; cada uno abre el navegador para iniciar sesión.
En `agy`, confiar en la carpeta cuando lo pregunte: así carga el hook de permisos del tester y
del revisor (`.agents/hooks.json`, ver `docs/AGENTES.md`).

## Verificación
```powershell
git --version
dotnet --version        # 10.x
dotnet ef --version
node --version
newman --version
claude --version
agy --version
agy -p "/hooks"         # desde PowerShell, en el proyecto: debe listar permisos-por-rol
```

## No hace falta
- Un gateway de modelos (LiteLLM, Bedrock): cada agente usa su propia suscripción.
- Docker ni modelos locales (en una laptop de 2 núcleos serían lentos).
