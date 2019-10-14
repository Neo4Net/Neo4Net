/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	using User = Neo4Net.Kernel.impl.security.User;
	using FormatException = Neo4Net.Server.Security.Auth.exception.FormatException;
	using HexString = Neo4Net.Strings.HexString;

	/// <summary>
	/// Serializes user authorization and authentication data to a format similar to unix passwd files.
	/// </summary>
	public class UserSerialization : FileRepositorySerializer<User>
	{
		 private const string USER_SEPARATOR = ":";
		 private const string CREDENTIAL_SEPARATOR = ",";

		 protected internal override string Serialize( User user )
		 {
			  return string.join( USER_SEPARATOR, user.Name(), Serialize((LegacyCredential) user.Credentials()), string.join(",", user.Flags) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.kernel.impl.security.User deserializeRecord(String line, int lineNumber) throws org.neo4j.server.security.auth.exception.FormatException
		 protected internal override User DeserializeRecord( string line, int lineNumber )
		 {
			  string[] parts = line.Split( USER_SEPARATOR, false );
			  if ( parts.Length != 3 )
			  {
					throw new FormatException( format( "wrong number of line fields, expected 3, got %d [line %d]", parts.Length, lineNumber ) );
			  }

			  User.Builder b = ( new User.Builder() ).withName(parts[0]).withCredentials(DeserializeCredentials(parts[1], lineNumber));

			  foreach ( string flag in parts[2].Split( ",", false ) )
			  {
					string trimmed = flag.Trim();
					if ( trimmed.Length > 0 )
					{
						 b = b.WithFlag( trimmed );
					}
			  }

			  return b.Build();
		 }

		 protected internal virtual string Serialize( LegacyCredential cred )
		 {
			  string encodedSalt = HexString.encodeHexString( cred.Salt() );
			  string encodedPassword = HexString.encodeHexString( cred.PasswordHash() );
			  return string.join( CREDENTIAL_SEPARATOR, LegacyCredential.DIGEST_ALGO, encodedPassword, encodedSalt );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private LegacyCredential deserializeCredentials(String part, int lineNumber) throws org.neo4j.server.security.auth.exception.FormatException
		 private LegacyCredential DeserializeCredentials( string part, int lineNumber )
		 {
			  string[] split = part.Split( CREDENTIAL_SEPARATOR, false );
			  if ( split.Length != 3 )
			  {
					throw new FormatException( format( "wrong number of credential fields [line %d]", lineNumber ) );
			  }
			  if ( !split[0].Equals( LegacyCredential.DIGEST_ALGO ) )
			  {
					throw new FormatException( format( "unknown digest \"%s\" [line %d]", split[0], lineNumber ) );
			  }
			  sbyte[] decodedPassword = HexString.decodeHexString( split[1] );
			  sbyte[] decodedSalt = HexString.decodeHexString( split[2] );
			  return new LegacyCredential( decodedSalt, decodedPassword );
		 }
	}

}