using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using Org.Neo4j.cluster.com.message;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.limit;


	internal class ProposerContextImpl : AbstractContextImpl, ProposerContext
	{
		 public const int MAX_CONCURRENT_INSTANCES = 10;

		 // ProposerContext
		 private readonly Deque<Message> _pendingValues;
		 private readonly IDictionary<InstanceId, Message> _bookedInstances;

		 private readonly PaxosInstanceStore _paxosInstances;
		 private HeartbeatContext _heartbeatContext;

		 internal ProposerContextImpl( Org.Neo4j.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, PaxosInstanceStore paxosInstances, HeartbeatContext heartbeatContext ) : base( me, commonState, logging, timeouts )
		 {
			  this._paxosInstances = paxosInstances;
			  this._heartbeatContext = heartbeatContext;
			  _pendingValues = new LinkedList<Message>();
			  _bookedInstances = new Dictionary<InstanceId, Message>();
		 }

		 private ProposerContextImpl( Org.Neo4j.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, Deque<Message> pendingValues, IDictionary<InstanceId, Message> bookedInstances, PaxosInstanceStore paxosInstances, HeartbeatContext heartbeatContext ) : base( me, commonState, logging, timeouts )
		 {
			  this._pendingValues = pendingValues;
			  this._bookedInstances = bookedInstances;
			  this._paxosInstances = paxosInstances;
			  this._heartbeatContext = heartbeatContext;
		 }

		 public override InstanceId NewInstanceId()
		 {
			  // Never propose something lower than last received instance id
			  if ( CommonState.lastKnownLearnedInstanceInCluster() >= CommonState.nextInstanceId() )
			  {
					CommonState.NextInstanceId = CommonState.lastKnownLearnedInstanceInCluster() + 1;
			  }

			  return new InstanceId( CommonState.AndIncrementInstanceId );
		 }

		 public override void Leave()
		 {
			  _pendingValues.clear();
			  _bookedInstances.Clear();
			  CommonState.NextInstanceId = 0;

			  _paxosInstances.leave();
		 }

		 public override void BookInstance( InstanceId instanceId, Message message )
		 {
			  if ( message.Payload == null )
			  {
					throw new System.ArgumentException( "null payload for booking instance: " + message );
			  }
			  _bookedInstances[instanceId] = message;
		 }

		 public override PaxosInstance GetPaxosInstance( InstanceId instanceId )
		 {
			  return _paxosInstances.getPaxosInstance( instanceId );
		 }

		 public override void PendingValue( Message message )
		 {
			  _pendingValues.offerFirst( message );
		 }

		 public override bool HasPendingValues()
		 {
			  return !_pendingValues.Empty;
		 }

		 public override Message PopPendingValue()
		 {
			  return _pendingValues.remove();
		 }

		 public override bool CanBookInstance()
		 {
			  return _bookedInstances.Count < MAX_CONCURRENT_INSTANCES;
		 }

		 public override Message GetBookedInstance( InstanceId id )
		 {
			  return _bookedInstances[id];
		 }

		 public override Message<ProposerMessage> UnbookInstance( InstanceId id )
		 {
			  return _bookedInstances.Remove( id );
		 }

		 public override int NrOfBookedInstances()
		 {
			  return _bookedInstances.Count;
		 }

		 public virtual IList<URI> Acceptors
		 {
			 get
			 {
				  IEnumerable<URI> aliveMembers = Iterables.map( instanceId => _heartbeatContext.getUriForId( instanceId ), _heartbeatContext.Alive );
   
				  return new IList<URI> { limit( ( int ) Math.Min( Iterables.count( aliveMembers ), CommonState.MaxAcceptors ), aliveMembers ) };
			 }
		 }

		 public override int GetMinimumQuorumSize( IList<URI> acceptors )
		 {
			  return ( acceptors.Count / 2 ) + 1;
		 }

		 /// <summary>
		 /// This patches the booked instances that are pending in case the configuration of the cluster changes. This
		 /// should be called only when we learn a ConfigurationChangeState i.e. when we receive an accepted for
		 /// such a message. This won't "learn" the message, as in applying it on the cluster configuration, but will
		 /// just update properly the set of acceptors for pending instances.
		 /// </summary>
		 public override void PatchBookedInstances( ClusterMessage.ConfigurationChangeState value )
		 {
			  if ( value.Join != null )
			  {
					foreach ( InstanceId instanceId in _bookedInstances.Keys )
					{
						 PaxosInstance instance = _paxosInstances.getPaxosInstance( instanceId );
						 if ( instance.Acceptors != null )
						 {
							  instance.Acceptors.Remove( CommonState.configuration().Members[value.Join] );

							  GetLog( typeof( ProposerContext ) ).debug( "For booked instance " + instance + " removed gone member " + CommonState.configuration().Members[value.Join] + " added joining member " + value.JoinUri );

							  if ( !instance.Acceptors.Contains( value.JoinUri ) )
							  {
									instance.Acceptors.Add( value.JoinUri );
							  }
						 }
					}
			  }
			  else if ( value.Leave != null )
			  {
					foreach ( InstanceId instanceId in _bookedInstances.Keys )
					{
						 PaxosInstance instance = _paxosInstances.getPaxosInstance( instanceId );
						 if ( instance.Acceptors != null )
						 {
							  GetLog( typeof( ProposerContext ) ).debug( "For booked instance " + instance + " removed leaving member " + value.Leave + " (at URI " + CommonState.configuration().Members[value.Leave] + ")" );
							  instance.Acceptors.Remove( CommonState.configuration().Members[value.Leave] );
						 }
					}
			  }
		 }

		 public virtual ProposerContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, PaxosInstanceStore paxosInstancesSnapshot, HeartbeatContext heartbeatContext )
		 {
			  return new ProposerContextImpl( Me, commonStateSnapshot, logging, timeouts, new LinkedList<>( _pendingValues ), new Dictionary<>( _bookedInstances ), paxosInstancesSnapshot, heartbeatContext );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ProposerContextImpl that = ( ProposerContextImpl ) o;

			  if ( _bookedInstances != null ?!_bookedInstances.Equals( that._bookedInstances ) : that._bookedInstances != null )
			  {
					return false;
			  }
			  if ( _paxosInstances != null ?!_paxosInstances.Equals( that._paxosInstances ) : that._paxosInstances != null )
			  {
					return false;
			  }
			  return _pendingValues != null ? _pendingValues.Equals( that._pendingValues ) : that._pendingValues == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = _pendingValues != null ? _pendingValues.GetHashCode() : 0;
			  result = 31 * result + ( _bookedInstances != null ? _bookedInstances.GetHashCode() : 0 );
			  result = 31 * result + ( _paxosInstances != null ? _paxosInstances.GetHashCode() : 0 );
			  return result;
		 }
	}

}