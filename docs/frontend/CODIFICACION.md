# Pautas de codificación del frontend

Etapa del ciclo de vida: **codificación**. Requisitos (`docs/frontend/ESPECIFICACION.md`) y diseño
(`docs/frontend/PLAN.md`, `docs/frontend/PLAN_PRUEBAS.md`) ya están cerrados.

Este documento define **cómo** se escribe el frontend, con un patrón de referencia completo para
Países. Las demás pantallas siguen el mismo patrón, como las entidades del backend siguen el de
`docs/CODIFICACION.md`.

> Los fragmentos son un patrón de referencia escrito antes de tener el código: se compilan y se
> ajustan en las tareas 35 a 38. Si algo no funciona con Angular 22 o PrimeNG 22, se corrige el
> código y este documento.

## 1. Configuración del proyecto
- Angular 22: componentes standalone, sin zone.js (la detección de cambios la disparan los
  signals), control de flujo `@if`/`@for`.
- TypeScript con `strict` y `strictTemplates`; sin `any` (ni explícito ni implícito).
- Prettier (`.prettierrc`): ancho 100, comillas simples, `endOfLine: auto`. ESLint con
  angular-eslint y las reglas ARQF. Prefijo de selectores: `rrhh`.
- `npm run lint`, `npm run format:check`, `npm test` y `npm run build` antes de cada commit.

## 2. Nombres
| Elemento | Convención | Ejemplo |
|---|---|---|
| Archivo | kebab-case, sin sufijo de tipo (estilo 2025 de Angular) | `paises-listado.ts`, `paises-listado.html` |
| Componente | PascalCase, sin sufijo `Component` | `PaisesListado`, `PaisFormulario` |
| Selector | `rrhh-` + kebab-case | `rrhh-paises-listado` |
| Tipos de los DTOs | El nombre del record de `RRHH.Contratos` | `PaisDto`, `GuardarPaisDto` |
| Propiedades de los DTOs | camelCase, como las serializa la API | `codigoIso2`, `regla29Febrero` |
| Tokens de inyección | Mayúsculas con guion bajo | `CLIENTE_RRHH`, `URL_API` |
| Lógica de pantalla | `crear<Pieza>` | `crearListado`, `crearFormulario` |
| Signals y propiedades | Sustantivo en camelCase | `elementos`, `cargando`, `noExiste` |
| Métodos | Verbo en infinitivo | `guardar()`, `eliminar(pais)` |
| Pruebas | `describe('<Unidad>')`, `it('Metodo_Escenario_ResultadoEsperado')` | `it('enviar_ConError400_PoneElMensajeEnSuControl')` |

Código en español sin tildes; textos para el usuario en español con tildes. Esos textos son parte
del «Contrato de interfaz» de la especificación: no se cambian sin cambiarla.

## 3. Estructura y dependencias
```
contratos/    tipos e interfaces; sin lógica                     → nada (sólo tipos de rxjs, @angular/core y @angular/forms)
api/          ClienteRrhhHttp, interceptor de errores, URL_API   → contratos
servicios/    lógica de pantallas, formatos, CLIENTE_RRHH        → contratos
componentes/  piezas de interfaz reutilizables                   → servicios, contratos
vistas/       una carpeta por mantenimiento                      → servicios, componentes, contratos
app.config.ts raíz de composición                                → todo, incluidos api/ y environments/
```
Las reglas ARQF1 a ARQF3 (`docs/frontend/PLAN.md`) las verifica `npm run lint`. Las pruebas
(`frontend/tests/`) pueden importar cualquier capa.

## 4. Componentes
- `changeDetection: ChangeDetectionStrategy.OnPush` en todos.
- Dependencias con `inject()`, no por constructor. Entradas y salidas con `input()` y `output()`.
- Estado de la pantalla en signals. Los `Observable` quedan en el cliente y en `servicios/`; el
  componente que se suscribe a uno usa `takeUntilDestroyed`.
- Plantilla en su propio `.html`. En la plantilla sólo se leen signals y se llaman métodos: los
  cálculos van en el componente (`computed`) o en `servicios/formatos.ts`.
- `@for` siempre con `track` (el `id` del DTO).
- Texto de la API con interpolación `{{ }}`, nunca con `[innerHTML]` (RNF5).
- **Enlace o botón**, según el «Contrato de interfaz» de la especificación (las pruebas los
  buscan con `getByRole`): lo que navega es `<a pButton routerLink>`; lo que actúa,
  `<button pButton type="button">`; «Guardar» es el `type="submit"` del formulario.
