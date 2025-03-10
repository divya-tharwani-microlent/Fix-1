import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface Task {
  ID: number;
  TaskCategory: string;
  FromDate: string;
  ToDate: string;
  Notes: string;
}

export interface TaskDetailModel {
  id: number;
  taskCategory: string;
  fromDate: string;
  toDate: string;
  notes: string;
  isDeleted: boolean;
  Project: string;
  Task: string;
  SubTask: string;
  TotalMinutes: number;
  BillStatus: string;
  Approver: string;
}

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  apiURL: string = environment.baseUrl;

  constructor(private http: HttpClient) { }

  // Fetch all tasks
  getTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(this.apiURL + 'Task');
  }

  // Fetch task by ID
  getTask(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiURL}/${id}`);
  }

  // Fetch tasks + Zoho tasks
  getTasksWithZoho(userId: any, start_date: any): Observable<any> {
    return this.http.get<any>(`${this.apiURL}Task/fetch-with-zoho?userId=` + userId + `&start_date=` + start_date);
  }

  // Create a new task
  addTask(taskmodel: any, userZuid: any): Observable<Task> {
    return this.http.post<Task>(this.apiURL + `Task?userZuid=${userZuid}`, taskmodel);
  }

  // Soft delete task
  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiURL}/${id}`);
  }

  submitTimesheet(timesheetList: any[]) {
    const payload = { timesheetList }; // Wrap in an object
    return this.http.post(`${this.apiURL}Task/submit`, payload);
  }

  getPendingTimesheets(approverId: any, groupBy?: string, filterByUser?: string, filterByDate?: string) {
    let params: any = { ApproverId: approverId }; //"60030099161"
  
    if (groupBy) params.groupBy = groupBy;
    if (filterByUser) params.filterbyUser = filterByUser;
    if (filterByDate) params.filterbyDate = filterByDate;
  
    return this.http.get(`${this.apiURL}Task/approver`, { params });
  }
  

  updateTimesheetStatus(timesheetId: number, status: string) {
    return this.http.post(`${this.apiURL}Task/update-status`, { timesheetId, status });
  }

  checktimesheetExists(date: any) {
    return this.http.get(`${this.apiURL}Task/timesheet-isexists?searchDate=` + date+`&userid=`+localStorage.getItem('ZUID'));
  }
}
