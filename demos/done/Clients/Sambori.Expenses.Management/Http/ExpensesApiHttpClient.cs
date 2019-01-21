using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sambori.Expenses.Management.Extensions;

namespace Sambori.Expenses.Management.Http
{
    public class ExpensesApiHttpClient
    {
        public HttpClient HttpClient { get; internal set; }
        public string AccessToken { get; set; }
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExpensesApiHttpClient(
            HttpClient client, 
            ITokenAcquisition tokenAcquisition,
            IHttpContextAccessor httpContextAccessor)
        {            
            _tokenAcquisition = tokenAcquisition;
            _httpContextAccessor = httpContextAccessor;

            HttpClient = client;
            HttpClient.BaseAddress = new Uri("https://localhost:44382");
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> GetStringAsync(string requestUri)
        {
            var scopes = new[]
            {
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Manage.All",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read.All",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Approve",
                "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read"
            };

            AccessToken = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                _httpContextAccessor.HttpContext,
                _httpContextAccessor.HttpContext.User, 
                scopes);

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            return await HttpClient.GetStringAsync(requestUri);
        }
    }
}
