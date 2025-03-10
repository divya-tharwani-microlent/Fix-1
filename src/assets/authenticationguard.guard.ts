import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class AuthenticationguardGuard implements CanActivate {
  constructor(
    public auth:AuthService){

  }
  canActivate(route: ActivatedRouteSnapshot,state: RouterStateSnapshot) {
      var token = localStorage.getItem('accessToken');
        if (token && token != null && token != '') {
            return true;
        } else {
            this.auth.logout();
            return false;
        }
    // return true;
  }
  // constructor(private router: Router) {}

  // canActivate(): boolean {
  //   const token = localStorage.getItem('token');
  //   if (token) {
  //     return true;
  //   } else {
  //     this.router.navigate(['/login']);
  //     return false;
  //   }
  // }
}
