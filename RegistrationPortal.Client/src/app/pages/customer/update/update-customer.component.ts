import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { CardModule } from 'primeng/card';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { FluidModule } from 'primeng/fluid';
import { TextareaModule } from 'primeng/textarea';
import { StepsModule } from 'primeng/steps';
import { RadioButtonModule } from 'primeng/radiobutton';
import { CheckboxModule } from 'primeng/checkbox';
import { FileUploadModule } from 'primeng/fileupload';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { ImageModule } from 'primeng/image';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Client, CustMast, AccountMast, CreateAccountDto } from '../../../api/client';
import { CreateCustomerDto } from '../../../models/create-customer.dto';
import { ApiWrapperService } from '../../../services/api-wrapper.service';
import { FormOptionsService } from '../../../shared/form-options.service';
import { IdMaskDirective } from '../../../shared/id-mask.directive';

@Component({
  selector: 'app-create-customer',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    DatePickerModule,
    SelectModule,
    ToastModule,
    CardModule,
    InputGroupModule,
    InputGroupAddonModule,
    FluidModule,
    TextareaModule,
    StepsModule,
    RadioButtonModule,
    CheckboxModule,
    FileUploadModule,
    DividerModule,
    DialogModule,
    ImageModule,
    IdMaskDirective
  ],
  providers: [MessageService],
  templateUrl: './update-customer.component.html',
  styleUrls: ['./update-customer.component.css']
})
export class UpdateCustomerComponent implements OnInit {
  customerForm: FormGroup;
  accountForm: FormGroup;
  loading = false;
  soleSudanResidencyChecked = false;
  fullName = '';
  englishFullName = '';
  activeIndex: number = Number(0);
  isTesting = true; // Set this to true for testing
  
  // File upload properties
  uploadedFiles: any[] = [];
  totalSize: number = 0;
  totalSizePercent: number = 0;
  
  // File persistence properties
  private persistedFiles: { [key: string]: File } = {};
  private filePreviews: { [key: string]: string } = {};
  
  // Example image dialog properties
  displayExampleDialog: boolean = false;
  currentExampleImage: string = '';
  currentExampleTitle: string = '';

  // Custom validator for minimum age (18 years)
  private minimumAgeValidator(minAge: number) {
    return (control: any) => {
      if (!control.value) {
        return null; // Don't validate empty values - let required validator handle this
      }
      
      const birthDate = new Date(control.value);
      const today = new Date();
      
      // Calculate exact age in days
      const minAgeInDays = minAge * 365.25; // Account for leap years
      const ageInMilliseconds = today.getTime() - birthDate.getTime();
      const ageInDays = ageInMilliseconds / (1000 * 60 * 60 * 24);
      
      // Calculate years and days for error message
      const years = Math.floor(ageInDays / 365.25);
      const remainingDays = Math.floor(ageInDays % 365.25);
      
      return ageInDays >= minAgeInDays ? null : { 
        minimumAge: { 
          requiredAge: minAge, 
          actualAge: years,
          actualDays: Math.floor(ageInDays),
          remainingDays: remainingDays
        } 
      };
    };
  }
  
  items: MenuItem[] = [
    { label: 'معلومات الحساب' },
    { label: 'معلومات العميل' },
    { label: 'تأكيد الطلب' }
  ];

