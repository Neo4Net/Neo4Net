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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Neo4Net.Helpers;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using LogProvider = Neo4Net.Logging.LogProvider;

	internal class HeartbeatContextImpl : AbstractContextImpl, HeartbeatContext
	{
		 // HeartbeatContext
		 private ISet<InstanceId> _failed = new HashSet<InstanceId>();

		 private IDictionary<InstanceId, ISet<InstanceId>> _nodeSuspicions = new Dictionary<InstanceId, ISet<InstanceId>>();

		 private readonly Listeners<HeartbeatListener> _heartBeatListeners;

		 private readonly Executor _executor;
		 private ClusterContext _clusterContext;
		 private LearnerContext _learnerContext;

		 internal HeartbeatContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, Executor executor ) : base( me, commonState, logging, timeouts )
		 {
			  this._executor = executor;
			  this._heartBeatListeners = new Listeners<HeartbeatListener>();
		 }

		 private HeartbeatContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, ISet<InstanceId> failed, IDictionary<InstanceId, ISet<InstanceId>> nodeSuspicions, Listeners<HeartbeatListener> heartBeatListeners, Executor executor ) : base( me, commonState, logging, timeouts )
		 {
			  this._failed = failed;
			  this._nodeSuspicions = nodeSuspicions;
			  this._heartBeatListeners = heartBeatListeners;
			  this._executor = executor;
		 }

		 internal virtual void SetCircularDependencies( ClusterContext clusterContext, LearnerContext learnerContext )
		 {
			  this._clusterContext = clusterContext;
			  this._learnerContext = learnerContext;
		 }

		 public override void Started()
		 {
			  _failed.Clear();
		 }

		 /// <returns> True iff the node was suspected </returns>
		 public override bool Alive( InstanceId node )
		 {
			  ISet<InstanceId> serverSuspicions = SuspicionsFor( MyId );
			  bool suspected = serverSuspicions.remove( node );

			  if ( !IsFailedBasedOnSuspicions( node ) && _failed.remove( node ) )
			  {
					GetLog( typeof( HeartbeatContext ) ).info( "Notifying listeners that instance " + node + " is alive" );
					_heartBeatListeners.notify( _executor, listener => listener.alive( node ) );
			  }

			  return suspected;
		 }

		 public override void Suspect( InstanceId node )
		 {
			  ISet<InstanceId> serverSuspicions = SuspicionsFor( MyId );

			  if ( !serverSuspicions.Contains( node ) )
			  {
					serverSuspicions.Add( node );

					GetLog( typeof( HeartbeatContext ) ).info( MyId + "(me) is now suspecting " + node );
			  }

			  if ( IsFailedBasedOnSuspicions( node ) && !_failed.Contains( node ) )
			  {
					GetLog( typeof( HeartbeatContext ) ).info( "Notifying listeners that instance " + node + " is failed" );
					_failed.Add( node );
					_heartBeatListeners.notify( _executor, listener => listener.failed( node ) );
			  }

			  if ( CheckSuspectEverybody() )
			  {
					GetLog( typeof( HeartbeatContext ) ).warn( "All other instances are being suspected. Moving on to mark all other instances as failed" );
					MarkAllOtherMembersAsFailed();
			  }
		 }

		 /*
		  * Alters state so that all instances are marked as failed. The state is changed so that any timeouts will not
		  * reset an instance to alive, allowing only for real heartbeats from an instance to mark it again as alive. This
		  * method is expected to be called in the event where all instances are being suspected, in which case a network
		  * partition has happened and we need to set ourselves in an unavailable state.
		  * The way this method achieves its task is by introducing suspicions from everybody about everybody. This mimics
		  * the normal way of doing things, effectively faking a series of suspicion messages from every other instance
		  * before connectivity was lost. As a result, when connectivity is restored, the state will be restored properly
		  * for every instance that actually manages to reconnect.
		  */
		 private void MarkAllOtherMembersAsFailed()
		 {
			  ISet<InstanceId> everyoneElse = new HashSet<InstanceId>();
			  foreach ( InstanceId instanceId in Members.Keys )
			  {
					if ( !IsMe( instanceId ) )
					{
						 everyoneElse.Add( instanceId );
					}
			  }

			  foreach ( InstanceId instanceId in everyoneElse )
			  {
					ISet<InstanceId> instancesThisInstanceSuspects = new HashSet<InstanceId>( everyoneElse );
					instancesThisInstanceSuspects.remove( instanceId ); // obviously an instance cannot suspect itself
					Suspicions( instanceId, instancesThisInstanceSuspects );
			  }
		 }

		 /// <summary>
		 /// Returns true iff this instance suspects every other instance currently in the cluster, except for itself.
		 /// </summary>
		 private bool CheckSuspectEverybody()
		 {
			  IDictionary<InstanceId, URI> allClusterMembers = Members;
			  ISet<InstanceId> suspectedInstances = GetSuspicionsFor( MyId );
			  int suspected = 0;
			  foreach ( InstanceId suspectedInstance in suspectedInstances )
			  {
					if ( allClusterMembers.ContainsKey( suspectedInstance ) )
					{
						 suspected++;
					}
			  }

			  return suspected == allClusterMembers.Count - 1;
		 }

		 public override void Suspicions( InstanceId from, ISet<InstanceId> suspicions )
		 {
			  /*
			   * A thing to be careful about here is the case where a cluster member is marked as failed but it's not yet
			   * in the failed set. This implies the member has gathered enough suspicions to be marked as failed but is
			   * not yet marked as such. This can happen if there is a cluster partition containing only us, in which case
			   * markAllOthersAsFailed() will suspect everyone but not add them to failed (this happens here, further down).
			   * In this case, all suspicions must be processed, since after processing half, the other half of the cluster
			   * will be marked as failed (it has gathered enough suspicions) but we still need to process their messages, in
			   * order to mark as failed the other half.
			   */
			  if ( IsFailedBasedOnSuspicions( from ) && !_failed.Contains( from ) )
			  {
					GetLog( typeof( HeartbeatContext ) ).info( "Ignoring suspicions from failed instance " + from + ": " + Iterables.ToString( suspicions, "," ) );
					return;
			  }

			  ISet<InstanceId> serverSuspicions = SuspicionsFor( from );

			  // Check removals
			  IEnumerator<InstanceId> suspicionsIterator = serverSuspicions.GetEnumerator();
			  while ( suspicionsIterator.MoveNext() )
			  {
					InstanceId currentSuspicion = suspicionsIterator.Current;
					if ( !suspicions.Contains( currentSuspicion ) )
					{
						 GetLog( typeof( HeartbeatContext ) ).info( from + " is no longer suspecting " + currentSuspicion );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 suspicionsIterator.remove();
					}
			  }

			  // Check additions
			  foreach ( InstanceId suspicion in suspicions )
			  {
					if ( !serverSuspicions.Contains( suspicion ) )
					{
						 GetLog( typeof( HeartbeatContext ) ).info( from + " is now suspecting " + suspicion );
						 serverSuspicions.Add( suspicion );
					}
			  }

			  // Check if anyone is considered failed
			  foreach ( InstanceId node in suspicions )
			  {
					if ( IsFailedBasedOnSuspicions( node ) && !_failed.Contains( node ) )
					{
						 _failed.Add( node );
						 _heartBeatListeners.notify( _executor, listener => listener.failed( node ) );
					}
			  }
		 }

		 public virtual ISet<InstanceId> Failed
		 {
			 get
			 {
				  return _failed;
			 }
		 }

		 public virtual IEnumerable<InstanceId> Alive
		 {
			 get
			 {
				  return Iterables.filter( item => !IsFailedBasedOnSuspicions( item ), CommonState.configuration().MemberIds );
			 }
		 }

		 public override void AddHeartbeatListener( HeartbeatListener listener )
		 {
			  _heartBeatListeners.add( listener );
		 }

		 public override void RemoveHeartbeatListener( HeartbeatListener listener )
		 {
			  _heartBeatListeners.remove( listener );
		 }

		 public override void ServerLeftCluster( InstanceId node )
		 {
			  _failed.remove( node );
			  foreach ( ISet<InstanceId> uris in _nodeSuspicions.Values )
			  {
					uris.remove( node );
			  }
		 }

		 public override bool IsFailedBasedOnSuspicions( InstanceId node )
		 {
			  IList<InstanceId> suspicionsForNode = GetSuspicionsOf( node );
			  int countOfInstancesSuspectedByMe = GetSuspicionsFor( MyId ).Count;

			  /*
			   * If more than half *non suspected instances* suspect this node, fail it. This takes care of partitions
			   * that contain less than half of the cluster, ensuring that they will eventually detect the disconnect without
			   * waiting to have a majority of suspicions. This is accomplished by counting as quorum only instances
			   * that are not suspected by me.
			   */
			  return suspicionsForNode.Count > ( CommonState.configuration().Members.Count - countOfInstancesSuspectedByMe ) / 2;
		 }

		 /// <summary>
		 /// Get all of the servers which suspect a specific member.
		 /// </summary>
		 /// <param name="instanceId"> for the member of interest. </param>
		 /// <returns> a set of servers which suspect the specified member. </returns>
		 public override IList<InstanceId> GetSuspicionsOf( InstanceId instanceId )
		 {
			  IList<InstanceId> suspicions = new List<InstanceId>();
			  foreach ( InstanceId member in CommonState.configuration().MemberIds )
			  {
					ISet<InstanceId> memberSuspicions = _nodeSuspicions[member];
					if ( memberSuspicions != null && !_failed.Contains( member ) && memberSuspicions.Contains( instanceId ) )
					{
						 suspicions.Add( member );
					}
			  }

			  return suspicions;
		 }

		 /// <summary>
		 /// Get the suspicions as reported by a specific server.
		 /// </summary>
		 /// <param name="instanceId"> which might suspect someone. </param>
		 /// <returns> a list of those members which server suspects. </returns>
		 public override ISet<InstanceId> GetSuspicionsFor( InstanceId instanceId )
		 {
			  ISet<InstanceId> suspicions = SuspicionsFor( instanceId );
			  return new HashSet<InstanceId>( suspicions );
		 }

		 private ISet<InstanceId> SuspicionsFor( InstanceId instanceId )
		 {
			  return _nodeSuspicions.computeIfAbsent( instanceId, k => new HashSet<>() );
		 }

		 public virtual IEnumerable<InstanceId> OtherInstances
		 {
			 get
			 {
				  return _clusterContext.OtherInstances;
			 }
		 }

		 public virtual long LastKnownLearnedInstanceInCluster
		 {
			 get
			 {
				  return _learnerContext.LastKnownLearnedInstanceInCluster;
			 }
		 }

		 public virtual long LastLearnedInstanceId
		 {
			 get
			 {
				  return _learnerContext.LastLearnedInstanceId;
			 }
		 }

		 public override void Failed( InstanceId instanceId )
		 {
			  _failed.Add( instanceId );
		 }

		 public virtual HeartbeatContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, Executor executor )
		 {
			  return new HeartbeatContextImpl( Me, commonStateSnapshot, logging, timeouts, new HashSet<>( _failed ), new Dictionary<>( _nodeSuspicions ), new Listeners<>( _heartBeatListeners ), executor );
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

			  HeartbeatContextImpl that = ( HeartbeatContextImpl ) o;

			  if ( _failed != null ?!_failed.SetEquals( that._failed ) : that._failed != null )
			  {
					return false;
			  }
			  return _nodeSuspicions != null ? _nodeSuspicions.Equals( that._nodeSuspicions ) : that._nodeSuspicions == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = _failed != null ? _failed.GetHashCode() : 0;
			  result = 31 * result + ( _nodeSuspicions != null ? _nodeSuspicions.GetHashCode() : 0 );
			  return result;
		 }
	}

}