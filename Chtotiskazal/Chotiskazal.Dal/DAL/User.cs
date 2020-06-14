using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.DAL
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Login { get; set; }
        public string Passwors { get; set; }
        public string Email { get; set; }

        public DateTime Created { get; set; }
        public bool Online { get; set; }
    }
}
