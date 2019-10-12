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
namespace Org.Neo4j.causalclustering.core.state.machines
{

	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using LeaderLocator = Org.Neo4j.causalclustering.core.consensus.LeaderLocator;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using RaftReplicator = Org.Neo4j.causalclustering.core.replication.RaftReplicator;
	using Replicator = Org.Neo4j.causalclustering.core.replication.Replicator;
	using DummyMachine = Org.Neo4j.causalclustering.core.state.machines.dummy.DummyMachine;
	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using IdAllocationState = Org.Neo4j.causalclustering.core.state.machines.id.IdAllocationState;
	using IdReusabilityCondition = Org.Neo4j.causalclustering.core.state.machines.id.IdReusabilityCondition;
	using ReplicatedIdAllocationStateMachine = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdAllocationStateMachine;
	using ReplicatedIdGeneratorFactory = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdGeneratorFactory;
	using ReplicatedIdRangeAcquirer = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdRangeAcquirer;
	using LeaderOnlyLockManager = Org.Neo4j.causalclustering.core.state.machines.locks.LeaderOnlyLockManager;
	using ReplicatedLockTokenState = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;
	using ReplicatedLockTokenStateMachine = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using ReplicatedLabelTokenHolder = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedLabelTokenHolder;
	using ReplicatedPropertyKeyTokenHolder = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedPropertyKeyTokenHolder;
	using ReplicatedRelationshipTypeTokenHolder = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedRelationshipTypeTokenHolder;
	using ReplicatedTokenStateMachine = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedTokenStateMachine;
	using RecoverConsensusLogIndex = Org.Neo4j.causalclustering.core.state.machines.tx.RecoverConsensusLogIndex;
	using ReplicatedTransactionCommitProcess = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransactionCommitProcess;
	using ReplicatedTransactionStateMachine = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransactionStateMachine;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using EditionLocksFactories = Org.Neo4j.Graphdb.factory.EditionLocksFactories;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using TokenRegistry = Org.Neo4j.Kernel.impl.core.TokenRegistry;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using LocksFactory = Org.Neo4j.Kernel.impl.locking.LocksFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.array_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.id_alloc_state_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.label_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.label_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.neostore_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.node_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.node_labels_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.property_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.property_key_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.property_key_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.relationship_group_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.relationship_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.relationship_type_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.relationship_type_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.replicated_lock_token_state_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.schema_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.state_machine_apply_max_batch_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.string_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.EditionLocksFactories.createLockFactory;

	public class CoreStateMachinesModule
	{
		 public const string ID_ALLOCATION_NAME = "id-allocation";
		 public const string LOCK_TOKEN_NAME = "lock-token";

		 public readonly IdGeneratorFactory IdGeneratorFactory;
		 public readonly IdTypeConfigurationProvider IdTypeConfigurationProvider;
		 public readonly TokenHolders TokenHolders;
		 public readonly System.Func<Locks> LocksSupplier;
		 public readonly CommitProcessFactory CommitProcessFactory;

		 public readonly CoreStateMachines CoreStateMachines;
		 public readonly System.Func<bool> FreeIdCondition;

