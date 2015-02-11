namespace TicTacToe.Web.DataModels
{
    using System.ComponentModel.DataAnnotations;

    public class GameParamModel
    {
        [Required]
        public string GameId { get; set; }
    }
}