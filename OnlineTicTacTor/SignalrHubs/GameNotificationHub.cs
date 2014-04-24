using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OnlineTicTacTor.Models;

namespace OnlineTicTacTor.SignalrHubs
{
    // specifies the hub name for client to use.
    [HubName("gameNotificationHub")]
    [Authorize]
    public class GameNotificationHub : Hub
    {
        /// <summary>
        /// Challenges the specified connection identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void Challenge(string connectionId, string userId)
        {
            // Calls the specified client by connectionId.
            this.Clients.Client(connectionId).getChallengeResponse(Context.ConnectionId, userId);
            // The calling client wait user response.
            this.Clients.Caller.waitForResponse(userId);
        }

        /// <summary>
        /// Acceptes the challenge.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        public void ChallengeAccepted(string connectionId)
        {
            // Creates a game instance.
            var details = Manager.Instance.NewGame(Context.ConnectionId, connectionId);
            // Adds the part a and b in the same group by game id.
            this.Groups.Add(Context.ConnectionId, details.GameId.ToString());
            this.Groups.Add(connectionId, details.GameId.ToString());
            // Starts the game between connection client.
            this.Clients.All.beginGame(details);
        }

        /// <summary>
        /// Refuses the challenge.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        public void ChallengeRefused(string connectionId)
        {
            // Refuses the challenge by connectionId.
            this.Clients.Client(connectionId).challengeRefused();
        }

        /// <summary>
        /// When the game moved, updated both side canvas.
        /// </summary>
        /// <param name="gameGuid">The game unique identifier.</param>
        /// <param name="rowCol">The row col.</param>
        public void GameMove(string gameGuid, dynamic rowCol)
        {
            var game = Manager.Instance.Game(new Guid(gameGuid));
            if (game != null)
            {
                string result = game.SetPlayerMove(rowCol, Context.User.Identity.Name);
                if (!string.IsNullOrEmpty(result))
                {
                    // Calls group to draw the user step.
                    this.Clients.Group(game.GameId.ToString()).drawPlay(rowCol, game, result);
                }
            }
        }

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override System.Threading.Tasks.Task OnConnected()
        {
            string connectionId = Context.ConnectionId;
            string connectionName = string.Empty;

            GameDetails gd = null;

            if (Context.User != null && Context.User.Identity.IsAuthenticated)
            {
                // Retrieves user session in the cache.
                // If not found, create a new one.
                gd = Manager.Instance.UpdateCache(
                    Context.User.Identity.Name,
                    Context.ConnectionId,
                    ConnectionStatus.Connected);
                connectionName = Context.User.Identity.Name;
            }
            if (gd != null && gd.GameStatus == Status.Progress)
            {
                // Creates a group.
                this.Groups.Add(connectionId, gd.GameId.ToString());
                //// No need to update the client by specified id.
                ////this.Clients.Client(connectionId).rejoinGame(Manager.Instance.AllUsers(), connectionName, gd);
                this.Clients.Group(gd.GameId.ToString()).rejoinGame(Manager.Instance.AllUsers(), connectionName, gd);
            }
            else
            {
                // Update the user list in the client.
                this.Clients.Caller.updateSelf(Manager.Instance.AllUsers(), connectionName);
            }
            this.Clients.Others.joined(
                new
                    {
                        UserId = connectionName,
                        ConnectionStatus = (int)ConnectionStatus.Connected,
                        ConnectionId = connectionId
                    },
                    DateTime.Now.ToString());
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            Manager.Instance.Disconnect(Context.ConnectionId);
            return Clients.All.leave(Context.ConnectionId,
                DateTime.Now.ToString());
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            string connectionName = string.Empty;
            if (!string.IsNullOrEmpty(Context.User.Identity.Name))
            {
                Manager.Instance.UpdateCache(
                    Context.User.Identity.Name,
                    Context.ConnectionId,
                    ConnectionStatus.Connected);
                connectionName = Context.User.Identity.Name;
            }
            return Clients.All.rejoined(connectionName);
        }
    }
}