		 public CoreStateMachinesModule( MemberId myself, PlatformModule platformModule, File clusterStateDirectory, Config config, RaftReplicator replicator, RaftMachine raftMachine, Dependencies dependencies, LocalDatabase localDatabase )
		 {
			  StateStorage<IdAllocationState> idAllocationState;
			  StateStorage<ReplicatedLockTokenState> lockTokenState;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
			  LogService logging = platformModule.Logging;
			  LogProvider logProvider = logging.InternalLogProvider;

			  lockTokenState = life.Add( new DurableStateStorage<>( fileSystem, clusterStateDirectory, LOCK_TOKEN_NAME, new ReplicatedLockTokenState.Marshal( new MemberId.Marshal() ), config.Get(replicated_lock_token_state_size), logProvider ) );

			  idAllocationState = life.Add( new DurableStateStorage<>( fileSystem, clusterStateDirectory, ID_ALLOCATION_NAME, new IdAllocationState.Marshal(), config.Get(id_alloc_state_size), logProvider ) );

			  ReplicatedIdAllocationStateMachine idAllocationStateMachine = new ReplicatedIdAllocationStateMachine( idAllocationState );

			  IDictionary<IdType, int> allocationSizes = GetIdTypeAllocationSizeFromConfig( config );

			  ReplicatedIdRangeAcquirer idRangeAcquirer = new ReplicatedIdRangeAcquirer( replicator, idAllocationStateMachine, allocationSizes, myself, logProvider );

			  IdTypeConfigurationProvider = new EnterpriseIdTypeConfigurationProvider( config );
			  CommandIndexTracker commandIndexTracker = dependencies.SatisfyDependency( new CommandIndexTracker() );
			  FreeIdCondition = new IdReusabilityCondition( commandIndexTracker, raftMachine, myself );
			  this.IdGeneratorFactory = CreateIdGeneratorFactory( fileSystem, idRangeAcquirer, logProvider, IdTypeConfigurationProvider );

			  TokenRegistry relationshipTypeTokenRegistry = new TokenRegistry( Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE );
			  System.Func<StorageEngine> storageEngineSupplier = () => localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(StorageEngine));
			  ReplicatedRelationshipTypeTokenHolder relationshipTypeTokenHolder = new ReplicatedRelationshipTypeTokenHolder( relationshipTypeTokenRegistry, replicator, this.IdGeneratorFactory, storageEngineSupplier );

			  TokenRegistry propertyKeyTokenRegistry = new TokenRegistry( Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
			  ReplicatedPropertyKeyTokenHolder propertyKeyTokenHolder = new ReplicatedPropertyKeyTokenHolder( propertyKeyTokenRegistry, replicator, this.IdGeneratorFactory, storageEngineSupplier );

			  TokenRegistry labelTokenRegistry = new TokenRegistry( Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL );
			  ReplicatedLabelTokenHolder labelTokenHolder = new ReplicatedLabelTokenHolder( labelTokenRegistry, replicator, this.IdGeneratorFactory, storageEngineSupplier );

			  ReplicatedLockTokenStateMachine replicatedLockTokenStateMachine = new ReplicatedLockTokenStateMachine( lockTokenState );

			  VersionContextSupplier versionContextSupplier = platformModule.VersionContextSupplier;
			  ReplicatedTokenStateMachine labelTokenStateMachine = new ReplicatedTokenStateMachine( labelTokenRegistry, logProvider, versionContextSupplier );

			  ReplicatedTokenStateMachine propertyKeyTokenStateMachine = new ReplicatedTokenStateMachine( propertyKeyTokenRegistry, logProvider, versionContextSupplier );

			  ReplicatedTokenStateMachine relationshipTypeTokenStateMachine = new ReplicatedTokenStateMachine( relationshipTypeTokenRegistry, logProvider, versionContextSupplier );

			  PageCursorTracerSupplier cursorTracerSupplier = platformModule.Tracers.pageCursorTracerSupplier;
			  ReplicatedTransactionStateMachine replicatedTxStateMachine = new ReplicatedTransactionStateMachine( commandIndexTracker, replicatedLockTokenStateMachine, config.Get( state_machine_apply_max_batch_size ), logProvider, cursorTracerSupplier, versionContextSupplier );

			  dependencies.SatisfyDependencies( replicatedTxStateMachine );

			  LocksFactory lockFactory = createLockFactory( config, logging );
			  LocksSupplier = () => CreateLockManager(lockFactory, config, platformModule.Clock, replicator, myself, raftMachine, replicatedLockTokenStateMachine);

			  RecoverConsensusLogIndex consensusLogIndexRecovery = new RecoverConsensusLogIndex( localDatabase, logProvider );

			  CoreStateMachines = new CoreStateMachines( replicatedTxStateMachine, labelTokenStateMachine, relationshipTypeTokenStateMachine, propertyKeyTokenStateMachine, replicatedLockTokenStateMachine, idAllocationStateMachine, new DummyMachine(), localDatabase, consensusLogIndexRecovery );

			  CommitProcessFactory = ( appender, applier, ignored ) =>
			  {
				localDatabase.RegisterCommitProcessDependencies( appender, applier );
				return new ReplicatedTransactionCommitProcess( replicator );
			  };

			  this.TokenHolders = new TokenHolders( propertyKeyTokenHolder, labelTokenHolder, relationshipTypeTokenHolder );
			  dependencies.SatisfyDependencies( TokenHolders );
		 }