  // Use shared service for form options
  get genderOptions() { return this.formOptionsService.genderOptions; }
  get religionOptions() { return this.formOptionsService.religionOptions; }
  get educationOptions() { return this.formOptionsService.educationOptions; }
  get maritalStatusOptions() { return this.formOptionsService.maritalStatusOptions; }
  get nationalityOptions() { return this.formOptionsService.nationalityOptions; }
  get idTypeOptions() { return this.formOptionsService.idTypeOptions; }
  get accountTypeOptions() { return this.formOptionsService.accountTypeOptions; }
  get currencyOptions() { return this.formOptionsService.currencyOptions; }
  get countriesOfBirth() { return this.formOptionsService.countriesOfBirth; }
  get countryCodeOptions() { return this.formOptionsService.countryCodeOptions; }
  get branches() { return this.formOptionsService.branches; }



  
  

    
  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    private apiWrapper: ApiWrapperService,
    private formOptionsService: FormOptionsService,
    private http: HttpClient
  ) {
    this.customerForm = this.fb.group({
      branchCCode: [''],
      custCName: [{ value: '', disabled: true }],
      custCFname: ['', Validators.required],
      custCSname: ['', Validators.required],
      custCTname: ['', Validators.required],
      custCFoname: ['', Validators.required],
      custCMname: ['', Validators.required],
      custDBdate: [null, [Validators.required, this.minimumAgeValidator(18)]],
      custCSex: ['', Validators.required],
      custCReligion: ['', Validators.required],
      custCCaste: [''],
      custCMaritalsts: ['', Validators.required],
      custCPadd1: ['', [Validators.required]],
      custCPCity: ['', Validators.required],
      mobileCNo: ['', [Validators.required, Validators.pattern('^[1-9][0-9]{8}$')]],
      mobileCountryCode: ['+249'],
      emailCAdd: [''],
      idCType: ['', Validators.required],
      idCNo: ['', Validators.required],
      idDIssdate: ['', Validators.required],
      idCIssplace: ['', Validators.required],
      idDExpdate: [null, Validators.required],
      custCAuthority: [''],
      husbCName: [''],
      countryCCode: [''],
      placeCBirth: ['', Validators.required],
      custCNationality: ['', Validators.required],
      custCWife1: [''],
      idCType2: ['NATIONAL_ID'],
      idCNo2: ['', [Validators.required, Validators.pattern('^[0-9]{11}$')]], // Required and exactly 11 digits
      custCOccupation: ['', Validators.required],
      homeINumber: [null],
      homeCountryCode: ['+249'],
      custIIdentify: [''],
      custCCountrybrith: ['', Validators.required],
      custCStatebrith: ['', Validators.required],
      custCCitizenship: [''],
      custCEmployer: ['', Validators.required],
      custFIncome: [null],
      custCHigheducation: ['', Validators.required],
      custFAvgmonth: [null],
      tradeCNameenglish: [''],
      custCEngfname: ['', Validators.required],
      custCEngsname: ['', Validators.required],
      custCEngtname: ['', Validators.required],
      custCEngfoname: ['', Validators.required],
      
      // Document upload fields
      identification: [null, Validators.required],
      nationalId: [null, Validators.required],
      personalImage: [null, Validators.required],
      imageFortheRequesterHoldingTheID: [null, Validators.required],
      signitureTemplate: [null, Validators.required],
      handwrittenRequest: [null, Validators.required],
      employmentCertificate: [null] // Optional - not required
    });

    this.accountForm = this.fb.group({
      branchCCode: ['', Validators.required],
      actCType: ['', Validators.required],
      cust_i_no: ['', Validators.required],
      currencyCCode: ['SDG', Validators.required],
      actCName: [''],
      actCOcccode: [''],
      actDOpdate: [new Date(), Validators.required],
      actCOrgn: [''],
      actCActsts: ['ACTIVE'],
      actCOdsts: [''],
      actCChqfac: [''],
      actCIntrotype: ['', Validators.required],
      actCOpmode: [''],
      actIIntroid: [null],
      actITrbrcode: [null],
      actDClosdt: [null],
      actCAbbsts: [''],
      withdrawCFlag: [''],
      introCRem: [''],
      actCAtm: [''],
      actCInternet: [''],
      actCTelebnk: [''],
      groupCCode: ['']
    });
  }

  // Getter for gender-based field visibility
  get isMale(): boolean {
    return this.customerForm.get('custCSex')?.value === 'M';
  }

  get isFemale(): boolean {
    return this.customerForm.get('custCSex')?.value === 'F';
  }

  get isMarried(): boolean {
    return this.customerForm.get('custCMaritalsts')?.value === 'MARRIED';
  }

  get shouldShowWifeName(): boolean {
    return this.isMale && this.isMarried;
  }

  get shouldShowHusbandName(): boolean {
    return this.isFemale && this.isMarried;
  }

  get isAccountStep(): boolean {
    return this.activeIndex === 0;
  }
  
  get isCustomerStep(): boolean {
    return this.activeIndex === 1;
  }
  
  get stepLabel(): string {
    if (this.activeIndex === 1) {
      return 'حفظ';
    }
    return this.activeIndex === 2 ? 'إنهاء' : 'التالي';
  }

  // Getter for age validation error
  get isAgeInvalid(): boolean {
    const birthDateControl = this.customerForm.get('custDBdate');
    return birthDateControl?.hasError('minimumAge') || false;
  }

  get ageErrorMessage(): string {
    const birthDateControl = this.customerForm.get('custDBdate');
    if (birthDateControl?.hasError('minimumAge')) {
      return 'يجب أن يكون العمر 18 سنة على الأقل';
    }
    return '';
  }

  ngOnInit(): void {
    this.activeIndex = 0;
    
    // Initialize forms with test data if testing is enabled
    if (this.isTesting) {
      this.initializeTestData();
    }
    
    // Initialize spouse field validators
    this.updateSpouseFieldValidators();
    
    // Listen to gender changes to clear spouse fields
    this.customerForm.get('custCSex')?.valueChanges.subscribe(() => {
      this.clearSpouseFields();
    });
    
    // Listen to marital status changes to clear spouse fields
    this.customerForm.get('custCMaritalsts')?.valueChanges.subscribe(() => {
      this.clearSpouseFields();
    });
  }

  private initializeTestData(): void {
    console.log('Initializing form with test data...');
    
    // Customer form test data
    this.customerForm.patchValue({
      branchCCode: '058',
      custCFname: 'عماد',
      custCSname: 'محمد',
      custCTname: 'علي',
      custCFoname: 'عبدالله',
      custCMname: 'ام الكرام الأمين الطاهر يوسف',
      custDBdate: new Date('2006-11-30'),
      custCSex: 'M',
      custCReligion: 'MUSLIM',
      custCCaste: '',
      custCMaritalsts: 'MARRIED',
      custCPadd1: 'sdfgsdfgsdfg',
      custCPCity: 'المدينة المنورة',
      mobileCNo: '532704983',
      emailCAdd: 'emadfriend2all@targetpointgroup.com',
      idCType: 'PASSPORT',
      idCNo: '3425345',
      idDIssdate: new Date('2025-12-27'),
      idCIssplace: '23452352345',
      idDExpdate: new Date('2025-12-30'),
      custCAuthority: '',
      husbCName: '',
      countryCCode: '',
      placeCBirth: 'قرية الكمر الجعليين',
      custCNationality: 'Sudanese',
      custCWife1: 'أسرار بابكر البشير علي',
      idCType2: 'NATIONAL_ID',
      idCNo2: '54545646555',
      custCOccupation: 'موظف',
      homeCountryCode: '+249',
      custIIdentify: '',
      custCCountrybrith: 'Sudan',
      custCStatebrith: 'الجزيرة',
      custCCitizenship: '',
      custCEmployer: 'بنك الإدخار',
      custCHigheducation: 'UNIVERSITY',
      custCEngfname: 'Emad',
      custCEngsname: 'Mohamed',
      custCEngtname: 'Ali',
      custCEngfoname: 'Abdalla'
    });

    // Account form test data
    this.accountForm.patchValue({
      branchCCode: '058',
      actCType: '20101',
      currencyCCode: '001',
      actCName: '',
      actCOcccode: '',
      actDOpdate: new Date(),
      actCOrgn: '',
      actCActsts: 'ACTIVE',
      actCOdsts: '',
      actCChqfac: '',
      actCIntrotype: 'INDIVIDUAL',
      actCOpmode: '',
      actCAbbsts: '',
      withdrawCFlag: '',
      introCRem: '',
      actCAtm: '',
      actCInternet: '',
      actCTelebnk: '',
      groupCCode: ''
    });

    // Update full names
    this.updateFullName();
    this.updateEnglishFullName();
    
    console.log('Test data initialized successfully');
  }

  clearSpouseFields(): void {
    // Clear husband name when gender or marital status changes
    this.customerForm.get('husbCName')?.setValue('');
    // Clear wife name when gender or marital status changes
    this.customerForm.get('custCWife1')?.setValue('');
    // Update validators
    this.updateSpouseFieldValidators();
  }

  updateSpouseFieldValidators(): void {
    const husbandNameControl = this.customerForm.get('husbCName');
    const wifeNameControl = this.customerForm.get('custCWife1');
    
    // Clear existing validators
    husbandNameControl?.clearValidators();
    wifeNameControl?.clearValidators();
    
    // Add required validators based on gender and marital status
    if (this.shouldShowHusbandName) {
      husbandNameControl?.setValidators([Validators.required]);
    }
    
    if (this.shouldShowWifeName) {
      wifeNameControl?.setValidators([Validators.required]);
    }
    
    // Update validation status
    husbandNameControl?.updateValueAndValidity();
    wifeNameControl?.updateValueAndValidity();
  }

  updateFullName(): void {
    const fname = this.customerForm.get('custCFname')?.value || '';
    const sname = this.customerForm.get('custCSname')?.value || '';
    const tname = this.customerForm.get('custCTname')?.value || '';
    const foname = this.customerForm.get('custCFoname')?.value || '';
    
    // Combine all names with spaces, filtering out empty values
    let fullName = [fname, sname, tname, foname]
      .filter(name => name && name.trim() !== '')
      .join(' ');
    
    // Ensure no double spaces in the final full name
    fullName = fullName.replace(/\s+/g, ' ').trim();
    
    // Update the display property and form field
    this.fullName = fullName;
    this.customerForm.get('custCName')?.setValue(fullName);
  }

  updateEnglishFullName(): void {
    const fname = this.customerForm.get('custCEngfname')?.value || '';
    const sname = this.customerForm.get('custCEngsname')?.value || '';
    const tname = this.customerForm.get('custCEngtname')?.value || '';
    const foname = this.customerForm.get('custCEngfoname')?.value || '';
    
    // Combine all names with spaces, filtering out empty values
    let englishFullName = [fname, sname, tname, foname]
      .filter(name => name && name.trim() !== '')
      .join(' ');
    
    // Ensure no double spaces in the final full name
    englishFullName = englishFullName.replace(/\s+/g, ' ').trim();
    
    // Update the display property and form field
    this.englishFullName = englishFullName;
    this.customerForm.get('tradeCNameenglish')?.setValue(englishFullName);
  }

  cleanNameField(fieldName: string): void {
    const field = this.customerForm.get(fieldName);
    if (field && field.value) {
      // Replace multiple spaces with single space in real-time
      const cleanedValue = field.value.replace(/\s+/g, ' ');
      field.setValue(cleanedValue);
      this.updateFullName();
    }
  }

  cleanEnglishNameField(fieldName: string): void {
    const field = this.customerForm.get(fieldName);
    if (field && field.value) {
      // Replace multiple spaces with single space in real-time
      const cleanedValue = field.value.replace(/\s+/g, ' ');
      field.setValue(cleanedValue);
      this.updateEnglishFullName();
    }
  }

  trimNameField(fieldName: string): void {
    const field = this.customerForm.get(fieldName);
    if (field && field.value) {
      // Trim spaces and replace multiple spaces with single space
      const cleanedValue = field.value.trim().replace(/\s+/g, ' ');
      field.setValue(cleanedValue);
      this.updateFullName();
    }
  }

  trimEnglishNameField(fieldName: string): void {
    const field = this.customerForm.get(fieldName);
    if (field && field.value) {
      // Trim spaces and replace multiple spaces with single space
      const cleanedValue = field.value.trim().replace(/\s+/g, ' ');
      field.setValue(cleanedValue);
      this.updateEnglishFullName();
    }
  }

  saveCustomerAndAccount(): void {
    if (this.customerForm.invalid || this.accountForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تحقق من البيانات',
        detail: 'يرجى ملء جميع الحقول المطلوبة والموافقة على صحة البيانات.',
        life: 3000
      });
      return;
    }

    this.loading = true;

    // Convert account form data to CreateAccountDto
    const accountData = new CreateAccountDto({
      ...this.accountForm.value,
      actDOpdate: this.accountForm.value.actDOpdate || new Date()
    });

    // Convert CustMast to CreateCustomerDto for API call with accountMasts
    // Format dates properly for server
    const formatDateForServer = (date: any) => {
      if (!date) return undefined;
      const dateObj = new Date(date);
      // Return ISO string format to match the working actDOpdate format
      return dateObj.toISOString();
    };

    // Debug: Check form values before creating DTO
    console.log('Form values:', this.customerForm.value);
    console.log('Date fields - custDBdate:', this.customerForm.value.custDBdate);
    console.log('Date fields - idDIssdate:', this.customerForm.value.idDIssdate);
    console.log('Date fields - idDExpdate:', this.customerForm.value.idDExpdate);

    const customerData: CreateCustomerDto = {
      ...this.customerForm.value,
      custCName: this.fullName,
      branchCCode: this.customerForm.value.branchCCode || '', // Ensure branchCCode is not undefined
      entryCDate: new Date(),
      tradeCNameenglish: this.englishFullName,
      status: 'Update', // Set status to "Update" for customer update requests
      // Combine country codes with phone numbers (remove + sign)
      mobileCNo: this.customerForm.value.mobileCountryCode.replace('+', '') + this.customerForm.value.mobileCNo,
      homeINumber: this.customerForm.value.homeCountryCode ? 
        this.customerForm.value.homeCountryCode.replace('+', '') + this.customerForm.value.homeINumber : 
        this.customerForm.value.homeINumber,
      // Map form field names to DTO field names
      custDBdate: formatDateForServer(this.customerForm.value.custDBdate),
      idDIssdate: formatDateForServer(this.customerForm.value.idDIssdate),
      idDExpdate: formatDateForServer(this.customerForm.value.idDExpdate),
      idCIssueDate2: formatDateForServer(this.customerForm.value.idCIssueDate2),
      idCExpiryDate2: formatDateForServer(this.customerForm.value.idCExpiryDate2),
      custDEntrydt: formatDateForServer(this.customerForm.value.custDEntrydt),
      // Include accountMasts
      accountMasts: [accountData],
      // Document files
      identification: this.customerForm.value.identification,
      nationalId: this.customerForm.value.nationalId,
      personalImage: this.customerForm.value.personalImage,
      imageFortheRequesterHoldingTheID: this.customerForm.value.imageFortheRequesterHoldingTheID,
      signitureTemplate: this.customerForm.value.signitureTemplate,
      handwrittenRequest: this.customerForm.value.handwrittenRequest,
      employmentCertificate: this.customerForm.value.employmentCertificate
    };

    console.log('DTO idDIssdate:', (customerData as any).idDIssdate);
    console.log('DTO idDExpdate:', (customerData as any).idDExpdate);

    // Create customer with account in single request
    this.createCustomerWithDocuments(customerData, accountData).subscribe({
      next: (customer: CustMast) => {
        // Move to confirmation step
        this.activeIndex = 2;
        this.loading = false;
      },
      error: (customerError: any) => {
        // Restore persisted files to form before showing error
        this.restorePersistedFiles();
        
        // Parse the new JSON error format from server
        let errorMessage = 'فشل في إنشاء العميل. يرجى المحاولة مرة أخرى.';
        let errorSummary = 'خطأ';
        
        // Handle the new JSON error format: { error: "...", message: "...", statusCode: ... }
        if (customerError?.error) {
          // This is our new JSON error format
          if (typeof customerError.error === 'object') {
            // Parsed JSON error
            errorSummary = customerError.error.error || 'خطأ';
            errorMessage = customerError.error.message || errorMessage;
            
            // Handle specific error types
            if (customerError.error.statusCode === 400) {
              errorSummary = 'خطأ في الطلب';
            } else if (customerError.error.statusCode === 500) {
              errorSummary = 'خطأ في الخادم';
            }
          } else {
            // String error message (fallback)
            errorMessage = customerError.error;
          }
        } else if (customerError?.status === 400 && customerError?.errors) {
          // Handle validation errors (old format, keep for compatibility)
          const validationErrors = customerError.errors;
          const errorMessages: string[] = [];
          
          // Extract validation error messages
          Object.keys(validationErrors).forEach(field => {
            const fieldErrors = validationErrors[field];
            if (Array.isArray(fieldErrors)) {
              fieldErrors.forEach(error => {
                // Format field name to be more user-friendly
                const fieldName = field.replace('AccountMasts[0].', '');
                errorMessages.push(`${fieldName}: ${error}`);
              });
            }
          });
          
          errorSummary = 'خطأ في التحقق';
          errorMessage = errorMessages.join('\n');
        } else if (customerError?.message) {
          // Handle direct error messages
          errorMessage = customerError.message;
        } else if (customerError?.response) {
          // Handle response-based errors
          if (typeof customerError.response === 'object') {
            errorMessage = customerError.response.message || customerError.response.error || errorMessage;
          } else {
            errorMessage = customerError.response;
          }
        }
        
        // Show error to user
        this.messageService.add({
          severity: 'error',
          summary: errorSummary,
          detail: errorMessage,
          life: errorSummary === 'خطأ في التحقق' ? 10000 : 5000 // Longer display for validation errors
        });
        
        console.error('Error creating customer:', customerError);
        this.loading = false;
      },
      complete: () => {
        // Loading is handled in both next and error callbacks
      }
    });
  }

  private mapClientToServerProperty(clientKey: string): string {
    // Map camelCase client properties to PascalCase server properties
    const propertyMap: { [key: string]: string } = {
      'identification': 'Identification',
      'nationalId': 'NationalId', 
      'personalImage': 'PersonalImage',
      'imageFortheRequesterHoldingTheID': 'ImageFortheRequesterHoldingTheID',
      'signitureTemplate': 'SignitureTemplate',
      'handwrittenRequest': 'HandwrittenRequest',
      'employmentCertificate': 'EmploymentCertificate'
    };
    
    return propertyMap[clientKey] || clientKey;
  }

  private createCustomerWithDocuments(customerData: CreateCustomerDto, accountData: CreateAccountDto): Observable<CustMast> {
    // Create FormData object
    const formData = new FormData();
    
    // Debug: Log the customerData to see what's being sent
    console.log('Customer data being sent:', customerData);
    console.log('Account data being sent:', accountData);
    
    // Add all customer data fields to FormData
    Object.keys(customerData).forEach(key => {
      const value = (customerData as any)[key];
      if (value !== null && value !== undefined) {
        if (value instanceof File) {
          // Handle file uploads - map camelCase to PascalCase for server
          const serverKey = this.mapClientToServerProperty(key);
          console.log(`Adding file: ${serverKey}`, value.name);
          formData.append(serverKey, value);
        } else if (key === 'accountMasts' && Array.isArray(value)) {
          // Handle account array - send each account as separate form fields
          value.forEach((account: any, index: number) => {
            console.log(`Adding account[${index}]:`, account);
            Object.keys(account).forEach(accountKey => {
              const accountValue = account[accountKey];
              if (accountValue !== null && accountValue !== undefined) {
                const formKey = `AccountMasts[${index}].${accountKey}`;
                // Handle dates in account fields specially
                if (accountValue instanceof Date) {
                  console.log(`Adding account date field: ${formKey}`, accountValue.toISOString());
                  formData.append(formKey, accountValue.toISOString());
                } else {
                  console.log(`Adding account field: ${formKey}`, accountValue);
                  formData.append(formKey, accountValue.toString());
                }
              }
            });
          });
        } else if (value instanceof Date) {
          // Handle dates
          console.log(`Adding date: ${key}`, value.toISOString());
          formData.append(key, value.toISOString());
        } else {
          // Handle regular fields
          console.log(`Adding field: ${key}`, value);
          formData.append(key, value.toString());
        }
      }
    });
    
    // Debug: Log FormData contents
    console.log('FormData contents:');
    for (let pair of formData.entries()) {
      console.log(pair[0], pair[1]);
    }
    
    // Make direct HTTP request using HttpClient
    return this.http.post<CustMast>(`${this.apiWrapper.getBaseUrl()}/api/CustMast`, formData);
  }

  // File upload methods
  onSelectedFiles(event: any): void {
    this.totalSize = 0;
    this.totalSizePercent = 0;
    
    if (event.files) {
      for (let file of event.files) {
        this.totalSize += file.size;
      }
      
      if (this.totalSize > 1000000) { // 1MB limit
        this.messageService.add({
          severity: 'warn',
          summary: 'حجم الملفات كبير',
          detail: 'يجب أن يكون حجم الملفات الإجمالي أقل من 1 ميجابايت',
          life: 3000
        });
        return;
      }
      
      this.totalSizePercent = Math.round((this.totalSize / 1000000) * 100);
    }
  }

  onTemplatedUpload(): void {
    // This will be handled by the form submission
    console.log('Files ready for upload');
  }

  onRemoveTemplatingFile(event: any, file: any, removeFileCallback: Function, index: number): void {
    removeFileCallback(index);
    this.totalSize -= file.size;
    this.totalSizePercent = Math.round((this.totalSize / 1000000) * 100);
  }

  formatSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  uploadEvent(callback: any): void {
    // This will be handled by the form submission
    callback();
  }

  onFileChange(event: any, documentType: string): void {
    // Handle both p-fileupload and basic file input events
    let file: File | null = null;
    
    if (event.files && event.files.length > 0) {
      // p-fileupload component
      file = event.files[0];
    } else if (event.target && event.target.files && event.target.files.length > 0) {
      // Basic file input
      file = event.target.files[0];
    }
    
    if (file) {
      // Store file in persistence
      this.persistedFiles[documentType] = file;
      
      // Create preview URL for images
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.filePreviews[documentType] = e.target.result;
        };
        reader.readAsDataURL(file);
      }
      
      // Update the customerForm with the file
      this.customerForm.patchValue({
        [documentType]: file
      });
      
      // Optional: Show file selection feedback
      console.log(`Selected ${documentType} file:`, file.name);
    }
  }
  
  // Method to restore persisted files to form
  private restorePersistedFiles(): void {
    Object.keys(this.persistedFiles).forEach(documentType => {
      const file = this.persistedFiles[documentType];
      if (file) {
        this.customerForm.patchValue({
          [documentType]: file
        });
      }
    });
  }
  
  // Method to check if a file is persisted
  hasPersistedFile(documentType: string): boolean {
    return !!this.persistedFiles[documentType];
  }
  
  // Method to get persisted file name
  getPersistedFileName(documentType: string): string {
    return this.persistedFiles[documentType]?.name || '';
  }
  
  // Method to get file preview URL
  getFilePreview(documentType: string): string {
    return this.filePreviews[documentType] || '';
  }
  
  // Method to clear persisted file
  clearPersistedFile(documentType: string): void {
    delete this.persistedFiles[documentType];
    delete this.filePreviews[documentType];
    this.customerForm.patchValue({
      [documentType]: null
    });
  }

  resetForms(): void {
    this.customerForm.reset();
    this.accountForm.reset();
    this.fullName = '';
    this.englishFullName = '';
    this.activeIndex = 0;
    
    // Clear persisted files
    this.persistedFiles = {};
    this.filePreviews = {};
  }

  onHomeNumberKeyPress(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which || event.keyCode);
    if (!/[0-9]/.test(char)) {
      event.preventDefault();
    }
  }

  onHomeNumberInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;
    
    // Remove any non-digit characters
    value = value.replace(/\D/g, '');
    
    // Limit to 15 digits
    if (value.length > 15) {
      value = value.substring(0, 15);
    }
    
    // Update the input value
    input.value = value;
    
    // Update the form control
    this.customerForm.get('homeINumber')?.setValue(value, { emitEvent: false });
  }

  onBranchChange(event: any): void {
    const branchValue = event.value;
    // Sync branch value to customer form
    this.customerForm.get('branchCCode')?.setValue(branchValue);
  }

  nextStep(): void {
    if (this.activeIndex === 0) {
      // Validate account form before proceeding
      if (this.accountForm.invalid) {
        this.accountForm.markAllAsTouched();
        
        // Get specific missing fields
        const missingFields: string[] = [];
        const controls = this.accountForm.controls;
        
        // Check each required field and add Arabic field names if missing
        if (controls['branchCCode']?.invalid) missingFields.push('فرع الحساب');
        if (controls['actCType']?.invalid) missingFields.push('نوع الحساب');
        if (controls['cust_i_no']?.invalid) missingFields.push('رقم الحساب');
        if (controls['currencyCCode']?.invalid) missingFields.push('عملة الحساب');
        if (controls['actCName']?.invalid) missingFields.push('اسم الحساب');
        if (controls['actDOpdate']?.invalid) missingFields.push('تاريخ فتح الحساب');
        if (controls['actCIntrotype']?.invalid) missingFields.push('نوع المقدم');
        
        const fieldNames = missingFields.join(', ');
        this.messageService.add({
          severity: 'warn',
          summary: 'تحقق من بيانات الحساب',
          detail: `يرجى ملء جميع حقول الحساب المطلوبة. الحقول المفقودة: ${fieldNames}`,
          life: 5000
        });
        return;
      }
      this.activeIndex = 1; // Go to customer step
    } else if (this.activeIndex === 1) {
      // This is the save step - save all customer and account data
      this.saveCustomerAndAccount();
    } else if (this.activeIndex === 2) {
      // This is the confirmation step - already saved
      this.activeIndex = 2; // Stay on confirmation
    }
  }

  previousStep(): void {
    if (this.activeIndex > 0) {
      this.activeIndex--;
    }
  }

  resetForm(): void {
    this.customerForm.reset();
    this.fullName = '';
    this.englishFullName = '';
  }

  onPhoneNumberKeyPress(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which || event.keyCode);
    if (!/[0-9]/.test(char)) {
      event.preventDefault();
    }
  }

  onPhonMobileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove all non-digits
    
    if (value.startsWith('0')) {
      value = '' + value.slice(1);
    }
    
    // Limit to 9 digits
    if (value.length > 9) {
      value = value.slice(0, 9);
    }
    
    input.value = value;
    
    // Update the form control
    this.customerForm.get('mobileCNo')?.setValue(value, { emitEvent: false });
  }

  // Example image methods
  showExampleImage(documentType: string): void {
    const exampleImages: { [key: string]: { path: string, title: string } } = {
      'identification': {
        path: '/global/passportTemplate.jpg',
        title: 'صورة الهوية - مثال'
      },
      'imageFortheRequesterHoldingTheID': {
        path: '/global/personHoldingIdTemplate.jpg',
        title: 'صورة طالب الخدمة بالهوية - مثال'
      },
      'signitureTemplate': {
        path: '/global/signaturesTemplate.jpg',
        title: 'نموذج التوقيع - مثال'
      },
      'handwrittenRequest': {
        path: '/global/handwritten-letters.png',
        title: 'الطلب المكتوب بخط اليد - مثال'
      },
      'nationalId': {
        path: '/global/nationalId.jpg',
        title: 'الرقم الوطني - مثال'
      },
      'passportImage': {
        path: '/global/passport-personal-image.jpg',
        title: 'الصورة الشخصية - مثال'
      },
      'employmentLetter': {
        path: '/global/employmentLetter.png',
        title: 'شهادة عمل - مثال'
      }
    };

    const example = exampleImages[documentType];
    if (example) {
      this.currentExampleImage = example.path;
      this.currentExampleTitle = example.title;
      this.displayExampleDialog = true;
    } else {
      // For documents without examples, show a message
      this.messageService.add({
        severity: 'info',
        summary: 'لا يوجد مثال',
        detail: 'لا يوجد مثال متاح لهذا المستند',
        life: 3000
      });
    }
  }

  closeExampleDialog(): void {
    this.displayExampleDialog = false;
    this.currentExampleImage = '';
    this.currentExampleTitle = '';
  }
}
