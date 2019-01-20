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
                    Id = Guid.NewGuid(), Amount = 100, Subject = "NetCoreConf hotel", User = "Luis"
                });
        }
    }
}
