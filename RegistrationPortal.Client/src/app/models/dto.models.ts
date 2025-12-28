export interface PaginatedResultDto<T> {
    data: T[];
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface UserListDto {
    id: number;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    createdAt: Date;
    roles: string[];
}

export interface RoleListDto {
    id: number;
    name: string;
    description: string;
    isActive: boolean;
    createdAt: Date;
    permissions: string[];
}

export interface PaginationParameters {
    pageNumber: number;
    pageSize: number;
    searchTerm?: string;
    sortBy?: string;
    sortDescending?: boolean;
    status?: string;
}
