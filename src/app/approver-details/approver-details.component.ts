import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApproverService } from '../services/approver.service';
import { NotfiyService } from '../services/notify.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-approver-details',
  templateUrl: './approver-details.component.html',
  styleUrls: ['./approver-details.component.css']
})
export class ApproverDetailsComponent {
  approverForm: FormGroup;
  approvers: any[] = [];
  selectedIndex: number | null = null;
  isLoading = false;// for loader icon
  projects: any[] = [];
  users: any[] = [];
  isEditmode: boolean = false;


  constructor(private fb: FormBuilder, private approverService: ApproverService, private notyS: NotfiyService, private router: Router) {
    this.approverForm = this.fb.group({
      empName: [{ value: '' }, Validators.required],
      empEmail: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      empZuid: [{ value: '', disabled: true }, [Validators.required]],
      approverEmail: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      appName: [{ value: '' }, Validators.required],
      appZuid: [{ value: '', disabled: true }, [Validators.required]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.fetchAllUsers()
    this.fetchAllApprovers();
    this.isLoading = false;
  }

  fetchAllApprovers() {
    this.approverService.fetchApproverList().subscribe((data: any) => {
      if (data != null) {
        this.approvers = data;
      }
    });
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

  onEmployeeSelect(event: any) {
    var filteruser = this.users.find(f => f.name == event.target.value);
    this.approverForm.patchValue({ "empEmail": filteruser.email, "empName": filteruser.name, "empZuid": filteruser.userId });
  }

  onAppSelect() {
    var filteruser = this.users.find(f => f.name == this.approverForm.get('appName')?.value);
    this.approverForm.patchValue({ "approverEmail": filteruser.email, "appName": filteruser.name, "appZuid": filteruser.userId });
  }

  onSubmit() {
    this.approverForm.get('empEmail')?.enable(); //Enable it temporarily
    this.approverForm.get('empZuid')?.enable();
    this.approverForm.get('appZuid')?.enable();
    this.approverForm.get('approverEmail')?.enable();

    if (this.approverForm.invalid) {
      return;
    }
    if (this.approverForm.value.empEmail == this.approverForm.value.approverEmail) {
      this.notyS.showError("Error", "Employee and Approver email should not be same.");
      return;
    }

    this.approverService.saveApprover(this.approverForm.value).subscribe({
      next: (response) => {
        this.notyS.showSuccess("Success", response.message);
        this.fetchAllApprovers();
      },
      error: (error) => {
        const errorData = JSON.parse(error.message);
        if (errorData.message.includes("Do you want to update?")) {
          this.notyS.showConfimApprover(
            "Confirmation",
            errorData.message,
            () => {
              this.updateApprover(errorData.id);
            },
            () => {
              this.notyS.showInfo("", "Approver update canceled.");
            }
          );
        } else {
          this.notyS.showError("", errorData.message);
        }
      }
    });

    this.approverForm.get('empEmail')?.disable(); //Disable it temporarily
    this.approverForm.get('empZuid')?.disable();
    this.approverForm.get('appZuid')?.disable();
    this.approverForm.get('approverEmail')?.disable();
  }

  updateApprover(id: any) {
    if (!id) {
      this.notyS.showError("Error", "Approver ID is missing.");
      return;
    }

    this.approverForm.get('empEmail')?.enable(); //Enable it temporarily
    this.approverForm.get('empZuid')?.enable();
    this.approverForm.get('appZuid')?.enable();
    this.approverForm.get('approverEmail')?.enable();

    this.approverService.updateApprover(this.approverForm.value, id).subscribe({
      next: (response) => {
        this.notyS.showSuccess("Success", response.message);
        this.fetchAllApprovers();
      },
      error: (error) => {
        this.notyS.showError("", "Failed to update approver.");
      }
    });

    this.approverForm.get('empEmail')?.disable(); //Disable it temporarily
    this.approverForm.get('empZuid')?.disable();
    this.approverForm.get('appZuid')?.disable();
    this.approverForm.get('approverEmail')?.disable();
  }

  editApprover(index: number) {
    this.selectedIndex = index;
    this.approverForm.setValue(this.approvers[index]);
  }

  deleteApprover(id: number) {
    this.notyS.showConfimApprover(
      "Confirmation",
      "Are you sure you want to delete this approver?",
      () => {
        this.approverService.deleteApprover(id).subscribe({
          next: (response) => {
            this.notyS.showSuccess("Success", response.message);
            this.fetchAllApprovers(); // Refresh the list
          },
          error: (error) => {
            this.notyS.showError("", error.message);
          }
        });
      },
      () => {
        this.notyS.showInfo("", "Approver deletion canceled.");
      }
    );
  }

  onActivechange(id: number) {
    this.notyS.showConfimApprover(
      "Confirmation",
      "Are you sure you want to de-activate this approver?",
      () => {
        this.approverService.deactiveApprover(id, this.approverForm.get('isActive')?.value).subscribe({
          next: (response) => {
            this.notyS.showSuccess("Success", response.message);
            this.fetchAllApprovers(); // Refresh the list
          },
          error: (error) => {
            this.notyS.showError("", error.message);
            this.fetchAllApprovers();
          }
        });
      },
      () => {
        this.notyS.showInfo("", "Operation canceled.");
        this.fetchAllApprovers();
      }
    );
  }
}
