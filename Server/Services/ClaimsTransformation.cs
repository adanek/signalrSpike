using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Services
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if(principal.Claims.Any(c => c.Type.Equals("client_id"))){
                principal.AddIdentity(new ClaimsIdentity(new[] { new Claim("sub", "@me") }));
            }

            return Task.FromResult(principal);
        }
    }
}
