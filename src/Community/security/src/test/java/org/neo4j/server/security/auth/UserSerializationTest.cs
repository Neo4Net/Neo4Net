using System.Collections.Generic;

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
	using Test = org.junit.Test;

	using User = Neo4Net.Kernel.impl.security.User;
	using UTF8 = Neo4Net.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	public class UserSerializationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserialize()
		 {
			  // Given
			  UserSerialization serialization = new UserSerialization();

			  IList<User> users = new IList<User> { ( new User.Builder( "Mike", LegacyCredential.ForPassword( "1234321" ) ) ).withFlag( "not_as_nice" ).build(), (new User.Builder("Steve", LegacyCredential.ForPassword("1234321"))).build(), (new User.Builder("steve.stevesson@WINDOMAIN", LegacyCredential.ForPassword("1234321"))).build(), (new User.Builder("Bob", LegacyCredential.ForPassword("0987654"))).build() };

			  // When
			  sbyte[] serialized = serialization.Serialize( users );

			  // Then
			  assertThat( serialization.DeserializeRecords( serialized ), equalTo( users ) );
		 }

		 /// <summary>
		 /// This is a future-proofing test. If you come here because you've made changes to the serialization format,
		 /// this is your reminder to make sure to build this is in a backwards compatible way.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadV1SerializationFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadV1SerializationFormat()
		 {
			  // Given
			  UserSerialization serialization = new UserSerialization();
			  sbyte[] salt1 = new sbyte[] { unchecked( ( sbyte ) 0xa5 ), ( sbyte ) 0x43 };
			  sbyte[] hash1 = new sbyte[] { unchecked( ( sbyte ) 0xfe ), ( sbyte ) 0x00, ( sbyte ) 0x56, unchecked( ( sbyte ) 0xc3 ), ( sbyte ) 0x7e };
			  sbyte[] salt2 = new sbyte[] { ( sbyte ) 0x34, unchecked( ( sbyte ) 0xa4 ) };
			  sbyte[] hash2 = new sbyte[] { ( sbyte ) 0x0e, ( sbyte ) 0x1f, unchecked( ( sbyte ) 0xff ), unchecked( ( sbyte ) 0xc2 ), ( sbyte ) 0x3e };

			  // When
			  IList<User> deserialized = serialization.DeserializeRecords( UTF8.encode( "Mike:SHA-256,FE0056C37E,A543:\n" + "Steve:SHA-256,FE0056C37E,A543:nice_guy,password_change_required\n" + "Bob:SHA-256,0E1FFFC23E,34A4:password_change_required\n" ) );

			  // Then
			  assertThat(deserialized, equalTo(asList((new User.Builder("Mike", new LegacyCredential(salt1, hash1))).build(), new User.Builder("Steve", new LegacyCredential(salt1, hash1))
										 .withRequiredPasswordChange( true ).withFlag( "nice_guy" ).build(), (new User.Builder("Bob", new LegacyCredential(salt2, hash2))).withRequiredPasswordChange(true).build())));
		 }
	}

}