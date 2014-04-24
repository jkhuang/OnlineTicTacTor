using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineTicTacTor.Models
{
    /// <summary>
    ///  A manager of games (actions to create games) 
    /// </summary>
    public class Manager
    {
        // The single object.
        private static readonly Manager _instance = new Manager();
        private Dictionary<string, UserCredential> _connections;
        private Dictionary<Guid, GameDetails> _games;

        /// <summary>
        /// Prevents a default instance of the class from being created.
        /// </summary>
        static Manager()
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Manager"/> class from being created.
        /// </summary>
        private Manager()
        {
            _connections = new Dictionary<string, UserCredential>();
            _games = new Dictionary<Guid, GameDetails>();
        }

        public static Manager Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// When the challenge started, create a game instance.
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <returns>a game instance</returns>
        public GameDetails Game(Guid gameId)
        {
            if (!_games.ContainsKey(gameId))
            {
                _games[gameId] = new GameDetails { GameId = gameId };
            }
            return _games[gameId];
        }

        /// <summary>
        /// Gets all users in the connection.
        /// </summary>
        /// <returns></returns>
        public object AllUsers()
        {
            var u = _connections.Values.Select(s => new
            {
                UserId = s.UserId,
                ConnectionStatus = (int)s.ConnectionStatus,
                ConnectionId = s.Sessions[s.Sessions.Count - 1].ConnectionId
            });
            return u;
        }

        /// <summary>
        /// Creates the new user session.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        private void CreateNewUserSession(string userId, string connectionId)
        {
            UserCredential curCred = new UserCredential
                {
                    ConnectionStatus = ConnectionStatus.Connected,
                    UserId = userId
                };

            curCred.Sessions.Add(new ConnectionSession
                {
                    ConnectionId = connectionId,
                    ConnectedTime = DateTime.Now.Ticks,
                    DisconnectedTime = 0L
                });

            _connections.Add(userId, curCred);

        }


        /// <summary>
        /// Updates the user session.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="status">The status.</param>
        private void UpdateUserSession(string userId, string connectionId, ConnectionStatus status)
        {
            UserCredential curCred = _connections[userId];
            ExpireSession(curCred);
            curCred.Sessions.Add(new ConnectionSession
            {
                // The connection ID of the calling client.
                ConnectionId = connectionId,
                ConnectedTime = DateTime.Now.Ticks,
                DisconnectedTime = 0L
            });
            curCred.ConnectionStatus = status;
        }

        /// <summary>
        /// Expires the session.
        /// </summary>
        /// <param name="curCred">The current cred.</param>
        private static void ExpireSession(UserCredential curCred)
        {
            var curSession = curCred.Sessions.Find
                (s => s.DisconnectedTime == 0);

            if (curSession != null)
            {
                curSession.DisconnectedTime = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Updates the cache.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        internal GameDetails UpdateCache(string userId, string connectionId, ConnectionStatus status)
        {
            if (!string.IsNullOrWhiteSpace(userId) && _connections.ContainsKey(userId))
            {
                UpdateUserSession(userId, connectionId, status);
            }
            else
            {
                CreateNewUserSession(userId, connectionId);
            }

            var gd = _games.Values.LastOrDefault<GameDetails>(g => g.User1Id.UserId == userId || g.User2Id.UserId == userId);
            return gd;
        }

        /// <summary>
        /// Disconnects the specified connection identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        internal void Disconnect(string connectionId)
        {
            ConnectionSession session = null;

            if (_connections.Values.Count > 0)
            {
                foreach (var userCredential in _connections.Values)
                {
                    session = userCredential.Sessions.Find(s => s.ConnectionId == connectionId);
                    if (session != null)
                    {
                        session.DisconnectedTime = DateTime.Now.Ticks;
                        break;
                    }
                }
            }
        }

        internal void Logout(string userId)
        {
            ExpireSession(_connections[userId]);

            // Removes the connection.
            _connections.Remove(userId);
        }

        /// <summary>
        /// News the game.
        /// </summary>
        /// <param name="playerAId">The player a identifier.</param>
        /// <param name="playerBId">The player b identifier.</param>
        /// <returns>The GameDetails object</returns>
        internal GameDetails NewGame(string playerAId, string playerBId)
        {
            // Gets the playerA user credential.
            var playerA = _connections.Values.FirstOrDefault<UserCredential>
                (c => c.Sessions.FirstOrDefault<ConnectionSession>
                    (s => s.ConnectionId == playerAId) != null);

            // Gets the playerB user credential.
            var playerB = _connections.Values.FirstOrDefault<UserCredential>
                (c => c.Sessions.FirstOrDefault<ConnectionSession>
                    (s => s.ConnectionId == playerBId) != null);

            // When the game started, created a game instance.
            var newGame = new GameDetails
                {
                    GameId = Guid.NewGuid(),
                    User1Id = playerA,
                    User2Id = playerB,
                    NextTurn = playerA.UserId
                };

            // Stores the game instance into cache.
            _games[newGame.GameId] = newGame;
            return newGame;
        }
    }

}