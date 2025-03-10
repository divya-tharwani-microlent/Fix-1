import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  constructor(private http: HttpClient, private router: Router, private authService: AuthService) { }

  login() {
    let authToken = this.authService.getAccessToken();
    if (authToken) {
     window.location.href = '/timesheet';
    }
    else {
      sessionStorage.setItem('isLoggedIn', 'false');
      // Redirect user to Zoho OAuth API consent page
      const clientId = environment.clientId;
      const redirectUri = environment.redirectUrl;
      const authUrl = environment.authUrl + `/oauth/v2/auth?scope=ZohoProjects.portals.read,ZohoProjects.tasks.read,ZohoProjects.projects.read,ZohoProjects.timesheets.read,aaaserver.profile.READ,ZohoProjects.users.read&client_id=${clientId}&response_type=code&access_type=offline&redirect_uri=${redirectUri}&prompt=consent`;

      window.location.href = authUrl;
    }
  }
}
