import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NotfiyService } from './notify.service';


@Injectable({
  providedIn: 'root'
})
export class HelperService {
  //excel file;
  public excelDataFile: any;
  emailPattern: string = "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,100}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z0-9]+[\\w-]+\\.)+[a-zA-Z]{2,9})$";
  phonePattern: string = "^(\\+\\d{1,2}\\s)?\\(?\\d{3}\\)?[\\s.-]?\\d{3}[\\s.-]?\\d{4}$";
  numberPattern: string = "^[0-9]*$";
  pricePattern: string = "^[0-9]+([.][0-9]{0,6})?$";
  namePattern: string = "^[A-Za-z ]+$";
  numberandSpeccharPattern: string = "^[0-9()\-]*$";

  constructor(private notyS: NotfiyService, private route: ActivatedRoute) {

  }

  GetErrorsFromFormGroup(formgroup: FormGroup, errorMapping: any) {
    var Errors: any[] = [];

    Object.keys(formgroup.controls).forEach(key => {
      const controlErrors: any = formgroup.get(key)?.errors;
      if (controlErrors != null) Object.keys(controlErrors).forEach(keyError => { Errors.push(errorMapping[key][keyError]); });
    });
    if (Errors.length > 0) this.notyS.showWarning("Warning", Errors[0]);

    return Errors;
  }
}