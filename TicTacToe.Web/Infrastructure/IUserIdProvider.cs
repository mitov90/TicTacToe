namespace TicTacToe.Web.Infrastructure
{
    public interface IUserInfoProvider
    {
        bool IsUserAuthenticated();

        string GetUsername();

        string GetUserId();
    }
}