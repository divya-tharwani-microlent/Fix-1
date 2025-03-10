import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  apiURL: string = environment.baseUrl;
  private userDetailSubject: BehaviorSubject<any>;
  public userDetail$: Observable<any>;

  constructor(private http: HttpClient, private router: Router) { 
    const userDetail = {
      username: localStorage.getItem('username'),
      email: localStorage.getItem('email')
    };
    this.userDetailSubject = new BehaviorSubject(userDetail);
    this.userDetail$ = this.userDetailSubject.asObservable();
  }

  getUserId(): string | null {
    return localStorage.getItem('ZUID');
  }
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  saveTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  // saveUserDetail(username: string, email: string) {
  //   localStorage.setItem('username', username);
  //   localStorage.setItem('email', email);
  // }

  getUserDetail(): Observable<any> {
    return this.userDetail$;
  }

  saveUserDetail(username: string, email: string): void {
    const userDetail = { username, email };
    localStorage.setItem('username', username);
    localStorage.setItem('email', email);
    this.userDetailSubject.next(userDetail);
  }

  async refreshToken(): Promise<string | null> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.logout();
      return null;
    }

    try {
      const response: any = await this.http
        .post(`${this.apiURL}zoho/refreshToken`, { refreshToken })
        .toPromise();

      if (response.accessToken) {
        this.saveTokens(response.accessToken, refreshToken);
        return response.accessToken;
      }
    } catch (error) {
      this.logout();
    }
    return null;
  }

  logout() {
    localStorage.clear();
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
