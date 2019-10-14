using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.tools.rebuild
{

	using ExternallyManagedPageCache = Neo4Net.com.storecopy.ExternallyManagedPageCache;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using InconsistentStoreException = Neo4Net.Consistency.checking.InconsistentStoreException;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using FullCheck = Neo4Net.Consistency.checking.full.FullCheck;
	using ConsistencySummaryStatistics = Neo4Net.Consistency.report.ConsistencySummaryStatistics;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using Neo4Net.Cursors;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using Args = Neo4Net.Helpers.Args;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using DirectStoreAccess = Neo4Net.Kernel.api.direct.DirectStoreAccess;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionQueue = Neo4Net.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Neo4Net.Kernel.impl.core.ReadOnlyTokenCreator;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using Neo4Net.Kernel.impl.transaction.log;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using ReaderLogVersionBridge = Neo4Net.Kernel.impl.transaction.log.ReaderLogVersionBridge;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using FormattedLog = Neo4Net.Logging.FormattedLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// Tool to rebuild store based on available transaction logs.
	/// </summary>
	internal class RebuildFromLogs
	{
		 private const string UP_TO_TX_ID = "tx";

		 private readonly FileSystemAbstraction _fs;

		 internal RebuildFromLogs( FileSystemAbstraction fs )
		 {
			  this._fs = fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception, org.neo4j.consistency.checking.InconsistentStoreException
		 public static void Main( string[] args )
		 {
			  if ( args == null )
			  {
					PrintUsage();
					return;
			  }
			  Args @params = Args.parse( args );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") long txId = params.getNumber(UP_TO_TX_ID, BASE_TX_ID).longValue();
			  long txId = @params.GetNumber( UP_TO_TX_ID, BASE_TX_ID ).longValue();
			  IList<string> orphans = @params.Orphans();
			  args = orphans.ToArray();
			  if ( args.Length != 2 )
			  {
					PrintUsage( "Exactly two positional arguments expected: " + "<source dir with logs> <target dir for graphdb>, got " + args.Length );
					Environment.Exit( -1 );
					return;
			  }
			  File source = new File( args[0] );
			  File target = new File( args[1] );
			  if ( !source.Directory )
			  {
					PrintUsage( source + " is not a directory" );
					Environment.Exit( -1 );
					return;
			  }
			  if ( target.exists() )
			  {
					if ( target.Directory )
					{
						 if ( DirectoryContainsDb( target.toPath() ) )
						 {
							  PrintUsage( "target graph database already exists" );
							  Environment.Exit( -1 );
							  return;
						 }
						 Console.Error.WriteLine( "WARNING: the directory " + target + " already exists" );
					}
					else
					{
						 PrintUsage( target + " is a file" );
						 Environment.Exit( -1 );
						 return;
					}
			  }

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					( new RebuildFromLogs( fileSystem ) ).Rebuild( source, target, txId );
			  }
		 }

		 private static bool DirectoryContainsDb( Path path )
		 {
			  return Files.exists( DatabaseLayout.of( path.toFile() ).metadataStore().toPath() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void rebuild(java.io.File source, java.io.File target, long txId) throws Exception, org.neo4j.consistency.checking.InconsistentStoreException
		 public virtual void Rebuild( File source, File target, long txId )
		 {
			  using ( PageCache pageCache = StandalonePageCacheFactory.createPageCache( _fs, createInitializedScheduler() ) )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( source, _fs ).build();
					long highestVersion = logFiles.HighestLogVersion;
					if ( highestVersion < 0 )
					{
						 PrintUsage( "Inconsistent number of log files found in " + source );
						 return;
					}

					long lastTxId;
					using ( TransactionApplier applier = new TransactionApplier( _fs, target, pageCache ) )
					{
						 lastTxId = applier.ApplyTransactionsFrom( source, txId );
					}

					// set last tx id in neostore otherwise the db is not usable
					MetaDataStore.setRecord( pageCache, DatabaseLayout.of( target ).metadataStore(), MetaDataStore.Position.LAST_TRANSACTION_ID, lastTxId );

					CheckConsistency( target, pageCache );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkConsistency(java.io.File target, org.neo4j.io.pagecache.PageCache pageCache) throws Exception, org.neo4j.consistency.checking.InconsistentStoreException
		 internal virtual void CheckConsistency( File target, PageCache pageCache )
		 {
			  using ( ConsistencyChecker checker = new ConsistencyChecker( target, pageCache ) )
			  {
					checker.CheckConsistency();
			  }
		 }

		 private static void PrintUsage( params string[] msgLines )
		 {
			  foreach ( string line in msgLines )
			  {
					Console.Error.WriteLine( line );
			  }
			  Console.Error.WriteLine( Args.jarUsage( typeof( RebuildFromLogs ), "[-full] <source dir with logs> <target dir for graphdb>" ) );
			  Console.Error.WriteLine( "WHERE:   <source dir>  is the path for where transactions to rebuild from are stored" );
			  Console.Error.WriteLine( "         <target dir>  is the path for where to create the new graph database" );
			  Console.Error.WriteLine( "         -tx       --  to rebuild the store up to a given transaction" );
		 }

		 private class TransactionApplier : AutoCloseable
		 {
			  internal readonly GraphDatabaseAPI Graphdb;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly TransactionCommitProcess CommitProcess;

			  internal TransactionApplier( FileSystemAbstraction fs, File dbDirectory, PageCache pageCache )
			  {
					this.Fs = fs;
					this.Graphdb = StartTemporaryDb( dbDirectory.AbsoluteFile, pageCache );
					this.CommitProcess = Graphdb.DependencyResolver.resolveDependency( typeof( TransactionCommitProcess ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long applyTransactionsFrom(java.io.File sourceDir, long upToTxId) throws Exception
			  internal virtual long ApplyTransactionsFrom( File sourceDir, long upToTxId )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( sourceDir, Fs ).build();
					int startVersion = 0;
					ReaderLogVersionBridge versionBridge = new ReaderLogVersionBridge( logFiles );
					PhysicalLogVersionedStoreChannel startingChannel = logFiles.OpenForVersion( startVersion );
					ReadableLogChannel channel = new ReadAheadLogChannel( startingChannel, versionBridge );
					long txId = BASE_TX_ID;
					TransactionQueue queue = new TransactionQueue( 10_000, ( tx, last ) => CommitProcess.commit( tx, NULL, EXTERNAL ) );
					LogEntryReader<ReadableClosablePositionAwareChannel> entryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					using ( IOCursor<CommittedTransactionRepresentation> cursor = new PhysicalTransactionCursor<CommittedTransactionRepresentation>( channel, entryReader ) )
					{
						 while ( cursor.next() )
						 {
							  txId = cursor.get().CommitEntry.TxId;
							  TransactionRepresentation transaction = cursor.get().TransactionRepresentation;
							  queue.Queue( new TransactionToApply( transaction, txId ) );
							  if ( upToTxId != BASE_TX_ID && upToTxId == txId )
							  {
									break;
							  }
						 }
					}
					queue.Empty();
					return txId;
			  }

			  public override void Close()
			  {
					Graphdb.shutdown();
			  }
		 }

		 private class ConsistencyChecker : AutoCloseable
		 {
			  internal readonly GraphDatabaseAPI Graphdb;
			  internal readonly LabelScanStore LabelScanStore;
			  internal readonly Config TuningConfiguration = Config.defaults();
			  internal readonly IndexProviderMap Indexes;

			  internal ConsistencyChecker( File dbDirectory, PageCache pageCache )
			  {
					this.Graphdb = StartTemporaryDb( dbDirectory.AbsoluteFile, pageCache );
					DependencyResolver resolver = Graphdb.DependencyResolver;
					this.LabelScanStore = resolver.ResolveDependency( typeof( LabelScanStore ) );
					this.Indexes = resolver.ResolveDependency( typeof( IndexProviderMap ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkConsistency() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException, org.neo4j.consistency.checking.InconsistentStoreException
			  internal virtual void CheckConsistency()
			  {
					StoreAccess nativeStores = ( new StoreAccess( Graphdb.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores() ) ).initialize();

					TokenHolders tokenHolders = new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );

					DirectStoreAccess stores = new DirectStoreAccess( nativeStores, LabelScanStore, Indexes, tokenHolders );

					FullCheck fullCheck = new FullCheck( TuningConfiguration, ProgressMonitorFactory.textual( System.err ), Statistics.NONE, ConsistencyCheckService.defaultConsistencyCheckThreadsNumber(), false );

					ConsistencySummaryStatistics summaryStatistics = fullCheck.Execute( stores, FormattedLog.toOutputStream( System.err ) );
					if ( !summaryStatistics.Consistent )
					{
						 throw new InconsistentStoreException( summaryStatistics );
					}

			  }

			  public override void Close()
			  {
					Graphdb.shutdown();
			  }
		 }

		 internal static GraphDatabaseAPI StartTemporaryDb( File targetDirectory, PageCache pageCache )
		 {
			  GraphDatabaseFactory factory = ExternallyManagedPageCache.graphDatabaseFactoryWithPageCache( pageCache );
			  return ( GraphDatabaseAPI ) factory.NewEmbeddedDatabaseBuilder( targetDirectory ).newGraphDatabase();
		 }
	}

}