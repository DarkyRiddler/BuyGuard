interface Request {
  id: number;
  userEmail: string;
  userId: number;
  userName: string;
  managerId: number;
  managerName: string;
  description: string;
  amountPln: number;
  reason: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

type RequestStatus = 'czeka' | 'potwierdzono' | 'odrzucono' | 'zakupione'

interface PaginatedResponse {
  request: Request[];
  totalPages: number;
  currentPage: number;
  totalRequests: number;
  filters?: {
    status?: string;
    minAmount?: number;
    maxAmount?: number;
    dateFrom?: string;
    dateTo?: string;
    searchName?: string;
    sortBy?: string;
    sortOrder?: string;
  };
}