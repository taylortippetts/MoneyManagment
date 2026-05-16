# How to Run the Money Manager Application

## Prerequisites

Before running the app, make sure you have these installed:

1. **.NET 8 SDK** - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - After installing, verify by running: `dotnet --version`

2. **Node.js 18+** - Download from: https://nodejs.org/
   - After installing, verify by running: `node --version` and `npm --version`

## Step-by-Step Instructions

### Step 1: Open Terminal/Command Prompt

Open a new terminal or command prompt window.

### Step 2: Navigate to Project Directory

```bash
cd C:\Users\Taylo\Desktop\MoneyManager
```

### Step 3: Start the Backend API

Open a terminal and run:

```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

**What this does:**
- `dotnet restore` - Downloads all required NuGet packages
- `dotnet build` - Compiles the project
- `dotnet run` - Starts the API server

**You should see output like:**
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

**Keep this terminal open** - the backend needs to stay running.

### Step 4: Start the Frontend (New Terminal)

Open a **second terminal** window and run:

```bash
cd frontend
npm install
npm start
```

**What this does:**
- `npm install` - Downloads all required npm packages (this may take a few minutes)
- `npm start` - Starts the Angular development server

**You should see output like:**
```
✔ Browser application bundle generation complete.
Initial Chunk Files | Names | Raw Size
...
Server at: http://localhost:4200
```

Your web browser should automatically open to `http://localhost:4200`.

### Step 5: Use the Application

1. **Import Sample Data**: 
   - Click "Import CSV" in the navigation
   - Drag and drop the `sample-transactions.csv` file (located in the project root)
   - Or click to browse and select the file

2. **View Dashboard**: Click "Dashboard" to see your financial overview

3. **Manage Transactions**: Click "Transactions" to view, filter, and edit transactions

4. **Manage Categories**: Click "Categories" to customize your spending categories

5. **View Reports**: Click "Reports" to see visual charts of your spending

## Troubleshooting

### Backend Issues

**Error: "dotnet: command not found"**
- Make sure .NET 8 SDK is installed
- Restart your terminal after installation

**Error: Port 5000 already in use**
- Close any other applications using port 5000
- Or modify the port in `backend/Program.cs` (change `app.Run("http://localhost:5000")`)

**Database errors**
- Delete the `moneymanager.db` file in the backend folder
- Restart the backend (it will recreate the database)

### Frontend Issues

**Error: "npm: command not found"**
- Make sure Node.js is installed
- Restart your terminal after installation

**Error: Module not found**
- Delete the `node_modules` folder in frontend
- Run `npm install` again

**Error: Cannot connect to backend**
- Make sure the backend is running on http://localhost:5000
- Check the `proxy.conf.json` file in frontend directory

## Quick Start Commands

If you want to run everything quickly:

**Terminal 1 (Backend):**
```bash
cd MoneyManager/backend
dotnet run
```

**Terminal 2 (Frontend):**
```bash
cd MoneyManager/frontend
npm install
npm start
```

## Stopping the Application

- **Backend**: Press `Ctrl+C` in the backend terminal
- **Frontend**: Press `Ctrl+C` in the frontend terminal

## Access Points

- **Frontend UI**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger

## Sample Data

Use the included `sample-transactions.csv` file to test the import feature. It contains sample transactions for January 2024 with various categories already assigned.