import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { App } from '../../src/app/app';

// Prueba de la estructura (tarea 31): confirma que `ng test` encuentra las pruebas de
// frontend/tests/ y que Testing Library funciona con esta versión de Angular.
describe('App', () => {
  it('render_SinRuta_MuestraElTituloDeLaAplicacion', async () => {
    // Arrange y Act
    await render(App, { providers: [provideRouter([])] });

    // Assert
    expect(screen.getByRole('heading', { name: 'Recursos Humanos' })).toBeTruthy();
  });
});
