using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Data
{
    public class RegistrationPortalDbContext : DbContext
    {
        public RegistrationPortalDbContext(DbContextOptions<RegistrationPortalDbContext> options)
            : base(options)
        {
        }

        public DbSet<CustMast> CustMasts { get; set; }
        public DbSet<AccountMast> AccountMasts { get; set; }
        public DbSet<CustomerFatca> CustomerFATCA { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CustMast entity
            ConfigureCustMast(modelBuilder);

            // Configure AccountMast entity
            ConfigureAccountMast(modelBuilder);

            // Configure CustomerFatca entity
            ConfigureCustomerFatca(modelBuilder);

            // Configure CustomerDocument entity
            ConfigureCustomerDocument(modelBuilder);

            // Configure relationship
            ConfigureRelationship(modelBuilder);
        }

        private void ConfigureCustMast(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustMast>(entity =>
            {
                // Table configuration
                entity.ToTable("cust_mast", "SSDBONLINE");

                // New string primary key
                entity.HasKey(e => e.Id)
                    .HasName("pk_cust_mast");

                // Configure index for Status column
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_cust_mast_status");
            });
        }

        private void ConfigureAccountMast(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountMast>(entity =>
            {
                // Table configuration
                entity.ToTable("account_mast", "SSDBONLINE");

                // New string primary key
                entity.HasKey(e => e.Id)
                    .HasName("pk_account_mast");
            });
        }

        private void ConfigureCustomerFatca(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerFatca>(entity =>
            {
                // Table configuration
                entity.ToTable("customer_fatca", "SSDBONLINE");

                // Primary key
                entity.HasKey(e => e.Id)
                    .HasName("pk_customer_fatca");

                // Configure foreign key relationship
                entity.HasOne(cf => cf.Customer)
                    .WithOne(cm => cm.CustomerFatca)
                    .HasForeignKey<CustomerFatca>(cf => cf.CustomerId)
                    .HasConstraintName("fk_fatca_customer")
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes for better performance
                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("ix_fatca_customer_id");
            });
        }

        private void ConfigureCustomerDocument(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerDocument>(entity =>
            {
                // Table configuration
                entity.ToTable("customer_documents", "SSDBONLINE");

                // Primary key
                entity.HasKey(e => e.Id)
                    .HasName("pk_customer_documents");

                // Configure foreign key relationship
                entity.HasOne(cd => cd.Customer)
                    .WithMany()
                    .HasForeignKey(cd => cd.CustomerId)
                    .HasConstraintName("fk_document_customer")
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes for better performance
                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("ix_document_customer_id");
                
                entity.HasIndex(e => new { e.CustomerId, e.DocumentType })
                    .HasDatabaseName("ix_document_customer_type");
            });
        }

        private void ConfigureRelationship(ModelBuilder modelBuilder)
        {
            // Configure one-to-many relationship: CustMast -> AccountMast using new string PKs
            modelBuilder.Entity<AccountMast>()
                .HasOne(am => am.CustMast)
                .WithMany(cm => cm.AccountMasts)
                .HasForeignKey(am => am.CustId) // Use string CustId as FK to match CustMast.Id
                .HasConstraintName("fk_acc_cust")
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental customer deletion if accounts exist
        }
    }
}
