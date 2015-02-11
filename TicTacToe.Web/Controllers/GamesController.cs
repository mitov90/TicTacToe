﻿namespace TicTacToe.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Cors;

    using TicTacToe.Data.Contracts;
    using TicTacToe.GameLogic.Contracts;
    using TicTacToe.GameLogic.Enum;
    using TicTacToe.Models;
    using TicTacToe.Web.DataModels;
    using TicTacToe.Web.Infrastructure;

    [EnableCors("*", "*", "*")]
    public class GamesController : BaseApiController
    {
        private readonly IGameResultValidator resultValidator;

        private readonly IUserInfoProvider userInfoProvider;

        public GamesController(
            ITicTacToeData ticTacToeData, 
            IGameResultValidator resultValidator, 
            IUserInfoProvider userIdProvider)
            : base(ticTacToeData)
        {
            this.resultValidator = resultValidator;
            this.userInfoProvider = userIdProvider;
        }

        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> MyGames()
        {
            var userId = this.userInfoProvider.GetUserId();
            var games =
                this.Data.Games.All()
                    .Where(
                        g =>
                        (g.FirstPlayerId == userId
                         || (g.SecondPlayerId == userId && g.State != GameState.TurnO && g.State != GameState.TurnX
                             && g.State != GameState.WaitingForSecondPlayer)))
                    .Select(ListGameDataModel.FromGame);
            return games;
        }

        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> JoinedGames()
        {
            var userId = this.userInfoProvider.GetUserId();
            var games =
                this.Data.Games.All()
                    .Where(
                        g => g.SecondPlayerId == userId && (g.State == GameState.TurnO || g.State == GameState.TurnX))
                    .Select(ListGameDataModel.FromGame);
            return games;
        }

        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> AvailableGames()
        {
            var userId = this.userInfoProvider.GetUserId();
            var games =
                this.Data.Games.All()
                    .Where(g => g.State == GameState.WaitingForSecondPlayer && g.FirstPlayerId != userId)
                    .OrderByDescending(g => g.DateCreated)
                    .Select(ListGameDataModel.FromGame);
            return games;
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(CreateGameDataModel gameModel)
        {
            var currentUserId = this.userInfoProvider.GetUserId();

            var newGame = new Game { Name = gameModel.Name, FirstPlayerId = currentUserId, DateCreated = DateTime.Now };

            this.Data.Games.Add(newGame);
            this.Data.SaveChanges();

            return this.Ok(newGame.GameId);
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult Join(GameParamModel gameModel)
        {
            var currentUserId = this.userInfoProvider.GetUserId();

            var game =
                this.Data.Games.All()
                    .Where(
                        g =>
                        g.GameId.ToString() == gameModel.GameId && g.State == GameState.WaitingForSecondPlayer
                        && g.FirstPlayerId != currentUserId)
                    .FirstOrDefault();

            if (game == null)
            {
                return this.NotFound();
            }

            game.SecondPlayerId = currentUserId;
            game.State = GameState.TurnX;
            this.Data.SaveChanges();

            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult Status(GameParamModel gameModel)
        {
            var currentUserId = this.userInfoProvider.GetUserId();
            var idAsGuid = new Guid(gameModel.GameId);

            var game =
                this.Data.Games.All()
                    .Where(x => x.GameId == idAsGuid)
                    .Select(x => new { x.FirstPlayerId, x.SecondPlayerId })
                    .FirstOrDefault();

            if (game == null)
            {
                return this.NotFound();
            }

            if (game.FirstPlayerId != currentUserId && game.SecondPlayerId != currentUserId)
            {
                return this.BadRequest("This is not your game!");
            }

            var gameInfo =
                this.Data.Games.All()
                    .Where(g => g.GameId == idAsGuid)
                    .Select(
                        g =>
                        new GameInfoDataModel
                            {
                                Board = g.Board, 
                                Id = g.GameId, 
                                State = g.State, 
                                FirstPlayerName = g.FirstPlayer.UserName, 
                                SecondPlayerName = g.SecondPlayer.UserName
                            })
                    .FirstOrDefault();

            return this.Ok(gameInfo);
        }

        /// <summary>
        /// The play.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="IHttpActionResult"/>.
        /// </returns>
        [HttpPost]
        [Authorize]
        public IHttpActionResult Play(PlayRequestDataModel request)
        {
            var currentUserId = this.userInfoProvider.GetUserId();

            if (request == null || !this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var idAsGuid = new Guid(request.GameId);

            var game = this.Data.Games.Find(idAsGuid);
            if (game == null)
            {
                return this.BadRequest("Invalid game id!");
            }

            if (game.FirstPlayerId != currentUserId && game.SecondPlayerId != currentUserId)
            {
                return this.BadRequest("This is not your game!");
            }

            if (game.State != GameState.TurnX && game.State != GameState.TurnO)
            {
                return this.BadRequest("Invalid game state!");
            }

            if ((game.State == GameState.TurnX && game.FirstPlayerId != currentUserId)
                || (game.State == GameState.TurnO && game.SecondPlayerId != currentUserId))
            {
                return this.BadRequest("It's not your turn!");
            }

            var positionIndex = ((request.Row - 1) * 3) + (request.Col - 1);
            if (game.Board[positionIndex] != '-')
            {
                return this.BadRequest("Invalid position!");
            }

            // Update games state and board
            var boardAsStringBuilder = new StringBuilder(game.Board);
            boardAsStringBuilder[positionIndex] = game.State == GameState.TurnX ? 'X' : 'O';
            game.Board = boardAsStringBuilder.ToString();
            game.State = game.State == GameState.TurnX ? GameState.TurnO : GameState.TurnX;
            this.Data.SaveChanges();

            var gameResult = this.resultValidator.GetResult(game.Board);
            this.ChangeGameState(gameResult, game);

            return this.Ok();
        }

        private void ChangeGameState(GameResult gameResult, Game game)
        {
            switch (gameResult)
            {
                case GameResult.WonByX:
                    {
                        game.State = GameState.WonByX;
                        this.AddToScore(game, game.FirstPlayerId, true);
                        this.AddToScore(game, game.SecondPlayerId, false);

                        this.Data.SaveChanges();
                        break;
                    }

                case GameResult.WonByO:
                    {
                        game.State = GameState.WonByO;
                        this.AddToScore(game, game.FirstPlayerId, false);
                        this.AddToScore(game, game.SecondPlayerId, true);

                        this.Data.SaveChanges();
                        break;
                    }

                case GameResult.Draw:
                    {
                        game.State = GameState.Draw;
                        this.Data.SaveChanges();
                        break;
                    }
            }
        }

        private void AddToScore(Game game, string playerId, bool isWin)
        {
            this.Data.Scores.Add(new Score { GameId = game.GameId, PlayerId = Guid.Parse(playerId), IsWin = isWin });
        }
    }
}