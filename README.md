# IdentityRef
A .NET object for creating ID fields for Domain Driven Design objects or Document Database ID's.
The Id's generated are 16 charactors long so they are much more easy to work with than a GUID.

Identity is a struct that can be used as an identity of a class. It's URL safe, 
case-insensitive, and very short for a global unique identifier at 16 chars. 

Identity uses a 80 bits, 24 bits are date/time and the rest (56 bits) are random.
There are over 72 quadrillion possible values every hour (approximate) 
with over 1 septillion total possible values.

Note: The valid charactors are [0123456789abcdefghjkmnprstuvwxyz] case insensitive.
The missing letters from the alphabet are [oqli]

Example:
	using IdentityRef;

	public class Person
	{
		public Identity Id { get; set; }
		public string FirstName  { get; set; }
		public string LastName  { get; set; }

		public Person()
		{
			Id = Identity.NewIdentity();
		}
	}

	//...

	var user = new Person();
	string message = string.Format("Your ID is: {0}", user.Id);

