namespace TicTacToe.GameLogic.Contracts
{
    using TicTacToe.GameLogic.Enum;

    public interface IGameResultValidator
    {
        GameResult GetResult(string board);
    }
}