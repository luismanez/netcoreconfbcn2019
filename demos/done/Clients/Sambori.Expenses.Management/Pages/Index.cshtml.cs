using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sambori.Expenses.Management.Extensions;
using Sambori.Expenses.Management.Http;

namespace Sambori.Expenses.Management.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly ExpensesApiHttpClient _client;

        public string AccessToken { get; set; }
        public string Data { get; set; }
        public string UserName { get; set; }

        public IndexModel(
            ITokenAcquisition tokenAcquisition, ExpensesApiHttpClient client)
        {
            _tokenAcquisition = tokenAcquisition;
            _client = client;
        }

        public async Task OnGet()
        {
            UserName = User.Identity.Name;

            var data = await _client.GetStringAsync("/api/expenses/my");

            AccessToken = _client.AccessToken;

            Data = JToken.Parse(data).ToString(Formatting.Indented);
        }
    }
}
