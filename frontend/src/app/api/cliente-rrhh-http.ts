import { Injectable } from '@angular/core';

import type { ClienteRrhh } from '../contratos/cliente';

// Esqueleto hasta la tarea 37: cada método lanza «No implementado» (E-014).
const noImplementado = (): never => {
  throw new Error('No implementado');
};

// Cliente de la API con HttpClient (docs/frontend/PLAN.md, «Contrato del cliente»).
@Injectable()
export class ClienteRrhhHttp implements ClienteRrhh {
  readonly paises: ClienteRrhh['paises'] = {
    listar: noImplementado,
    obtener: noImplementado,
    crear: noImplementado,
    actualizar: noImplementado,
    eliminar: noImplementado,
    departamentos: noImplementado,
  };

  readonly departamentos: ClienteRrhh['departamentos'] = {
    listar: noImplementado,
    obtener: noImplementado,
    crear: noImplementado,
    actualizar: noImplementado,
    eliminar: noImplementado,
    municipios: noImplementado,
  };

  readonly municipios: ClienteRrhh['municipios'] = {
    listar: noImplementado,
    obtener: noImplementado,
    crear: noImplementado,
    actualizar: noImplementado,
    eliminar: noImplementado,
  };

  readonly empresas: ClienteRrhh['empresas'] = {
    listar: noImplementado,
    obtener: noImplementado,
    crear: noImplementado,
    actualizar: noImplementado,
    eliminar: noImplementado,
    colaboradores: noImplementado,
  };

  readonly colaboradores: ClienteRrhh['colaboradores'] = {
    listar: noImplementado,
    obtener: noImplementado,
    crear: noImplementado,
    actualizar: noImplementado,
    eliminar: noImplementado,
    asociarEmpresa: noImplementado,
    actualizarEmpresa: noImplementado,
    quitarEmpresa: noImplementado,
  };
}
