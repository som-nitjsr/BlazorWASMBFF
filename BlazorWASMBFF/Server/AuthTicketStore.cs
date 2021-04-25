using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
//<copyright file = "Framework" company="MVS GURUS LLC">
//Copyright (c) 2020 All Rights Reserved
//<author>Pardha Jasti</author>
//</copyright>
namespace BlazorWASMBFF.Server
{
    public class AuthTicketStore : ITicketStore
    {
        static readonly ConcurrentDictionary<string, AuthenticationTicket> store = new ConcurrentDictionary<string, AuthenticationTicket>(); 
        public Task RemoveAsync(string key)
        {
            store.Remove(key, out AuthenticationTicket authenticationTicket);
           return  Task.CompletedTask;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            store.AddOrUpdate(key, ticket,(k,ti)=>ticket);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
           return Task.FromResult(store.GetValueOrDefault(key));

        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Guid.NewGuid().ToString();
            await RenewAsync(key, ticket);
            return key;
        }
    }
}
