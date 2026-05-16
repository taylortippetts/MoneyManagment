using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyManager.API.Models;

/// <summary>
/// Represents a category for organizing transactions
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the category
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the category
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Color for UI representation
    /// </summary>
    [MaxLength(7)] // HEX color code like #FF5733
    public string? Color { get; set; }
    
    /// <summary>
    /// Icon name for UI representation
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Whether this is an income category
    /// </summary>
    public bool IsIncome { get; set; }
    
    /// <summary>
    /// Whether this category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Parent category ID for hierarchical categories
    /// </summary>
    public int? ParentCategoryId { get; set; }
    
    /// <summary>
    /// Navigation property for parent category
    /// </summary>
    public Category? ParentCategory { get; set; }
    
    /// <summary>
    /// Navigation property for child categories
    /// </summary>
    public ICollection<Category>? SubCategories { get; set; }
    
    /// <summary>
    /// Navigation property for transactions in this category
    /// </summary>
    public ICollection<Transaction>? Transactions { get; set; }
    
    /// <summary>
    /// Keywords used for automatic categorization
    /// </summary>
    public string? AutoCategorizeKeywords { get; set; }
}