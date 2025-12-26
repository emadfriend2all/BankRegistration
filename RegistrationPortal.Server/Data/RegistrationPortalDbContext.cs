using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Entities.Identity;
using Oracle.EntityFrameworkCore;

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

        // Identity tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure for Oracle 11g compatibility
            ConfigureOracle11gCompatibility(modelBuilder);

            // Configure CustMast entity
            ConfigureCustMast(modelBuilder);

            // Configure AccountMast entity
            ConfigureAccountMast(modelBuilder);

            // Configure CustomerFatca entity
            ConfigureCustomerFatca(modelBuilder);

            // Configure CustomerDocument entity
            ConfigureCustomerDocument(modelBuilder);

            // Configure Identity entities
            ConfigureIdentityEntities(modelBuilder);

            // Configure relationship
            ConfigureRelationship(modelBuilder);
        }

        private void ConfigureOracle11gCompatibility(ModelBuilder modelBuilder)
        {
            // Configure Oracle 11g specific settings
            modelBuilder.HasAnnotation("Relational:Collation", "BINARY_CI");
            
            // Disable features not supported in Oracle 11g
            modelBuilder.HasAnnotation("ProductVersion", "11.2.0.1.0");
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

        private void ConfigureIdentityEntities(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_USERS");

                entity.Property(e => e.Id)
                    .HasColumnName("USER_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.Username)
                    .HasColumnName("USERNAME")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("PASSWORD_HASH")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("FIRST_NAME")
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .HasColumnName("LAST_NAME")
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasDefaultValueSql("SYSDATE");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("UPDATED_AT");

                entity.Property(e => e.LastLoginAt)
                    .HasColumnName("LAST_LOGIN_AT");

                entity.Property(e => e.Branch)
                    .HasColumnName("BRANCH")
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_USERS_USERNAME");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_USERS_EMAIL");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("ROLES");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_ROLES");

                entity.Property(e => e.Id)
                    .HasColumnName("ROLE_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(200);

                entity.Property(e => e.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasDefaultValueSql("SYSDATE");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("UPDATED_AT");

                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_ROLES_NAME");
            });

            // Configure UserRole entity (many-to-many junction)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("USER_ROLES");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_USER_ROLES");

                entity.Property(e => e.Id)
                    .HasColumnName("USER_ROLE_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("ROLE_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.AssignedAt)
                    .HasColumnName("ASSIGNED_AT")
                    .HasDefaultValueSql("SYSDATE");

                entity.Property(e => e.AssignedBy)
                    .HasColumnName("ASSIGNED_BY")
                    .HasColumnType("NUMBER(10)");

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .IsUnique()
                    .HasDatabaseName("IX_USER_ROLES_USER_ROLE");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("FK_USER_ROLES_USER")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .HasConstraintName("FK_USER_ROLES_ROLE")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("PERMISSIONS");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_PERMISSIONS");

                entity.Property(e => e.Id)
                    .HasColumnName("PERMISSION_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(200);

                entity.Property(e => e.Module)
                    .HasColumnName("MODULE")
                    .HasMaxLength(50);

                entity.Property(e => e.Action)
                    .HasColumnName("ACTION")
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasDefaultValueSql("SYSDATE");

                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_PERMISSIONS_NAME");

                entity.HasIndex(e => new { e.Module, e.Action })
                    .IsUnique()
                    .HasDatabaseName("IX_PERMISSIONS_MODULE_ACTION")
                    .HasFilter("MODULE IS NOT NULL AND ACTION IS NOT NULL");
            });

            // Configure RolePermission entity (many-to-many junction)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("ROLE_PERMISSIONS");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_ROLE_PERMISSIONS");

                entity.Property(e => e.Id)
                    .HasColumnName("ROLE_PERMISSION_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("ROLE_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.PermissionId)
                    .HasColumnName("PERMISSION_ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.GrantedAt)
                    .HasColumnName("GRANTED_AT")
                    .HasDefaultValueSql("SYSDATE");

                entity.Property(e => e.GrantedBy)
                    .HasColumnName("GRANTED_BY")
                    .HasColumnType("NUMBER(10)");

                entity.HasIndex(e => new { e.RoleId, e.PermissionId })
                    .IsUnique()
                    .HasDatabaseName("IX_ROLE_PERMISSIONS_ROLE_PERMISSION");

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .HasConstraintName("FK_ROLE_PERMISSIONS_ROLE")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .HasConstraintName("FK_ROLE_PERMISSIONS_PERMISSION")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
