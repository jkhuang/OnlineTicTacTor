using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineTicTacTor.Models
{
    /// <summary>
    /// The game details model.
    /// </summary>
    public class GameDetails
    {
        public Guid GameId { get; set; }
        public int[,] GameMatrix { get; set; }
        public string NextTurn { get; set; }
        public string Message { get; set; }
        public Status GameStatus { get; set; }
        public UserCredential User1Id { get; set; }
        public UserCredential User2Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameDetails"/> class.
        /// </summary>
        public GameDetails()
        {
            GameMatrix = new int[3,3];
        }

        /// <summary>
        /// Checks the game status.
        /// </summary>
        /// <returns></returns>
        private void CheckGameStatus()
        {
            string status = CheckRows();
            if (string.IsNullOrEmpty(status))
            {
                status = CheckCols();
            }
            if (string.IsNullOrEmpty(status))
            {
                status = CheckDiagonal();
            }
            Message = !string.IsNullOrEmpty(status) ? status + " wins!" : string.Empty;
            if (string.IsNullOrEmpty(status))
            {
                status = CheckDraw();
                Message = status;
            }
        }

        /// <summary>
        /// Checks the game is draw or not.
        /// </summary>
        /// <returns></returns>
        private string CheckDraw()
        {
            bool isDefault = false;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (GameMatrix[row, col] == default(int))
                    {
                        isDefault = true;
                        GameStatus = Status.Progress;
                        break;
                    }
                }
                if (isDefault)
                {
                    break;
                }
            }
            if (!isDefault)
            {
                GameStatus = Status.Draw;
            }
            return isDefault ? "In Progress" : "Game Drawn";
        }

        /// <summary>
        /// Sets the player move step.
        /// </summary>
        /// <param name="rowCol">The board cell</param>
        /// <param name="currentPlayerId">The current player identifier.</param>
        /// <returns>The step mark</returns>
        public string SetPlayerMove(dynamic rowCol, string currentPlayerId)
        {
            int x = int.Parse(rowCol.row.ToString());
            int y = int.Parse(rowCol.col.ToString());
            string returnString = string.Empty;

            if (!string.IsNullOrEmpty(currentPlayerId) &&
                GameMatrix[x - 1, y - 1] == default(int))
            {
                if (currentPlayerId == User1Id.UserId)
                {
                    returnString = "O";
                    GameMatrix[x - 1, y - 1] = 1;
                    NextTurn = User2Id.UserId;
                }
                else
                {
                    returnString = "X";
                    GameMatrix[x - 1, y - 1] = 10;
                    NextTurn = User1Id.UserId;
                }
            }
            CheckGameStatus();
            return returnString;
        }

        /// <summary>
        /// Checks the game status rows.
        /// </summary>
        /// <returns></returns>
        protected string CheckRows()
        {
            for (int r = 0; r < 3; r++)
            {
                int value = 0;
                for (int c = 0; c < 3; c++)
                {
                    value += GameMatrix[r, c];
                }
                if (3 == value)
                {
                    GameStatus = Status.Result;
                    return User1Id.UserId;
                }
                else if (30 == value)
                {
                    GameStatus = Status.Result;
                    return User2Id.UserId;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks the game status with cols.
        /// </summary>
        /// <returns></returns>
        protected string CheckCols()
        {
            for (int c = 0; c < 3; c++)
            {
                int value = 0;
                for (int r = 0; r < 3; r++)
                {
                    value += GameMatrix[r, c];
                }
                if (3 == value)
                {
                    GameStatus = Status.Result;
                    return User1Id.UserId;
                }
                else if (30 == value)
                {
                    GameStatus = Status.Result;
                    return User2Id.UserId;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks the game status in diagonal direction.
        /// </summary>
        /// <returns></returns>
        protected string CheckDiagonal()
        {
            int diagValueF = 0;
            int diagValueB = 0;
            for (int positonF = 0, positonB = 2; positonF < 3; positonF++, positonB--)
            {
                diagValueF += GameMatrix[positonF, positonF];
                diagValueB += GameMatrix[positonF, positonB];
            }
            if (diagValueF == 3)
            {
                GameStatus = Status.Result;
                return User1Id.UserId;
            }
            else if (diagValueF == 30)
            {
                GameStatus = Status.Result;
                return User2Id.UserId;
            }
            if (diagValueB == 3)
            {
                GameStatus = Status.Result;
                return User1Id.UserId;
            }
            else if (diagValueB == 30)
            {
                GameStatus = Status.Result;
                return User2Id.UserId;
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// The game status.
    /// </summary>
    public enum Status
    {
        Progress = 0,
        Result,
        Draw
    }

}