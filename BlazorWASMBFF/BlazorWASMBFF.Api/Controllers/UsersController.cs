using BlazorWASMBFF.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWASMBFF.Api.Controllers
{
    public class UsersController : Controller
    {
        [AllowAnonymous]
        [HttpGet("me")]
        public ActionResult<UserProfile> GetUserProfileById()
        {
            // need to put logic based on authentication.

            return new UserProfile()
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                Role = "CustomeRole"

            };

              
        }
    }
}
