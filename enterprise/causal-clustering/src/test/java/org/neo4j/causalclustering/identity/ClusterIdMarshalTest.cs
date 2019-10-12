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
namespace Org.Neo4j.causalclustering.identity
{
	using Test = org.junit.Test;


	using Org.Neo4j.causalclustering.messaging.marshalling;
	using InputStreamReadableChannel = Org.Neo4j.causalclustering.messaging.marshalling.InputStreamReadableChannel;
	using OutputStreamWritableChannel = Org.Neo4j.causalclustering.messaging.marshalling.OutputStreamWritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class ClusterIdMarshalTest
	{
		 private ChannelMarshal<ClusterId> _marshal = ClusterId.Marshal.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalClusterId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalClusterId()
		 {
			  // given
			  ClusterId original = new ClusterId( System.Guid.randomUUID() );
			  MemoryStream outputStream = new MemoryStream();

			  // when
			  OutputStreamWritableChannel writableChannel = new OutputStreamWritableChannel( outputStream );
			  _marshal.marshal( original, writableChannel );

			  InputStreamReadableChannel readableChannel = new InputStreamReadableChannel( new MemoryStream( outputStream.toByteArray() ) );
			  ClusterId result = _marshal.unmarshal( readableChannel );

			  // then
			  assertNotSame( original, result );
			  assertEquals( original, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalNullClusterId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalNullClusterId()
		 {
			  // given
			  MemoryStream outputStream = new MemoryStream();

			  // when
			  OutputStreamWritableChannel writableChannel = new OutputStreamWritableChannel( outputStream );
			  _marshal.marshal( null, writableChannel );

			  InputStreamReadableChannel readableChannel = new InputStreamReadableChannel( new MemoryStream( outputStream.toByteArray() ) );
			  ClusterId result = _marshal.unmarshal( readableChannel );

			  // then
			  assertNull( result );
		 }
	}

}