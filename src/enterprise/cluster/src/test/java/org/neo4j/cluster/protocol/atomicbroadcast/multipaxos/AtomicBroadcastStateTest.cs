using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;


	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.to;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage.failed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastState.broadcasting;

	public class AtomicBroadcastStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBroadcastWhenHavingNoQuorumNoCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBroadcastWhenHavingNoQuorumNoCoordinator()
		 {
			  // GIVEN
			  AtomicBroadcastContext context = mock( typeof( AtomicBroadcastContext ) );
			  when( context.HasQuorum() ).thenReturn(false);

			  InstanceId coordinator = Id( 1 );
			  when( context.Coordinator ).thenReturn( coordinator );
			  when( context.GetUriForId( coordinator ) ).thenReturn( Uri( 1 ) );
			  when( context.GetLog( typeof( AtomicBroadcastState ) ) ).thenReturn( NullLog.Instance );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Neo4Net.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder outgoing = messages.add;

			  // WHEN
			  broadcasting.handle( context, Message( 1 ), outgoing );
			  // THEN
			  assertEquals( 0, messages.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBroadcastWhenHavingNoQuorumButCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBroadcastWhenHavingNoQuorumButCoordinator()
		 {
			  // GIVEN
			  AtomicBroadcastContext context = mock( typeof( AtomicBroadcastContext ) );
			  when( context.HasQuorum() ).thenReturn(false);
			  when( context.Coordinator ).thenReturn( null );
			  when( context.GetLog( typeof( AtomicBroadcastState ) ) ).thenReturn( NullLog.Instance );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Neo4Net.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder outgoing = messages.add;

			  // WHEN
			  broadcasting.handle( context, Message( 1 ), outgoing );
			  // THEN
			  assertEquals( 0, messages.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBroadcastWhenHavingQuorumAndCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBroadcastWhenHavingQuorumAndCoordinator()
		 {
			  // GIVEN
			  AtomicBroadcastContext context = mock( typeof( AtomicBroadcastContext ) );
			  when( context.HasQuorum() ).thenReturn(true);

			  InstanceId coordinator = Id( 1 );
			  when( context.Coordinator ).thenReturn( coordinator );
			  when( context.GetUriForId( coordinator ) ).thenReturn( Uri( 1 ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Neo4Net.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder outgoing = messages.add;

			  // WHEN
			  broadcasting.handle( context, Message( 1 ), outgoing );
			  // THEN
			  assertEquals( 1, messages.Count );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBroadcastWhenHavingQuorumNoCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBroadcastWhenHavingQuorumNoCoordinator()
		 {
			  // GIVEN
			  AtomicBroadcastContext context = mock( typeof( AtomicBroadcastContext ) );
			  when( context.HasQuorum() ).thenReturn(true);
			  when( context.Coordinator ).thenReturn( null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Neo4Net.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder outgoing = messages.add;
			  // WHEN
			  broadcasting.handle( context, Message( 1 ), outgoing );
			  // THEN
			  assertEquals( 1, messages.Count );
		 }

		 private Message<AtomicBroadcastMessage> Message( int id )
		 {
			  return to( failed, Uri( id ), "some payload" ).setHeader( Message.HEADER_CONVERSATION_ID, "some id" );
		 }

		 private URI Uri( int i )
		 {
			  return URI.create( "http://localhost:" + ( 6000 + i ) + "?serverId=" + i );
		 }

		 private InstanceId Id( int i )
		 {
			  return new InstanceId( i );
		 }
	}

}