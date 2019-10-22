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
namespace Neo4Net.cluster.protocol.atomicbroadcast
{

	using Test = org.junit.Test;


	using MemberIsAvailable = Neo4Net.cluster.member.paxos.MemberIsAvailable;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LenientObjectOutputStreamTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseStoredSerialVersionUIDWhenSerialisingAnObject() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseStoredSerialVersionUIDWhenSerialisingAnObject()
		 {
			  // given
			  MemberIsAvailable memberIsAvailable = memberIsAvailable();

			  VersionMapper versionMapper = new VersionMapper();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  versionMapper.AddMappingFor( memberIsAvailable.GetType().FullName, 12345L );

			  // when
			  object deserialisedObject = Deserialise( Serialise( memberIsAvailable, versionMapper ) );

			  // then
			  assertEquals( 12345L, SerialVersionUIDFor( deserialisedObject ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDefaultSerialVersionUIDWhenSerialisingAnObjectifNoMappingExists() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseDefaultSerialVersionUIDWhenSerialisingAnObjectifNoMappingExists()
		 {
			  // given
			  VersionMapper emptyVersionMapper = new VersionMapper();
			  MemberIsAvailable memberIsAvailable = memberIsAvailable();

			  // when
			  object deserialisedObject = Deserialise( Serialise( memberIsAvailable, emptyVersionMapper ) );

			  // then
			  assertEquals( SerialVersionUIDFor( memberIsAvailable ), SerialVersionUIDFor( deserialisedObject ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object deserialise(byte[] bytes) throws java.io.IOException, ClassNotFoundException
		 private object Deserialise( sbyte[] bytes )
		 {
			  return ( new ObjectInputStream( InputStreamFor( new Payload( bytes, bytes.Length ) ) ) ).readObject();
		 }

		 private long SerialVersionUIDFor( object memberIsAvailable )
		 {
			  return ObjectStreamClass.lookup( memberIsAvailable.GetType() ).SerialVersionUID;
		 }

		 private MemoryStream InputStreamFor( Payload payload )
		 {
			  return new MemoryStream( payload.Buf, 0, payload.Len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] serialise(Object value, VersionMapper versionMapper) throws java.io.IOException
		 private sbyte[] Serialise( object value, VersionMapper versionMapper )
		 {
			  MemoryStream bout = new MemoryStream();
			  ObjectOutputStream oout = new LenientObjectOutputStream( bout, versionMapper );
			  oout.writeObject( value );
			  oout.close();
			  return bout.toByteArray();
		 }

		 private MemberIsAvailable MemberIsAvailable()
		 {
			  return new MemberIsAvailable( "r1", new InstanceId( 1 ), URI.create( "http://me" ), URI.create( "http://me?something" ), StoreId.DEFAULT );
		 }
	}

}