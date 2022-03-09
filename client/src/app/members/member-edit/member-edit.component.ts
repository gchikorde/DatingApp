import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm:NgForm;

  member:Member; 
  user:User;

  @HostListener('window:beforeunload', ['$event']) unloadNotification($event:any){
    if($event.editForm.dirty){
      $event.returnValue = true;
    }
  }

  constructor(private memberService : MembersService, private accountService: AccountService, private toastrService:ToastrService) {
    accountService.currentUser$.pipe(take(1)).subscribe(u => this.user=u);
   }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember(){
    this.memberService.getMember(this.user.username).subscribe(m=>this.member=m);
  }

  updateMember(){
    this.memberService.updateMember(this.member).subscribe(()=>{
      this.toastrService.success('Profile updated successfully.');
      this.editForm.reset(this.member);
    })
   
  }
}
