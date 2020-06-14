using Chotiskazal.Logic.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.App.Autorisation
{
    static class Autorize
    {
        public static int? CreateNewUser(UserRepo userRepo)
        {
            return null;
        }
        public static User LoginUser(UserRepo userRepo)
        {
            //check if User Exist
            //if (user exists) { return User}
            //if (user not exists) { return null}
            return new User();
        }
    }
}
