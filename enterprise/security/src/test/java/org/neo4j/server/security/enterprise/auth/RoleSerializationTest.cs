using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	public class RoleSerializationTest
	{
		 private SortedSet<string> _steveBob;
		 private SortedSet<string> _kellyMarie;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _steveBob = new SortedSet<string>();
			  _steveBob.Add( "Steve" );
			  _steveBob.Add( "Bob" );

			  _kellyMarie = new SortedSet<string>();
			  _kellyMarie.Add( "Kelly" );
			  _kellyMarie.Add( "Marie" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserialize()
		 {
			  // Given
			  RoleSerialization serialization = new RoleSerialization();

			  IList<RoleRecord> roles = asList( new RoleRecord( "admin", _steveBob ), new RoleRecord( "publisher", _kellyMarie ) );

			  // When
			  sbyte[] serialized = serialization.Serialize( roles );

			  // Then
			  assertThat( serialization.DeserializeRecords( serialized ), equalTo( roles ) );
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
			  RoleSerialization serialization = new RoleSerialization();

			  // When
			  IList<RoleRecord> deserialized = serialization.DeserializeRecords( UTF8.encode( "admin:Bob,Steve\n" + "publisher:Kelly,Marie\n" ) );

			  // Then
			  assertThat( deserialized, equalTo( asList( new RoleRecord( "admin", _steveBob ), new RoleRecord( "publisher", _kellyMarie ) ) ) );
		 }
	}

}