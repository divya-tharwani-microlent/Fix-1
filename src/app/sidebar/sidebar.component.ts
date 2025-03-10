import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonService } from '../services/common.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  isAdmin: boolean = false;
  constructor(private router: Router, private comonServ: CommonService) { }

  ngOnInit() {
    this.comonServ.getAdminList().subscribe((data: any) => {
      if (data.filter((x: any) => x.email == localStorage.getItem('email')).length > 0) {
        this.isAdmin = true;
      }
      else this.isAdmin = false;
    });
  }
  logout() {
    // Clear session data and redirect to login
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }

  isActive(route: string): boolean {
    return this.router.url.includes(route);
  }
}
