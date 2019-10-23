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
namespace Neo4Net.causalclustering.core.state.machines
{

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using RaftReplicator = Neo4Net.causalclustering.core.replication.RaftReplicator;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using DummyMachine = Neo4Net.causalclustering.core.state.machines.dummy.DummyMachine;
	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using IdAllocationState = Neo4Net.causalclustering.core.state.machines.id.IdAllocationState;
	using IdReusabilityCondition = Neo4Net.causalclustering.core.state.machines.id.IdReusabilityCondition;
	using ReplicatedIdAllocationStateMachine = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationStateMachine;
	using ReplicatedIdGeneratorFactory = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdGeneratorFactory;
	using ReplicatedIdRangeAcquirer = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdRangeAcquirer;
	using LeaderOnlyLockManager = Neo4Net.causalclustering.core.state.machines.locks.LeaderOnlyLockManager;
	using ReplicatedLockTokenState = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;
	using ReplicatedLockTokenStateMachine = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using ReplicatedLabelTokenHolder = Neo4Net.causalclustering.core.state.machines.token.ReplicatedLabelTokenHolder;
	using ReplicatedPropertyKeyTokenHolder = Neo4Net.causalclustering.core.state.machines.token.ReplicatedPropertyKeyTokenHolder;
	using ReplicatedRelationshipTypeTokenHolder = Neo4Net.causalclustering.core.state.machines.token.ReplicatedRelationshipTypeTokenHolder;
	using ReplicatedTokenStateMachine = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenStateMachine;
	using RecoverConsensusLogIndex = Neo4Net.causalclustering.core.state.machines.tx.RecoverConsensusLogIndex;
	using ReplicatedTransactionCommitProcess = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionCommitProcess;
	using ReplicatedTransactionStateMachine = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionStateMachine;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EditionLocksFactories = Neo4Net.GraphDb.factory.EditionLocksFactories;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using EnterpriseIdTypeConfigurationProvider = Neo4Net.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LocksFactory = Neo4Net.Kernel.impl.locking.LocksFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.array_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.id_alloc_state_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.label_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.label_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.neostore_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.node_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.node_labels_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.property_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.property_key_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.property_key_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.relationship_group_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.relationship_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.relationship_type_token_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.relationship_type_token_name_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.replicated_lock_token_state_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.schema_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.state_machine_apply_max_batch_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.string_block_id_allocation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.EditionLocksFactories.createLockFactory;

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
//ORIGINAL LINE: final org.Neo4Net.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
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

			  TokenRegistry relationshipTypeTokenRegistry = new TokenRegistry( Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE );
			  System.Func<StorageEngine> storageEngineSupplier = () => localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(StorageEngine));
			  ReplicatedRelationshipTypeTokenHolder relationshipTypeTokenHolder = new ReplicatedRelationshipTypeTokenHolder( relationshipTypeTokenRegistry, replicator, this.IdGeneratorFactory, storageEngineSupplier );

			  TokenRegistry propertyKeyTokenRegistry = new TokenRegistry( Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
			  ReplicatedPropertyKeyTokenHolder propertyKeyTokenHolder = new ReplicatedPropertyKeyTokenHolder( propertyKeyTokenRegistry, replicator, this.IdGeneratorFactory, storageEngineSupplier );

			  TokenRegistry labelTokenRegistry = new TokenRegistry( Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL );
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
//ORIGINAL LINE: private org.Neo4Net.kernel.impl.store.id.IdGeneratorFactory createIdGeneratorFactory(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, final org.Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdRangeAcquirer idRangeAcquirer, final org.Neo4Net.logging.LogProvider logProvider, org.Neo4Net.kernel.impl.store.id.configuration.IdTypeConfigurationProvider idTypeConfigurationProvider)
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