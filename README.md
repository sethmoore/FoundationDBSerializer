FoundationDBSerializer
======================

This library allows users to easily serialize objects into FoundationDB's key-value store as documents and retrieve populated objects given the key of the objects.  The partion of the database that's written to is simply the full name (including namespace) of the class and the key is the key of the object that's stored.

The only modifications that are required for the classes being serialized is to have the FoundationDbSerializer.Key attribute defined on one of the properties in the class.

The library currently only serializes primitive and string properties/fields of classes, but in future versions support for nested classes will be supported.

Here's an example:

    class User
    {
        [Key]
        public int ID { get; set; }
        public string Email { get; set; }
        public string First;
        public string Last;
    }

    // Create a collection of users to write out    
    List<User> users = new List<User>();
    users.Add(new User() { ID = 1, First = "Bob", Last = "Smith", Email = "bob@smith.com" });
    users.Add(new User() { ID = 2, First = "Jim", Last = "Smith", Email = "jim@smith.com" });
    users.Add(new User() { ID = 3, First = "Ed", Last = "Smith", Email = "ed@smith.com" });
    
	// Write out the users
    Serializer.Write(users);

	// Read back in the users
    for (int i = 1; i <= 3; i++)
    {
        Console.WriteLine(await Serializer.Read<User>(i.ToString()));
    }