- `p-toast` está una sola vez, en el layout. `p-confirmdialog` lo incluye cada vista que confirma
  (los listados y el detalle del colaborador), para que su prueba de componente pueda confirmar.
- PrimeNG 22: botones con la directiva `pButton` y el texto adentro (el componente `p-button` está
  obsoleto). Cada componente se importa por separado: `ButtonDirective` (`primeng/button`),
  `InputText`, `InputNumber`, `Select`, `DatePicker`, `Message`, `TableModule`, `Toast`,
  `ConfirmDialog`; `MessageService` y `ConfirmationService` de `primeng/api`.
- Formularios reactivos tipados con `NonNullableFormBuilder`. Cada campo va dentro de
  `rrhh-campo-formulario`, que asocia la etiqueta (`<label for>`) con el control (`id` o
  `inputId` en PrimeNG) y el error con el control (`aria-describedby`), para RNF3 y para que las
  pruebas lo encuentren con `getByLabelText`.
- Los mensajes de las validaciones del navegador (los del «Contrato de interfaz») salen de
  `servicios/formatos.ts`. Un error de la API se muestra con el texto de la API (RF5).

## 5. Patrón de referencia: Países

### DTOs (`contratos/dtos.ts`)
Uno por record de `RRHH.Contratos`, con el archivo de origen en un comentario:

```ts
// src/RRHH.Contratos/Paises/Regla29Febrero.cs
export type Regla29Febrero = 'VeintiochoDeFebrero' | 'PrimeroDeMarzo';

// src/RRHH.Contratos/Paises/PaisDto.cs
export interface PaisDto {
  id: number;
  nombre: string;
  codigoIso2: string;
  edadMinima: number;
  edadMaxima: number;
  regla29Febrero: Regla29Febrero;
}

// src/RRHH.Contratos/Paises/GuardarPaisDto.cs
export interface GuardarPaisDto {
  nombre: string;
  codigoIso2: string;
  edadMinima: number;
  edadMaxima: number;
  regla29Febrero: Regla29Febrero;
}

// src/RRHH.Contratos/Comun/Consulta.cs
export interface Consulta {
  pagina: number;
  tamanio: number;
  buscar?: string;
}

// src/RRHH.Contratos/Comun/Pagina.cs (totalPaginas es una propiedad calculada del record)
export interface Pagina<T> {
  elementos: T[];
  total: number;
  numero: number;
  tamanio: number;
  totalPaginas: number;
}
```

### Error de la API (`contratos/errores.ts`)
```ts
// Toda respuesta no exitosa, ya traducida por el interceptor (docs/frontend/PLAN.md, «Errores»).
export interface ErrorApi {
  estado: number; // 0: sin respuesta
  detalle?: string;
  errores: Record<string, string[]>; // claves normalizadas: 'nombre', 'empresas[0].fechaIngreso'
}
```

### Cliente (`api/cliente-rrhh-http.ts`)
```ts
@Injectable()
export class ClienteRrhhHttp implements ClienteRrhh {
  private readonly http = inject(HttpClient);
  private readonly url = inject(URL_API);

  readonly paises = {
    listar: (consulta: Consulta) =>
      this.http.get<Pagina<PaisDto>>(`${this.url}/api/paises`, { params: parametros(consulta) }),
    obtener: (id: number) => this.http.get<PaisDto>(`${this.url}/api/paises/${id}`),
    crear: (datos: GuardarPaisDto) => this.http.post<PaisDto>(`${this.url}/api/paises`, datos),
    actualizar: (id: number, datos: GuardarPaisDto) =>
      this.http.put<PaisDto>(`${this.url}/api/paises/${id}`, datos),
    eliminar: (id: number) => this.http.delete<void>(`${this.url}/api/paises/${id}`),
    departamentos: (paisId: number) =>
      this.http.get<DepartamentoDto[]>(`${this.url}/api/paises/${paisId}/departamentos`),
  };
  // departamentos, municipios, empresas y colaboradores siguen igual
}

// `buscar` sólo viaja si tiene texto
function parametros(consulta: Consulta): HttpParams {
  let parametros = new HttpParams().set('pagina', consulta.pagina).set('tamanio', consulta.tamanio);
  if (consulta.buscar?.trim()) {
    parametros = parametros.set('buscar', consulta.buscar.trim());
  }
  return parametros;
}
```
El interceptor (`api/errores.interceptor.ts`) es una función `HttpInterceptorFn` que, con
`catchError`, convierte el `HttpErrorResponse` en un `ErrorApi` según la tabla «Errores» del plan.
El cliente no maneja errores.

