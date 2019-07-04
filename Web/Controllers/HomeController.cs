using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Privacy()
        {
            ViewBag.accessToken = await HttpContext.GetTokenAsync("access_token");
            ViewBag.refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            var authority = "https://localhost:5001";

            // discover endpoints from metadata
            //var discoveryClient = new DiscoveryClient(authority);
            //var metadataResponse = await discoveryClient.GetAsync();

            //var tokenClient = new TokenClient(metadataResponse.TokenEndpoint, "mvc", "secret");

            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            //var tokenResult = await tokenClient.RequestRefreshTokenAsync(refreshToken);

            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(authority);
            var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "mvc",
                ClientSecret = "secret",

                RefreshToken = refreshToken
            });


            if (!response.IsError)
            {
                var updatedTokens = new List<AuthenticationToken>();
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = response.IdentityToken
                });

                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = response.AccessToken
                });

                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = response.RefreshToken
                });

                var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                });

                var authResult = await HttpContext.AuthenticateAsync("Cookies");

                authResult.Properties.StoreTokens(updatedTokens);

                await HttpContext.SignInAsync("Cookies", authResult.Principal, authResult.Properties);
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
