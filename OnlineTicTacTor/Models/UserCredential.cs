using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineTicTacTor.Models
{
    /// <summary>
    /// The user credential model.
    /// </summary>
    public class UserCredential
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        // Stores a list of connection session.
        public List<ConnectionSession> Sessions { get; set; }

        /// <summary>
        /// Gets the session length in ticks.
        /// </summary>
        /// <returns></returns>
        public long GetSessionLengthInTicks()
        {
            long totalSession = 0;
            foreach (var session in Sessions)
            {
                if (session.DisconnectedTime != 0)
                {
                    totalSession += session.DisconnectedTime - session.ConnectedTime;
                }
                else
                {
                    totalSession += DateTime.Now.Ticks - session.ConnectedTime;
                }
            }
            return totalSession;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCredential"/> class.
        /// </summary>
        public UserCredential()
        {
            Sessions = new List<ConnectionSession>();
        }
    }
}