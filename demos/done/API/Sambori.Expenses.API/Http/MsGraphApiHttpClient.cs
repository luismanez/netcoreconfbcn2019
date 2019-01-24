using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

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

            string token;
            string sendMailEndpoint;
            if (_httpContextAccessor.HttpContext.User.FindFirstValue("azp") == "e2b3ebbd-91a3-4bee-ae3a-2667e02f3ba4")
            {
                // As an App, we need an App token for Graph,
                // and a specific User ID to send the email (kind of service account to send the email as that User)
                token = await _tokenAcquisition.GetAccessTokenForApp();
                sendMailEndpoint = "users/23a5c189-32af-45c7-b10f-a6bf1aac7345";
            }
            else
            {
                token = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                    _httpContextAccessor.HttpContext,
                    scopes);

                sendMailEndpoint = "me";
            }            

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var sendMailRequestMessage = new GraphSendMailRequest
            {
                Message = new Message
                {
                    Subject = "Expenses DELETED in system!!",
                    Body = new Body
                    {
                        Content = message,
                        ContentType = "Text"
                    },
                    ToRecipients = new List<ToRecipient>
                    {
                        new ToRecipient
                        {
                            EmailAddress = new EmailAddress { Address = to }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"/v1.0/{sendMailEndpoint}/sendMail", sendMailRequestMessage);
        }
    }

    public class GraphSendMailRequest
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
        public string Subject { get; set; }

        [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
        public Body Body { get; set; }

        [JsonProperty("toRecipients", NullValueHandling = NullValueHandling.Ignore)]
        public List<ToRecipient> ToRecipients { get; set; }
    }

    public class Body
    {
        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }
    }

    public class ToRecipient
    {
        [JsonProperty("emailAddress", NullValueHandling = NullValueHandling.Ignore)]
        public EmailAddress EmailAddress { get; set; }
    }

    public class EmailAddress
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }
    }
}
