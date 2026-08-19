import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Store {
  id: string;
  organizationId: string;
  name: string;
  code: string;
  address?: string;
  contactEmail?: string;
  contactPhone?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class StoreSelectionService {
  private activeStoreSignal = signal<Store | null>(null);
  public activeStore = this.activeStoreSignal.asReadonly();
  public hasActiveStore = computed(() => this.activeStoreSignal() !== null);

  constructor(private http: HttpClient) {}

  /**
   * Fetches all stores for the current tenant and auto-selects the first one if none is selected.
   */
  loadStores(): Observable<Store[]> {
    return this.http.get<Store[]>(`${environment.apiUrl}/stores`).pipe(
      tap(stores => {
        if (stores && stores.length > 0 && !this.activeStoreSignal()) {
          this.setActiveStore(stores[0]);
        }
      })
    );
  }

  setActiveStore(store: Store) {
    this.activeStoreSignal.set(store);
  }

  getActiveStoreId(): string | null {
    const store = this.activeStoreSignal();
    return store ? store.id : null;
  }
}
