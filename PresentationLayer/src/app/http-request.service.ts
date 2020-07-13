import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class HttpRequestService {

    constructor(
        private _httpService: HttpClient,) { }

    get(url: string) {
        return this._httpService.get(url);
    }

    async getAsync(url: string): Promise<JsonParsed>{
        return this._httpService.get<JsonParsed>(url).toPromise();
    }

    async getWithParamstAsync(url: string, param: string): Promise<JsonParsed> {
        let params = new HttpParams().set('storeid', param);
        return this._httpService.get<JsonParsed>(url, { params: params }).toPromise();
    }

    async getWithParamsAsync(url: string, parameters: Map<string, string | null>): Promise<JsonParsed> {

        let paramsToSend = new HttpParams();

        parameters.forEach((value: string | null, key: string) => {
            paramsToSend = paramsToSend.set(key, value);
        });
        return this._httpService.get<JsonParsed>(url, { params: paramsToSend }).toPromise();
    }

    getWithParams(url: string, param: string) {
        let params = new HttpParams().set('storeid', param);
        return this._httpService.get(url, { params: params });
    }

    post(url: string, body: Object) {
        return this._httpService.post(url, body);
    }

    postAsync(url: string, body: Object): Promise<JsonParsed> {
        return this._httpService.post<JsonParsed>(url, body).toPromise();
    }
}

export interface JsonParsed {
    Success: boolean;
    AnswerOrExceptionJson: string;
}
