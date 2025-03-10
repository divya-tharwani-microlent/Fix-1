import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class NotfiyService {
  constructor(private toastr: ToastrService) { }

  showSuccess(title: string, message: string, timeout: number = 3000) {
    this.toastr.success(message, title, { timeOut: timeout });
  }

  showError(title: string, message: string, timeout: number = 3000) {
    this.toastr.error(message, title, { timeOut: timeout });
  }

  showWarning(title: string, message: string, timeout: number = 3000) {
    this.toastr.warning(message, title, { timeOut: timeout });
  }

  showInfo(title: string, message: string, timeout: number = 3000) {
    this.toastr.info(message, title, { timeOut: timeout });
  }

  showInfoWithCallback(title: string, message: string, timeout: number = 3000, onTap: any) {
    this.toastr.info(message, "", { timeOut: timeout, positionClass: "toast-top-right", closeButton: true, enableHtml: true }).onTap.subscribe(() => {
      onTap();
    });
  }

  showConfim(title: string, text: string, yesCallback: Function, noCallback?: Function) {
    var yes = 'Yes, delete it';
    var no = 'No, keep it';
    title = title ? title : 'Are you sure?';
    text = text ? text : 'You will not be able to recover.';
    Swal.fire({
      title: title,
      text: text,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: yes,
      cancelButtonText: no
    }).then((result) => {
      if (result.value)
        yesCallback()
      else if (noCallback)
        noCallback()
    })
  }

  showConfimCancel(title: string, text: any, yesCallback: Function, noCallback?: Function) {
    var yes = 'Yes, Cancel it';
    var no = 'No, keep it';
    title = title ? title : 'Are you sure?';
    text = text ? text : 'You will not be able to recover.';
    let htmlText = `Payable No: ${text.PayableNo}<br>Posting Date: ${text.Date}<br>Total Due Payable: ${text.TotalduePayable}`;
    Swal.fire({
      title: title,
      text: text,
      html:htmlText,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: yes,
      cancelButtonText: no
    }).then((result) => {
      if (result.value)
        yesCallback()
      else if (noCallback)
        noCallback()
    })
  }

  showConfimApprover(title: string, text: any, yesCallback: Function, noCallback?: Function) {
    var yes = 'Yes';
    var no = 'No';
    title = title ? title : 'Are you sure?';
    Swal.fire({
      title: title,
      text: text,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: yes,
      cancelButtonText: no
    }).then((result) => {
      if (result.value)
        yesCallback()
      else if (noCallback)
        noCallback()
    })
  }
}
