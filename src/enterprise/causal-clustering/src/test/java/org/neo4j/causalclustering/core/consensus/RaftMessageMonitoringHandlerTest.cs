using System;

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
namespace Neo4Net.causalclustering.core.consensus
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class RaftMessageMonitoringHandlerTest
	{
		private bool InstanceFieldsInitialized = false;

		public RaftMessageMonitoringHandlerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_clock = Clocks.tickOnAccessClock( _now, _messageProcessingDelay );
			_handler = new RaftMessageMonitoringHandler( _downstream, _clock, _monitors );
		}

		 private Instant _now = Instant.now();
		 private Monitors _monitors = new Monitors();
		 private RaftMessageProcessingMonitor _monitor = mock( typeof( RaftMessageProcessingMonitor ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Neo4Net.causalclustering.messaging.LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> downstream = mock(Neo4Net.causalclustering.messaging.LifecycleMessageHandler.class);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _downstream = mock( typeof( LifecycleMessageHandler ) );

		 private Duration _messageQueueDelay = Duration.ofMillis( 5 );
		 private Duration _messageProcessingDelay = Duration.ofMillis( 7 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private RaftMessages_ReceivedInstantClusterIdAwareMessage<?> message = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(now.minus(messageQueueDelay), new Neo4Net.causalclustering.identity.ClusterId(java.util.UUID.randomUUID()), new RaftMessages_Heartbeat(new Neo4Net.causalclustering.identity.MemberId(java.util.UUID.randomUUID()), 0, 0, 0)
		 private RaftMessages_ReceivedInstantClusterIdAwareMessage<object> message = RaftMessages_ReceivedInstantClusterIdAwareMessage.of(_now.minus(_messageQueueDelay), new ClusterId(System.Guid.randomUUID()), new RaftMessages_Heartbeat(new MemberId(System.Guid.randomUUID()), 0, 0, 0)
		);
		 private Clock _clock = Clocks.tickOnAccessClock( _now, _messageProcessingDelay );

		 private RaftMessageMonitoringHandler _handler = new RaftMessageMonitoringHandler( _downstream, _clock, _monitors );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public void setUp()
		 {
			  _monitors.addMonitorListener( _monitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendMessagesToDelegate()
		 public void shouldSendMessagesToDelegate()
		 {
			  // when
			  _handler.handle( message );

			  // then
			  verify( _downstream ).handle( message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateDelayMonitor()
		 public void shouldUpdateDelayMonitor()
		 {
			  // when
			  _handler.handle( message );

			  // then
			  verify( _monitor ).Delay = _messageQueueDelay;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeDelegate()
		 public void shouldTimeDelegate()
		 {
			  // when
			  _handler.handle( message );

			  // then
			  verify( _monitor ).updateTimer( RaftMessages_Type.Heartbeat, _messageProcessingDelay );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStart() throws Throwable
		 public void shouldDelegateStart() throws Exception
		 {
			  // given
			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );

			  // when
			  _handler.start( clusterId );

			  // then
			  Mockito.verify( _downstream ).start( clusterId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStop() throws Throwable
		 public void shouldDelegateStop() throws Exception
		 {
			  // when
			  _handler.stop();

			  // then
			  Mockito.verify( _downstream ).stop();
		 }
	}

}