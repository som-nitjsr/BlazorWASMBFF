
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace BlazorWASMBFF.Server
{
    public class Config
    {
       
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
             //   new Organization()
            };
        }
        public static IEnumerable<ApiScope> GetApiResources()
        {
            return new List<ApiScope>
            {
                 new ApiScope("core", "Core API"),
               
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<IdentityServer4.Models.Client> GetClients()
        {
            // client credentials client
            return new List<IdentityServer4.Models.Client>
            {
            
               
                new IdentityServer4.Models.Client
                {
                    ClientId = "testclient",
                    ClientName = "BlazorWASMBFF",
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireConsent=false,
                    RequireClientSecret=true,

                    ClientSecrets =
                    {
                        new Secret("Secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                      
                        "core",
                        

                    },
                     
                     RedirectUris = {  "https://localhost:44346/signin-oidc" },
                    PostLogoutRedirectUris = {
                    "https://localhost:44346/signout-callback-oidc",
                    },
                    AllowOfflineAccess = true
                },
                  
                
            };

        }
    }
}