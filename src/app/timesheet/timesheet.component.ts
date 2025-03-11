import { Component, OnInit } from '@angular/core';
import { ZohoService } from '../services/zoho.service';
import { AuthService } from '../services/auth.service';
import { CommonService } from '../services/common.service';
import { TaskService } from '../services/task.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { HelperService } from '../services/helper.service';
import { NotfiyService } from '../services/notify.service';
import { ApproverService } from '../services/approver.service';
import { Router } from '@angular/router';
enum TimesheetStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

@Component({
  selector: 'app-timesheet',
  templateUrl: './timesheet.component.html',
  styleUrls: ['./timesheet.component.css']
})
export class TimesheetComponent implements OnInit {
  timesheets: any[] = [];
  newEntry = { date: '', hours: 0, taskId: '' };
  userId: any;
  from_date: any;// = new Date().toISOString();
  to_date: any;
  maxDate: string = "";
  minDate: string = "";
  filteredTimesheets: any[] = [];
  searchText: string = '';
  pageSize: number = 10;
  currentPage: number = 1;
  totalRecords: number = 0;
  sortAscending: boolean = false;
  categories: any[] = [];
  projects: any[] = [];
  tasks: any[] = [];

  TaskCreateForm: FormGroup | any;
  isLoading = false;// for loader icon
  approver: any;
  laggingTime: number = 0;
  AddlogFlag: number = 0;
  timesheetStatuses: any[] = [];
  isRejected: boolean = false;

  validationMapping: any =
    {
      "ProjectId": { required: "Project is required." },
      "ProjectTaskId": { required: "Task is required." },
      "FromDate": { required: "From Date is required." },
      "FromTime": { required: "From Time is required." },
      "ToDate": { required: "To Date is required." },
      "ToTime": { required: "To Time is required." },
      "Notes": { required: "Notes required.", pattern: "Notes not valid" }
    };
  isModalOpen: boolean = false;
  isDateselected: boolean = false;
  SaveSubmitFlag: number = 0;

  constructor(private router: Router, private approverService: ApproverService, private commonService: CommonService, private taskService: TaskService, private hs: HelperService, private notyS: NotfiyService, private authService: AuthService) { }

  ngOnInit(): void {
    debugger
    this.userId = "60030099161";//this.authService.getUserId();//"60030099161";//"60010572588";
    this.commonService.getProjectList().subscribe((data: any) => {
      if (data != null) {
        this.projects = data;
      }
    });

    this.approverService.getapproverofEmp().subscribe((data: any) => {
      if (data != null) {
        this.approver = data;
      }
    });
    this.TaskCreateForm = new FormGroup({
      ID: new FormControl('0'),
      ProjectId: new FormControl('', Validators.required),
      ProjectTaskId: new FormControl('', Validators.required),
      FromDate: new FormControl({ value: '', disabled: true }, Validators.required),
      FromTime: new FormControl('', [Validators.required]),
      ToDate: new FormControl('', [Validators.required,]),
      ToTime: new FormControl('', [Validators.required]),
      Notes: new FormControl('', [Validators.required,Validators.pattern("^[a-zA-Z0-9$& ]+$"), Validators.maxLength(500)])
      ,
    });
    this.timesheetStatuses = [
      { value: TimesheetStatus.Pending, label: 'Pending' },
      { value: TimesheetStatus.Approved, label: 'Approved' },
      { value: TimesheetStatus.Rejected, label: 'Rejected' }
    ];

    const today = new Date();
    this.maxDate = today.toISOString().split('T')[0]; // Set max date to today

    const pastDate = new Date();
    pastDate.setFullYear(today.getFullYear() - 1); // Set min date to 1 year ago (or adjust as needed)
    this.minDate = pastDate.toISOString().split('T')[0];
  }

  getZohotimeSheet() {
    this.loadTasksWithZoho();
  }

