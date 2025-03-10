import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class ZohoService {
  private baseUrl = 'https://projectsapi.zoho.com/restapi';
  apiURL: string = environment.baseUrl;
  private accessToken = localStorage.getItem('accessToken');

  constructor(private http: HttpClient) { }

  getPortals() {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${this.accessToken}`);
    return this.http.get(`${this.apiURL}zoho/portals`, { headers });
  }

  getUserTimesheetLogs(userId: any, start_date: any, end_date: any) {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${this.accessToken}`);
    return this.http.get(`${this.apiURL}zoho/timesheet-logs?userId=` + userId + `&start_date=` + start_date + `&end_date=` + end_date, { headers });
  }

  addTimesheetEntry(data: any) {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${this.accessToken}`);
    return this.http.post(`${this.apiURL}zoho/timesheets`, data, { headers });
  }

  approveOrRejectTimesheet(timesheetId: string, status: 'approve' | 'reject') {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${this.accessToken}`);
    return this.http.post(`${this.apiURL}zoho/timesheets/${timesheetId}/${status}`, null, { headers });
  }
}
