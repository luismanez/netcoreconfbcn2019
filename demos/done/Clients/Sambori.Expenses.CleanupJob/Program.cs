using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Sambori.Expenses.CleanupJob.Models;

namespace Sambori.Expenses.CleanupJob
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            var config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            var clientCredentials = new ClientCredential(config.ClientSecret);

            var app = new ConfidentialClientApplication(
                config.ClientId, 
                config.Authority, 
                "https://NOT_APPLY_AS_IS_DAEMON", 
                clientCredentials, 
                null, 
                new TokenCache());

            var scopes = new[] {"https://inheritscloud.com/Sambori.Expenses.API/.default"};

            try
            {
                var authenticationResult = await app.AcquireTokenForClientAsync(scopes);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

                var json = await httpClient.GetStringAsync("https://localhost:44382/api/expenses/all");

                var expenses = JsonConvert.DeserializeObject<IEnumerable<Expense>>(json);

                foreach (var expense in expenses)
                {
                    Console.WriteLine(expense);
                    if (expense.Amount < 0)
                    {
                        Console.Write("DELETING...{0}...", expense);
                        var response = await httpClient.DeleteAsync($"https://localhost:44382/api/expenses/{expense.Id}");
                        Console.WriteLine(response.StatusCode);
                    }
                }                
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
            }
        }
    }
}
