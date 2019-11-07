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

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using MonitoredRaftLog = Neo4Net.causalclustering.core.consensus.log.MonitoredRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using InFlightCacheFactory = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCacheFactory;
	using CoreLogPruningStrategy = Neo4Net.causalclustering.core.consensus.log.segmented.CoreLogPruningStrategy;
	using CoreLogPruningStrategyFactory = Neo4Net.causalclustering.core.consensus.log.segmented.CoreLogPruningStrategyFactory;
	using SegmentedRaftLog = Neo4Net.causalclustering.core.consensus.log.segmented.SegmentedRaftLog;
	using MemberIdSetBuilder = Neo4Net.causalclustering.core.consensus.membership.MemberIdSetBuilder;
	using RaftMembershipManager = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipManager;
	using RaftMembershipState = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipState;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using RaftLogShippingManager = Neo4Net.causalclustering.core.consensus.shipping.RaftLogShippingManager;
	using MonitoredTermStateStorage = Neo4Net.causalclustering.core.consensus.term.MonitoredTermStateStorage;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using SendToMyself = Neo4Net.causalclustering.core.replication.SendToMyself;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using RaftCoreTopologyConnector = Neo4Net.causalclustering.discovery.RaftCoreTopologyConnector;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging.marshalling;
	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.catchup_batch_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.join_catch_up_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.log_shipping_max_lag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.refuse_to_be_leader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.time.Clocks.systemClock;

	public class ConsensusModule
	{
		 public const string RAFT_MEMBERSHIP_NAME = "membership";
		 public const string RAFT_TERM_NAME = "term";
		 public const string RAFT_VOTE_NAME = "vote";

		 private readonly MonitoredRaftLog _raftLog;
		 private readonly RaftMachine _raftMachine;
		 private readonly RaftMembershipManager _raftMembershipManager;
		 private readonly InFlightCache _inFlightCache;

		 private readonly LeaderAvailabilityTimers _leaderAvailabilityTimers;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ConsensusModule(Neo4Net.causalclustering.identity.MemberId myself, final Neo4Net.graphdb.factory.module.PlatformModule platformModule, Neo4Net.causalclustering.messaging.Outbound<Neo4Net.causalclustering.identity.MemberId,RaftMessages_RaftMessage> outbound, java.io.File clusterStateDirectory, Neo4Net.causalclustering.discovery.CoreTopologyService coreTopologyService)
		 public ConsensusModule( MemberId myself, PlatformModule platformModule, Outbound<MemberId, RaftMessages_RaftMessage> outbound, File clusterStateDirectory, CoreTopologyService coreTopologyService )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.configuration.Config config = platformModule.config;
			  Config config = platformModule.Config;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;

			  LogProvider logProvider = logging.InternalLogProvider;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.storage.SafeChannelMarshal<Neo4Net.causalclustering.core.replication.ReplicatedContent> marshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal.marshaller();
			  SafeChannelMarshal<ReplicatedContent> marshal = CoreReplicatedContentMarshal.marshaller();

			  RaftLog underlyingLog = CreateRaftLog( config, life, fileSystem, clusterStateDirectory, marshal, logProvider, platformModule.JobScheduler );

			  _raftLog = new MonitoredRaftLog( underlyingLog, platformModule.Monitors );

			  StateStorage<TermState> termState;
			  StateStorage<VoteState> voteState;
			  StateStorage<RaftMembershipState> raftMembershipStorage;

			  StateStorage<TermState> durableTermState = life.Add( new DurableStateStorage<TermState>( fileSystem, clusterStateDirectory, RAFT_TERM_NAME, new TermState.Marshal(), config.Get(CausalClusteringSettings.term_state_size), logProvider ) );

			  termState = new MonitoredTermStateStorage( durableTermState, platformModule.Monitors );

			  voteState = life.Add( new DurableStateStorage<>( fileSystem, clusterStateDirectory, RAFT_VOTE_NAME, new VoteState.Marshal( new MemberId.Marshal() ), config.Get(CausalClusteringSettings.vote_state_size), logProvider ) );

			  raftMembershipStorage = life.Add( new DurableStateStorage<>( fileSystem, clusterStateDirectory, RAFT_MEMBERSHIP_NAME, new RaftMembershipState.Marshal(), config.Get(CausalClusteringSettings.raft_membership_state_size), logProvider ) );

			  TimerService timerService = new TimerService( platformModule.JobScheduler, logProvider );

			  _leaderAvailabilityTimers = CreateElectionTiming( config, timerService, logProvider );

			  int? minimumConsensusGroupSize = config.Get( CausalClusteringSettings.minimum_core_cluster_size_at_runtime );

			  MemberIdSetBuilder memberSetBuilder = new MemberIdSetBuilder();

			  SendToMyself leaderOnlyReplicator = new SendToMyself( myself, outbound );

			  _raftMembershipManager = new RaftMembershipManager( leaderOnlyReplicator, memberSetBuilder, _raftLog, logProvider, minimumConsensusGroupSize.Value, _leaderAvailabilityTimers.ElectionTimeout, systemClock(), config.Get(join_catch_up_timeout).toMillis(), raftMembershipStorage );
			  platformModule.Dependencies.satisfyDependency( _raftMembershipManager );

			  life.Add( _raftMembershipManager );

			  _inFlightCache = InFlightCacheFactory.create( config, platformModule.Monitors );

			  RaftLogShippingManager logShipping = new RaftLogShippingManager( outbound, logProvider, _raftLog, timerService, systemClock(), myself, _raftMembershipManager, _leaderAvailabilityTimers.ElectionTimeout, config.Get(catchup_batch_size), config.Get(log_shipping_max_lag), _inFlightCache );

			  bool supportsPreVoting = config.Get( CausalClusteringSettings.enable_pre_voting );

			  _raftMachine = new RaftMachine( myself, termState, voteState, _raftLog, _leaderAvailabilityTimers, outbound, logProvider, _raftMembershipManager, logShipping, _inFlightCache, config.Get( refuse_to_be_leader ), supportsPreVoting, platformModule.Monitors );

			  DurationSinceLastMessageMonitor durationSinceLastMessageMonitor = new DurationSinceLastMessageMonitor();
			  platformModule.Monitors.addMonitorListener( durationSinceLastMessageMonitor );
			  platformModule.Dependencies.satisfyDependency( durationSinceLastMessageMonitor );

			  string dbName = config.Get( CausalClusteringSettings.database );

			  life.Add( new RaftCoreTopologyConnector( coreTopologyService, _raftMachine, dbName ) );

			  life.Add( logShipping );
		 }

		 private LeaderAvailabilityTimers CreateElectionTiming( Config config, TimerService timerService, LogProvider logProvider )
		 {
			  Duration electionTimeout = config.Get( CausalClusteringSettings.leader_election_timeout );
			  return new LeaderAvailabilityTimers( electionTimeout, electionTimeout.dividedBy( 3 ), systemClock(), timerService, logProvider );
		 }

		 private RaftLog CreateRaftLog( Config config, LifeSupport life, FileSystemAbstraction fileSystem, File clusterStateDirectory, ChannelMarshal<ReplicatedContent> marshal, LogProvider logProvider, IJobScheduler scheduler )
		 {
			  EnterpriseCoreEditionModule.RaftLogImplementation raftLogImplementation = Enum.Parse( typeof( EnterpriseCoreEditionModule.RaftLogImplementation ), config.Get( CausalClusteringSettings.raft_log_implementation ) );
			  switch ( raftLogImplementation )
			  {
			  case EnterpriseCoreEditionModule.RaftLogImplementation.IN_MEMORY:
			  {
					return new InMemoryRaftLog();
			  }

			  case EnterpriseCoreEditionModule.RaftLogImplementation.SEGMENTED:
			  {
					long rotateAtSize = config.Get( CausalClusteringSettings.raft_log_rotation_size );
					int readerPoolSize = config.Get( CausalClusteringSettings.raft_log_reader_pool_size );

					CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( config.Get( CausalClusteringSettings.raft_log_pruning_strategy ), logProvider ) ).newInstance();
					File directory = new File( clusterStateDirectory, RAFT_LOG_DIRECTORY_NAME );
					return life.Add( new SegmentedRaftLog( fileSystem, directory, rotateAtSize, marshal, logProvider, readerPoolSize, systemClock(), scheduler, pruningStrategy ) );
			  }
			  default:
					throw new System.InvalidOperationException( "Unknown raft log implementation: " + raftLogImplementation );
			  }
		 }

		 public virtual RaftLog RaftLog()
		 {
			  return _raftLog;
		 }

		 public virtual RaftMachine RaftMachine()
		 {
			  return _raftMachine;
		 }

		 public virtual RaftMembershipManager RaftMembershipManager()
		 {
			  return _raftMembershipManager;
		 }

		 public virtual InFlightCache InFlightCache()
		 {
			  return _inFlightCache;
		 }

		 public virtual LeaderAvailabilityTimers LeaderAvailabilityTimers
		 {
			 get
			 {
				  return _leaderAvailabilityTimers;
			 }
		 }
	}

}