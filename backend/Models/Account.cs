using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyManager.API.Models;

/// <summary>
/// Represents a bank account for tracking transactions
/// </summary>
public class Account
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the account (e.g., "Chase Checking", "Discover Card")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Account type (checking, savings, credit, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? AccountType { get; set; }
    
    /// <summary>
    /// Last 4 digits of the account number (for display purposes)
    /// </summary>
    [MaxLength(10)]
    public string? LastFourDigits { get; set; }
    
    /// <summary>
    /// Current balance of the account
    /// </summary>
    public decimal? CurrentBalance { get; set; }
    
    /// <summary>
    /// Whether this account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Color for visual identification
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Icon for visual identification
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Notes about this account
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date when this account was added
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when this account was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
    
    /// <summary>
    /// Navigation property for transactions
    /// </summary>
    public ICollection<Transaction>? Transactions { get; set; }
}