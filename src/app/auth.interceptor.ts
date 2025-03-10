import {
    HttpInterceptor,
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpErrorResponse,
  } from '@angular/common/http';
  import { Injectable } from '@angular/core';
  import { AuthService } from './services/auth.service';
  import { from, Observable, throwError } from 'rxjs';
  import { catchError, switchMap } from 'rxjs/operators';
  
  @Injectable()
  export class AuthInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) {}
  
    intercept(
      req: HttpRequest<any>,
      next: HttpHandler
    ): Observable<HttpEvent<any>> {
      let authToken = this.authService.getAccessToken();
      
      const authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${authToken}` },
      });
  
      return next.handle(authReq).pipe(
        catchError((error) => {
          if (error instanceof HttpErrorResponse && error.status === 401) {
            // Convert Promise to Observable using `from()`
            return from(this.authService.refreshToken()).pipe(
              switchMap((newToken) => {
                if (newToken) {
                  const newAuthReq = req.clone({
                    setHeaders: { Authorization: `Bearer ${newToken}` },
                  });
                  return next.handle(newAuthReq);
                }
                return throwError(() => error);
              })
            );
          }
          return throwError(() => error);
        })
      );
    }
  }
  