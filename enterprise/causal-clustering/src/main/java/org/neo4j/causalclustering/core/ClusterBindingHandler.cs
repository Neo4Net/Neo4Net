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
namespace Org.Neo4j.causalclustering.core
{

	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using ComposableMessageHandler = Org.Neo4j.causalclustering.messaging.ComposableMessageHandler;
	using Org.Neo4j.causalclustering.messaging;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class ClusterBindingHandler : LifecycleMessageHandler<Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.messaging.LifecycleMessageHandler<org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> delegateHandler;
		 private readonly LifecycleMessageHandler<Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _delegateHandler;
		 private volatile ClusterId _boundClusterId;
		 private readonly Log _log;

		 public ClusterBindingHandler<T1>( LifecycleMessageHandler<T1> delegateHandler, LogProvider logProvider )
		 {
			  this._delegateHandler = delegateHandler;
			  _log = logProvider.getLog( this.GetType() );
		 }

		 public static ComposableMessageHandler Composable( LogProvider logProvider )
		 {
			  return @delegate => new ClusterBindingHandler( @delegate, logProvider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start(org.neo4j.causalclustering.identity.ClusterId clusterId) throws Throwable
		 public override void Start( ClusterId clusterId )
		 {
			  this._boundClusterId = clusterId;
			  _delegateHandler.start( clusterId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  this._boundClusterId = null;
			  _delegateHandler.stop();
		 }

		 public override void Handle<T1>( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
		 {
			  if ( Objects.isNull( _boundClusterId ) )
			  {
					_log.debug( "Message handling has been stopped, dropping the message: %s", message.message() );
			  }
			  else if ( !Objects.Equals( _boundClusterId, message.clusterId() ) )
			  {
					_log.info( "Discarding message[%s] owing to mismatched clusterId. Expected: %s, Encountered: %s", message.message(), _boundClusterId, message.clusterId() );
			  }
			  else
			  {
					_delegateHandler.handle( message );
			  }
		 }
	}

}