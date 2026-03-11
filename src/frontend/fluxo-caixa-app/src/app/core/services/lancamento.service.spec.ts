import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LancamentoService } from './lancamento.service';
import { Lancamento, CreateLancamentoDto, TipoLancamento } from '../models/lancamento.model';
import { environment } from '../../../environments/environment';

describe('LancamentoService', () => {
  let service: LancamentoService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LancamentoService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(LancamentoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get lancamentos', () => {
    const mockLancamentos: Lancamento[] = [
      {
        id: '1', descricao: 'Test', valor: 100, tipo: TipoLancamento.Credito,
        dataLancamento: '2024-01-01', createdAt: '2024-01-01'
      }
    ];

    service.getLancamentos().subscribe(lancamentos => {
      expect(lancamentos.length).toBe(1);
      expect(lancamentos[0].descricao).toBe('Test');
    });

    const req = httpMock.expectOne(`${environment.apiLancamentosUrl}/api/v1/lancamentos`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLancamentos);
  });

  it('should get a single lancamento', () => {
    const mockLancamento: Lancamento = {
      id: '1', descricao: 'Test', valor: 100, tipo: TipoLancamento.Credito,
      dataLancamento: '2024-01-01', createdAt: '2024-01-01'
    };

    service.getLancamento('1').subscribe(lancamento => {
      expect(lancamento.id).toBe('1');
    });

    const req = httpMock.expectOne(`${environment.apiLancamentosUrl}/api/v1/lancamentos/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLancamento);
  });

  it('should create lancamento', () => {
    const dto: CreateLancamentoDto = {
      descricao: 'New', valor: 200, tipo: TipoLancamento.Debito,
      dataLancamento: '2024-01-01'
    };

    service.createLancamento(dto).subscribe(result => {
      expect(result.valor).toBe(200);
    });

    const req = httpMock.expectOne(`${environment.apiLancamentosUrl}/api/v1/lancamentos`);
    expect(req.request.method).toBe('POST');
    req.flush({ ...dto, id: 'new-id', createdAt: '2024-01-01' });
  });
});
