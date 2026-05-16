using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Data;
using MoneyManager.API.Models;

namespace MoneyManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvFormatsController : ControllerBase
{
    private readonly IDbContextFactory<MoneyManagerDbContext> _contextFactory;
    private readonly ILogger<CsvFormatsController> _logger;

    public CsvFormatsController(
        IDbContextFactory<MoneyManagerDbContext> contextFactory,
        ILogger<CsvFormatsController> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all CSV format configurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CsvFormatConfiguration>>> GetCsvFormats(bool activeOnly = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.CsvFormatConfigurations.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }
        
        return await query.OrderByDescending(c => c.Name).ToListAsync();
    }

    /// <summary>
    /// Get CSV format by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CsvFormatConfiguration>> GetCsvFormat(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var format = await context.CsvFormatConfigurations.FindAsync(id);
        
        if (format == null)
        {
            return NotFound();
        }
        
        return format;
    }

    /// <summary>
    /// Get CSV format by key
    /// </summary>
    [HttpGet("key/{key}")]
    public async Task<ActionResult<CsvFormatConfiguration>> GetCsvFormatByKey(string key)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var format = await context.CsvFormatConfigurations
            .FirstOrDefaultAsync(c => c.FormatKey.ToLower() == key.ToLower());
        
        if (format == null)
        {
            return NotFound();
        }
        
        return format;
    }

    /// <summary>
    /// Create a new CSV format configuration
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CsvFormatConfiguration>> CreateCsvFormat(CsvFormatConfiguration format)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Check for duplicate format key
        if (await context.CsvFormatConfigurations.AnyAsync(c => c.FormatKey.ToLower() == format.FormatKey.ToLower()))
        {
            return BadRequest("A format with this key already exists.");
        }
        
        format.CreatedDate = DateTime.UtcNow;
        context.CsvFormatConfigurations.Add(format);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetCsvFormat), new { id = format.Id }, format);
    }

    /// <summary>
    /// Update a CSV format configuration
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCsvFormat(int id, CsvFormatConfiguration format)
    {
        if (id != format.Id)
        {
            return BadRequest();
        }
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingFormat = await context.CsvFormatConfigurations.FindAsync(id);
        if (existingFormat == null)
        {
            return NotFound();
        }
        
        existingFormat.Name = format.Name;
        existingFormat.FormatKey = format.FormatKey;
        existingFormat.Delimiter = format.Delimiter;
        existingFormat.HasHeader = format.HasHeader;
        existingFormat.SkipRows = format.SkipRows;
        existingFormat.DateColumn = format.DateColumn;
        existingFormat.DateFormat = format.DateFormat;
        existingFormat.DescriptionColumn = format.DescriptionColumn;
        existingFormat.AmountColumn = format.AmountColumn;
        existingFormat.DebitColumn = format.DebitColumn;
        existingFormat.CreditColumn = format.CreditColumn;
        existingFormat.TypeColumn = format.TypeColumn;
        existingFormat.CategoryColumn = format.CategoryColumn;
        existingFormat.BalanceColumn = format.BalanceColumn;
        existingFormat.AccountNumberColumn = format.AccountNumberColumn;
        existingFormat.UniqueIdentifierColumns = format.UniqueIdentifierColumns;
        existingFormat.HeaderSignature = format.HeaderSignature;
        existingFormat.IsActive = format.IsActive;
        existingFormat.UseDebitCreditConvention = format.UseDebitCreditConvention;
        existingFormat.CleanCurrencyFormat = format.CleanCurrencyFormat;
        existingFormat.ModifiedDate = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Delete a CSV format configuration
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCsvFormat(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var format = await context.CsvFormatConfigurations.FindAsync(id);
        if (format == null)
        {
            return NotFound();
        }
        
        context.CsvFormatConfigurations.Remove(format);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Detect CSV format from file content
    /// </summary>
    [HttpPost("detect")]
    public async Task<ActionResult<CsvFormatDetectionResult>> DetectCsvFormat([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Read first few lines to detect format
        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync();
        
        if (string.IsNullOrEmpty(headerLine))
        {
            return BadRequest("Empty file");
        }
        
        // Get all active formats
        var formats = await context.CsvFormatConfigurations
            .Where(c => c.IsActive)
            .ToListAsync();
        
        // Try to match by header signature
        var headerLower = headerLine.ToLower();
        var matchingFormat = formats.FirstOrDefault(f => 
            !string.IsNullOrEmpty(f.HeaderSignature) && 
            headerLower.Contains(f.HeaderSignature.ToLower()));
        
        // If no match, try to match by column names
        if (matchingFormat == null)
        {
            var headerColumns = headerLower.Split(',').Select(c => c.Trim()).ToList();
            
            foreach (var format in formats)
            {
                var requiredColumns = new List<string>();
                if (!string.IsNullOrEmpty(format.DateColumn)) requiredColumns.Add(format.DateColumn.ToLower());
                if (!string.IsNullOrEmpty(format.DescriptionColumn)) requiredColumns.Add(format.DescriptionColumn.ToLower());
                
                if (requiredColumns.All(col => headerColumns.Contains(col)))
                {
                    matchingFormat = format;
                    break;
                }
            }
        }
        
        var result = new CsvFormatDetectionResult
        {
            DetectedFormat = matchingFormat,
            HeaderLine = headerLine,
            Confidence = matchingFormat != null ? "high" : "low",
            Suggestions = matchingFormat == null ? formats.Take(3).ToList() : new List<CsvFormatConfiguration>()
        };
        
        return result;
    }
}

public class CsvFormatDetectionResult
{
    public CsvFormatConfiguration? DetectedFormat { get; set; }
    public string HeaderLine { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public List<CsvFormatConfiguration> Suggestions { get; set; } = new();
}