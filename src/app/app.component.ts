import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'zoho-timesheet-app';
  isLoggedIn = false;
  profilePic: any;
  email: string | null = null;
  userName: string | null = null;
  isSidebarCollapsed = false;
  userID:string|null=null;

  constructor(private router: Router, private authService: AuthService) {
    this.router.events.subscribe(() => {
      // Update login status based on the current route
      this.isLoggedIn = !['/login', '/auth/callback'].includes(this.router.url);
    });
  }
  ngOnInit() {
    
    // Set initial login status based on session
    this.isLoggedIn = sessionStorage.getItem('isLoggedIn') === 'true';
    this.authService.getUserDetail().subscribe(userDetail => {
      this.email = userDetail.email;
      this.userName = userDetail.username;
      sessionStorage.setItem('isLoggedIn', 'true');
      this.userID= localStorage.getItem('ZUID');
    });
  }

  logOut() {
    localStorage.clear();
    sessionStorage.clear();
    // this.router.navigate(['/login']);
    this.router.navigate(['/login']).then(() => {
      window.history.replaceState({}, '', '/login');
    });
  }
  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
