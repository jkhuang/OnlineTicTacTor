
namespace OnlineTicTacTor.Models
{
    /// <summary>
    /// The connection session model.
    /// </summary>
    public class ConnectionSession
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public long ConnectedTime { get; set; }
        public long DisconnectedTime { get; set; }
    }
}