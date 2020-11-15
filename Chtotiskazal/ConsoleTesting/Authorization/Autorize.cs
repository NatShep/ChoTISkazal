using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTesting.Authorization
{
    public static class Autorize
    {
        internal static User CreateNewUser(UserService userService)
        {
            Console.Write("Enter your name: ");
            var name = Console.ReadLine();
            Console.WriteLine("Enter login: ");
            var login = Console.ReadLine();
            Console.WriteLine("Enter password: ");
            var password = Console.ReadLine();
            Console.WriteLine("Enter email: ");
            var email = Console.ReadLine();

            var user = new User(name, login, password, email);
            userService.AddUser(user);
            return user;
        }

        internal static User LoginUser(UserService userService)
        {
            Console.WriteLine("Enter login: ");
            var login = Console.ReadLine();
            Console.WriteLine("Enter password: ");
            var password = Console.ReadLine();

            var user = userService.GetUserByLoginOrNull(login);
            if (user?.Password == password)
            {
                return user;
            }
            else
                return null;
        }
    }
}
