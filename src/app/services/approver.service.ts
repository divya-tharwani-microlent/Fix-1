import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class ApproverService {
  apiURL: string = environment.baseUrl;

  constructor(private http: HttpClient) { }

  getUsersList(){
    return this.http.get(`${this.apiURL}Approver/fetch-zohoproject-users?authToken=`+localStorage.getItem('accessToken'));
  }
  
  saveApprover(data:any){
    return this.http.post<any>(`${this.apiURL}Approver/save-approver`, data).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 409) {
          return throwError(() => new Error(JSON.stringify(error.error)));
        }
        return throwError(() => new Error('An error occurred while saving the approver.'));
      })
    );
  }

  updateApprover(approver: any,ID:any) {
    return this.http.put<any>(`${this.apiURL}Approver/update-approver?ID=${ID}`, approver).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => new Error(error.error.message || 'Failed to update approver.'));
      })
    );
  }
  
  fetchApproverList(){
    return this.http.get(`${this.apiURL}Approver/approver-list`);
  }

  deleteApprover(id: number) {
    return this.http.post<any>(`${this.apiURL}Approver/delete-approver/${id}`, {}).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => new Error(error.error.message || 'Failed to delete approver.'));
      })
    );
  }

  deactiveApprover(id: number,isactive:boolean) {
    return this.http.post<any>(`${this.apiURL}Approver/deactive-approver/${id}?isactive=${isactive}`, {}).pipe(
    // return this.http.post<any>(`${this.apiURL}Approver/delete-approver/${id}`, {}).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => new Error(error.error.message || 'Failed to delete approver.'));
      })
    );
  }
 
  getapproverofEmp(){
    // return this.http.get(`${this.apiURL}Approver/get-approver?empId=60030099161`);//60018552382`);

     return this.http.get(`${this.apiURL}Approver/get-approver?empId=`+localStorage.getItem('ZUID'));
  }

  updateTaskLogStatus(data:any,reason:any){
    return this.http.post<any>(`${this.apiURL}Approver/update-status?reason=`+reason, data).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 409) {
          return throwError(() => new Error(JSON.stringify(error.error)));
        }
        return throwError(() => new Error('An error occurred while saving the approver.'));
      })
    );
  }  

}