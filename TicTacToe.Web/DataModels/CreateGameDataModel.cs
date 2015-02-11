namespace TicTacToe.Web.DataModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateGameDataModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}