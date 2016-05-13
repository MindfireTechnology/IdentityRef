using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IdentityFramework
{
	/// <summary>
	/// Identity is a struct that can be used as an identity of a class. It's URL safe, 
	/// case-insensitive, and very short for a global unique identifier at 16 chars. 
	/// </summary>
	/// <remarks>
	/// Identity uses a 80 bits, 24 bits are date/time and the rest (56 bits) are random.
	/// There are over 72 quadrillion possible values every hour (approximate) 
	/// with 1 septillion total possible values.
	/// 
	/// Note: The valid charactors are [0123456789abcdefghjkmnprstuvwxyz] case insensitive.
	/// The missing letters from the alphabet are [oqli]
	/// </remarks>
	public struct Identity
	{
		// 32 distinct case-insensitive chars (missing o, q, l, and i) 5 bits each / 16 chars per ID = 80 bits
		private static readonly char[] Chars = "0123456789abcdefghjkmnprstuvwxyz".ToCharArray();
		private static readonly Random Rnd = new Random();
		private readonly byte[] Id;

		public static readonly Identity Empty;

		/// <summary>
		/// Create a new Identity from an existing string
		/// </summary>
		/// <param name="id">identity string</param>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="ArgumentOutOfRangeException" />
		/// <exception cref="FormatException" />
		public Identity(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				throw new ArgumentNullException(nameof(id));
			
			// Make lower case for the rest of the compairisons
			id = id.ToLower();

			if (id.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(id), "Lenth is expected to be 16");

			if (id.Any(n => !Chars.Contains(n)))
				throw new FormatException("Argument '" + nameof(id) + "' contains invalid charactors.");

			Id = ParseIdString(id);
		}

		/// <summary>
		/// Create a new Identity from an existing byte array
		/// </summary>
		/// <param name="id">byte[] id</param>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="ArgumentOutOfRangeException" />
		public Identity(byte[] id)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			if (id.Length != 10)
				throw new ArgumentOutOfRangeException(nameof(id), "Lenth is expected to be 10 bytes");

			Id = id;
		}

		private Identity(bool nothing = true /* unused parameter to allow for a unique type signature */)
		{
			Id = GenerateNewId();
		}

		public byte[] ToByteArray()
		{
			return Id;
		}

		private string ConvertToString(byte[] value)
		{
			var result = new StringBuilder();
			int bitNum = 0;
			int accumulator = 0;
			foreach (bool bit in EnumerateBits(value))
			{
				bitNum++;
				accumulator = (accumulator << 1) + (bit ? 1 : 0);

				if (bitNum % 5 == 0 && bitNum > 0)
				{
					result.Append(Chars[accumulator]);
					accumulator = 0;
				}
			}

			return result.ToString();
		}

		public override string ToString()
		{
			return ConvertToString(Id);
		}

		public override bool Equals(object obj)
		{
			if (obj is Identity)
				return this == (Identity)obj;

			if (obj is string)
				return this == (string)obj;

			if (obj is byte[])
			{
				Identity id;
				if (TryParse((byte[])obj, out id))
					return this == id;
			}

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = 0;
				for (int index = 0; index < 10; index++)
				{
					int val = Id[index];
					result += val << index * 3 + val;
				}

				return result;
			}
		}

		public static Identity NewIdentity()
		{
			Identity result = new Identity(true);
			return result;
		}

		/// <summary>
		/// Parse a string into an Identity
		/// </summary>
		/// <param name="id">string id</param>
		/// <returns>Identity</returns>
		/// <exception cref="ArgumentNullException">Null ARgument</exception>
		/// <exception cref="ArgumentOutOfRangeException" />
		/// <exception cref="FormatException" />
		public static Identity Parse(string id)
		{
			return new Identity(id);
		}

		/// <summary>
		/// Parse a byte array into an Identity
		/// </summary>
		/// <param name="id">byte array</param>
		/// <returns>Identity</returns>
		public static Identity Parse(byte[] id)
		{
			return new Identity(id);
		}

		public static bool TryParse(byte[] id, out Identity result)
		{
			if (id == null || id.Length != 10)
			{
				result = Identity.Empty;
				return false;
			}

			result = new Identity(id);
			return true;
		}

		public static bool TryParse(string id, out Identity result)
		{
			id = id.ToLower();
			if (string.IsNullOrWhiteSpace(id) || id.Length != 16 || id.Any(n => !Chars.Contains(n)))
			{
				result = Identity.Empty;
				return false;
			}

			result = new Identity(id);
			return true;
		}

		public static implicit operator string(Identity id)
		{
			return id.ToString();
		}

		public static explicit operator Identity(string id)
		{
			return new Identity(id);
		}

		public static implicit operator byte[] (Identity id)
		{
			return id.Id;
		}

		public static explicit operator Identity(byte[] id)
		{
			return new Identity(id);
		}

		public static bool operator ==(Identity a, Identity b)
		{
			if (a.Id == null)
			{
				if (b.Id == null)
					return true;

				return false;
			}
			else if (b.Id != null)
			{
				if (a.Id == null && b.Id == null)
					return true;

				if (a.Id == null && b.Id != null)
					return false;

				for (int i = 0; i < 10; i++)
					if (a.Id[i] != b.Id[i])
						return false;

				return true;
			}
			else
				return false;
		}

		public static bool operator !=(Identity a, Identity b)
		{
			return !(a == b);
		}

		private static byte[] GenerateNewId()
		{
			// 24 Bits Date/Time
			byte[] result = new byte[10];
			byte[] binDateTime = BitConverter.GetBytes(DateTime.Now.ToBinary());

			// Use the most significant bits of the DateStamp
			result[0] = binDateTime[7];
			result[1] = binDateTime[6];
			result[2] = binDateTime[5];

			// Generate the Random portion (56 bits)
			byte[] randomBytes = new byte[7];
			Rnd.NextBytes(randomBytes);

			// Now add the random portion
			Array.Copy(randomBytes, 0, result, 3, 7);

			// We now have our number!
			return result;
		}

		private static byte[] ParseIdString(string id)
		{
			byte[] result = new byte[10];
			var bits = new bool[80];
			int index = 0;
			foreach (char ch in id)
			{
				for (int shiftBits = 4; shiftBits >= 0; shiftBits--)
				{
					bits[index++] = (Array.IndexOf(Chars, ch) & (1 << shiftBits)) > 0;
				}
			}

			for (index = 0; index < 80; index++)
			{
				result[index / 8] |= (byte)((bits[index] ? 1 : 0) << (7 - (index % 8)));
			}

			return result;
		}

		private static IEnumerable<bool> EnumerateBits(byte[] values)
		{
			return EnumerateBits(new MemoryStream(values));
		}

		private static IEnumerable<bool> EnumerateBits(Stream stream)
		{
			int val;

			while ((val = stream.ReadByte()) != -1)
			{
				for (int bit = 7; bit >= 0; bit--)
					yield return ((val >> bit) & 0x1) == 1;
			}
		}
	}
}
