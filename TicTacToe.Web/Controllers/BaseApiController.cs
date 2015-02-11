namespace TicTacToe.Web.Controllers
{
    using System.Web.Http;

    using TicTacToe.Data.Contracts;

    public abstract class BaseApiController : ApiController
    {
        protected BaseApiController(ITicTacToeData data)
        {
            this.Data = data;
        }

        protected ITicTacToeData Data { get; set; }
    }
}