using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using InteractiveLeads.Infrastructure.Tenancy.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractiveLeads.Infrastructure.Context.Tenancy
{
    public class TenantDbContext(DbContextOptions<TenantDbContext> options)
        : EFCoreStoreDbContext<InteractiveTenantInfo>(options)
    {
        public DbSet<UserTenantMapping> UserTenantMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InteractiveTenantInfo>()
                .ToTable("Tenants", "Multitenancy");

            // Configuração da tabela de mapeamento usuário-tenant
            modelBuilder.Entity<UserTenantMapping>(entity =>
            {
                entity.ToTable("UserTenantMappings", "Multitenancy");
                
                entity.HasKey(m => m.Email);
                
                entity.Property(m => m.Email)
                    .HasMaxLength(256)
                    .IsRequired();
                
                entity.Property(m => m.TenantId)
                    .HasMaxLength(64)
                    .IsRequired();
                
                entity.Property(m => m.IsActive)
                    .HasDefaultValue(true);
                
                entity.Property(m => m.CreatedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("now() at time zone 'utc'");
                
                // Índices otimizados para máxima performance
                entity.HasIndex(m => m.Email)
                    .IsUnique(true) // Email único - performance máxima
                    .HasDatabaseName("IX_UserTenantMappings_Email_Unique");
                
                entity.HasIndex(m => new { m.Email, m.IsActive })
                    .IsUnique(false)
                    .HasDatabaseName("IX_UserTenantMappings_Email_IsActive");
            });
        }
    }
}
