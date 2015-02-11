namespace TicTacToe.Data
{
    using System.Data.Entity;

    using Microsoft.AspNet.Identity.EntityFramework;

    using TicTacToe.Common;
    using TicTacToe.Data.Contracts;
    using TicTacToe.Data.Migrations;
    using TicTacToe.Models;

    public class TicTacToeDbContext : IdentityDbContext<User>, ITicTacToeDbContext
    {
        public TicTacToeDbContext()
            : base(ConnectionStrings.DefaultConnection)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TicTacToeDbContext, Configuration>());
        }

        public IDbSet<Game> Games { get; set; }

        public IDbSet<Score> Scores { get; set; }

        public new IDbSet<T> Set<T>() where T : class
        {
            return this.Set<T>();
        }

        void IDbContext.SaveChanges()
        {
            this.SaveChanges();
        }

        public static TicTacToeDbContext Create()
        {
            return new TicTacToeDbContext();
        }
    }
}