using IdentityModel.Client;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            Run().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        static async Task Run()
        {
            await Task.Delay(10000);

            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token resource owner flow
            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ro.client",
                ClientSecret = "secret",

                UserName = "alice",
                Password = "password",
                Scope = "Chat"
            });

            // request token client credential flow
            //var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            //{
            //    Address = disco.TokenEndpoint,
            //    ClientId = "netClient",
            //    ClientSecret = "secret",

            //    Scope = "Chat"
            //});

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("https://localhost:5002/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }


            var c = new HubConnectionBuilder()
                .WithUrl("https://localhost:5002/chat", config =>                 {
                    config.AccessTokenProvider = () => Task.FromResult(tokenResponse.AccessToken);
                })
                .Build();

            c.Closed += C_Closed;
            c.On<string>("Echo", m => Console.WriteLine(m));

            await c.StartAsync();

            try
            {
                await c.SendAsync("Echo", "Hallo Echo");
                var e = await c.InvokeAsync<string>("Echo2", "Hallo Echo2");
                Console.WriteLine(e);
            }
            catch (Exception e)
            {

                throw e;
            }



            await Task.Delay(5000);

            await c.StopAsync();
        }

        private static Task C_Closed(Exception arg)
        {
            return Task.CompletedTask;
        }
    }
}
