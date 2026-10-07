import { Injectable } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

// Textos del paginador del Contrato de interfaz (RF2); Material los trae en inglés.
@Injectable()
export class PaginadorEnEspanol extends MatPaginatorIntl {
  override itemsPerPageLabel = 'Registros por página';
  override nextPageLabel = 'Página siguiente';
  override previousPageLabel = 'Página anterior';
  override firstPageLabel = 'Primera página';
  override lastPageLabel = 'Última página';

  // «1 – 10 de 45»
  override getRangeLabel = (pagina: number, tamanio: number, total: number): string => {
    if (total === 0 || tamanio === 0) {
      return `0 de ${total}`;
    }
    const desde = pagina * tamanio;
    return `${desde + 1} – ${Math.min(desde + tamanio, total)} de ${total}`;
  };
}