  loadTasksWithZoho() {
    this.isLoading = true;
    this.taskService.getTasksWithZoho(this.userId, this.from_date).subscribe(
      (response) => {
        if (response && Array.isArray(response)) {
          if (response.filter((x: any) => x.taskStatus == TimesheetStatus.Rejected).length > 0) {
            this.isLoading = false;
            this.isRejected = true;
            this.AddlogFlag = 1;
          }
          else {
            this.timesheets = this.calculateTotalMinutesforNewrecords(response);
            this.AddlogFlag = 1;

            this.toggleSort(true);
            this.filteredTimesheets = this.timesheets;
            this.totalRecords = response.length;

            this.applyPagination();
            const totaltime = this.timesheets.reduce((sum, item) => sum + (item.totalMinutes || 0), 0);
            if (totaltime > 480) {
              this.laggingTime = 0;
              this.SaveSubmitFlag = 1;
            }
            else this.laggingTime = 480 - totaltime;
            this.isLoading = false;

            this.taskService.checktimesheetExists(this.from_date).subscribe((data: any) => {
              if (data.length > 0) {
                if (data[0].status == TimesheetStatus.Rejected) {
                  this.SaveSubmitFlag = 1;
                  this.AddlogFlag = 1;
                }
                else { this.SaveSubmitFlag = 0; this.AddlogFlag = 0; }
              }
              else {
                this.laggingTime == 0 ? this.SaveSubmitFlag = 1 : this.SaveSubmitFlag = 0;
                this.AddlogFlag = 1;
              }
            });
          }
        }
        else {
          this.notyS.showError("Error", 'Response is empty!');
          this.isLoading = false;
          localStorage.clear();
          this.router.navigate(['/login']);
        }
      },
      (error) => {
        console.error('Error fetching tasks:', error); // Handle any errors
        this.isLoading = false;
      }
    );
  }
  toggleSort(dir: any): void {
    if (dir != true) {
      this.sortAscending = !this.sortAscending;
    }
    else {
      this.sortAscending = true;
    }
    this.timesheets = this.timesheets.sort((a, b) => {
      return this.sortAscending
        ? new Date(a.fromDate).getTime() - new Date(b.fromDate).getTime()
        : new Date(b.fromDate).getTime() - new Date(a.fromDate).getTime();
    });
    this.filteredTimesheets = this.timesheets;
    this.applyPagination();
  }

  calculateTotalMinutesforNewrecords(timehseets: any) {
    timehseets.forEach((entry: any) => {
      if (entry.id !== null) {
        const fromDate = new Date(entry.fromDate);
        const toDate = new Date(entry.toDate);

        // Calculate total minutes difference
        const diffMinutes = Math.round((toDate.getTime() - fromDate.getTime()) / (1000 * 60));

        // Update totalMinutes property
        entry.totalMinutes = diffMinutes;
      }
    });
    return timehseets;
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.applyPagination();
  }