### Listado (`vistas/paises/paises-listado.ts` y `.html`)
```ts
@Component({
  selector: 'rrhh-paises-listado',
  imports: [RouterLink, ButtonDirective, TablaPaginada],
  templateUrl: './paises-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisesListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) => this.cliente.paises.listar(consulta));
  protected readonly eliminacion = crearEliminacion(
    (pais: PaisDto) => this.cliente.paises.eliminar(pais.id),
    () => this.listado.recargar(),
  );
  protected readonly textoRegla = textoRegla29Febrero; // servicios/formatos.ts
}
```

```html
<h1>Países</h1>
<a pButton routerLink="/paises/nuevo">Nuevo</a>

<rrhh-tabla-paginada [listado]="listado">
  <ng-template #encabezado>
    <tr>
      <th>Nombre</th>
      <th>Código ISO</th>
      <th>Edad mínima</th>
      <th>Edad máxima</th>
      <th>29 de febrero</th>
      <th><span class="oculto">Acciones</span></th>
    </tr>
  </ng-template>
  <ng-template #fila let-pais>
    <tr>
      <td>{{ pais.nombre }}</td>
      <td>{{ pais.codigoIso2 }}</td>
      <td>{{ pais.edadMinima }}</td>
      <td>{{ pais.edadMaxima }}</td>
      <td>{{ textoRegla(pais.regla29Febrero) }}</td>
      <td>
        <a pButton [routerLink]="['/paises', pais.id, 'editar']">Editar</a>
        <button pButton type="button" severity="danger" (click)="eliminacion.eliminar(pais)">
          Eliminar
        </button>
      </td>
    </tr>
  </ng-template>
</rrhh-tabla-paginada>
```
`TablaPaginada` pone el campo «Buscar», la tabla de PrimeNG en modo lazy, el paginador (10, 20 y
50), el total, el indicador de carga y el error del listado (RF2, RF8). La plantilla de la vista
agrega `<p-confirmdialog />` para la confirmación de «Eliminar».

### Formulario (`vistas/paises/pais-formulario.ts` y `.html`)
Un mismo componente crea y edita: el `:id` llega como entrada (`withComponentInputBinding()` en
`app.config.ts`), lo que permite probarlo con `render(PaisFormulario, { inputs: { id: '7' } })`.

```ts
@Component({
  selector: 'rrhh-pais-formulario',
  imports: [ReactiveFormsModule, RouterLink, ButtonDirective, InputText, InputNumber, Select,
    Message, CampoFormulario],
  templateUrl: './pais-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisFormulario implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);

  readonly id = input<string>(); // vacío al crear

  protected readonly noExiste = signal(false);
  protected readonly opcionesRegla = OPCIONES_REGLA_29_FEBRERO; // servicios/formatos.ts

  // RF9: al crear propone 18, 100 y «28 de febrero»
  protected readonly grupo = inject(NonNullableFormBuilder).group(
    {
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      codigoIso2: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)]],
      edadMinima: [18, [Validators.required, Validators.min(0)]],
      edadMaxima: [100, [Validators.required, Validators.min(0)]],
      regla29Febrero: ['VeintiochoDeFebrero' as Regla29Febrero, Validators.required],
    },
    { validators: edadMinimaNoMayorQueMaxima }, // VC4, servicios/formatos.ts
  );

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: GuardarPaisDto) => {
      const id = Number(this.id());
      return id ? this.cliente.paises.actualizar(id, datos) : this.cliente.paises.crear(datos);
    },
    { volverA: '/paises' }, // RF4: aviso y regreso al listado
  );

  ngOnInit(): void {
    const id = Number(this.id());
    if (!id) {
      return;
    }
    this.cliente.paises
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (pais) => this.grupo.patchValue(pais), // patchValue ignora el id
        error: (error: ErrorApi) => this.noExiste.set(error.estado === 404), // RF7
      });
  }
}
```

