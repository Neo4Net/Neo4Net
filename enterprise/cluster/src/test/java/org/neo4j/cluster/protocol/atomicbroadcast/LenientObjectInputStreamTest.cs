﻿/*
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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast
{
	using Test = org.junit.Test;


	using MemberIsAvailable = Org.Neo4j.cluster.member.paxos.MemberIsAvailable;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class LenientObjectInputStreamTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreTheSerialVersionIdOfAClassTheFirstTimeItsDeserialised() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreTheSerialVersionIdOfAClassTheFirstTimeItsDeserialised()
		 {
			  // given
			  MemberIsAvailable memberIsAvailable = memberIsAvailable();
			  Payload payload = PayloadFor( memberIsAvailable );
			  VersionMapper versionMapper = new VersionMapper();

			  // when
			  ( new LenientObjectInputStream( InputStreamFor( payload ), versionMapper ) ).readObject();

			  // then
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertTrue( versionMapper.HasMappingFor( memberIsAvailable.GetType().FullName ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertEquals( SerialVersionUIDFor( memberIsAvailable ), versionMapper.MappingFor( memberIsAvailable.GetType().FullName ) );
		 }

		 private long SerialVersionUIDFor( MemberIsAvailable memberIsAvailable )
		 {
			  return ObjectStreamClass.lookup( memberIsAvailable.GetType() ).SerialVersionUID;
		 }

		 private MemoryStream InputStreamFor( Payload payload )
		 {
			  return new MemoryStream( payload.Buf, 0, payload.Len );
		 }

		 private MemberIsAvailable MemberIsAvailable()
		 {
			  return new MemberIsAvailable( "r1", new InstanceId( 1 ), URI.create( "http://me" ), URI.create( "http://me?something" ), StoreId.DEFAULT );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Payload payloadFor(Object value) throws java.io.IOException
		 private Payload PayloadFor( object value )
		 {
			  MemoryStream bout = new MemoryStream();
			  ObjectOutputStream oout = new ObjectOutputStream( bout );
			  oout.writeObject( value );
			  oout.close();
			  sbyte[] bytes = bout.toByteArray();
			  return new Payload( bytes, bytes.Length );
		 }
	}

}