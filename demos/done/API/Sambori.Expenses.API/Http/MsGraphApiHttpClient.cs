using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

            var token = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                _httpContextAccessor.HttpContext,
                scopes);

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

            var response = await _httpClient.PostAsJsonAsync("/v1.0/me/sendMail", sendMailRequestMessage);
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
