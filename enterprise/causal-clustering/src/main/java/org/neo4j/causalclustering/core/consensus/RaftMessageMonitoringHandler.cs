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
namespace Org.Neo4j.causalclustering.core.consensus
{

	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using ComposableMessageHandler = Org.Neo4j.causalclustering.messaging.ComposableMessageHandler;
	using Org.Neo4j.causalclustering.messaging;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

	public class RaftMessageMonitoringHandler : LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.messaging.LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> raftMessageHandler;
		 private readonly LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _raftMessageHandler;
		 private readonly Clock _clock;
		 private readonly RaftMessageProcessingMonitor _raftMessageDelayMonitor;

		 public RaftMessageMonitoringHandler<T1>( LifecycleMessageHandler<T1> raftMessageHandler, Clock clock, Monitors monitors )
		 {
			  this._raftMessageHandler = raftMessageHandler;
			  this._clock = clock;
			  this._raftMessageDelayMonitor = monitors.NewMonitor( typeof( RaftMessageProcessingMonitor ) );
		 }

		 public static ComposableMessageHandler Composable( Clock clock, Monitors monitors )
		 {
			  return @delegate => new RaftMessageMonitoringHandler( @delegate, clock, monitors );
		 }

		 public override void Handle<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> incomingMessage )
		 {
			 lock ( this )
			 {
				  Instant start = _clock.instant();
      
				  LogDelay( incomingMessage, start );
      
				  TimeHandle( incomingMessage, start );
			 }
		 }

		 private void TimeHandle<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> incomingMessage, Instant start )
		 {
			  try
			  {
					_raftMessageHandler.handle( incomingMessage );
			  }
			  finally
			  {
					Duration duration = Duration.between( start, _clock.instant() );
					_raftMessageDelayMonitor.updateTimer( incomingMessage.type(), duration );
			  }
		 }

		 private void LogDelay<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> incomingMessage, Instant start )
		 {
			  Duration delay = Duration.between( incomingMessage.receivedAt(), start );

			  _raftMessageDelayMonitor.Delay = delay;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start(org.neo4j.causalclustering.identity.ClusterId clusterId) throws Throwable
		 public override void Start( ClusterId clusterId )
		 {
			  _raftMessageHandler.start( clusterId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _raftMessageHandler.stop();
		 }
	}

}