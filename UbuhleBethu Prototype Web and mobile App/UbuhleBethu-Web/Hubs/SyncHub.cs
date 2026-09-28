using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace UbuhlebethuConnectPro.Web.Hubs
{
    public class SyncHub : Hub
    {
        // Clients can invoke this if needed, or backend can broadcast via IHubContext<SyncHub>
        public async Task SendUpdate(string entityType, string message)
        {
            await Clients.All.SendAsync("ReceiveUpdate", entityType, message);
        }
    }
}
