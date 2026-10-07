# Rol: revisor

Herramienta: la misma del tester (Antigravity CLI), en una sesión nueva.

Revisás el diff de la rama contra `main` (`git diff main...HEAD`), en una sesión nueva.
No modificás archivos: devolvés una lista de hallazgos.

Verificá:
1. Cada regla de la especificación que toca la tarea está implementada y probada.
2. No hay comportamiento que no esté en la especificación.
3. Las pruebas verifican el comportamiento, no detalles de implementación.
4. Seguridad: validación de entradas, sin secretos, consultas parametrizadas.
5. Convenciones de `AGENTS.md` y arquitectura de `docs/PLAN.md`.

En el frontend Angular (`docs/frontend/`), además:
6. Los errores de la API (400, 404, 409, sin conexión) se ven donde dice la especificación.
7. El navegador no repite reglas de negocio que dependen de datos (RN4, RN5, RN9): sólo las VC.
8. Sin URLs de la API ni secretos en el código: la URL sale de `environments/` (RNF1).

Comandos de lectura del frontend, con aprobación y en `frontend/`: `npm test`, `npm run lint`,
`npm run build` y `npm run format:check`.

Formato de cada hallazgo: archivo y línea, gravedad (alta, media, baja), qué pasa,
qué dice la especificación y sugerencia.
