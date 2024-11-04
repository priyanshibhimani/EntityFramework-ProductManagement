//using Microsoft.EntityFrameworkCore;
//using ProductManagemet.Models;

//namespace ProductManagemet.Context
//{

//        public class AppDbContext : DbContext
//        {
//            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//            {

//            }
//        public DbSet<Product> Products { get; set; }
//        public DbSet<Invoice> Invoices { get; set; }
//        public DbSet<ProductRate> ProductRates { get; set; }
//        public DbSet<Party> Parties { get; set; }
//    }

//}
//using Microsoft.EntityFrameworkCore;
//using ProductManagemet.Models;
//using System;

//namespace ProductManagemet.Context
//{

//    public class AppDbContext : DbContext
//    {
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {

//        }
//        public DbSet<Product> Products { get; set; }
//        public DbSet<Party> Parties { get; set; }
//        public DbSet<PartyWiseProduct> PartyWiseProducts { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<PartyWiseProduct>()
//                .HasOne(pwp => pwp.Party)
//                .WithMany()
//                .HasForeignKey(pwp => pwp.PartyId);

//            modelBuilder.Entity<PartyWiseProduct>()
//                .HasOne(pwp => pwp.Product)
//                .WithMany()
//                .HasForeignKey(pwp => pwp.ProductId);
//        }
//    }
//}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Models;
using ProductsManagementSystem.Models;
namespace ProductManagemet.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Party> Parties { get; set; }

        public DbSet<PartyWiseProduct> PartyWiseProducts { get; set; } // Add this line
        public DbSet<ProductRate> ProductRates { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PartyTotal> PartyTotal { get; set; }
        public DbSet<InvoiceEntry> InvoiceEntry { get; set; }
        //public DbSet<PartyWiseInvoice> PartyWiseInvoices { get; set; }
    
            // Other DbSets and constructor...

            public async Task<List<Invoice>> GetInvoicesByPartyAsync(int partyId, string? searchTerm)
            {
                var partyIdParameter = new SqlParameter("@PartyId", partyId);
                var searchTermParameter = new SqlParameter("@SearchTerm", (object)searchTerm ?? DBNull.Value);

                // Fetch the data from the stored procedure and convert to in-memory collection
                var invoices = await Invoices
                    .FromSqlRaw("EXEC GetInvoicesByParty @PartyId, @SearchTerm", partyIdParameter, searchTermParameter)
                    .ToListAsync(); // Load into memory

                // Optionally, load related entities for each invoice
                foreach (var invoice in invoices)
                {
                    Entry(invoice).Reference(i => i.Parties).Load();
                    Entry(invoice).Reference(i => i.Product).Load();
                }

                return invoices;
            }
        



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(iul => new { iul.LoginProvider, iul.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(iur => new { iur.UserId, iur.RoleId });
            modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(iut => new { iut.UserId, iut.LoginProvider, iut.Name });
            //modelBuilder.Entity<PartyWiseProduct>()
            //    .Property(pwp => pwp.ProductRate)
            //    .HasColumnType("decimal(18, 2)"); 

            modelBuilder.Entity<PartyWiseProduct>()
                .HasOne(pwp => pwp.Party)
                .WithMany()
                .HasForeignKey(pwp => pwp.PartyId);

            modelBuilder.Entity<PartyWiseProduct>()
                .HasOne(pwp => pwp.Product)
                .WithMany()
                .HasForeignKey(pwp => pwp.ProductId);

            modelBuilder.Entity<ProductRate>()
                .HasOne(pr => pr.Product) // Specify the navigation property
                .WithMany() // Optional: can specify a navigation property in Product for related ProductRates
                .HasForeignKey(pr => pr.ProductId) // Specify the foreign key property
                .OnDelete(DeleteBehavior.Cascade); // Optional: specify delete behavior

            modelBuilder.Entity<Invoice>()
           .HasOne(pwp => pwp.Parties)
           .WithMany()
           .HasForeignKey(pwp => pwp.PartyId);

            modelBuilder.Entity<Invoice>()
                .HasOne(pwp => pwp.Product)
                .WithMany()
                .HasForeignKey(pwp => pwp.ProductId);
            modelBuilder.Entity<InvoiceEntry>()
.HasOne(pwp => pwp.Parties)
.WithMany()
.HasForeignKey(pwp => pwp.PartyId);

            modelBuilder.Entity<InvoiceEntry>()
                .HasOne(pwp => pwp.Product)
                .WithMany()
                .HasForeignKey(pwp => pwp.ProductId);
            modelBuilder.Entity<PartyTotal>()
            .HasOne(pwp => pwp.Party)
            .WithMany()
            .HasForeignKey(pwp => pwp.PartyId);
            //       modelBuilder.Entity<PartyWiseInvoice>()
            // .HasOne(pwp => pwp.Parties)
            // .WithMany()
            // .HasForeignKey(pwp => pwp.PartyId);
            //       modelBuilder.Entity<PartyWiseInvoice>()
            //.HasOne(pwp => pwp.Invoices)
            //.WithMany()
            //.HasForeignKey(pwp => pwp.InvoiceId);

        }
    }

}
