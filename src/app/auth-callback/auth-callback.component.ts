import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { AuthService } from '../services/auth.service';


@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
})
export class AuthCallbackComponent implements OnInit {
  apiURL: string = environment.baseUrl;
  isLoading = false;
  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit() {
    // Get the authorization code from URL parameters
    const code = this.route.snapshot.queryParamMap.get('code');
    const error = this.route.snapshot.queryParamMap.get('error');

    if (error) {
      // Redirect to login page if consent is not accepted
      this.router.navigate(['/login']);
      return;
    }

    if (code) {
      this.isLoading = true;
      // Exchange code for access token
      const tokenApiUrl = this.apiURL + 'zoho/getToken'; // Your backend API endpoint

      this.http.post(tokenApiUrl, { code }).subscribe({
        next: (response: any) => {
          console.log(response);
          // Save login status in session
          // sessionStorage.setItem('isLoggedIn', 'true');
          localStorage.setItem('ZUID', response.userId);
          this.authService.saveTokens(response.access_token, response.refresh_token);
          this.authService.saveUserDetail(response.display_Name, response.email);
          // Redirect to timesheet page on successful login
          this.router.navigate(['/timesheet']);
          this.isLoading = false;
        },
        error: () => {
          // Handle error and redirect to login page
          this.router.navigate(['/login']);
          this.isLoading = false;
        },
      });
    }
  }
}
