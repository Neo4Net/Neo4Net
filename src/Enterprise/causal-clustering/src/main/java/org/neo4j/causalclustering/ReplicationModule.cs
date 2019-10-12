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
namespace Neo4Net.causalclustering
{

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ProgressTrackerImpl = Neo4Net.causalclustering.core.replication.ProgressTrackerImpl;
	using RaftReplicator = Neo4Net.causalclustering.core.replication.RaftReplicator;
	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using GlobalSessionTrackerState = Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using LocalSessionPool = Neo4Net.causalclustering.core.replication.session.LocalSessionPool;
	using Neo4Net.causalclustering.core.state.storage;
	using ExponentialBackoffStrategy = Neo4Net.causalclustering.helper.ExponentialBackoffStrategy;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class ReplicationModule
	{
		 public const string SESSION_TRACKER_NAME = "session-tracker";

		 private readonly RaftReplicator _replicator;
		 private readonly ProgressTrackerImpl _progressTracker;
		 private readonly SessionTracker _sessionTracker;

		 public ReplicationModule( RaftMachine raftMachine, MemberId myself, PlatformModule platformModule, Config config, Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, File clusterStateDirectory, FileSystemAbstraction fileSystem, LogProvider logProvider, AvailabilityGuard globalAvailabilityGuard, LocalDatabase localDatabase )
		 {
			  LifeSupport life = platformModule.Life;

			  DurableStateStorage<GlobalSessionTrackerState> sessionTrackerStorage;
			  sessionTrackerStorage = life.Add( new DurableStateStorage<>( fileSystem, clusterStateDirectory, SESSION_TRACKER_NAME, new GlobalSessionTrackerState.Marshal( new MemberId.Marshal() ), config.Get(CausalClusteringSettings.global_session_tracker_state_size), logProvider ) );

			  _sessionTracker = new SessionTracker( sessionTrackerStorage );

			  GlobalSession myGlobalSession = new GlobalSession( System.Guid.randomUUID(), myself );
			  LocalSessionPool sessionPool = new LocalSessionPool( myGlobalSession );
			  _progressTracker = new ProgressTrackerImpl( myGlobalSession );

			  Duration initialBackoff = config.Get( CausalClusteringSettings.replication_retry_timeout_base );
			  Duration upperBoundBackoff = config.Get( CausalClusteringSettings.replication_retry_timeout_limit );

			  TimeoutStrategy progressRetryStrategy = new ExponentialBackoffStrategy( initialBackoff, upperBoundBackoff );
			  long availabilityTimeoutMillis = config.Get( CausalClusteringSettings.replication_retry_timeout_base ).toMillis();
			  _replicator = new RaftReplicator( raftMachine, myself, outbound, sessionPool, _progressTracker, progressRetryStrategy, availabilityTimeoutMillis, globalAvailabilityGuard, logProvider, localDatabase, platformModule.Monitors );
		 }

		 public virtual RaftReplicator Replicator
		 {
			 get
			 {
				  return _replicator;
			 }
		 }

		 public virtual ProgressTrackerImpl ProgressTracker
		 {
			 get
			 {
				  return _progressTracker;
			 }
		 }

		 public virtual SessionTracker SessionTracker
		 {
			 get
			 {
				  return _sessionTracker;
			 }
		 }
	}

}