using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Data;
using MoneyManager.API.Models;

namespace MoneyManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IDbContextFactory<MoneyManagerDbContext> _contextFactory;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        IDbContextFactory<MoneyManagerDbContext> contextFactory,
        ILogger<CategoriesController> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories(bool? isActive, bool? isIncome)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .AsQueryable();
        
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);
        
        if (isIncome.HasValue)
            query = query.Where(c => c.IsIncome == isIncome.Value);
        
        var categories = await query
            .OrderBy(c => c.IsIncome)
            .ThenBy(c => c.Name)
            .ToListAsync();
        
        return Ok(categories);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var category = await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (category == null)
            return NotFound();
        
        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Check for duplicate name
        if (await context.Categories.AnyAsync(c => c.Name == category.Name))
            return BadRequest("Category name already exists");
        
        // Validate parent category if provided
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await context.Categories.FindAsync(category.ParentCategoryId.Value);
            if (parent == null)
                return BadRequest("Invalid parent category ID");
        }
        
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category category)
    {
        if (id != category.Id)
            return BadRequest("ID mismatch");
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingCategory = await context.Categories.FindAsync(id);
        if (existingCategory == null)
            return NotFound();
        
        // Check for duplicate name (excluding current category)
        if (await context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != id))
            return BadRequest("Category name already exists");
        
        // Validate parent category if provided
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await context.Categories.FindAsync(category.ParentCategoryId.Value);
            if (parent == null)
                return BadRequest("Invalid parent category ID");
            
            // Prevent self-referencing
            if (parent.Id == id)
                return BadRequest("Category cannot be its own parent");
        }
        
        // Update fields
        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.Color = category.Color;
        existingCategory.Icon = category.Icon;
        existingCategory.IsIncome = category.IsIncome;
        existingCategory.IsActive = category.IsActive;
        existingCategory.ParentCategoryId = category.ParentCategoryId;
        existingCategory.AutoCategorizeKeywords = category.AutoCategorizeKeywords;
        
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();
        
        // Check if category has transactions
        var hasTransactions = await context.Transactions
            .AnyAsync(t => t.CategoryId == id);
        
        if (hasTransactions)
        {
            // Move transactions to Uncategorized (ID 10)
            var uncategorized = await context.Categories.FindAsync(10);
            if (uncategorized != null)
            {
                var transactions = await context.Transactions
                    .Where(t => t.CategoryId == id)
                    .ToListAsync();
                
                foreach (var transaction in transactions)
                {
                    transaction.CategoryId = 10;
                }
            }
        }
        
        // Remove subcategories
        var subCategories = await context.Categories
            .Where(c => c.ParentCategoryId == id)
            .ToListAsync();
        
        context.Categories.RemoveRange(subCategories);
        context.Categories.Remove(category);
        
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Get category statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult> GetCategoryStats(int id, DateTime? startDate, DateTime? endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();
        
        var query = context.Transactions
            .Where(t => t.CategoryId == id)
            .AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);
        
        var totalAmount = await query.SumAsync(t => t.Amount);
        var transactionCount = await query.CountAsync();
        var averageAmount = transactionCount > 0 ? totalAmount / transactionCount : 0;
        
        // Get monthly breakdown
        var monthlyBreakdown = await query
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(g => g.Year)
            .ThenByDescending(g => g.Month)
            .ToListAsync();
        
        return Ok(new
        {
            Category = category,
            TotalAmount = totalAmount,
            TransactionCount = transactionCount,
            AverageAmount = averageAmount,
            MonthlyBreakdown = monthlyBreakdown
        });
    }
}