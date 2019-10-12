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
namespace Neo4Net.causalclustering.core
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class ClusterBindingHandlerTest
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterBindingHandlerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_heartbeat = Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), _clusterId, new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat(new MemberId(System.Guid.randomUUID()), 0L, 0, 0) );
			_handler = new ClusterBindingHandler( @delegate, NullLogProvider.Instance );
		}

		 private ClusterId _clusterId = new ClusterId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?> heartbeat = org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of(java.time.Instant.now(), clusterId, new org.neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat(new org.neo4j.causalclustering.identity.MemberId(java.util.UUID.randomUUID()), 0L, 0, 0));
		 private Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<object> _heartbeat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.causalclustering.messaging.LifecycleMessageHandler<org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> delegate = org.mockito.Mockito.mock(org.neo4j.causalclustering.messaging.LifecycleMessageHandler.class);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private LifecycleMessageHandler<Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> @delegate = Mockito.mock( typeof( LifecycleMessageHandler ) );

		 private ClusterBindingHandler _handler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropMessagesIfHasNotBeenStarted()
		 public virtual void ShouldDropMessagesIfHasNotBeenStarted()
		 {
			  // when
			  _handler.handle( _heartbeat );

			  // then
			  verify( @delegate, Mockito.never() ).handle(_heartbeat);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropMessagesIfHasBeenStopped() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropMessagesIfHasBeenStopped()
		 {
			  // given
			  _handler.start( _clusterId );
			  _handler.stop();

			  // when
			  _handler.handle( _heartbeat );

			  // then
			  verify( @delegate, Mockito.never() ).handle(_heartbeat);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropMessagesIfForDifferentClusterId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropMessagesIfForDifferentClusterId()
		 {
			  // given
			  _handler.start( _clusterId );

			  // when
			  _handler.handle(Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of(Instant.now(), new ClusterId(System.Guid.randomUUID()), new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat(new MemberId(System.Guid.randomUUID()), 0L, 0, 0)
			 ));

			  // then
			  verify( @delegate, Mockito.never() ).handle(ArgumentMatchers.any(typeof(Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateMessages() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateMessages()
		 {
			  // given
			  _handler.start( _clusterId );

			  // when
			  _handler.handle( _heartbeat );

			  // then
			  verify( @delegate ).handle( _heartbeat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStartCalls() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateStartCalls()
		 {
			  // when
			  _handler.start( _clusterId );

			  // then
			  verify( @delegate ).start( _clusterId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStopCalls() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateStopCalls()
		 {
			  // when
			  _handler.stop();

			  // then
			  verify( @delegate ).stop();
		 }
	}

}