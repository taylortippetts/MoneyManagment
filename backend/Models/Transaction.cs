using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyManager.API.Models;

/// <summary>
/// Represents a financial transaction imported from bank CSV files
/// </summary>
public class Transaction
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Date of the transaction
    /// </summary>
    [Required]
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Description or memo from the bank transaction
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount of the transaction (positive for deposits, negative for withdrawals)
    /// </summary>
    [Required]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Transaction type (debit, credit, transfer, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }
    
    /// <summary>
    /// Category ID for categorizing transactions
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property for category
    /// </summary>
    public Category? Category { get; set; }
    
    /// <summary>
    /// Account identifier from the bank
    /// </summary>
    [MaxLength(100)]
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// Balance after this transaction
    /// </summary>
    public decimal? Balance { get; set; }
    
    /// <summary>
    /// Account ID this transaction belongs to
    /// </summary>
    public int? AccountId { get; set; }
    
    /// <summary>
    /// Navigation property for account
    /// </summary>
    public Account? Account { get; set; }
    
    /// <summary>
    /// Hash for duplicate detection
    /// </summary>
    [MaxLength(255)]
    public string? DuplicateHash { get; set; }
    
    /// <summary>
    /// Whether the transaction has been reconciled
    /// </summary>
    public bool IsReconciled { get; set; }
    
    /// <summary>
    /// Date when this transaction was imported
    /// </summary>
    public DateTime ImportedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Source file name from which this transaction was imported
    /// </summary>
    [MaxLength(255)]
    public string? SourceFile { get; set; }
}