		 private IDictionary<IdType, int> GetIdTypeAllocationSizeFromConfig( Config config )
		 {
			  IDictionary<IdType, int> allocationSizes = new Dictionary<IdType, int>( Enum.GetValues( typeof( IdType ) ).length );
			  allocationSizes[IdType.NODE] = config.Get( node_id_allocation_size );
			  allocationSizes[IdType.RELATIONSHIP] = config.Get( relationship_id_allocation_size );
			  allocationSizes[IdType.PROPERTY] = config.Get( property_id_allocation_size );
			  allocationSizes[IdType.STRING_BLOCK] = config.Get( string_block_id_allocation_size );
			  allocationSizes[IdType.ARRAY_BLOCK] = config.Get( array_block_id_allocation_size );
			  allocationSizes[IdType.PROPERTY_KEY_TOKEN] = config.Get( property_key_token_id_allocation_size );
			  allocationSizes[IdType.PROPERTY_KEY_TOKEN_NAME] = config.Get( property_key_token_name_id_allocation_size );
			  allocationSizes[IdType.RELATIONSHIP_TYPE_TOKEN] = config.Get( relationship_type_token_id_allocation_size );
			  allocationSizes[IdType.RELATIONSHIP_TYPE_TOKEN_NAME] = config.Get( relationship_type_token_name_id_allocation_size );
			  allocationSizes[IdType.LABEL_TOKEN] = config.Get( label_token_id_allocation_size );
			  allocationSizes[IdType.LABEL_TOKEN_NAME] = config.Get( label_token_name_id_allocation_size );
			  allocationSizes[IdType.NEOSTORE_BLOCK] = config.Get( neostore_block_id_allocation_size );
			  allocationSizes[IdType.SCHEMA] = config.Get( schema_id_allocation_size );
			  allocationSizes[IdType.NODE_LABELS] = config.Get( node_labels_id_allocation_size );
			  allocationSizes[IdType.RELATIONSHIP_GROUP] = config.Get( relationship_group_id_allocation_size );
			  return allocationSizes;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.id.IdGeneratorFactory createIdGeneratorFactory(org.neo4j.io.fs.FileSystemAbstraction fileSystem, final org.neo4j.causalclustering.core.state.machines.id.ReplicatedIdRangeAcquirer idRangeAcquirer, final org.neo4j.logging.LogProvider logProvider, org.neo4j.kernel.impl.store.id.configuration.IdTypeConfigurationProvider idTypeConfigurationProvider)
		 private IdGeneratorFactory CreateIdGeneratorFactory( FileSystemAbstraction fileSystem, ReplicatedIdRangeAcquirer idRangeAcquirer, LogProvider logProvider, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  return new ReplicatedIdGeneratorFactory( fileSystem, idRangeAcquirer, logProvider, idTypeConfigurationProvider );
		 }

		 private Locks CreateLockManager( LocksFactory locksFactory, Config config, Clock clock, Replicator replicator, MemberId myself, LeaderLocator leaderLocator, ReplicatedLockTokenStateMachine lockTokenStateMachine )
		 {
			  Locks localLocks = EditionLocksFactories.createLockManager( locksFactory, config, clock );
			  return new LeaderOnlyLockManager( myself, replicator, leaderLocator, localLocks, lockTokenStateMachine );
		 }
	}

}