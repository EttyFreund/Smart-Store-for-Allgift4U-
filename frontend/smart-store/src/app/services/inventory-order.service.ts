import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class InventoryOrderService {
  private api = 'http://localhost:5187/api/inventory';

  constructor(private http: HttpClient) {}

  processOrder(orderName: string): Observable<any> {
    return this.http.post<any>(`${this.api}/process-order`, { OrderName: orderName }).pipe(
      timeout(120000)
    );
  }
}
