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
namespace Neo4Net.causalclustering.core.state
{

	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using GlobalSessionTrackerState = Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using IdAllocationState = Neo4Net.causalclustering.core.state.machines.id.IdAllocationState;
	using ReplicatedLockTokenState = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;
	using LogIndexTxHeaderEncoding = Neo4Net.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateType = Neo4Net.causalclustering.core.state.snapshot.CoreStateType;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecoveryRequiredChecker = Neo4Net.Kernel.impl.recovery.RecoveryRequiredChecker;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadOnlyTransactionIdStore = Neo4Net.Kernel.impl.transaction.log.ReadOnlyTransactionIdStore;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.ARRAY_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.LABEL_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.LABEL_TOKEN_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.NEOSTORE_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.NODE_LABELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.PROPERTY_KEY_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.PROPERTY_KEY_TOKEN_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP_GROUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP_TYPE_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP_TYPE_TOKEN_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdType.STRING_BLOCK;

	public class CoreBootstrapper
	{
		 private const long FIRST_INDEX = 0L;
		 private const long FIRST_TERM = 0L;

		 private readonly DatabaseLayout _databaseLayout;
		 private readonly PageCache _pageCache;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Config _config;
		 private readonly LogProvider _logProvider;
		 private readonly RecoveryRequiredChecker _recoveryRequiredChecker;
		 private readonly Log _log;

		 internal CoreBootstrapper( DatabaseLayout databaseLayout, PageCache pageCache, FileSystemAbstraction fs, Config config, LogProvider logProvider, Monitors monitors )
		 {
			  this._databaseLayout = databaseLayout;
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._config = config;
			  this._logProvider = logProvider;
			  this._log = logProvider.getLog( this.GetType() );
			  this._recoveryRequiredChecker = new RecoveryRequiredChecker( fs, pageCache, config, monitors );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot bootstrap(java.util.Set<org.Neo4Net.causalclustering.identity.MemberId> members) throws Exception
		 public virtual CoreSnapshot Bootstrap( ISet<MemberId> members )
		 {
			  if ( _recoveryRequiredChecker.isRecoveryRequiredAt( _databaseLayout ) )
			  {
					string message = "Cannot bootstrap. Recovery is required. Please ensure that the store being seeded comes from a cleanly shutdown " +
							  "instance of Neo4Net or a Neo4Net backup";
					_log.error( message );
					throw new System.InvalidOperationException( message );
			  }
			  StoreFactory factory = new StoreFactory( _databaseLayout, _config, new DefaultIdGeneratorFactory( _fs ), _pageCache, _fs, _logProvider, EmptyVersionContextSupplier.EMPTY );

			  NeoStores neoStores = factory.OpenAllNeoStores( true );
			  neoStores.Close();

			  CoreSnapshot coreSnapshot = new CoreSnapshot( FIRST_INDEX, FIRST_TERM );
			  coreSnapshot.Add( CoreStateType.ID_ALLOCATION, DeriveIdAllocationState( _databaseLayout ) );
			  coreSnapshot.Add( CoreStateType.LOCK_TOKEN, new ReplicatedLockTokenState() );
			  coreSnapshot.Add( CoreStateType.RAFT_CORE_STATE, new RaftCoreState( new MembershipEntry( FIRST_INDEX, members ) ) );
			  coreSnapshot.Add( CoreStateType.SESSION_TRACKER, new GlobalSessionTrackerState() );
			  AppendNullTransactionLogEntryToSetRaftIndexToMinusOne();
			  return coreSnapshot;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendNullTransactionLogEntryToSetRaftIndexToMinusOne() throws java.io.IOException
		 private void AppendNullTransactionLogEntryToSetRaftIndexToMinusOne()
		 {
			  ReadOnlyTransactionIdStore readOnlyTransactionIdStore = new ReadOnlyTransactionIdStore( _pageCache, _databaseLayout );
			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( _databaseLayout, _fs, _pageCache ).withConfig( _config ).withLastCommittedTransactionIdSupplier( () => readOnlyTransactionIdStore.LastClosedTransactionId - 1 ).build();

			  long dummyTransactionId;
			  using ( Lifespan lifespan = new Lifespan( logFiles ) )
			  {
					FlushableChannel channel = logFiles.LogFile.Writer;
					TransactionLogWriter writer = new TransactionLogWriter( new LogEntryWriter( channel ) );

					long lastCommittedTransactionId = readOnlyTransactionIdStore.LastCommittedTransactionId;
					PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( Collections.emptyList() );
					sbyte[] txHeaderBytes = LogIndexTxHeaderEncoding.encodeLogIndexAsTxHeader( -1 );
					tx.SetHeader( txHeaderBytes, -1, -1, -1, lastCommittedTransactionId, -1, -1 );

					dummyTransactionId = lastCommittedTransactionId + 1;
					writer.Append( tx, dummyTransactionId );
					channel.PrepareForFlush().flush();
			  }

			  File neoStoreFile = _databaseLayout.metadataStore();
			  MetaDataStore.setRecord( _pageCache, neoStoreFile, LAST_TRANSACTION_ID, dummyTransactionId );
		 }

		 private IdAllocationState DeriveIdAllocationState( DatabaseLayout databaseLayout )
		 {
			  DefaultIdGeneratorFactory factory = new DefaultIdGeneratorFactory( _fs );

			  long[] highIds = new long[]{ GetHighId( factory, NODE, databaseLayout.IdNodeStore() ), GetHighId(factory, RELATIONSHIP, databaseLayout.IdRelationshipStore()), GetHighId(factory, PROPERTY, databaseLayout.IdPropertyStore()), GetHighId(factory, STRING_BLOCK, databaseLayout.IdPropertyStringStore()), GetHighId(factory, ARRAY_BLOCK, databaseLayout.IdPropertyArrayStore()), GetHighId(factory, PROPERTY_KEY_TOKEN, databaseLayout.IdPropertyKeyTokenStore()), GetHighId(factory, PROPERTY_KEY_TOKEN_NAME, databaseLayout.IdPropertyKeyTokenNamesStore()), GetHighId(factory, RELATIONSHIP_TYPE_TOKEN, databaseLayout.IdRelationshipTypeTokenStore()), GetHighId(factory, RELATIONSHIP_TYPE_TOKEN_NAME, databaseLayout.IdRelationshipTypeTokenNamesStore()), GetHighId(factory, LABEL_TOKEN, databaseLayout.IdLabelTokenStore()), GetHighId(factory, LABEL_TOKEN_NAME, databaseLayout.IdLabelTokenNamesStore()), GetHighId(factory, NEOSTORE_BLOCK, databaseLayout.IdMetadataStore()), GetHighId(factory, SCHEMA, databaseLayout.IdSchemaStore()), GetHighId(factory, NODE_LABELS, databaseLayout.IdNodeLabelStore()), GetHighId(factory, RELATIONSHIP_GROUP, databaseLayout.IdRelationshipGroupStore()) };

			  return new IdAllocationState( highIds, FIRST_INDEX );
		 }

		 private static long GetHighId( DefaultIdGeneratorFactory factory, IdType idType, File idFile )
		 {
			  IdGenerator idGenerator = factory.Open( idFile, idType, () => -1L, long.MaxValue );
			  long highId = idGenerator.HighId;
			  idGenerator.Dispose();
			  return highId;
		 }
	}

}