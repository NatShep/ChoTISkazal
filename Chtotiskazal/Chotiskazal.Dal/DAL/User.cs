using System;
using System.Collections.Generic;

namespace Chotiskazal.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public long? TelegramId { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public bool Online { get; set; }

        public List<Exam> Exams { get; set; }
        public List<UserPair> UsersWords { get; set; }

        public User()
        {
            TelegramId = null;
            Exams = new List<Exam>();
            UsersWords = new List<UserPair>();
            Online = false;
            Created = DateTime.Now;
        }
        
        public User (string name,string login, string password, string email)
        {
            TelegramId = null;
            Name = name;
            Login = login;
            Password = password;
            Email = email;
            Exams = new List<Exam>();
            UsersWords = new List<UserPair>();
            Online = false;
            Created = DateTime.Now;
        }
        
        public User (long telegramId, string name)
        {
            TelegramId = telegramId;
            Name = name;
            Login = null;
            Password = null;
            Email = null;
            Exams = new List<Exam>();
            UsersWords = new List<UserPair>();
            Online = false;
            Created = DateTime.Now;
        }
    }
}
