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

            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return BadRequest();
            }

            // request token
            var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                disco.
                ClientId = "mvc",
                ClientSecret = "secret",
                GrantType= "refresh_token ",

                RefreshToken = ViewBag.refreshToken
            });

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
