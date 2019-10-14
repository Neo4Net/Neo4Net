/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.cluster.com.message
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class MessageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void respondingToInternalMessageShouldProduceCorrectMessage()
		 public virtual void RespondingToInternalMessageShouldProduceCorrectMessage()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object payload = new Object();
			  object payload = new object();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageType type = mock(MessageType.class);
			  MessageType type = mock( typeof( MessageType ) );
			  Message message = Message.Internal( type, payload );

			  // When
			  Message response = Message.Respond( type, message, payload );

			  // Then
			  assertTrue( response.Internal );
			  assertEquals( payload, response.Payload );
			  assertEquals( type, response.MessageType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void respondingToExternalMessageShouldProperlySetToHeaders()
		 public virtual void RespondingToExternalMessageShouldProperlySetToHeaders()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object payload = new Object();
			  object payload = new object();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MessageType type = mock(MessageType.class);
			  MessageType type = mock( typeof( MessageType ) );
			  URI to = URI.create( "cluster://to" );
			  URI from = URI.create( "cluster://from" );
			  Message incoming = Message.To( type, to, payload );
			  incoming.setHeader( Message.HEADER_FROM, from.ToString() );

			  // When
			  Message response = Message.Respond( type, incoming, payload );

			  // Then
			  assertFalse( response.Internal );
			  assertEquals( from.ToString(), response.getHeader(Message.HEADER_TO) );
			  assertEquals( payload, response.Payload );
			  assertEquals( type, response.MessageType );
		 }
	}

}