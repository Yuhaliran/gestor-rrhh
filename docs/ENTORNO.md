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
SQL Server: instalar **SQL Server Express o LocalDB** desde el instalador oficial de Microsoft.
No hace falta SSMS: las migraciones crean la base.

## Agentes
```powershell
irm https://claude.ai/install.ps1 | iex                 # Claude Code (requiere plan Pro, Max, Team, Enterprise o Console)
irm https://antigravity.google/cli/install.ps1 | iex    # Antigravity CLI (inicio de sesión con Google)
```
Primer inicio: `claude` y `agy` dentro de la carpeta del proyecto; cada uno abre el navegador para iniciar sesión.

## Verificación
```powershell
git --version
dotnet --version        # 10.x
dotnet ef --version
node --version
newman --version
claude --version
agy --version
```

## No hace falta
- Un gateway de modelos (LiteLLM, Bedrock): cada agente usa su propia suscripción.
- Docker ni modelos locales (en una laptop de 2 núcleos serían lentos).
