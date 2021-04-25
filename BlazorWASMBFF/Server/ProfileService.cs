using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWASMBFF.Server
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
           
            var name = context.Subject.Claims.First(a => a.Type == "name")?.Value;
           
                context.IssuedClaims = new List<System.Security.Claims.Claim> { 
                    new System.Security.Claims.Claim("name",name)
                };
           
            return Task.CompletedTask;
        }

        public  Task IsActiveAsync(IsActiveContext context)
        {
            
            context.IsActive = context.Subject.Identity.IsAuthenticated;
            return Task.CompletedTask;
        }
    }
}
