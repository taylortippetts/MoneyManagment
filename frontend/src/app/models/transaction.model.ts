export interface Transaction {
  id: number;
  date: Date | string;
  description: string;
  amount: number;
  type?: string;
  categoryId?: number;
  category?: Category;
  accountId?: number;
  account?: Account;
  accountNumber?: string;
  balance?: number;
  isReconciled: boolean;
  importedDate: Date | string;
  sourceFile?: string;
}

export interface Account {
  id: number;
  name: string;
  accountType?: string;
  lastFourDigits?: string;
  currentBalance?: number;
  isActive: boolean;
  color?: string;
  icon?: string;
  notes?: string;
  createdDate: Date | string;
  modifiedDate?: Date | string;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  isIncome: boolean;
  isActive: boolean;
  parentCategoryId?: number;
  parentCategory?: Category;
  subCategories?: Category[];
  autoCategorizeKeywords?: string;
}

export interface ImportResult {
  totalRows: number;
  importedCount: number;
  duplicateCount: number;
  errorCount: number;
  errors: string[];
  importedTransactions: Transaction[];
}

export interface TransactionSummary {
  totalIncome: number;
  totalExpenses: number;
  netAmount: number;
  transactionCount: number;
  categoryBreakdown: CategoryBreakdown[];
}

export interface CategoryBreakdown {
  categoryId: number;
  categoryName: string;
  totalAmount: number;
  transactionCount: number;
}

export interface TransactionFilter {
  startDate?: Date | string;
  endDate?: Date | string;
  categoryId?: number;
  isReconciled?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}