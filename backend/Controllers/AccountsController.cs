using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Data;
using MoneyManager.API.Models;

namespace MoneyManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IDbContextFactory<MoneyManagerDbContext> _contextFactory;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IDbContextFactory<MoneyManagerDbContext> contextFactory,
        ILogger<AccountsController> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all accounts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(bool activeOnly = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Accounts
            .Include(a => a.Transactions)
            .AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }
        
        return await query.OrderByDescending(a => a.Name).ToListAsync();
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var account = await context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (account == null)
        {
            return NotFound();
        }
        
        return account;
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount(Account account)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Check for duplicate name
        if (await context.Accounts.AnyAsync(a => a.Name.ToLower() == account.Name.ToLower()))
        {
            return BadRequest("An account with this name already exists.");
        }
        
        account.CreatedDate = DateTime.UtcNow;
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>
    /// Update an account
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, Account account)
    {
        if (id != account.Id)
        {
            return BadRequest();
        }
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingAccount = await context.Accounts.FindAsync(id);
        if (existingAccount == null)
        {
            return NotFound();
        }
        
        existingAccount.Name = account.Name;
        existingAccount.AccountType = account.AccountType;
        existingAccount.LastFourDigits = account.LastFourDigits;
        existingAccount.CurrentBalance = account.CurrentBalance;
        existingAccount.IsActive = account.IsActive;
        existingAccount.Color = account.Color;
        existingAccount.Icon = account.Icon;
        existingAccount.Notes = account.Notes;
        existingAccount.ModifiedDate = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var account = await context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        
        // Check if account has transactions
        var hasTransactions = await context.Transactions.AnyAsync(t => t.AccountId == id);
        if (hasTransactions)
        {
            // Instead of deleting, deactivate the account
            account.IsActive = false;
            await context.SaveChangesAsync();
            return Ok(new { message = "Account deactivated (has associated transactions)" });
        }
        
        context.Accounts.Remove(account);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Get account summary with balances
    /// </summary>
    [HttpGet("summary/totals")]
    public async Task<ActionResult<AccountSummary>> GetAccountTotals()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var accounts = await context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync();
        
        var summary = new AccountSummary
        {
            TotalAccounts = accounts.Count,
            TotalBalance = accounts.Sum(a => a.CurrentBalance ?? 0),
            Accounts = accounts.Select(a => new AccountBalanceInfo
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType,
                Balance = a.CurrentBalance ?? 0,
                Color = a.Color
            }).ToList()
        };
        
        return summary;
    }
}

public class AccountSummary
{
    public int TotalAccounts { get; set; }
    public decimal TotalBalance { get; set; }
    public List<AccountBalanceInfo> Accounts { get; set; } = new();
}

public class AccountBalanceInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountType { get; set; }
    public decimal Balance { get; set; }
    public string? Color { get; set; }
}