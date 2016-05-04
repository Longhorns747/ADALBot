﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace TestBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            AuthenticationResult ar;

            //Logout functionality
            if (message.Text.Equals("Logout"))
            {
                var client = new ConnectorClient();
                var getData = await client.Bots.GetUserDataAsync(Constants.botId, message.From.Id);
                getData.Data = "";
                var foo = await client.Bots.SetUserDataAsync(Constants.botId, message.From.Id, getData);
                return message.CreateReplyMessage("Logged out");
            }

            //Try to authenticate
            try
            {
                string d = (string)message.BotUserData;
                ar = AuthenticationResult.Deserialize(d);
                AuthenticationContext ac = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
                ar = DateTimeOffset.Compare(DateTimeOffset.Now, ar.ExpiresOn) < 0 ? ar : await ac.AcquireTokenByRefreshTokenAsync(ar.RefreshToken, new ClientCredential(Constants.ADClientId, Constants.ADClientSecret));
            }
            catch (Exception ex)
            {
                return message.CreateReplyMessage($"You must authenticate to use bot: https://testbot747.azurewebsites.net/api/{message.From.Id}/login");
            }

            //Handle general messages
            if (message.Type == "Message")
            {
                //lol
                if(message.Text.Equals("Say hi to Ahmed"))
                {
                    return message.CreateReplyMessage($"Hi Ahmed!");
                }

                // return our reply to the user
                return message.CreateReplyMessage(await GetUserInfoAsync(ar.AccessToken));
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }

        //Retrieve user info from ADAL
        private async Task<String> GetUserInfoAsync(string accessToken)
        {
            String name = "";
            String address = "";
            String raw = "";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                            name = json?["displayName"]?.ToString();
                            address = json?["mail"]?.ToString().Trim().Replace(" ", string.Empty);
                        }
                        else
                        {
                            raw = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }

            //return accessToken + " " + raw;
            return "Name: " + name + " Address: " + address;
        }
    }
}