import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiWrapperService } from './api-wrapper.service';
import { CustMast, AccountMast, CustMastDtoPaginatedResultDto, CustMastDto, GetCustMastByIdDto, UpdateCustomerReviewDto } from '../api/client';

export interface PaginationParameters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  status?: string;
  reviewStatus?: string;
}

export interface PaginatedResultDto<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  constructor(private apiWrapper: ApiWrapperService) {}

  getAllCustomers(params: PaginationParameters): Observable<PaginatedResultDto<CustMastDto>> {
    return new Observable(observer => {
      this.apiWrapper.custMastGETPaginated(
        params.pageNumber,
        params.pageSize,
        params.searchTerm,
        params.sortBy,
        params.sortDescending,
        params.status,
        params.reviewStatus
      ).subscribe({
        next: (response: CustMastDtoPaginatedResultDto) => {
          const result: PaginatedResultDto<CustMastDto> = {
            data: response.data || [],
            currentPage: response.currentPage || params.pageNumber || 1,
            pageSize: response.pageSize || params.pageSize || 10,
            totalCount: response.totalCount || 0,
            totalPages: response.totalPages || 0
          };
          observer.next(result);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  getCustomer(branchCode: string, custId: number): Observable<CustMast> {
    return this.apiWrapper.custMastGET(branchCode, custId);
  }

  getCustomerById(customerId: string): Observable<CustMastDto | null> {
    // Use the search approach for now, but in a real implementation
    // this should be a dedicated endpoint
    return new Observable(observer => {
      this.getAllCustomers({
        pageNumber: 1,
        pageSize: 1,
        searchTerm: customerId
      }).subscribe({
        next: (response) => {
          if (response.data && response.data.length > 0) {
            observer.next(response.data[0]);
          } else {
            observer.next(null);
          }
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  getCustomerByIdFromApi(customerId: string): Observable<GetCustMastByIdDto> {
    return this.apiWrapper.custMastGET2(customerId);
  }

  getCustomerAccounts(branchCode: string, custId: number): Observable<AccountMast[]> {
    return this.apiWrapper.accounts(branchCode, custId);
  }

  createCustomer(customer: any): Observable<CustMast> {
    return this.apiWrapper.custMastPOST(customer);
  }

  updateCustomer(branchCode: string, custId: number, customer: CustMast): Observable<CustMast> {
    return this.apiWrapper.custMastPUT(branchCode, custId, customer);
  }

  deleteCustomer(branchCode: string, custId: number): Observable<boolean> {
    return this.apiWrapper.custMastDELETE(branchCode, custId);
  }

  reviewCustomer(reviewData: UpdateCustomerReviewDto): Observable<boolean> {
    return this.apiWrapper.review(reviewData);
  }
}
