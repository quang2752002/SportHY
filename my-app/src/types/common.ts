export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedRequestParams {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}
