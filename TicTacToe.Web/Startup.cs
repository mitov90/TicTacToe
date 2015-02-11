using Microsoft.Owin;

using TicTacToe.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace TicTacToe.Web
{
    using System.Reflection;
    using System.Web.Http;

    using Data;
    using Data.Contracts;
    using Data.UnitOfWork;

    using GameLogic;
    using GameLogic.Contracts;

    using Infrastructure;

    using Ninject;
    using Ninject.Web.Common.OwinHost;
    using Ninject.Web.WebApi.OwinHost;

    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
            app.UseNinjectMiddleware(CreateKernel).UseNinjectWebApi(GlobalConfiguration.Configuration);
        }

        private static StandardKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            RegisterMappings(kernel);
            return kernel;
        }

        private static void RegisterMappings(StandardKernel kernel)
        {
            kernel.Bind<ITicTacToeData>()
                .To<TicTacToeData>()
                .WithConstructorArgument("context", c => new TicTacToeDbContext());

            kernel.Bind<IGameResultValidator>().To<GameResultValidator>();
            kernel.Bind<IUserInfoProvider>().To<AspNetUserInfoProvider>();
        }
    }
}