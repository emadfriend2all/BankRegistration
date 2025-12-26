import { Injectable, Inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Client, API_BASE_URL } from '../api/client';
import { CustMast, CustMastDto, CustMastDtoPaginatedResultDto, LoginDto, AccountMast, GetCustMastByIdDto, UpdateCustomerReviewDto } from '../api/client';

@Injectable({
  providedIn: 'root'
})
export class ApiWrapperService {
  private client: any;

  constructor(
    @Inject(API_BASE_URL) private baseUrl: string,
    private http: HttpClient
  ) {
    // Initialize client synchronously
    this.client = new Client(this.http, this.baseUrl);
  }

  getBaseUrl(): string {
    return this.baseUrl;
  }

  private ensureClient() {
    if (!this.client) {
      throw new Error('API client not initialized yet. Please try again in a moment.');
    }
    return this.client;
  }

  // Wrapper methods to fix Observable type mismatches
  accountMastAll(): Observable<AccountMast[]> {
    return this.ensureClient().accountMastAll();
  }

  accountMastPOST(body: AccountMast | undefined): Observable<AccountMast> {
    return this.ensureClient().accountMastPOST(body);
  }

  accountMastGET(branchCode: string, actType: string, custNo: number, currencyCode: string): Observable<AccountMast> {
    return this.ensureClient().accountMastGET(branchCode, actType, custNo, currencyCode);
  }

  accountMastPUT(branchCode: string, actType: string, custNo: number, currencyCode: string, body: AccountMast | undefined): Observable<AccountMast> {
    return this.ensureClient().accountMastPUT(branchCode, actType, custNo, currencyCode, body);
  }

  accountMastDELETE(branchCode: string, actType: string, custNo: number, currencyCode: string): Observable<boolean> {
    return this.ensureClient().accountMastDELETE(branchCode, actType, custNo, currencyCode);
  }

  customer(branchCode: string, custNo: number): Observable<AccountMast[]> {
    return this.ensureClient().customer(branchCode, custNo);
  }

  branch(branchCode: string): Observable<AccountMast[]> {
    return this.ensureClient().branch(branchCode);
  }

  custMastAll(): Observable<CustMast[]> {
    return this.ensureClient().custMastAll();
  }

  custMastPOST(body: any): Observable<CustMast> {
    return this.ensureClient().custMastPOST(body);
  }

  custMastGET(branchCode: string, custId: number): Observable<CustMast> {
    return this.ensureClient().custMastGET(branchCode, custId);
  }

  custMastPUT(branchCode: string, custId: number, body: CustMast | undefined): Observable<CustMast> {
    return this.ensureClient().custMastPUT(branchCode, custId, body);
  }

  custMastDELETE(branchCode: string, custId: number): Observable<boolean> {
    return this.ensureClient().custMastDELETE(branchCode, custId);
  }

  // New method for paginated customers
  custMastGETPaginated(
    pageNumber: number | undefined,
    pageSize: number | undefined,
    searchTerm: string | undefined,
    sortBy: string | undefined,
    sortDescending: boolean | undefined,
    status: string | undefined,
    reviewStatus: string | undefined
  ): Observable<CustMastDtoPaginatedResultDto> {
    return this.ensureClient().custMastGET(pageNumber, pageSize, searchTerm, sortBy, sortDescending, status, reviewStatus);
  }

  // New method for getting customer by ID
  custMastGET2(id: string): Observable<GetCustMastByIdDto> {
    return this.ensureClient().custMastGET2(id);
  }

  accounts(branchCode: string, custId: number): Observable<AccountMast[]> {
    return this.ensureClient().accounts(branchCode, custId);
  }

  // Authentication methods
  login(body: LoginDto | undefined): Observable<any> {
    return this.ensureClient().login(body);
  }

  register(body: any): Observable<any> {
    return this.ensureClient().register(body);
  }

  validateToken(): Observable<any> {
    return this.ensureClient().validateToken();
  }

  me(): Observable<any> {
    return this.ensureClient().me();
  }

  // Review method for customer review functionality
  review(body: UpdateCustomerReviewDto | undefined): Observable<boolean> {
    return this.ensureClient().review(body);
  }
}
