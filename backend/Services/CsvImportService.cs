using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MoneyManager.API.Data;
using MoneyManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MoneyManager.API.Services;

/// <summary>
/// Service for importing transactions from CSV files with support for multiple bank formats
/// </summary>
public class CsvImportService
{
    private readonly IDbContextFactory<MoneyManagerDbContext> _contextFactory;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(
        IDbContextFactory<MoneyManagerDbContext> contextFactory,
        ILogger<CsvImportService> logger)
    {
        _contextFactory = contextFactory; 
        _logger = logger;
    }

    /// <summary>
    /// Result of a CSV import operation
    /// </summary>
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public int DuplicateCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Transaction> ImportedTransactions { get; set; } = new();
    }

    /// <summary>
    /// Import transactions from a CSV file with format configuration
    /// </summary>
    public async Task<ImportResult> ImportFromCsvAsync(
        Stream csvStream, 
        string fileName, 
        int accountId,
        int? formatId = null)
    {
        var result = new ImportResult();
        
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Get format configuration
            CsvFormatConfiguration? format = null;
            if (formatId.HasValue)
            {
                format = await context.CsvFormatConfigurations.FindAsync(formatId.Value);
            }
            
            // If no format specified, try to auto-detect
            if (format == null)
            {
                format = await DetectFormatAsync(csvStream);
                if (format == null)
                {
                    result.Errors.Add("Could not detect CSV format. Please specify a format manually.");
                    result.ErrorCount++;
                    return result;
                }
            }
            
            // Reset stream position
            csvStream.Position = 0;
            
            // Parse CSV with format configuration
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = format.HasHeader,
                Delimiter = format.Delimiter,
                BadDataFound = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.ToLowerInvariant()
            });

            // Skip header rows if configured
            for (int i = 0; i < format.SkipRows; i++)
            {
                await reader.ReadLineAsync();
            }
            
            // Read header and create column index mapping
            if (csv.HeaderRecord == null)
            {
                result.Errors.Add("CSV file has no header record");
                result.ErrorCount++;
                return result;
            }
            
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < csv.HeaderRecord.Length; i++)
            {
                headerMap[csv.HeaderRecord[i].ToLowerInvariant()] = i;
            }
            
            // Read all records
            var records = new List<string[]>();
            while (await csv.ReadAsync())
            {
                var fields = new string[csv.HeaderRecord.Length];
                for (int i = 0; i < csv.HeaderRecord.Length; i++)
                {
                    fields[i] = csv.GetField<string>(i) ?? string.Empty;
                }
                records.Add(fields);
            }
            
            foreach (var fields in records)
            {
                result.TotalRows++;
                
                try
                {
                    // Parse fields based on format configuration
                    var date = ParseDate(fields, headerMap, format);
                    var description = ParseDescription(fields, headerMap, format);
                    var amount = ParseAmount(fields, headerMap, format);
                    var type = ParseType(fields, headerMap, format);
                    var category = ParseCategory(fields, headerMap, format);
                    var balance = ParseBalance(fields, headerMap, format);
                    var accountNumber = ParseAccountNumber(fields, headerMap, format);
                    
                    // Generate duplicate hash
                    var duplicateHash = GenerateDuplicateHash(date, description, amount);
                    
                    // Check for duplicates using hash
                    var isDuplicate = await context.Transactions
                        .AnyAsync(t => t.DuplicateHash == duplicateHash);
                    
                    if (isDuplicate)
                    {
                        result.DuplicateCount++;
                        continue;
                    }
                    
                    // Try to find matching category
                    int? categoryId = null;
                    if (!string.IsNullOrEmpty(category))
                    {
                        var categoryLower = category.ToLower();
                        var categories = await context.Categories.ToListAsync();
                        var matchingCategory = categories.FirstOrDefault(c => c.Name.ToLower() == categoryLower);
                        categoryId = matchingCategory?.Id;
                    }
                    
                    // If no category found, try auto-categorization
                    if (categoryId == null && !string.IsNullOrEmpty(description))
                    {
                        categoryId = await AutoCategorizeTransactionAsync(context, description);
                    }
                    
                    // Create transaction
                    var transaction = new Transaction
                    {
                        Date = date,
                        Description = description,
                        Amount = amount,
                        Type = type,
                        CategoryId = categoryId ?? 10, // Default to Uncategorized
                        AccountId = accountId,
                        AccountNumber = accountNumber,
                        Balance = balance,
                        DuplicateHash = duplicateHash,
                        SourceFile = fileName,
                        ImportedDate = DateTime.UtcNow
                    };
                    
                    context.Transactions.Add(transaction);
                    result.ImportedCount++;
                    result.ImportedTransactions.Add(transaction);
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Row {result.TotalRows}: {ex.Message}");
                    _logger.LogError(ex, $"Error importing row {result.TotalRows}");
                }
            }
            
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.ErrorCount++;
            result.Errors.Add($"File processing error: {ex.Message}");
            _logger.LogError(ex, "Error processing CSV file");
        }
        
        return result;
    }

    /// <summary>
    /// Detect CSV format from file content
    /// </summary>
    private async Task<CsvFormatConfiguration?> DetectFormatAsync(Stream csvStream)
    {
        csvStream.Position = 0;
        using var reader = new StreamReader(csvStream);
        var headerLine = await reader.ReadLineAsync();
        
        if (string.IsNullOrEmpty(headerLine))
        {
            return null;
        }
        
        using var context = await _contextFactory.CreateDbContextAsync();
        var formats = await context.CsvFormatConfigurations
            .Where(f => f.IsActive)
            .ToListAsync();
        
        var headerLower = headerLine.ToLower();
        
        // Try to match by header signature
        var matchingFormat = formats.FirstOrDefault(f => 
            !string.IsNullOrEmpty(f.HeaderSignature) && 
            headerLower.Contains(f.HeaderSignature.ToLower()));
        
        // If no match, try to match by required column names
        if (matchingFormat == null)
        {
            var headerColumns = headerLower.Split(',').Select(c => c.Trim()).ToList();
            
            foreach (var format in formats)
            {
                var requiredColumns = new List<string>();
                if (!string.IsNullOrEmpty(format.DateColumn)) requiredColumns.Add(format.DateColumn.ToLower());
                if (!string.IsNullOrEmpty(format.DescriptionColumn)) requiredColumns.Add(format.DescriptionColumn.ToLower());
                
                // Check if format uses debit/credit columns
                if (!string.IsNullOrEmpty(format.DebitColumn) && !string.IsNullOrEmpty(format.CreditColumn))
                {
                    if (headerColumns.Contains(format.DebitColumn.ToLower()) && 
                        headerColumns.Contains(format.CreditColumn.ToLower()))
                    {
                        matchingFormat = format;
                        break;
                    }
                }
                // Check if format uses single amount column
                else if (!string.IsNullOrEmpty(format.AmountColumn))
                {
                    if (headerColumns.Contains(format.AmountColumn.ToLower()) &&
                        requiredColumns.All(col => headerColumns.Contains(col)))
                    {
                        matchingFormat = format;
                        break;
                    }
                }
            }
        }
        
        return matchingFormat;
    }

    /// <summary>
    /// Generate a hash for duplicate detection
    /// </summary>
    private string GenerateDuplicateHash(DateTime date, string description, decimal amount)
    {
        var key = $"{date:yyyyMMdd}_{description.ToLowerInvariant().Trim()}_{amount:F2}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Get field value by column name using header map
    /// </summary>
    private string? GetFieldValue(string[] fields, Dictionary<string, int> headerMap, string? columnName)
    {
        if (string.IsNullOrEmpty(columnName) || !headerMap.TryGetValue(columnName.ToLowerInvariant(), out var index))
        {
            return null;
        }
        
        if (index >= fields.Length)
        {
            return null;
        }
        
        return fields[index];
    }

    /// <summary>
    /// Parse date from record based on format configuration
    /// </summary>
    private DateTime ParseDate(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var dateStr = GetFieldValue(fields, headerMap, format.DateColumn);
        if (string.IsNullOrEmpty(dateStr))
        {
            throw new Exception("Date column not found or empty");
        }
        
        dateStr = dateStr.Trim();
        
        // Try to parse with configured format first
        if (!string.IsNullOrEmpty(format.DateFormat))
        {
            if (DateTime.TryParseExact(dateStr, format.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
        }
        
        // Try common date formats
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }
        
        throw new Exception($"Unable to parse date: {dateStr}");
    }

    /// <summary>
    /// Parse description from record based on format configuration
    /// </summary>
    private string ParseDescription(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var description = GetFieldValue(fields, headerMap, format.DescriptionColumn);
        if (string.IsNullOrEmpty(description))
        {
            throw new Exception("Description column not found or empty");
        }
        
        return description.Trim().Trim('"');
    }

    /// <summary>
    /// Parse amount from record based on format configuration
    /// </summary>
    private decimal ParseAmount(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        // Try debit/credit columns first
        if (!string.IsNullOrEmpty(format.DebitColumn) && !string.IsNullOrEmpty(format.CreditColumn))
        {
            var debitStr = GetFieldValue(fields, headerMap, format.DebitColumn);
            var creditStr = GetFieldValue(fields, headerMap, format.CreditColumn);
            
            if (!string.IsNullOrEmpty(debitStr) && debitStr != "0")
            {
                return -ParseDecimalValue(debitStr, format.CleanCurrencyFormat);
            }
            
            if (!string.IsNullOrEmpty(creditStr) && creditStr != "0")
            {
                return ParseDecimalValue(creditStr, format.CleanCurrencyFormat);
            }
        }
        
        // Try single amount column
        var amountStr = GetFieldValue(fields, headerMap, format.AmountColumn);
        if (!string.IsNullOrEmpty(amountStr))
        {
            return ParseDecimalValue(amountStr, format.CleanCurrencyFormat);
        }
        
        throw new Exception("Amount column not found or empty");
    }

    /// <summary>
    /// Parse a decimal value, optionally cleaning currency format
    /// </summary>
    private decimal ParseDecimalValue(string value, bool cleanCurrencyFormat)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }
        
        value = value.Trim().Trim('"');
        
        if (cleanCurrencyFormat)
        {
            value = value.Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", "");
        }
        
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        
        throw new Exception($"Unable to parse amount: {value}");
    }

    /// <summary>
    /// Parse transaction type from record based on format configuration
    /// </summary>
    private string? ParseType(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var type = GetFieldValue(fields, headerMap, format.TypeColumn);
        return string.IsNullOrEmpty(type) ? null : type.Trim();
    }

    /// <summary>
    /// Parse category from record based on format configuration
    /// </summary>
    private string? ParseCategory(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var category = GetFieldValue(fields, headerMap, format.CategoryColumn);
        return string.IsNullOrEmpty(category) ? null : category.Trim();
    }

    /// <summary>
    /// Parse balance from record based on format configuration
    /// </summary>
    private decimal? ParseBalance(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var balanceStr = GetFieldValue(fields, headerMap, format.BalanceColumn);
        if (string.IsNullOrEmpty(balanceStr) || balanceStr == "0")
        {
            return null;
        }
        
        return ParseDecimalValue(balanceStr, format.CleanCurrencyFormat);
    }

    /// <summary>
    /// Parse account number from record based on format configuration
    /// </summary>
    private string? ParseAccountNumber(string[] fields, Dictionary<string, int> headerMap, CsvFormatConfiguration format)
    {
        var accountNumber = GetFieldValue(fields, headerMap, format.AccountNumberColumn);
        return string.IsNullOrEmpty(accountNumber) ? null : accountNumber.Trim();
    }

    /// <summary>
    /// Auto-categorize a transaction based on description keywords
    /// </summary>
    private async Task<int?> AutoCategorizeTransactionAsync(MoneyManagerDbContext context, string description)
    {
        var categories = await context.Categories
            .Where(c => !string.IsNullOrEmpty(c.AutoCategorizeKeywords))
            .ToListAsync();
        
        var descriptionLower = description.ToLower();
        
        foreach (var category in categories)
        {
            var keywords = category.AutoCategorizeKeywords
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLower());
            
            if (keywords.Any(k => descriptionLower.Contains(k)))
            {
                return category.Id;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    public async Task<byte[]> ExportToCsvAsync(DateTime? startDate, DateTime? endDate, int? categoryId, int? accountId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);
        
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId);
        
        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId);
        
        var transactions = await query.OrderBy(t => t.Date).ThenBy(t => t.Amount).ToListAsync();
        
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        // Write header
        csv.WriteField("Date");
        csv.WriteField("Description");
        csv.WriteField("Amount");
        csv.WriteField("Type");
        csv.WriteField("Category");
        csv.WriteField("Account");
        csv.WriteField("Balance");
        csv.NextRecord();
        
        // Write records
        foreach (var transaction in transactions)
        {
            csv.WriteField(transaction.Date.ToString("yyyy-MM-dd"));
            csv.WriteField(transaction.Description);
            csv.WriteField(transaction.Amount);
            csv.WriteField(transaction.Type);
            csv.WriteField(transaction.Category?.Name);
            csv.WriteField(transaction.Account?.Name);
            csv.WriteField(transaction.Balance);
            csv.NextRecord();
        }
        
        await writer.FlushAsync();
        return memoryStream.ToArray();
    }
}