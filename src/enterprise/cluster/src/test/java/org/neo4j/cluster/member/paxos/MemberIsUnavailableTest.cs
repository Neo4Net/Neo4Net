/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.cluster.member.paxos
{
	using Test = org.junit.Test;


	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class MemberIsUnavailableTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeSerializedWhenClusterUriIsNull() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeSerializedWhenClusterUriIsNull()
		 {
			  // Given
			  MemberIsUnavailable message = new MemberIsUnavailable( "master", new InstanceId( 1 ), null );

			  // When
			  sbyte[] serialized = Serialize( message );

			  // Then
			  assertNotEquals( 0, serialized.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeDeserializedWhenClusterUriIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeDeserializedWhenClusterUriIsNull()
		 {
			  // Given
			  MemberIsUnavailable message = new MemberIsUnavailable( "slave", new InstanceId( 1 ), null );
			  sbyte[] serialized = Serialize( message );

			  // When
			  MemberIsUnavailable deserialized = Deserialize( serialized );

			  // Then
			  assertNotSame( message, deserialized );
			  assertEquals( "slave", message.Role );
			  assertEquals( new InstanceId( 1 ), message.InstanceId );
			  assertNull( message.ClusterUri );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static byte[] serialize(MemberIsUnavailable message) throws java.io.IOException
		 private static sbyte[] Serialize( MemberIsUnavailable message )
		 {
			  ObjectOutputStream outputStream = null;
			  try
			  {
					MemoryStream byteArrayOutputStream = new MemoryStream();
					outputStream = ( new ObjectStreamFactory() ).create(byteArrayOutputStream);
					outputStream.writeObject( message );
					return byteArrayOutputStream.toByteArray();
			  }
			  finally
			  {
					if ( outputStream != null )
					{
						 outputStream.close();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MemberIsUnavailable deserialize(byte[] serialized) throws Exception
		 private static MemberIsUnavailable Deserialize( sbyte[] serialized )
		 {
			  ObjectInputStream inputStream = null;
			  try
			  {
					MemoryStream byteArrayInputStream = new MemoryStream( serialized );
					inputStream = ( new ObjectStreamFactory() ).create(byteArrayInputStream);
					return ( MemberIsUnavailable ) inputStream.readObject();
			  }
			  finally
			  {
					if ( inputStream != null )
					{
						 inputStream.close();
					}
			  }
		 }
	}

}