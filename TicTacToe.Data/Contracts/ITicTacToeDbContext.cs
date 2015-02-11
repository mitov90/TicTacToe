namespace TicTacToe.Data.Contracts
{
    using System.Data.Entity;

    using TicTacToe.Models;

    public interface ITicTacToeDbContext : IDbContext
    {
        IDbSet<Game> Games { get; set; }

        IDbSet<User> Users { get; set; }

        IDbSet<Score> Scores { get; set; }
    }
}