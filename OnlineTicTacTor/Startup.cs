using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(OnlineTicTacTor.Startup))]
namespace OnlineTicTacTor
{
    /// <summary>
    /// To define the route that clients will use to connect to your Hub.
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}