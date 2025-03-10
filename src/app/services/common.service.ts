import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class CommonService {
  apiURL: string = environment.baseUrl;

  constructor(private http: HttpClient) { }

  getCategoryList() {
    return this.http.get(`${this.apiURL}Common/category-list`);
  }

  getProjectList() {
    return this.http.get(`${this.apiURL}Common/project-list`);
  }

  getTaskList(ProjectId:any) {
    return this.http.get(`${this.apiURL}Common/projTask-list?ProjectId=`+ProjectId);
  }
  
  getAdminList() {
    return this.http.get(`${this.apiURL}Common/admin-list`);
  }

}