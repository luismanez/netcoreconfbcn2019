using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sambori.Expenses.API.Data;
using Sambori.Expenses.API.Models;

namespace Sambori.Expenses.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly ExpensesContext _context;

        public ExpensesController(ExpensesContext context)
        {
            _context = context;

            //force seeding Db
            _context.Database.EnsureCreated();
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            var data = $"Expenses Controller API v1.0. Now is: {DateTime.Now}";

            return Ok(data);
        }

        [HttpGet("all")]
        [Authorize("AdminsOnly")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Expenses.ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Expenses.FindAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            var upn = User.FindFirst(ClaimTypes.Upn)?.Value;

            if (User.HasClaim(ClaimTypes.Role, "Admin") 
                || User.HasClaim(ClaimTypes.Role, "Approver")
                || data.User == upn)
            {
                return Ok(data);
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Expense expense)
        {
            var upn = User.FindFirst(ClaimTypes.Upn)?.Value;
            expense.User = upn;

            await _context.Expenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new { id = expense.Id }, expense);
        }

        [HttpGet("{id:guid}/approve")]
        [Authorize("ApproversOnly")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var data = await _context.Expenses.FindAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            data.Status = ExpenseStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(data);
        }

        [HttpGet("{id:guid}/decline")]
        [Authorize("ApproversOnly")]
        public async Task<IActionResult> Decline(Guid id)
        {
            var data = await _context.Expenses.FindAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            data.Status = ExpenseStatus.Declined;

            await _context.SaveChangesAsync();

            return Ok(data);
        }

        [HttpDelete("{id:guid}")]
        [Authorize("AdminsOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.Expenses.FindAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(data);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}