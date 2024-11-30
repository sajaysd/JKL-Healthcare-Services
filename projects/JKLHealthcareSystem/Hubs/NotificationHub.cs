using Microsoft.AspNetCore.SignalR;

namespace JKLHealthcareSystem.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
