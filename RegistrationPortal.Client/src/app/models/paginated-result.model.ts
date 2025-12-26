export interface PaginatedResultDto<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
