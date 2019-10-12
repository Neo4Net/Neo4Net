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

	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Org.Neo4j.cluster.protocol.cluster.ClusterContext;
	using ElectionContext = Org.Neo4j.cluster.protocol.election.ElectionContext;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionRole = Org.Neo4j.cluster.protocol.election.ElectionRole;
	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Context that implements all the context interfaces used by the Paxos state machines.
	/// <para>
	/// The design here is that all shared state is handled in a common class, <seealso cref="CommonContextState"/>, while all
	/// state specific to some single context is contained within the specific context classes.
	/// </para>
	/// </summary>
	public class MultiPaxosContext
	{
		 private readonly ClusterContextImpl _clusterContext;
		 private readonly ProposerContextImpl _proposerContext;
		 private readonly AcceptorContextImpl _acceptorContext;
		 private readonly LearnerContextImpl _learnerContext;
		 private readonly HeartbeatContextImpl _heartbeatContext;
		 private readonly ElectionContextImpl _electionContext;
		 private readonly AtomicBroadcastContextImpl _atomicBroadcastContext;
		 private readonly CommonContextState _commonState;
		 private readonly PaxosInstanceStore _paxosInstances;

		 public MultiPaxosContext( InstanceId me, IEnumerable<ElectionRole> roles, ClusterConfiguration configuration, Executor executor, LogProvider logging, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, AcceptorInstanceStore instanceStore, Timeouts timeouts, ElectionCredentialsProvider electionCredentialsProvider, Config config )
		 {
			  _commonState = new CommonContextState( configuration, config.Get( ClusterSettings.max_acceptors ) );
			  _paxosInstances = new PaxosInstanceStore();

			  _heartbeatContext = new HeartbeatContextImpl( me, _commonState, logging, timeouts, executor );
			  _learnerContext = new LearnerContextImpl( me, _commonState, logging, timeouts, _paxosInstances, instanceStore, objectInputStreamFactory, objectOutputStreamFactory, _heartbeatContext );
			  _clusterContext = new ClusterContextImpl( me, _commonState, logging, timeouts, executor, objectOutputStreamFactory, objectInputStreamFactory, _learnerContext, _heartbeatContext, config );
			  _electionContext = new ElectionContextImpl( me, _commonState, logging, timeouts, roles, _clusterContext, _heartbeatContext, electionCredentialsProvider );
			  _proposerContext = new ProposerContextImpl( me, _commonState, logging, timeouts, _paxosInstances, _heartbeatContext );
			  _acceptorContext = new AcceptorContextImpl( me, _commonState, logging, timeouts, instanceStore );
			  _atomicBroadcastContext = new AtomicBroadcastContextImpl( me, _commonState, logging, timeouts, executor, _heartbeatContext );

			  _heartbeatContext.setCircularDependencies( _clusterContext, _learnerContext );
		 }

		 private MultiPaxosContext( ProposerContextImpl proposerContext, AcceptorContextImpl acceptorContext, LearnerContextImpl learnerContext, HeartbeatContextImpl heartbeatContext, ElectionContextImpl electionContext, AtomicBroadcastContextImpl atomicBroadcastContext, CommonContextState commonState, PaxosInstanceStore paxosInstances, ClusterContextImpl clusterContext )
		 {
			  this._clusterContext = clusterContext;
			  this._proposerContext = proposerContext;
			  this._acceptorContext = acceptorContext;
			  this._learnerContext = learnerContext;
			  this._heartbeatContext = heartbeatContext;
			  this._electionContext = electionContext;
			  this._atomicBroadcastContext = atomicBroadcastContext;
			  this._commonState = commonState;
			  this._paxosInstances = paxosInstances;
		 }

		 public virtual ClusterContext ClusterContext
		 {
			 get
			 {
				  return _clusterContext;
			 }
		 }

		 public virtual ProposerContext ProposerContext
		 {
			 get
			 {
				  return _proposerContext;
			 }
		 }

		 public virtual AcceptorContext AcceptorContext
		 {
			 get
			 {
				  return _acceptorContext;
			 }
		 }

		 public virtual LearnerContext LearnerContext
		 {
			 get
			 {
				  return _learnerContext;
			 }
		 }

		 public virtual HeartbeatContext HeartbeatContext
		 {
			 get
			 {
				  return _heartbeatContext;
			 }
		 }

		 public virtual ElectionContext ElectionContext
		 {
			 get
			 {
				  return _electionContext;
			 }
		 }

		 public virtual AtomicBroadcastContextImpl AtomicBroadcastContext
		 {
			 get
			 {
				  return _atomicBroadcastContext;
			 }
		 }

		 /// <summary>
		 /// Create a state snapshot. The snapshot will not duplicate services, and expects the caller to duplicate
		 /// <seealso cref="AcceptorInstanceStore"/>, since that is externally provided.  
		 /// </summary>
		 public virtual MultiPaxosContext Snapshot( LogProvider logging, Timeouts timeouts, Executor executor, AcceptorInstanceStore instanceStore, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, ElectionCredentialsProvider electionCredentialsProvider )
		 {
			  CommonContextState commonStateSnapshot = _commonState.snapshot( logging.GetLog( typeof( ClusterConfiguration ) ) );
			  PaxosInstanceStore paxosInstancesSnapshot = _paxosInstances.snapshot();

			  HeartbeatContextImpl snapshotHeartbeatContext = _heartbeatContext.snapshot( commonStateSnapshot, logging, timeouts, executor );
			  LearnerContextImpl snapshotLearnerContext = _learnerContext.snapshot( commonStateSnapshot, logging, timeouts, paxosInstancesSnapshot, instanceStore, objectInputStreamFactory, objectOutputStreamFactory, snapshotHeartbeatContext );
			  ClusterContextImpl snapshotClusterContext = _clusterContext.snapshot( commonStateSnapshot, logging, timeouts, executor, objectOutputStreamFactory, objectInputStreamFactory, snapshotLearnerContext, snapshotHeartbeatContext );
			  ElectionContextImpl snapshotElectionContext = _electionContext.snapshot( commonStateSnapshot, logging, timeouts, snapshotClusterContext, snapshotHeartbeatContext, electionCredentialsProvider );
			  ProposerContextImpl snapshotProposerContext = _proposerContext.snapshot( commonStateSnapshot, logging, timeouts, paxosInstancesSnapshot, _heartbeatContext );
			  AcceptorContextImpl snapshotAcceptorContext = _acceptorContext.snapshot( commonStateSnapshot, logging, timeouts, instanceStore );
			  AtomicBroadcastContextImpl snapshotAtomicBroadcastContext = _atomicBroadcastContext.snapshot( commonStateSnapshot, logging, timeouts, executor, snapshotHeartbeatContext );

			  snapshotHeartbeatContext.SetCircularDependencies( snapshotClusterContext, snapshotLearnerContext );

			  return new MultiPaxosContext( snapshotProposerContext, snapshotAcceptorContext, snapshotLearnerContext, snapshotHeartbeatContext, snapshotElectionContext, snapshotAtomicBroadcastContext, commonStateSnapshot, paxosInstancesSnapshot, snapshotClusterContext );
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

			  MultiPaxosContext that = ( MultiPaxosContext ) o;

			  if ( !_acceptorContext.Equals( that._acceptorContext ) )
			  {
					return false;
			  }
			  if ( !_atomicBroadcastContext.Equals( that._atomicBroadcastContext ) )
			  {
					return false;
			  }
			  if ( !_clusterContext.Equals( that._clusterContext ) )
			  {
					return false;
			  }
			  if ( !_commonState.Equals( that._commonState ) )
			  {
					return false;
			  }
			  if ( !_electionContext.Equals( that._electionContext ) )
			  {
					return false;
			  }
			  if ( !_heartbeatContext.Equals( that._heartbeatContext ) )
			  {
					return false;
			  }
			  if ( !_learnerContext.Equals( that._learnerContext ) )
			  {
					return false;
			  }
			  if ( !_paxosInstances.Equals( that._paxosInstances ) )
			  {
					return false;
			  }
			  return _proposerContext.Equals( that._proposerContext );
		 }

		 public override int GetHashCode()
		 {
			  int result = _clusterContext.GetHashCode();
			  result = 31 * result + _proposerContext.GetHashCode();
			  result = 31 * result + _acceptorContext.GetHashCode();
			  result = 31 * result + _learnerContext.GetHashCode();
			  result = 31 * result + _heartbeatContext.GetHashCode();
			  result = 31 * result + _electionContext.GetHashCode();
			  result = 31 * result + _atomicBroadcastContext.GetHashCode();
			  result = 31 * result + _commonState.GetHashCode();
			  result = 31 * result + _paxosInstances.GetHashCode();
			  return result;
		 }
	}

}