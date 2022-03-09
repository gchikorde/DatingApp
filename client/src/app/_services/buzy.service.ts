import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BuzyService {
  buzyRequestCount=0;

  constructor(private spinnerService :NgxSpinnerService) { }

  buzy(){
    this.buzyRequestCount++;
    this.spinnerService.show(undefined,{
      type:'line-scale-party',
      bdColor:'rgba(255,255,255,0)',
      color:'#333333',
    });
  }

  idle(){
    this.buzyRequestCount--;
    if(this.buzyRequestCount <=0){
      this.buzyRequestCount=0;
      this.spinnerService.hide();
    }
  }
}
