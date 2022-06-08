import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];
  createForm:any;
  httpClient;
  baseurl;
  selectedFile!: File;
  image:any;
  fileToUpload: any;
  

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string,private formBuilder: FormBuilder) {
    this.httpClient = http;
    this.baseurl = "http://localhost:5320/"
    
    // this.selectedFile=File;
    
    http.get<WeatherForecast[]>(baseUrl + 'azure').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
  }

  ngOnInit(): void {
    this.createForm = this.formBuilder.group({
      firstname: ['', Validators.required],
      lastname: ['', Validators.required],
      profilepic: ['', Validators.required],
      profilePicUrl: ['', Validators.required]
    });
  }
  handleFileInput(e: any) {
    this.fileToUpload = e?.target?.files[0];
  }
  onSelectFile(fileInput: any) {
    debugger;
    let base64String = null;
    this.selectedFile = <File>fileInput.target.files[0];
    var reader = new FileReader();
    reader.onload = event=> { 
      this.image = event.target?.result; 
      console.log(this.image);
    }
  reader.readAsDataURL(this.selectedFile);  
  }

  onSubmit(form:any) {
    debugger;
    this.createForm.setValue({profilepic:"",firstname:this.createForm.getRawValue().firstname,lastname:this.createForm.getRawValue().lastname,profilePicUrl:""});
    // this.createForm.getRawValue().profilepic = this.image;
    const formData = new FormData();
  //  var headers={
  //     headers: new HttpHeaders({
  //         'Content-Type': 'application/json'
  //     })
  // }
    formData.append('profilepic', this.createForm.getRawValue().profilepic);
    formData.append('firstname', this.createForm.getRawValue().firstname);
    formData.append('lastname', this.createForm.getRawValue().lastname);
    formData.append('profilePicUrl', this.createForm.getRawValue().profilePicUrl);
    formData.append('myFile', this.fileToUpload);
    console.log(this.createForm.getRawValue());
    this.httpClient.post<User>(this.baseurl + 'azure/create', formData,{
      headers : new HttpHeaders()}).subscribe(data => {
      // this.postId = data.id;
      console.log("called")
  })
    
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
export interface User {
  firstname: string;
  lastname: string;
  profilepic:string;
  profilePicUrl:string; 
}