using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sambori.Expenses.API.Models;

namespace Sambori.Expenses.API.Data
{
    public class ExpensesContext: DbContext
    {
        public ExpensesContext(DbContextOptions<ExpensesContext> options)
            : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>().HasData(
                new Expense
                {
                    Id = Guid.Parse("6bcc8f00-3488-4897-8a48-c55d3855b98d"),
                    Amount = 100,
                    Subject = "NetCoreConf hotel",
                    User = "mjwatson@inheritscloud.onmicrosoft.com"
                },
                new Expense
                {
                    Id = Guid.Parse("7e97191f-f726-4381-ad82-c6b48411fdef"),
                    Amount = 25,
                    Subject = "NetCoreConf dinner",
                    User = "mjwatson@inheritscloud.onmicrosoft.com"
                },
                new Expense
                {
                    Id = Guid.Parse("eeeb22b4-c3b8-45a1-bfe0-ae41498b3517"),
                    Amount = 2500,
                    Subject = "Flight to Seattle",
                    User = "nosborn@inheritscloud.onmicrosoft.com"
                },
                new Expense
                {
                    Id = Guid.Parse("0da52742-524a-48c6-a45e-6ad9dbd236bd"),
                    Amount = -10,
                    Subject = "Visit to Contoso offices",
                    User = "fhardy@inheritscloud.onmicrosoft.com"
                });
        }
    }
}
