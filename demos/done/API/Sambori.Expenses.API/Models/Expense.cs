using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sambori.Expenses.API.Models
{
    public enum ExpenseStatus
    {
        Pending,
        Approved,
        Declined
    }

    public class Expense
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public int Amount { get; set; }
        public string User { get; set; }
        public ExpenseStatus Status { get; set; }

        public Expense()
        {
            Id = Guid.NewGuid();
            Amount = 0;
            Subject = "...";
            User = "anonymous";
            Status = ExpenseStatus.Pending;
        }

        public override string ToString()
        {
            return $"[{Id}] - {Subject} ->{Amount} EUR by {User} Status: {Status.ToString()}";
        }
    }
}
