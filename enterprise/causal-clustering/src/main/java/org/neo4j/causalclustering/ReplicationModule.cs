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
namespace Org.Neo4j.causalclustering
{

	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ProgressTrackerImpl = Org.Neo4j.causalclustering.core.replication.ProgressTrackerImpl;
	using RaftReplicator = Org.Neo4j.causalclustering.core.replication.RaftReplicator;
	using GlobalSession = Org.Neo4j.causalclustering.core.replication.session.GlobalSession;
	using GlobalSessionTrackerState = Org.Neo4j.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using LocalSessionPool = Org.Neo4j.causalclustering.core.replication.session.LocalSessionPool;
	using Org.Neo4j.causalclustering.core.state.storage;
	using ExponentialBackoffStrategy = Org.Neo4j.causalclustering.helper.ExponentialBackoffStrategy;
	using TimeoutStrategy = Org.Neo4j.causalclustering.helper.TimeoutStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class ReplicationModule
	{
		 public const string SESSION_TRACKER_NAME = "session-tracker";

		 private readonly RaftReplicator _replicator;
		 private readonly ProgressTrackerImpl _progressTracker;
		 private readonly SessionTracker _sessionTracker;

		 public ReplicationModule( RaftMachine raftMachine, MemberId myself, PlatformModule platformModule, Config config, Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, File clusterStateDirectory, FileSystemAbstraction fileSystem, LogProvider logProvider, AvailabilityGuard globalAvailabilityGuard, LocalDatabase localDatabase )
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