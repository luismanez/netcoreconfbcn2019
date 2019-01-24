using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sambori.Expenses.CleanupJob.Models
{
    public class Expense
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public int Amount { get; set; }
        public string User { get; set; }

        public override string ToString()
        {
            return $"[{Id}] - {Subject} ->{Amount} EUR by {User}";
        }
    }
}
