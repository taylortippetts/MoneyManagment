# Database Migration Guide

This guide explains how to update your database to support the new multi-format CSV import and account management features.

## New Tables

### 1. Accounts Table

```sql
CREATE TABLE Accounts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    AccountType TEXT,
    LastFourDigits TEXT,
    CurrentBalance REAL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Color TEXT,
    Icon TEXT,
    Notes TEXT,
    CreatedDate TEXT NOT NULL,
    ModifiedDate TEXT
);

CREATE UNIQUE INDEX IX_Accounts_Name ON Accounts(Name);
CREATE INDEX IX_Accounts_IsActive ON Accounts(IsActive);
```

### 2. CsvFormatConfigurations Table

```sql
CREATE TABLE CsvFormatConfigurations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    FormatKey TEXT NOT NULL,
    Delimiter TEXT DEFAULT ',',
    HasHeader INTEGER NOT NULL DEFAULT 1,
    SkipRows INTEGER NOT NULL DEFAULT 0,
    DateColumn TEXT,
    DateFormat TEXT,
    DescriptionColumn TEXT,
    AmountColumn TEXT,
    DebitColumn TEXT,
    CreditColumn TEXT,
    TypeColumn TEXT,
    CategoryColumn TEXT,
    BalanceColumn TEXT,
    AccountNumberColumn TEXT,
    UniqueIdentifierColumns TEXT,
    HeaderSignature TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    UseDebitCreditConvention INTEGER NOT NULL DEFAULT 1,
    CleanCurrencyFormat INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL,
    ModifiedDate TEXT
);

CREATE UNIQUE INDEX IX_CsvFormatConfigurations_FormatKey ON CsvFormatConfigurations(FormatKey);
CREATE INDEX IX_CsvFormatConfigurations_IsActive ON CsvFormatConfigurations(IsActive);
```

## Modified Tables

### Transactions Table

Add the following columns to the existing Transactions table:

```sql
ALTER TABLE Transactions ADD COLUMN AccountId INTEGER;
ALTER TABLE Transactions ADD COLUMN DuplicateHash TEXT;

CREATE INDEX IX_Transactions_AccountId ON Transactions(AccountId);
CREATE INDEX IX_Transactions_DuplicateHash ON Transactions(DuplicateHash);
```

## Seed Data

### Default CSV Format Configurations

```sql
-- Discover Format
INSERT INTO CsvFormatConfigurations (
    Name, FormatKey, Delimiter, HasHeader, SkipRows,
    DateColumn, DateFormat, DescriptionColumn, AmountColumn,
    DebitColumn, CreditColumn, TypeColumn, CategoryColumn, BalanceColumn,
    AccountNumberColumn, UniqueIdentifierColumns, HeaderSignature,
    IsActive, UseDebitCreditConvention, CleanCurrencyFormat, CreatedDate
) VALUES (
    'Discover', 'discover', ',', 1, 0,
    'Transaction Date', 'MM/dd/yyyy', 'Transaction Description', NULL,
    'Debit', 'Credit', 'Transaction Type', NULL, 'Balance',
    NULL, 'Transaction Date,Transaction Description,Amount', 'transaction date,transaction description,transaction type,debit,credit,balance',
    1, 1, 1, datetime('now')
);

-- Chase Format
INSERT INTO CsvFormatConfigurations (
    Name, FormatKey, Delimiter, HasHeader, SkipRows,
    DateColumn, DateFormat, DescriptionColumn, AmountColumn,
    DebitColumn, CreditColumn, TypeColumn, CategoryColumn, BalanceColumn,
    AccountNumberColumn, UniqueIdentifierColumns, HeaderSignature,
    IsActive, UseDebitCreditConvention, CleanCurrencyFormat, CreatedDate
) VALUES (
    'Chase', 'chase', ',', 1, 0,
    'Posting Date', 'MM/dd/yyyy', 'Description', 'Amount',
    NULL, NULL, 'Type', NULL, 'Balance',
    NULL, 'Posting Date,Description,Amount', 'details,posting date,description,amount,type,balance',
    1, 0, 1, datetime('now')
);

-- Generic Format (for simple CSV files)
INSERT INTO CsvFormatConfigurations (
    Name, FormatKey, Delimiter, HasHeader, SkipRows,
    DateColumn, DateFormat, DescriptionColumn, AmountColumn,
    DebitColumn, CreditColumn, TypeColumn, CategoryColumn, BalanceColumn,
    AccountNumberColumn, UniqueIdentifierColumns, HeaderSignature,
    IsActive, UseDebitCreditConvention, CleanCurrencyFormat, CreatedDate
) VALUES (
    'Generic', 'generic', ',', 1, 0,
    'date', NULL, 'description', 'amount',
    NULL, NULL, 'type', 'category', 'balance',
    'accountnumber', 'date,description,amount', 'date,description,amount,type,category,accountnumber,balance',
    1, 0, 1, datetime('now')
);
```

### Sample Account (Optional)

```sql
INSERT INTO Accounts (
    Name, AccountType, LastFourDigits, CurrentBalance, IsActive,
    Color, Icon, Notes, CreatedDate
) VALUES (
    'Main Checking', 'checking', '1234', 5000.00, 1,
    '#2196F3', 'account_balance', 'Primary checking account', datetime('now')
);
```

## Running the Migration

### Option 1: Using EF Core Migrations (Recommended)

```bash
# In the backend directory
dotnet ef migrations add AddAccountsAndCsvFormats
dotnet ef database update
```

### Option 2: Manual SQL Migration

1. Backup your existing database
2. Run the SQL scripts above in order
3. Verify the migration was successful

## Verifying the Migration

After migration, verify the new tables exist:

```sql
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;
```

You should see:
- Accounts
- CsvFormatConfigurations
- Categories (existing)
- Transactions (existing, with new columns)

## Troubleshooting

If you encounter issues:

1. **Foreign key errors**: Ensure the Accounts table is created before adding the AccountId column to Transactions
2. **Duplicate column errors**: Check if columns already exist before adding them
3. **Data loss**: Always backup your database before running migrations

## Rolling Back

If you need to roll back:

```sql
-- Remove new columns from Transactions
-- Note: SQLite doesn't support DROP COLUMN directly, you may need to recreate the table

-- Drop new tables (if needed)
DROP TABLE IF EXISTS CsvFormatConfigurations;
DROP TABLE IF EXISTS Accounts;
```

## Next Steps

After migration:

1. Create at least one account in the system
2. Configure CSV formats for your bank(s)
3. Test importing a CSV file with the new system
4. Verify duplicate detection is working correctly