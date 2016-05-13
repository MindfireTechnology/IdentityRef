using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace IdentityFramework.UnitTests
{
	[TestClass]
	public class IdentityTests
	{
		[TestMethod]
		public void CTorTest()
		{
			Identity id = Identity.NewIdentity();
			Assert.IsNotNull(id);
		}

		[TestMethod]
		public void DefaultValueTest()
		{
			Identity id;
			Assert.AreEqual(Identity.Empty, id);
		}

		[TestMethod]
		public void CompareEmptyToNewTest()
		{
			Identity id1;
			Identity id2 = Identity.NewIdentity();

			Assert.AreNotEqual(id1, id2);
		}

		[TestMethod]
		public void TimedTest()
		{
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < 1000000; i++)
			{
				Identity id = Identity.NewIdentity();
			}
			sw.Stop();

			Trace.WriteLine("Total Time Elapsed: " + sw.ElapsedMilliseconds);
			Assert.IsTrue(sw.Elapsed.TotalSeconds < 2, "Identity generation too slow!");
		}

		[TestMethod]
		public void RoundTripIdentity()
		{
			string ident1 = Identity.NewIdentity();
			string ident2 = new Identity(ident1);
			Assert.AreEqual(ident1, ident2);
		}

		[TestMethod]
		public void EngineeredIdTest()
		{
			Identity id = (Identity)new byte[] { 85, 85, 85, 85, 85, 85, 85, 85, 85, 85 };
			string value = id;

			Assert.AreEqual("anananananananan", id);
		}

		[TestMethod]
		public void ReverseEngineeredIdTest()
		{
			byte[] expected = new byte[] { 85, 85, 85, 85, 85, 85, 85, 85, 85, 85 };

			Identity id = (Identity)"anananananananan";
			byte[] value = id.ToByteArray();

			for (int index = 0; index < 10; index++)
				Assert.AreEqual(expected[index], value[index]);
		}

		[TestMethod]
		public void EmptyIdTest()
		{
			Identity id;
			Assert.AreEqual(Identity.Empty, id);
		}

		[TestMethod]
		public void TryParseFailTest()
		{
			Identity id;
			bool result = Identity.TryParse("this is some bad data.", out id);

			Assert.IsFalse(result);
			Assert.AreEqual(id, Identity.Empty);
		}

		[TestMethod]
		public void TryParseSuccessTest()
		{
			Identity id;
			bool result = Identity.TryParse("ANANANANANANANAN", out id);

			Assert.IsTrue(result);
			Assert.AreEqual(id, "anananananananan");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ParseFailTest()
		{
			Identity id = Identity.Parse("ANANANANANANANANAAA");

			Assert.AreEqual(id, "anananananananan");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ParseFailArgEmptyTest()
		{
			Identity id = Identity.Parse("");

			Assert.AreEqual(id, "anananananananan");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ParseFailArgNullTest()
		{
			Identity id = Identity.Parse((string)null);
		}

		[TestMethod]
		[ExpectedException(typeof(FormatException))]
		public void ParseFailArgFormatTest()
		{
			Identity id = Identity.Parse("anananananan-ilq");
		}

		[TestMethod]
		public void TryParseSuccessByteArrayTest()
		{
			byte[] input = new byte[] { 85, 85, 85, 85, 85, 85, 85, 85, 85, 85 };

			Identity id;
			bool result = Identity.TryParse(input, out id);

			Assert.IsTrue(result);
			Assert.AreEqual(id, "anananananananan");
		}
	}
}
