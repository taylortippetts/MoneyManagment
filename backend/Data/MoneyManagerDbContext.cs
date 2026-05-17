using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Models;

namespace MoneyManager.API.Data;

/// <summary>
/// SQLite database context for the Money Manager application
/// </summary>
public class MoneyManagerDbContext : DbContext
{
    public MoneyManagerDbContext(DbContextOptions<MoneyManagerDbContext> options)
        : base(options)
    {
    }
    
    /// <summary>
    /// DbSet for transactions
    /// </summary>
    public DbSet<Transaction> Transactions { get; set; } = null!;
    
    /// <summary>
    /// DbSet for categories
    /// </summary>
    public DbSet<Category> Categories { get; set; } = null!;
    
    /// <summary>
    /// DbSet for accounts
    /// </summary>
    public DbSet<Account> Accounts { get; set; } = null!;
    
    /// <summary>
    /// DbSet for CSV format configurations
    /// </summary>
    public DbSet<CsvFormatConfiguration> CsvFormatConfigurations { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.ImportedDate);
            entity.HasIndex(e => e.IsReconciled);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.DuplicateHash);
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(e => e.CategoryId);
            
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(e => e.AccountId);
        });
        
        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
        // Configure CsvFormatConfiguration entity
        modelBuilder.Entity<CsvFormatConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.FormatKey).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsIncome);
            
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Seed default categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Income", Description = "Salary, wages, and other income", Color = "#4CAF50", Icon = "trending_up", IsIncome = true, IsActive = true },
            new Category { Id = 2, Name = "Housing", Description = "Rent, mortgage, utilities", Color = "#2196F3", Icon = "home", IsIncome = false, IsActive = true },
            new Category { Id = 3, Name = "Food & Dining", Description = "Groceries, restaurants, delivery", Color = "#FF9800", Icon = "restaurant", IsIncome = false, IsActive = true },
            new Category { Id = 4, Name = "Transportation", Description = "Gas, public transit, car maintenance", Color = "#9C27B0", Icon = "directions_car", IsIncome = false, IsActive = true },
            new Category { Id = 5, Name = "Healthcare", Description = "Medical, dental, pharmacy", Color = "#F44336", Icon = "local_hospital", IsIncome = false, IsActive = true },
            new Category { Id = 6, Name = "Entertainment", Description = "Movies, games, hobbies", Color = "#E91E63", Icon = "theaters", IsIncome = false, IsActive = true },
            new Category { Id = 7, Name = "Shopping", Description = "Clothing, electronics, miscellaneous", Color = "#00BCD4", Icon = "shopping_cart", IsIncome = false, IsActive = true },
            new Category { Id = 8, Name = "Bills & Fees", Description = "Bank fees, subscriptions, taxes", Color = "#607D8B", Icon = "receipt", IsIncome = false, IsActive = true },
            new Category { Id = 9, Name = "Savings & Investments", Description = "Transfers to savings and investments", Color = "#8BC34A", Icon = "savings", IsIncome = false, IsActive = true },
            new Category { Id = 10, Name = "Uncategorized", Description = "Transactions not yet categorized", Color = "#9E9E9E", Icon = "help", IsIncome = false, IsActive = true, AutoCategorizeKeywords = "" }
        );

        // Seed a CSV format configuration for Discover-style exports (Debit/Credit columns)
        modelBuilder.Entity<CsvFormatConfiguration>().HasData(
            new CsvFormatConfiguration
            {
                Id = 1,
                Name = "Discover (Debit/Credit)",
                FormatKey = "discover_debit_credit",
                Delimiter = ",",
                HasHeader = true,
                SkipRows = 0,
                DateColumn = "Transaction Date",
                DateFormat = "MM/dd/yyyy",
                DescriptionColumn = "Transaction Description",
                TypeColumn = "Transaction Type",
                DebitColumn = "Debit",
                CreditColumn = "Credit",
                BalanceColumn = "Balance",
                HeaderSignature = "Transaction Date,Transaction Description,Transaction Type,Debit,Credit,Balance",
                UseDebitCreditConvention = true,
                CleanCurrencyFormat = true,
                IsActive = true
            }
        );
    }
}