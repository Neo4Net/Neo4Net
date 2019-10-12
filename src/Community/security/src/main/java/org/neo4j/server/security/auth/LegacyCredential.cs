using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.Security.Auth
{

	using Credential = Neo4Net.Kernel.impl.security.Credential;
	using HexString = Neo4Net.@string.HexString;
	using UTF8 = Neo4Net.@string.UTF8;

	/// <summary>
	/// This class is used for community security, InternalFlatFile, SetDefaultAdminCommand and SetInitialPasswordCommand
	/// The new commercial security has its own more secure version of Credential
	/// </summary>
	public class LegacyCredential : Credential
	{
		 public const string DIGEST_ALGO = "SHA-256";

		 public static readonly LegacyCredential Inaccessible = new LegacyCredential( new sbyte[]{}, new sbyte[]{} );

		 private static readonly Random _random = new SecureRandom();

		 private readonly sbyte[] _salt;
		 private readonly sbyte[] _passwordHash;

		 public static LegacyCredential ForPassword( sbyte[] password )
		 {
			  sbyte[] salt = RandomSalt();
			  return new LegacyCredential( salt, Hash( salt, password ) );
		 }

		 // For testing purposes only!
		 public static LegacyCredential ForPassword( string password )
		 {
			  return forPassword( UTF8.encode( password ) );
		 }

		 public LegacyCredential( sbyte[] salt, sbyte[] passwordHash )
		 {
			  this._salt = salt;
			  this._passwordHash = passwordHash;
		 }

		 public virtual sbyte[] Salt()
		 {
			  return _salt;
		 }

		 public virtual sbyte[] PasswordHash()
		 {
			  return _passwordHash;
		 }

		 public override bool MatchesPassword( sbyte[] password )
		 {
			  return ByteEquals( _passwordHash, Hash( _salt, password ) );
		 }

		 // For testing purposes only!
		 public override bool MatchesPassword( string password )
		 {
			  return ByteEquals( _passwordHash, Hash( _salt, UTF8.encode( password ) ) );
		 }

		 public override string Serialize()
		 {
			  return ( new UserSerialization() ).Serialize(this);
		 }

		 /// <summary>
		 /// <para>Utility method that replaces Arrays.equals() to avoid timing attacks.
		 /// The length of the loop executed will always be the length of the given password.
		 /// Remember <seealso cref="INACCESSIBLE"/> credentials should still execute loop for the length of given password.</para>
		 /// </summary>
		 /// <param name="actual"> the actual password </param>
		 /// <param name="given"> password given by the user </param>
		 /// <returns> whether the two byte arrays are equal </returns>
		 private static bool ByteEquals( sbyte[] actual, sbyte[] given )
		 {
			  if ( actual == given )
			  {
					return true;
			  }
			  if ( actual == null || given == null )
			  {
					return false;
			  }

			  int actualLength = actual.Length;
			  int givenLength = given.Length;
			  bool result = true;

			  for ( int i = 0; i < givenLength; ++i )
			  {
					if ( actualLength > 0 )
					{
						 result &= actual[i % actualLength] == given[i];
					}
			  }
			  return result && actualLength == givenLength;
		 }

		 /// <summary>
		 /// <para>Equality to always check for both salt and password hash as a safeguard against timing attack.</para>
		 /// </summary>
		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  LegacyCredential that = ( LegacyCredential ) o;

			  bool saltEquals = ByteEquals( this._salt, that._salt );
			  bool passwordEquals = ByteEquals( this._passwordHash, that._passwordHash );
			  return saltEquals && passwordEquals;
		 }

		 public override int GetHashCode()
		 {
			  return 31 * Arrays.GetHashCode( _salt ) + Arrays.GetHashCode( _passwordHash );
		 }

		 public override string ToString()
		 {
			  return "Credential{" +
						"salt=0x" + HexString.encodeHexString( _salt ) +
						", passwordHash=0x" + HexString.encodeHexString( _passwordHash ) +
						'}';
		 }

		 private static sbyte[] Hash( sbyte[] salt, sbyte[] password )
		 {
			  try
			  {
					MessageDigest m = MessageDigest.getInstance( DIGEST_ALGO );
					m.update( salt, 0, salt.Length );
					m.update( password, 0, password.Length );
					return m.digest();
			  }
			  catch ( NoSuchAlgorithmException e )
			  {
					throw new Exception( "Hash algorithm is not available on this platform: " + e.Message, e );
			  }
		 }

		 private static sbyte[] RandomSalt()
		 {
			  sbyte[] salt = new sbyte[32];
			  _random.NextBytes( salt );
			  return salt;
		 }
	}

}