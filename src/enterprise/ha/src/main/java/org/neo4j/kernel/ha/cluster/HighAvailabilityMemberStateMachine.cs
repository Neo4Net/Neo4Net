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
namespace Neo4Net.Kernel.ha.cluster
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using Neo4Net.Helpers;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using AvailabilityRequirement = Neo4Net.Kernel.availability.AvailabilityRequirement;
	using DescriptiveAvailabilityRequirement = Neo4Net.Kernel.availability.DescriptiveAvailabilityRequirement;
	using ObservedClusterMembers = Neo4Net.Kernel.ha.cluster.member.ObservedClusterMembers;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.util.Quorums.isQuorum;

	/// <summary>
	/// State machine that listens for global cluster events, and coordinates
	/// the internal transitions between <seealso cref="HighAvailabilityMemberState"/>. Internal services
	/// that wants to know what is going on should register <seealso cref="HighAvailabilityMemberListener"/> implementations
	/// which will receive callbacks on state changes.
	/// <para>
	/// HA in Neo4Net is built on top of the clustering functionality. So, this state machine essentially reacts to cluster
	/// events,
	/// and implements the rules for how HA roles should change, for example, the cluster coordinator should become the HA
	/// master.
	/// </para>
	/// </summary>
	public class HighAvailabilityMemberStateMachine : LifecycleAdapter, HighAvailability
	{
		 public static readonly AvailabilityRequirement AvailabilityRequirement = new DescriptiveAvailabilityRequirement( "High Availability member state not ready" );
		 private readonly HighAvailabilityMemberContext _context;
		 private readonly AvailabilityGuard _databaseAvailabilityGuard;
		 private readonly ClusterMemberEvents _events;
		 private readonly Log _log;

		 private readonly Listeners<HighAvailabilityMemberListener> _memberListeners = new Listeners<HighAvailabilityMemberListener>();
		 private volatile HighAvailabilityMemberState _state;
		 private StateMachineClusterEventListener _eventsListener;
		 private readonly ObservedClusterMembers _members;
		 private readonly Election _election;

		 public HighAvailabilityMemberStateMachine( HighAvailabilityMemberContext context, AvailabilityGuard databaseAvailabilityGuard, ObservedClusterMembers members, ClusterMemberEvents events, Election election, LogProvider logProvider )
		 {
			  this._context = context;
			  this._databaseAvailabilityGuard = databaseAvailabilityGuard;
			  this._members = members;
			  this._events = events;
			  this._election = election;
			  this._log = logProvider.getLog( this.GetType() );
			  _state = HighAvailabilityMemberState.Pending;
		 }

		 public override void Init()
		 {
			  _events.addClusterMemberListener( _eventsListener = new StateMachineClusterEventListener( this ) );
			  // On initial startup, disallow database access
			  _databaseAvailabilityGuard.require( AvailabilityRequirement );
		 }

		 public override void Stop()
		 {
			  _events.removeClusterMemberListener( _eventsListener );
			  HighAvailabilityMemberState oldState = _state;
			  _state = HighAvailabilityMemberState.Pending;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(oldState, state, null, null);
			  HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( oldState, _state, null, null );
			  _memberListeners.notify( listener => listener.instanceStops( @event ) );

			  // If we were previously in a state that allowed access, we must now deny access
			  if ( oldState.AccessAllowed )
			  {
					_databaseAvailabilityGuard.require( AvailabilityRequirement );
			  }

			  _context.AvailableHaMasterId = null;
		 }

		 public override void AddHighAvailabilityMemberListener( HighAvailabilityMemberListener toAdd )
		 {
			  _memberListeners.add( toAdd );
		 }

		 public override void RemoveHighAvailabilityMemberListener( HighAvailabilityMemberListener toRemove )
		 {
			  _memberListeners.remove( toRemove );
		 }

		 public virtual HighAvailabilityMemberState CurrentState
		 {
			 get
			 {
				  return _state;
			 }
		 }

		 public virtual bool Master
		 {
			 get
			 {
				  return CurrentState == HighAvailabilityMemberState.Master;
			 }
		 }

		 /// <summary>
		 /// This listener will get all events about cluster instances, and depending on the current state it will
		 /// correctly transition to the next internal state and notify listeners of this change.
		 /// </summary>
		 private class StateMachineClusterEventListener : ClusterMemberListener
		 {
			 private readonly HighAvailabilityMemberStateMachine _outerInstance;

			 public StateMachineClusterEventListener( HighAvailabilityMemberStateMachine outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void CoordinatorIsElected( InstanceId coordinatorId )
			  {
				  lock ( this )
				  {
						try
						{
							 HighAvailabilityMemberState oldState = outerInstance.state;
							 InstanceId previousElected = outerInstance.context.ElectedMasterId;
      
							 outerInstance.context.AvailableHaMasterId = null;
							 if ( !AcceptNewState( outerInstance.state.masterIsElected( outerInstance.context, coordinatorId ) ) )
							 {
								  return;
							 }
      
							 outerInstance.context.ElectedMasterId = coordinatorId;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(oldState, state, coordinatorId, null);
							 HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( oldState, outerInstance.state, coordinatorId, null );
							 outerInstance.memberListeners.Notify( listener => listener.masterIsElected( @event ) );
      
							 if ( oldState.AccessAllowed && oldState != outerInstance.state )
							 {
								  outerInstance.databaseAvailabilityGuard.Require( AvailabilityRequirement );
							 }
      
							 outerInstance.log.Debug( "Got masterIsElected(" + coordinatorId + "), moved to " + outerInstance.state + " from " + oldState + ". Previous elected master is " + previousElected );
						}
						catch ( Exception t )
						{
							 throw new Exception( t );
						}
				  }
			  }

			  public override void MemberIsAvailable( string role, InstanceId instanceId, URI roleUri, StoreId storeId )
			  {
				  lock ( this )
				  {
						try
						{
							 /*
							  * Do different things depending on whether the cluster member is in master or slave state
							  */
							 if ( role.Equals( HighAvailabilityModeSwitcher.MASTER ) )
							 {
								  HighAvailabilityMemberState oldState = outerInstance.state;
								  outerInstance.context.AvailableHaMasterId = roleUri;
								  if ( !AcceptNewState( outerInstance.state.masterIsAvailable( outerInstance.context, instanceId, roleUri ) ) )
								  {
										return;
								  }
								  outerInstance.log.Debug( "Got masterIsAvailable(" + instanceId + "), moved to " + outerInstance.state + " from " + oldState );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(oldState, state, instanceId, roleUri);
								  HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( oldState, outerInstance.state, instanceId, roleUri );
								  outerInstance.memberListeners.Notify( listener => listener.masterIsAvailable( @event ) );
      
								  if ( oldState == HighAvailabilityMemberState.ToMaster && outerInstance.state == HighAvailabilityMemberState.Master )
								  {
										outerInstance.databaseAvailabilityGuard.Fulfill( AvailabilityRequirement );
								  }
							 }
							 else if ( role.Equals( HighAvailabilityModeSwitcher.SLAVE ) )
							 {
								  HighAvailabilityMemberState oldState = outerInstance.state;
								  if ( !AcceptNewState( outerInstance.state.slaveIsAvailable( outerInstance.context, instanceId, roleUri ) ) )
								  {
										return;
								  }
								  outerInstance.log.Debug( "Got slaveIsAvailable(" + instanceId + "), " + "moved to " + outerInstance.state + " from " + oldState );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(oldState, state, instanceId, roleUri);
								  HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( oldState, outerInstance.state, instanceId, roleUri );
								  outerInstance.memberListeners.Notify( listener => listener.slaveIsAvailable( @event ) );
      
								  if ( oldState == HighAvailabilityMemberState.ToSlave && outerInstance.state == HighAvailabilityMemberState.Slave )
								  {
										outerInstance.databaseAvailabilityGuard.Fulfill( AvailabilityRequirement );
								  }
							 }
						}
						catch ( Exception throwable )
						{
							 outerInstance.log.Warn( "Exception while receiving member availability notification", throwable );
						}
				  }
			  }

			  public override void MemberIsUnavailable( string role, InstanceId unavailableId )
			  {
					if ( outerInstance.context.MyId.Equals( unavailableId ) && HighAvailabilityModeSwitcher.SLAVE.Equals( role ) && outerInstance.state == HighAvailabilityMemberState.Slave )
					{
						 HighAvailabilityMemberState oldState = outerInstance.state;
						 ChangeStateToPending();
						 outerInstance.log.Debug( "Got memberIsUnavailable(" + unavailableId + "), moved to " + outerInstance.state + " from " + oldState );
					}
					else
					{
						 outerInstance.log.Debug( "Got memberIsUnavailable(" + unavailableId + ")" );
					}
			  }

			  public override void MemberIsFailed( InstanceId instanceId )
			  {
					// If we don't have quorum anymore with the currently alive members, then go to pending
					/*
					 * Unless this is a two instance cluster and we are the MASTER. This is an edge case in which a cluster
					 * of two instances gets a partition and we want to maintain write capability on one side.
					 * This, in combination with use of slave_only, is a cheap way to provide quasi-read-replica
					 * functionality for HA under the 2-instance scenario.
					 */
					if ( !isQuorum( AliveCount, TotalCount ) && !( TotalCount == 2 && outerInstance.state == HighAvailabilityMemberState.Master ) )
					{
						 HighAvailabilityMemberState oldState = outerInstance.state;
						 ChangeStateToDetached();
						 outerInstance.log.Debug( "Got memberIsFailed(" + instanceId + ") and cluster lost quorum to continue, moved to " + outerInstance.state + " from " + oldState + ", while maintaining read only capability." );
					}
					else if ( instanceId.Equals( outerInstance.context.ElectedMasterId ) && outerInstance.state == HighAvailabilityMemberState.Slave )
					{
						 HighAvailabilityMemberState oldState = outerInstance.state;
						 ChangeStateToDetached();
						 outerInstance.log.Debug( "Got memberIsFailed(" + instanceId + ") which was the master and i am a slave, moved to " + outerInstance.state + " from " + oldState + ", while maintaining read only capability." );
					}
					else
					{
						 outerInstance.log.Debug( "Got memberIsFailed(" + instanceId + ")" );
					}
			  }

			  public override void MemberIsAlive( InstanceId instanceId )
			  {
					// If we now have quorum and the previous state was pending, then ask for an election
					if ( isQuorum( AliveCount, TotalCount ) && outerInstance.state.Equals( HighAvailabilityMemberState.Pending ) )
					{
						 outerInstance.election.PerformRoleElections();
					}
			  }

			  internal virtual void ChangeStateToPending()
			  {
					if ( outerInstance.state.AccessAllowed )
					{
						 outerInstance.databaseAvailabilityGuard.Require( AvailabilityRequirement );
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(state, HighAvailabilityMemberState.PENDING, null, null);
					HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( outerInstance.state, HighAvailabilityMemberState.Pending, null, null );

					outerInstance.state = HighAvailabilityMemberState.Pending;

					outerInstance.memberListeners.Notify( listener => listener.instanceStops( @event ) );

					outerInstance.context.AvailableHaMasterId = null;
					outerInstance.context.ElectedMasterId = null;
			  }

			  internal virtual void ChangeStateToDetached()
			  {
					outerInstance.state = HighAvailabilityMemberState.Pending;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighAvailabilityMemberChangeEvent event = new HighAvailabilityMemberChangeEvent(state, HighAvailabilityMemberState.PENDING, null, null);
					HighAvailabilityMemberChangeEvent @event = new HighAvailabilityMemberChangeEvent( outerInstance.state, HighAvailabilityMemberState.Pending, null, null );
					outerInstance.memberListeners.Notify( listener => listener.instanceDetached( @event ) );

					outerInstance.context.AvailableHaMasterId = null;
					outerInstance.context.ElectedMasterId = null;
			  }

			  internal virtual long AliveCount
			  {
				  get
				  {
						return Iterables.count( outerInstance.members.AliveMembers );
				  }
			  }

			  internal virtual long TotalCount
			  {
				  get
				  {
						return Iterables.count( outerInstance.members.Members );
				  }
			  }

			  /// <summary>
			  /// Checks if the new state is ILLEGAL. If so, it sets the state to PENDING and issues a request for
			  /// elections. Otherwise it sets the current state to newState. </summary>
			  /// <returns> false iff the newState is illegal. true otherwise. </returns>
			  internal virtual bool AcceptNewState( HighAvailabilityMemberState newState )
			  {
					if ( newState == HighAvailabilityMemberState.Illegal )
					{
						 outerInstance.log.Warn( format( "Message received resulted in illegal state transition. I was in state %s, " + "context was %s. The error message is %s. This instance will now transition to PENDING state " + "and " + "ask for new elections. While this may fix the error, it may indicate that there is some " + "connectivity issue or some instability of cluster members.", outerInstance.state, outerInstance.context, newState.errorMessage() ) );
						 outerInstance.context.ElectedMasterId = null;
						 outerInstance.context.AvailableHaMasterId = null;
						 ChangeStateToPending();
						 outerInstance.election.PerformRoleElections();
						 return false;
					}
					else
					{
						 outerInstance.state = newState;
					}
					return true;
			  }
		 }
	}

}