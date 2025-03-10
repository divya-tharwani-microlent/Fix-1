import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({ name: 'cdate' })
export class CustomDatePipe implements PipeTransform {

    constructor(private datePipe: DatePipe) { }

    transform(date: string) {
        if (!date) return "";
        return this.datePipe.transform(date + 'Z', 'HH:mm, dd MMM yyyy');
    }
}

@Pipe({ name: 'codate' })
export class CustomOnlyDatePipe implements PipeTransform {

    constructor(private datePipe: DatePipe) { }

    transform(date: string) {
        if (!date) return "";
        return this.datePipe.transform(date, 'dd MMM yyyy');
    }
}