  applyPagination(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.filteredTimesheets = this.timesheets.slice(startIndex, endIndex);
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  getPages(): number[] {
    const totalPages = this.getTotalPages();
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  getTotalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  openModal() {
    this.TaskCreateForm.get('FromDate')?.enable();
    this.isModalOpen = true;
    this.TaskCreateForm.get('FromDate')?.disable();
  }

  closeModal() {
    this.isModalOpen = false;
    this.TaskCreateForm.reset({
      ProjectId: null,
      ProjectTaskId: null,
      FromDate: this.from_date,
      FromTime: '',
      ToDate: this.to_date,
      ToTime: '',
      Notes: ''
    });
  
    //change Modal dates 
    const date = new Date(this.from_date);
    // date.setDate(date.getDate() + 1);
    this.to_date = date.toISOString().split('T')[0];

    this.TaskCreateForm.get('FromDate')?.enable();  // Enable it temporarily
    this.TaskCreateForm.patchValue({ "FromDate": this.from_date, "ToDate": this.to_date });

    this.TaskCreateForm.get('FromDate')?.disable(); // Re-disable it
  }

  loadTasks() {
    this.taskService.getTasks().subscribe((data) => (this.timesheets = data));
  }
  compareTask(task1: any, task2: any): boolean {
    return task1 && task2 ? task1.id === task2.id : task1 === task2;
  }
  
  onProjectSelect(event: any) {
    debugger
    this.TaskCreateForm.patchValue({ ProjectTaskId: null });
    this.commonService.getTaskList(event.target.value).subscribe((data: any) => {
      if (data != null) {
        this.tasks = data;
        
        console.log(this.tasks);
      }
    });
  }

  addTask() {
    if (this.TaskCreateForm.valid) {
      this.TaskCreateForm.get('FromDate')?.enable();

      if (this.TaskCreateForm.value.FromDate && this.TaskCreateForm.value.FromTime) {
        // Combine Date and Time into a single Date object
        const dateTime = new Date(this.TaskCreateForm.value.FromDate);
        const timeParts = this.TaskCreateForm.value.FromTime.split(':');
        dateTime.setHours(Number(timeParts[0]), Number(timeParts[1]));

        const TodateTime = new Date(this.TaskCreateForm.value.ToDate);
        const TotimeParts = this.TaskCreateForm.value.ToTime.split(':');
        TodateTime.setHours(Number(TotimeParts[0]), Number(TotimeParts[1]));

        // Ensure FromDate + FromTime is before ToDate + ToTime
        if (dateTime >= TodateTime) {
          this.notyS.showWarning("Warning", "Start time must be before end time.");
          this.TaskCreateForm.get('FromDate')?.disable();
          return; // Stop execution
        }

        const isOverlapping = this.timesheets.some(task => {
          if (!task.fromDate || !task.toDate) return false; // Skip invalid entries

          const existingFrom = new Date(task.fromDate);
          const existingTo = new Date(task.toDate);

          // Ensure both tasks are on the same date for correct overlap detection
          if (dateTime.toDateString() === existingFrom.toDateString() ||
            TodateTime.toDateString() === existingTo.toDateString()) {
            return (dateTime < existingTo && TodateTime > existingFrom);
          }

          return false; // No overlap if different dates
        });

        if (isOverlapping) {
          this.notyS.showWarning("Warning", "This time slot is already occupied. Please choose another time.");
          this.TaskCreateForm.get('FromDate')?.disable();
          return; // Stop execution
        }
        // Assign combined DateTime to FormDate control
        this.TaskCreateForm.patchValue({
          FromDate: this.formatLocalDateTime(dateTime),
          ToDate: this.formatLocalDateTime(TodateTime),
          ID: 0
        });
      }

      this.taskService.addTask(this.TaskCreateForm.value, this.userId).subscribe(() => {
        this.loadTasksWithZoho();
      });
      this.TaskCreateForm.get('FromDate')?.disable(); // Re-disable it
    }
    else {
      this.hs.GetErrorsFromFormGroup(this.TaskCreateForm, this.validationMapping);
    }
    this.closeModal();
    this.TaskCreateForm.reset({
      ProjectId: null,
      ProjectTaskId: null,
      FromDate: this.from_date,
      FromTime: '',
      ToDate: this.to_date,
      ToTime: '',
      Notes: ''
    });
  }

  formatLocalDateTime(date: string | Date): string {
    if (!date) return '';

    const d = new Date(date);
    return `${d.getFullYear()}-${this.pad(d.getMonth() + 1)}-${this.pad(d.getDate())}T${this.pad(d.getHours())}:${this.pad(d.getMinutes())}:00.000`;
  }

  pad(value: number): string {
    return value < 10 ? '0' + value : value.toString();
  }

  onDateChange(event: any) {
    this.from_date = event.target.value;

    //change Modal dates 
    const date = new Date(this.from_date);
    date.setDate(date.getDate() + 1);
    this.to_date = date.toISOString().split('T')[0];

    this.TaskCreateForm.get('FromDate')?.enable();  // Enable it temporarily
    this.TaskCreateForm.patchValue({ "FromDate": this.from_date, "ToDate": this.from_date });
    this.TaskCreateForm.get('FromDate')?.disable(); // Re-disable it
    this.timesheets = [];
    this.filteredTimesheets = [];
    this.AddlogFlag = 0;
    this.SaveSubmitFlag = 0;
  }

  submitTimesheet() {
    if (this.approver == null || this.approver == "") {
      this.notyS.showError("Error", "No approver exists.");
      return;
    }

    const timesheetData = this.filteredTimesheets;
    this.SaveSubmitFlag = 1;
    this.AddlogFlag = 1;

    const updatedData = timesheetData.map(item => ({
      userZuid: localStorage.getItem('ZUID'),
      approverzuid: this.approver.appZuid,
      approveremail: this.approver.approverEmail,
      fromDate: item.fromDate,
      toDate: item.toDate,
      totalMinutes: item.totalMinutes,
      taskName: item.task,
      subTaskName: item.subTask,
      notes: item.notes,
      project: item.project,
      billStatus: item.billStatus,
      internalTaskId: item.id,
      sourceId: item.id !== null ? 1 : 2
    }));

    this.taskService.submitTimesheet(updatedData).subscribe(() => {
      this.notyS.showSuccess("Success", "Timesheet submitted successfully.");
      this.loadTasksWithZoho();
    }, error => {
      this.notyS.showError("Error", "Failed to submit timesheet.");
    });
  }

}
