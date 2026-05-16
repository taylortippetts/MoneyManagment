using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Data;
using MoneyManager.API.Models;
using MoneyManager.API.Services;

namespace MoneyManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IDbContextFactory<MoneyManagerDbContext> _contextFactory;
    private readonly CsvImportService _csvImportService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        IDbContextFactory<MoneyManagerDbContext> contextFactory,
        CsvImportService csvImportService,
        ILogger<TransactionsController> logger)
    {
        _contextFactory = contextFactory;
        _csvImportService = csvImportService;
        _logger = logger;
    }

    /// <summary>
    /// Get all transactions with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        bool? isReconciled,
        string? search,
        int page = 1,
        int pageSize = 50)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Transactions
            .Include(t => t.Category)
            .AsQueryable();
        
        // Apply filters
        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);
        
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId);
        
        if (isReconciled.HasValue)
            query = query.Where(t => t.IsReconciled == isReconciled.Value);
        
        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.Description.Contains(search));
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Amount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Count", ((int)Math.Ceiling((double)totalCount / pageSize)).ToString());
        
        return Ok(transactions);
    }

    /// <summary>
    /// Get a specific transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var transaction = await context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
        
        if (transaction == null)
            return NotFound();
        
        return Ok(transaction);
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Validate category exists if provided
        if (transaction.CategoryId.HasValue)
        {
            var category = await context.Categories.FindAsync(transaction.CategoryId.Value);
            if (category == null)
                return BadRequest("Invalid category ID");
        }
        
        transaction.ImportedDate = DateTime.UtcNow;
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
    {
        if (id != transaction.Id)
            return BadRequest("ID mismatch");
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingTransaction = await context.Transactions.FindAsync(id);
        if (existingTransaction == null)
            return NotFound();
        
        // Validate category exists if provided
        if (transaction.CategoryId.HasValue)
        {
            var category = await context.Categories.FindAsync(transaction.CategoryId.Value);
            if (category == null)
                return BadRequest("Invalid category ID");
        }
        
        // Update fields
        existingTransaction.Date = transaction.Date;
        existingTransaction.Description = transaction.Description;
        existingTransaction.Amount = transaction.Amount;
        existingTransaction.Type = transaction.Type;
        existingTransaction.CategoryId = transaction.CategoryId;
        existingTransaction.AccountNumber = transaction.AccountNumber;
        existingTransaction.Balance = transaction.Balance;
        existingTransaction.IsReconciled = transaction.IsReconciled;
        
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var transaction = await context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound();
        
        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Import transactions from a CSV file
    /// </summary>
    [HttpPost("import")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<ActionResult<CsvImportService.ImportResult>> ImportCsv(
        IFormFile file, 
        [FromForm] int accountId,
        [FromForm] int? formatId = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");
        
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid file format. Only CSV files are allowed.");
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Validate account exists
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null)
            return BadRequest("Invalid account ID");
        
        try
        {
            using var stream = file.OpenReadStream();
            var result = await _csvImportService.ImportFromCsvAsync(stream, file.FileName, accountId, formatId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing CSV file");
            return StatusCode(500, "Error processing file");
        }
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<FileResult> ExportCsv(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId = null)
    {
        try
        {
            var csvBytes = await _csvImportService.ExportToCsvAsync(startDate, endDate, categoryId, accountId);
            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting CSV");
            return File(System.Text.Encoding.UTF8.GetBytes("Error exporting data"), "text/plain", "error.txt");
        }
    }

    /// <summary>
    /// Get transaction summary statistics
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult> GetSummary(
        DateTime? startDate,
        DateTime? endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Transactions.AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);
        
        var totalIncome = await query.Where(t => t.Amount > 0).SumAsync(t => t.Amount);
        var totalExpenses = await query.Where(t => t.Amount < 0).SumAsync(t => Math.Abs(t.Amount));
        var transactionCount = await query.CountAsync();
        
        // Get category breakdown
        var categorySummary = await query
            .Include(t => t.Category)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(g => Math.Abs(g.TotalAmount))
            .ToListAsync();
        
        return Ok(new
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetAmount = totalIncome - totalExpenses,
            TransactionCount = transactionCount,
            CategoryBreakdown = categorySummary
        });
    }

    /// <summary>
    /// Bulk update categories for uncategorized transactions
    /// </summary>
    [HttpPut("bulk-categorize")]
    public async Task<IActionResult> BulkCategorize(
        [FromQuery] int categoryId,
        [FromQuery] string? descriptionContains)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Validate category exists
        var category = await context.Categories.FindAsync(categoryId);
        if (category == null)
            return BadRequest("Invalid category ID");
        
        var query = context.Transactions
            .Where(t => t.CategoryId == 10); // Uncategorized
        
        if (!string.IsNullOrEmpty(descriptionContains))
            query = query.Where(t => t.Description.Contains(descriptionContains));
        
        var transactions = await query.ToListAsync();
        
        foreach (var transaction in transactions)
        {
            transaction.CategoryId = categoryId;
        }
        
        var count = await context.SaveChangesAsync();
        
        return Ok(new { UpdatedCount = count });
    }
}