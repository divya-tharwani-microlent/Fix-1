import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TimesheetComponent } from './timesheet/timesheet.component';
import { LoginComponent } from './login/login.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';
import { NotfiyService } from './services/notify.service';
import { HelperService } from './services/helper.service';
import { ToastrModule } from 'ngx-toastr';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ApproverDetailsComponent } from './approver-details/approver-details.component';
import { ApproveTimesheetComponent } from './approve-timesheet/approve-timesheet.component';

@NgModule({
  declarations: [
    AppComponent,
    TimesheetComponent,
    LoginComponent,
    AuthCallbackComponent,
    SidebarComponent,
    ApproverDetailsComponent,
    ApproveTimesheetComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    ToastrModule,
    ToastrModule.forRoot(),
    BrowserAnimationsModule
  ],
  providers: [{ provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true}
    ,NotfiyService,HelperService],
  bootstrap: [AppComponent]
})
export class AppModule { }
