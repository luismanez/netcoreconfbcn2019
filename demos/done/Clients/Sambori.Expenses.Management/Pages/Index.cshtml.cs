using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sambori.Expenses.Management.Extensions;

namespace Sambori.Expenses.Management.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITokenAcquisition _tokenAcquisition;

        public string AccessToken { get; set; }

        public IndexModel(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task OnGet()
        {
            var scopes = new[]
            {
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Manage.All",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read.All",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Approve",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read"
            };

            AccessToken = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, User, scopes);
        }
    }
}
