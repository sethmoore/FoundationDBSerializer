using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FoundationDbSerializer;
using System.Net.Mail;
using System.Collections.Generic;

namespace SerializerTest
{
    [TestClass]
    public class SerializerTesting
    {
        [TestMethod]
        public void TestRoundTrip()
        {
            TestRoundTripAsync();
        }

        [TestMethod]
        public void TestConvertToKeyValue()
        {
            
        }

        private async void TestRoundTripAsync()
        {
            Dictionary<int, User> dbUsers = new Dictionary<int, User>();
            dbUsers.Add(1, new User() { ID = 1, First = "Justin", Last = "Jones", Email = "justin@jones.com" });
            dbUsers.Add(2, new User() { ID = 2, First = "Jerry", Last = "Jones", Email = "jerry@jones.com" });
            dbUsers.Add(3, new User() { ID = 3, First = "Jimmy", Last = "Jones", Email = "jimmy@jones.com" });

            Serializer.Write(dbUsers.Values);

            List<User> users = new List<User>();
            for (int i = 1; i <= 3; i++)
            {
                var user = await Serializer.Read<User>(i.ToString());
                Assert.IsTrue(dbUsers[user.ID].Equals(user));
            }
        }
    }

    class User
    {
        [Key]
        public int ID { get; set; }

        public string First;
        public string Last;
        public string Middle;

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                var temp = new MailAddress(value);
                _email = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} {2} {3} ({4})", ID, First, Middle, Last, Email);
        }

        public bool Equals(User user)
        {
            return First == user.First && Last == user.Last && Middle == user.Middle && ID == user.ID && Email == user.Email;
        }
    }
}