```html
<h1>{{ id() ? 'Editar país' : 'Nuevo país' }}</h1>

@if (noExiste()) {
  <p>El registro no existe.</p>
  <a routerLink="/paises">Volver al listado</a>
} @else {
  <form [formGroup]="grupo" (ngSubmit)="formulario.enviar()">
    <rrhh-campo-formulario etiqueta="Nombre" campo="nombre" [control]="grupo.controls.nombre">
      <input pInputText id="nombre" formControlName="nombre" maxlength="100" />
    </rrhh-campo-formulario>
    <!-- Código ISO, Edad mínima, Edad máxima y Cumpleaños del 29 de febrero, igual -->

    @if (formulario.errorGeneral(); as mensaje) {
      <p-message severity="error">{{ mensaje }}</p-message>
    }
    <button pButton type="submit" [disabled]="formulario.guardando()">Guardar</button>
    <a pButton severity="secondary" routerLink="/paises">Cancelar</a>
  </form>
}
```
`crearFormulario(grupo, guardar, opciones)`: si el grupo es inválido marca los controles y no
llama a la API (VC); al guardar, avisa «Se guardó correctamente.» y navega a `volverA`; un 400 pone
cada mensaje en su control y el resto va a `errorGeneral` (`docs/frontend/PLAN.md`, «Lógica de las
pantallas»).

## 6. Pruebas
Las escribe el tester (`docs/frontend/PLAN_PRUEBAS.md`). Estos ejemplos fijan la forma.

### Proveedores comunes (`tests/apoyo/proveedores.ts`)
```ts
// La API se simula con HttpTestingController; el cliente y el interceptor son los reales.
export function proveedoresDePrueba(): Provider[] {
  return [
    provideHttpClient(withInterceptors([erroresInterceptor])),
    provideHttpClientTesting(),
    provideRouter([]),
    { provide: URL_API, useValue: 'http://api.prueba' },
    { provide: CLIENTE_RRHH, useClass: ClienteRrhhHttp },
    MessageService,
    ConfirmationService,
  ];
}
```

### Prueba unitaria del cliente
```ts
describe('ClienteRrhhHttp', () => {
  it('listar_ConBusqueda_EnviaPaginaTamanioYBuscar', () => {
    // Arrange
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    // Act
    cliente.paises.listar({ pagina: 2, tamanio: 10, buscar: 'gua' }).subscribe();
    const pedido = api.expectOne((r) => r.url === 'http://api.prueba/api/paises');

    // Assert
    expect(pedido.request.params.get('pagina')).toBe('2');
    expect(pedido.request.params.get('tamanio')).toBe('10');
    expect(pedido.request.params.get('buscar')).toBe('gua');
    api.verify();
  });
});
```

### Prueba de componente
```ts
describe('PaisesListado', () => {
  it('eliminar_ConConflicto_MuestraElDetalleYConservaLaFila', async () => {
    // Arrange
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala]));
    await userEvent.click(await screen.findByRole('button', { name: 'Eliminar' }));

    // Act
    await userEvent.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));
    api
      .expectOne((r) => r.method === 'DELETE')
      .flush(conflicto('El país tiene departamentos.'), { status: 409, statusText: 'Conflict' });

    // Assert
    expect(await screen.findByText('El país tiene departamentos.')).toBeTruthy();
    expect(screen.getByText('Guatemala')).toBeTruthy();
  });
});
```
`paginaDe`, `guatemala` y `conflicto` son datos de `tests/apoyo/`, escritos desde
`docs/PLAN.md`, «API». La confirmación funciona porque la vista incluye su `p-confirmdialog`. Los
avisos de éxito (RF4) están en el `p-toast` del layout, que la prueba de una vista no tiene: se
verifican con un doble de `MessageService` (`vi.spyOn(TestBed.inject(MessageService), 'add')`) y
con la navegación.

## 7. Terminado de cada tarea
- [ ] `npm run lint` (incluye ARQF), `npm run format:check`, `npm test` y `npm run build` sin
      errores ni advertencias
- [ ] Requisitos de la tarea con su prueba y en la tabla de trazabilidad
- [ ] Textos iguales a los del «Contrato de interfaz»
- [ ] Sin URLs, secretos ni datos en el almacenamiento del navegador
- [ ] Si se tocó la API: `dotnet build`, `dotnet test` y Postman en verde
- [ ] Commit con el mensaje de `docs/frontend/TAREAS.md`, en la rama de la fase
