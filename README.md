# Money Manager - Personal Finance Application

A full-stack money management application built with **Angular** (frontend) and **ASP.NET Core** (backend) with **SQLite** database. Import bank transactions from CSV files, categorize them, and analyze your spending habits.

## Features

- **CSV Import**: Upload bank transaction CSV files - the app auto-detects duplicates and supports auto-categorization
- **Transaction Management**: View, filter, search, edit, and delete transactions with pagination
- **Category Management**: Create, edit, and organize expense/income categories with colors and icons
- **Auto-Categorization**: Set keywords for automatic transaction categorization
- **Dashboard**: Overview of income, expenses, net savings, and category breakdown
- **Reports**: Visual charts showing expense breakdown by category
- **Export**: Export filtered transactions back to CSV
- **Responsive UI**: Built with Angular Material for a clean, modern interface

## Project Structure

```
MoneyManager/
├── backend/                    # ASP.NET Core Web API
│   ├── Controllers/            # API endpoints
│   │   ├── TransactionsController.cs
│   │   └── CategoriesController.cs
│   ├── Data/                   # Database context
│   │   └── MoneyManagerDbContext.cs
│   ├── Models/                 # Data models
│   │   ├── Transaction.cs
│   │   └── Category.cs
│   ├── Services/               # Business logic
│   │   └── CsvImportService.cs
│   ├── Program.cs              # Application entry point
│   ├── appsettings.json        # Configuration
│   └── MoneyManager.API.csproj # Project file
│
├── frontend/                   # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/     # UI components
│   │   │   │   ├── dashboard/
│   │   │   │   ├── transactions/
│   │   │   │   ├── categories/
│   │   │   │   ├── import/
│   │   │   │   └── reports/
│   │   │   ├── models/         # TypeScript interfaces
│   │   │   ├── services/       # API services
│   │   │   └── app.routes.ts
│   │   ├── styles.scss         # Global styles
│   │   └── main.ts             # Application entry
│   ├── angular.json            # Angular configuration
│   ├── package.json            # Dependencies
│   ├── tsconfig.json           # TypeScript config
│   └── proxy.conf.json         # Dev proxy to backend
│
└── README.md
```

## Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm** (comes with Node.js)

## Getting Started

### 1. Start the Backend API

```bash
cd MoneyManager/backend

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the API server (runs on http://localhost:5000)
dotnet run
```

The API will start and automatically create the SQLite database (`moneymanager.db`) on first run. Swagger documentation is available at `http://localhost:5000/swagger`.

### 2. Start the Frontend

Open a new terminal:

```bash
cd MoneyManager/frontend

# Install npm dependencies
npm install

# Start the development server (runs on http://localhost:4200)
npm start
```

The Angular app will open in your browser at `http://localhost:4200`.

## CSV Format

The import feature expects CSV files with these columns (header names are case-insensitive):

| Column | Required | Description |
|--------|----------|-------------|
| `date` | Yes | Transaction date (YYYY-MM-DD, MM/DD/YYYY) |
| `description` | Yes | Transaction description/memo |
| `amount` | Yes | Amount (positive = deposit, negative = withdrawal) |
| `category` | No | Category name (matches existing categories) |
| `type` | No | Transaction type (debit, credit, etc.) |
| `accountnumber` | No | Account identifier |
| `balance` | No | Running balance after transaction |

### Example CSV

```csv
date,description,amount,category
2024-01-15,Grocery Store,-85.50,Food & Dining
2024-01-16,Paycheck,2500.00,Income
2024-01-17,Gas Station,-45.00,Transportation
```

## API Endpoints

### Transactions
- `GET /api/transactions` - List transactions (with filtering & pagination)
- `GET /api/transactions/{id}` - Get single transaction
- `POST /api/transactions` - Create transaction
- `PUT /api/transactions/{id}` - Update transaction
- `DELETE /api/transactions/{id}` - Delete transaction
- `POST /api/transactions/import` - Import CSV file
- `GET /api/transactions/export` - Export transactions to CSV
- `GET /api/transactions/summary` - Get summary statistics
- `PUT /api/transactions/bulk-categorize` - Bulk update categories

### Categories
- `GET /api/categories` - List all categories
- `GET /api/categories/{id}` - Get single category
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category
- `GET /api/categories/{id}/stats` - Get category statistics

## Default Categories

The app seeds these categories on first run:
- Income (green)
- Housing (blue)
- Food & Dining (orange)
- Transportation (purple)
- Healthcare (red)
- Entertainment (pink)
- Shopping (cyan)
- Bills & Fees (gray)
- Savings & Investments (light green)
- Uncategorized (gray)

## Technology Stack

### Backend
- **ASP.NET Core 8** - Web API framework
- **Entity Framework Core** - ORM
- **SQLite** - Database
- **CsvHelper** - CSV parsing library
- **Swashbuckle** - Swagger/OpenAPI documentation

### Frontend
- **Angular 17** - UI framework (standalone components)
- **Angular Material** - UI component library
- **RxJS** - Reactive programming
- **TypeScript** - Type-safe JavaScript

## Development

### Backend Development
```bash
cd backend
dotnet watch run  # Hot reload during development
```

### Frontend Development
```bash
cd frontend
npm start  # Auto-rebuild on file changes
```

The frontend proxy (`proxy.conf.json`) forwards `/api` requests to the backend at `http://localhost:5000`.

## Database

The SQLite database file (`moneymanager.db`) is created in the backend directory. To reset:
1. Stop the backend
2. Delete `moneymanager.db`
3. Restart the backend (it will recreate with seed data)

## License

MIT License