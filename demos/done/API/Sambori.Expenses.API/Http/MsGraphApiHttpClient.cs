using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Sambori.Expenses.API.Http
{
    public class MsGraphApiHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MsGraphApiHttpClient(HttpClient client,
            ITokenAcquisition tokenAcquisition,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = client;
            _tokenAcquisition = tokenAcquisition;

            _httpClient.BaseAddress = new Uri("https://graph.microsoft.com");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task SendEmail(string to, string message)
        {
            string[] scopes = { "Mail.Send" };

            var token = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                _httpContextAccessor.HttpContext,
                scopes);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = string.Format(@"{
                          ""message"": {
                            ""subject"": ""Meet for lunch?"",
                            ""body"": {
                              ""contentType"": ""Text"",
                              ""content"": ""{0}""
                            },
                            ""toRecipients"": [
                              {
                                ""emailAddress"": {
                                  ""address"": ""{1}""
                                }
                              }
                            ]
                          }
                        }", message, to);

            var response = await _httpClient.PostAsJsonAsync<string>("/me/sendMail", json);
        }
    }
}
