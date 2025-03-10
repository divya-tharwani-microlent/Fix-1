import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimesheetComponent } from './timesheet/timesheet.component';
import { LoginComponent } from './login/login.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { ApproverDetailsComponent } from './approver-details/approver-details.component';
import { ApproveTimesheetComponent } from './approve-timesheet/approve-timesheet.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'auth/callback', component: AuthCallbackComponent },
  { path: 'timesheet', component: TimesheetComponent },
  { path: 'addApprover', component: ApproverDetailsComponent },
  { path: 'approveTimesheet', component: ApproveTimesheetComponent },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
