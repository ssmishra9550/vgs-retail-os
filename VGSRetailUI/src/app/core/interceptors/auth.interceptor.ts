import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // TenantId interceptor pattern could also be handled here 
  // if required by backend, but backend uses JWT claims.
  // We may need to pass X-Tenant-Id for some requests if not in JWT.
  const tenantId = authService.getTenantId() || 'vgs-tenant-01'; // Default tenant for pilot phase
  if (tenantId && !req.headers.has('X-Tenant-Id')) {
     req = req.clone({
      setHeaders: {
        'X-Tenant-Id': tenantId
      }
    });
  }

  return next(req);
};
