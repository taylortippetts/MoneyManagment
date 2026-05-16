using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyManager.API.Models;

/// <summary>
/// Configuration for parsing different bank CSV formats
/// </summary>
public class CsvFormatConfiguration
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the format (e.g., "Discover", "Chase", "Bank of America")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique identifier for the format (used for auto-detection)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FormatKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Delimiter used in the CSV (comma, semicolon, tab, etc.)
    /// </summary>
    [MaxLength(10)]
    public string Delimiter { get; set; } = ",";
    
    /// <summary>
    /// Whether the CSV has a header row
    /// </summary>
    public bool HasHeader { get; set; } = true;
    
    /// <summary>
    /// Number of header rows to skip (for formats with multiple header lines)
    /// </summary>
    public int SkipRows { get; set; }
    
    /// <summary>
    /// Column name or index for the date field
    /// </summary>
    [MaxLength(100)]
    public string? DateColumn { get; set; }
    
    /// <summary>
    /// Date format pattern (e.g., "MM/dd/yyyy", "yyyy-MM-dd")
    /// </summary>
    [MaxLength(50)]
    public string? DateFormat { get; set; }
    
    /// <summary>
    /// Column name or index for the description field
    /// </summary>
    [MaxLength(100)]
    public string? DescriptionColumn { get; set; }
    
    /// <summary>
    /// Column name or index for the amount field (single column for net amounts)
    /// </summary>
    [MaxLength(100)]
    public string? AmountColumn { get; set; }
    
    /// <summary>
    /// Column name or index for debit amounts
    /// </summary>
    [MaxLength(100)]
    public string? DebitColumn { get; set; }
    
    /// <summary>
    /// Column name or index for credit amounts
    /// </summary>
    [MaxLength(100)]
    public string? CreditColumn { get; set; }
    
    /// <summary>
    /// Column name or index for the transaction type
    /// </summary>
    [MaxLength(100)]
    public string? TypeColumn { get; set; }
    
    /// <summary>
    /// Column name or index for the category field
    /// </summary>
    [MaxLength(100)]
    public string? CategoryColumn { get; set; }
    
    /// <summary>
    /// Column name or index for the balance field
    /// </summary>
    [MaxLength(100)]
    public string? BalanceColumn { get; set; }
    
    /// <summary>
    /// Column name or index for the account number field
    /// </summary>
    [MaxLength(100)]
    public string? AccountNumberColumn { get; set; }
    
    /// <summary>
    /// Unique identifier columns for duplicate detection (comma-separated)
    /// </summary>
    [MaxLength(200)]
    public string? UniqueIdentifierColumns { get; set; }
    
    /// <summary>
    /// Header signature for auto-detection (first few header values joined)
    /// </summary>
    [MaxLength(500)]
    public string? HeaderSignature { get; set; }
    
    /// <summary>
    /// Whether this format is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether to treat credits as positive and debits as negative
    /// </summary>
    public bool UseDebitCreditConvention { get; set; } = true;
    
    /// <summary>
    /// Whether to remove $ and , from amount fields
    /// </summary>
    public bool CleanCurrencyFormat { get; set; } = true;
    
    /// <summary>
    /// Date when this configuration was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when this configuration was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}