import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ConsolidadoService } from './consolidado.service';
import { ConsolidadoDiario } from '../models/consolidado-diario.model';
import { environment } from '../../../environments/environment';

describe('ConsolidadoService', () => {
  let service: ConsolidadoService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ConsolidadoService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ConsolidadoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get consolidado without date', () => {
    const mockConsolidado: ConsolidadoDiario = {
      id: '1', data: '2024-01-01', totalCreditos: 500, totalDebitos: 200,
      saldo: 300, updatedAt: '2024-01-01'
    };

    service.getConsolidado().subscribe(consolidado => {
      expect(consolidado.saldo).toBe(300);
    });

    const req = httpMock.expectOne(`${environment.apiConsolidadoUrl}/api/v1/consolidado`);
    expect(req.request.method).toBe('GET');
    req.flush(mockConsolidado);
  });

  it('should get consolidado with date', () => {
    const mockConsolidado: ConsolidadoDiario = {
      id: '1', data: '2024-01-15', totalCreditos: 1000, totalDebitos: 400,
      saldo: 600, updatedAt: '2024-01-15'
    };

    service.getConsolidado('2024-01-15').subscribe(consolidado => {
      expect(consolidado.data).toBe('2024-01-15');
    });

    const req = httpMock.expectOne(`${environment.apiConsolidadoUrl}/api/v1/consolidado/2024-01-15`);
    expect(req.request.method).toBe('GET');
    req.flush(mockConsolidado);
  });
});
