using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Server
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Context.Items.Add("ConnectionTime", DateTimeOffset.Now);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public Task Echo(string message)
        {
            Clients.Caller.SendAsync("Echo", message);
            return Task.CompletedTask;
        }

        [Authorize("PerformSurgery")]
        public Task<string> Echo2 (string message)
        {
            return Task.FromResult(message);
        }
    }
}