export type RecordStatusFilter = 'active' | 'inactive' | 'all';

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}