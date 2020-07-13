import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { AddOrRemovePermissionBodyRequest, MyStore } from '../contracts.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-manage-permissions',
  templateUrl: './manage-permissions.component.html',
  styleUrls: ['./manage-permissions.component.css']
})
export class ManagePermissionsComponent implements OnInit {
    managePermissionsFailed: boolean;
    MessageToUser: string;
    storeId: string;
    StoreName: string;
    form = new FormGroup({
        username: new FormControl('', Validators.required),
        formEditPermission: new FormControl('', Validators.required),
        addOrRemove: new FormControl('', Validators.required)
    });
    allPermissions: string[] = [
        "inventory", "appoint_owner", "appoint_manager", "edit_permissions", "remove_manager", "history", "remove_owner", "policy", "close_store", "requests"
    ];

    constructor(
        private router: ActivatedRoute,
        private routerAppoint: Router,
        private httpService: HttpRequestService,
    ) {
        this.managePermissionsFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.storeId = this.router.snapshot.paramMap.get('storeid');
        this.StoreName = this.router.snapshot.paramMap.get('storeName');
    }

    get f() {
        return this.form.controls;
    }

    editPermission() {
        let addOrRemovePermissionRequest: AddOrRemovePermissionBodyRequest =
        {
            StoreId: this.storeId,
            Username: this.form.value.username,
            Permission: this.form.value.formEditPermission
        }
        if (this.form.value.addOrRemove == "true") {
            this.httpService.post('api/store/AddPermission', addOrRemovePermissionRequest).subscribe((data: JsonParsed) => {
                if (data.Success) {
                    this.routerAppoint.navigateByUrl('/home/' + this.storeId);
                } else {
                    this.managePermissionsFailed = true;
                    let response = JSON.parse(data.AnswerOrExceptionJson);
                    if (response) {
                        this.MessageToUser = response.Message;
                    }
                }
            });
        }
        else {
            this.httpService.post('api/store/RemovePermission', addOrRemovePermissionRequest).subscribe((data: JsonParsed) => {
                if (data.Success) {
                    this.routerAppoint.navigateByUrl('/home/' + this.storeId);
                } else {
                    this.managePermissionsFailed = true;
                    let response = JSON.parse(data.AnswerOrExceptionJson);
                    if (response) {
                        this.MessageToUser = response.Message;
                    }
                }
            });
        }
    }

}
