using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;
using FoundationDbSerializer;

namespace foundationDbTest
{
    class User
    {
        [Key]
        public int ID { get; set; }

        public string First;
        public string Last;

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                new MailAddress(value);
                _email = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} {2} ({3})", ID, First, Last, Email);
        }

        public bool Equals(User user)
        {
            return First == user.First && Last == user.Last && ID == user.ID && Email == user.Email;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<User> users = new List<User>();
            users.Add(new User() { ID = 1, First = "Bob", Last = "Smith", Email = "bob@smith.com" });
            users.Add(new User() { ID = 2, First = "Jim", Last = "Smith", Email = "jim@smith.com" });
            users.Add(new User() { ID = 3, First = "Ed", Last = "Smith", Email = "ed@smith.com" });

            Serializer.Write(users);

            Read();

            Console.ReadLine();
        }
        
        static async void Read()
        {
            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine(await Serializer.Read<User>(i.ToString()));
            }
        }
    }
}
