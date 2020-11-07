using System;
using System.Collections.Generic;

namespace Chotiskazal.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public int Online { get; set; }

        public List<Exam> Exams { get; set; }
        public List<UserPair> UsersPairs { get; set; }

        public User() {
            Exams = new List<Exam>();
            UsersPairs = new List<UserPair>();
            Online = 0;
            Created = DateTime.Now;
        }
        
        public User (string name,string login, string password, string email)
        {
            Name = name;
            Login = login;
            Password = password;
            Email = email;
            Exams = new List<Exam>();
            UsersPairs = new List<UserPair>();
            Online = 0;
            Created = DateTime.Now;
        }
    }
}
