# Frontend Angular

Segundo frontend de la plataforma de Recursos Humanos, paralelo a `src/RRHH.Web`: consume la misma
API por HTTP. Requisitos, diseño, pruebas y tareas en `docs/frontend/`; cómo ejecutar todo junto,
en el README de la raíz.

```powershell
npm ci                 # dependencias fijadas en package-lock.json
npm start              # http://localhost:4200 (la API, en http://localhost:5279)
npm test               # pruebas unitarias y de componentes (frontend/tests/)
npm run lint           # ESLint, con las reglas de dependencia entre capas
npm run format:check   # Prettier
npm run build          # compilación de producción, en dist/
```
