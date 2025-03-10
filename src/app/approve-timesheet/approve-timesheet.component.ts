import { Component } from '@angular/core';
import { TaskService } from '../services/task.service';
import { ApproverService } from '../services/approver.service';
import { NotfiyService } from '../services/notify.service';
import { Router } from '@angular/router';

enum TimesheetStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

@Component({
  selector: 'app-approve-timesheet',
  templateUrl: './approve-timesheet.component.html',
  styleUrls: ['./approve-timesheet.component.css']
})
export class ApproveTimesheetComponent {
  timesheet: any[] = [];
  isLoading = false;// for loader icon
  pageSize: number = 10;
  currentPage: number = 1;
  totalRecords: number = 0;
  timesheetStatuses: any[] = [];
  groupbyoption: any = "Date";
  groupedTimesheet: any[] = [];
  expandedGroups: { [key: string]: boolean } = {};

  userFilter: string = '';
  fromDateFilter: string = '';
  users: any[] = [];
  isModalOpen: boolean = false;
  rejectionReason: string = '';
  selectedGroup: any = null;

  constructor(private taskService: TaskService, private approverService: ApproverService, private notyS: NotfiyService, private router: Router) {
    this.timesheetStatuses = [
      { value: TimesheetStatus.Pending, label: 'Pending' },
      { value: TimesheetStatus.Approved, label: 'Approved' },
      { value: TimesheetStatus.Rejected, label: 'Rejected' }
    ];
  }

  ngOnInit(): void {
    this.taskService.getPendingTimesheets(localStorage.getItem('ZUID'), this.groupbyoption).subscribe((data: any) => {
      if (data != null) {
        this.timesheet = data.flat();
        this.groupTimesheet(this.groupbyoption);
      }
    });

    this.fetchAllUsers();
  }

  fetchAllUsers() {
    this.approverService.getUsersList().subscribe({
      next: (data: any) => {
        if (data != null) {
          this.users = data;
        }
      },
      error: () => {
        // Handle error and redirect to login page
        localStorage.clear();
        this.router.navigate(['/login']);
        this.isLoading = false;
      },
    });
  }

  groupKeyLabel(groupKey: string, groupBy: 'Date' | 'userZuid'): string {
    return groupBy === 'Date' ? new Date(groupKey).toDateString() : `User: ${groupKey}`;
  }

  groupTimesheet(groupBy: 'Date' | 'userZuid') {
    const grouped = this.timesheet.reduce((acc, entry) => {
      const groupKey = groupBy === 'Date'
        ? new Date(entry.fromDate).toDateString()
        : entry.userZuid;

      if (!acc[groupKey]) {
        acc[groupKey] = [];
      }
      acc[groupKey].push(entry);
      return acc;
    }, {} as { [key: string]: any[] });

    this.groupedTimesheet = Object.keys(grouped).map(groupKey => ({
      groupKey,
      status: grouped[groupKey][0].status,
      entries: grouped[groupKey]
    }));

    this.groupedTimesheet.forEach(group => {
      this.expandedGroups[group.groupKey] = true; // Expand all by default
    });
  }

  toggleGroup(fromDate: string) {
    this.expandedGroups[fromDate] = !this.expandedGroups[fromDate];
  }

  openModal() {
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
    this.rejectionReason = '';

    this.groupedTimesheet.forEach(group => {
      if (group.status == TimesheetStatus.Rejected) {
        group.status = TimesheetStatus.Pending;
      }
    });
  }

  changeGroupStatus(group: any) {
    if (group.status == TimesheetStatus.Rejected && !this.rejectionReason) {
      this.selectedGroup = group; // Store the selected group
      this.openModal();
      return;
    }

    if (this.selectedGroup) {
      this.selectedGroup.entries.forEach((entry: any) => {
        entry.status = TimesheetStatus.Rejected;
        entry.rejectionReason = this.rejectionReason;
      });
      this.userFilter = '';
      this.fromDateFilter = '';
      this.updateTimesheetStatus(this.selectedGroup);
      this.closeModal();
    }
    else {
      // Apply selected status to all entries within the group
      group.entries.forEach((entry: any) => entry.status = group.status);
      this.updateTimesheetStatus(group);
    }
  }

  updateTimesheetStatus(group: any) {
    this.approverService.updateTaskLogStatus(group.entries, this.rejectionReason).subscribe({
      next: (data: any) => {
        this.notyS.showSuccess('Success', data.message);
        this.taskService.getPendingTimesheets(localStorage.getItem('ZUID'), this.groupbyoption).subscribe((data: any) => {
          if (data != null) {
            this.timesheet = data.flat();
            this.totalRecords = this.timesheet.length;
            this.groupTimesheet(this.groupbyoption);
          }
        });
      },
      error: (err) => {
        console.error('Error updating timesheet:', err);
        this.notyS.showError('Failed to update timesheet', 'Error');
      }
    });
  }

  groupBy() {
    const approverId = localStorage.getItem('ZUID');

    this.taskService.getPendingTimesheets(approverId, "Date", this.userFilter, this.fromDateFilter).subscribe({
      next: (data: any) => {
        if (data && data.length > 0) {
          this.timesheet = data.flat(); // Flatten in case of nested groups
          this.totalRecords = this.timesheet.length;
          this.groupbyoption = "Date";
          this.groupTimesheet(this.groupbyoption);
        } else {
          console.warn('No data returned from API');
          this.timesheet = [];
          this.totalRecords = 0;
        }
      },
      error: (err) => {
        console.error('Error fetching timesheets:', err);
      }
    });
  }

  resetFilter() {
    this.userFilter = '';
    this.fromDateFilter = '';
    this.taskService.getPendingTimesheets(localStorage.getItem('ZUID'), this.groupbyoption).subscribe((data: any) => {
      if (data != null) {
        this.timesheet = data.flat();
        this.totalRecords = this.timesheet.length;
        this.groupTimesheet(this.groupbyoption);
      }
    });
  }
}
