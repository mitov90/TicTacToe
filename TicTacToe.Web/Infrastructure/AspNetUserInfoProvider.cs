﻿namespace TicTacToe.Web.Infrastructure
{
    using System.Threading;

    using Microsoft.AspNet.Identity;

    public class AspNetUserInfoProvider : IUserInfoProvider
    {
        public bool IsUserAuthenticated()
        {
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        public string GetUsername()
        {
            return Thread.CurrentPrincipal.Identity.GetUserName();
        }

        public string GetUserId()
        {
            return Thread.CurrentPrincipal.Identity.GetUserId();
        }
    }
}