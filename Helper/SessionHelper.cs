using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria.Helper
{
    public static class SessionHelper
    {
        public static readonly string UsernameKey = "Session.Username";
        public static bool IsUsernameEmpty(ISession session)
        {
            return string.IsNullOrEmpty(session.GetString(UsernameKey));
        }

        public static void SetUsername(ISession session, string username)
        {
            session.SetString(UsernameKey, username);
        } 

        public static string GetUsername(ISession session)
        {
            return session.GetString(UsernameKey);
        } 
    }
}
