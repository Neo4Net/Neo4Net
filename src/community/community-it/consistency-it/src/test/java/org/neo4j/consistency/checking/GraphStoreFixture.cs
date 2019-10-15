using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Consistency.checking
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using AccessStatistics = Neo4Net.Consistency.statistics.AccessStatistics;
	using AccessStatsKeepingStoreAccess = Neo4Net.Consistency.statistics.AccessStatsKeepingStoreAccess;
	using DefaultCounts = Neo4Net.Consistency.statistics.DefaultCounts;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using VerboseStatistics = Neo4Net.Consistency.statistics.VerboseStatistics;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using DirectStoreAccess = Neo4Net.Kernel.api.direct.DirectStoreAccess;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Neo4Net.Kernel.extension.DatabaseKernelExtensions;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexStoreView = Neo4Net.Kernel.Impl.Api.index.IndexStoreView;
	using FullLabelStream = Neo4Net.Kernel.Impl.Api.scan.FullLabelStream;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Neo4Net.Kernel.impl.core.ReadOnlyTokenCreator;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using IJobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using ConfigurablePageCacheRule = Neo4Net.Test.rule.ConfigurablePageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.ConsistencyCheckService.defaultConsistencyCheckThreadsNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.Internal.SchemaIndexExtensionLoader.instantiateKernelExtensions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;

	public abstract class GraphStoreFixture : ConfigurablePageCacheRule, TestRule
	{
		 private DirectStoreAccess _directStoreAccess;
		 private Statistics _statistics;
		 private readonly bool _keepStatistics;
		 private NeoStores _neoStore;
		 private TestDirectory _directory;
		 private long _schemaId;
		 private long _nodeId;
		 private int _labelId;
		 private long _nodeLabelsId;
		 private long _relId;
		 private long _relGroupId;
		 private int _propId;
		 private long _stringPropId;
		 private long _arrayPropId;
		 private int _relTypeId;
		 private int _propKeyId;
		 private DefaultFileSystemAbstraction _fileSystem;
		 private readonly LifeSupport _life = new LifeSupport();

		 /// <summary>
		 /// Record format used to generate initial database.
		 /// </summary>
		 private string _formatName;

		 private GraphStoreFixture( bool keepStatistics, string formatName )
		 {
			  this._keepStatistics = keepStatistics;
			  this._formatName = formatName;
		 }

		 protected internal GraphStoreFixture( string formatName ) : this( false, formatName )
		 {
		 }

		 protected internal override void After( bool success )
		 {
			  base.After( success );
			  _life.shutdown();
			  if ( _fileSystem != null )
			  {
					try
					{
						 _fileSystem.Dispose();
					}
					catch ( IOException e )
					{
						 throw new AssertionError( "Failed to stop file system after test", e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(Transaction transaction) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void Apply( Transaction transaction )
		 {
			  ApplyTransaction( transaction );
		 }

		 public virtual DirectStoreAccess DirectStoreAccess()
		 {
			  return DirectStoreAccess( false );
		 }

		 public virtual DirectStoreAccess ReadOnlyDirectStoreAccess()
		 {
			  return DirectStoreAccess( true );
		 }

		 private DirectStoreAccess DirectStoreAccess( bool readOnly )
		 {
			  if ( _directStoreAccess == null )
			  {
					_life.start();
					JobScheduler scheduler = _life.add( IJobSchedulerFactory.createInitializedScheduler() );
					_fileSystem = new DefaultFileSystemAbstraction();
					PageCache pageCache = GetPageCache( _fileSystem );
					LogProvider logProvider = NullLogProvider.Instance;
					Config config = Config.defaults( GraphDatabaseSettings.read_only, readOnly ? TRUE : FALSE );
					DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( _fileSystem );
					StoreFactory storeFactory = new StoreFactory( _directory.databaseLayout(), config, idGeneratorFactory, pageCache, _fileSystem, logProvider, EmptyVersionContextSupplier.EMPTY );
					_neoStore = storeFactory.OpenAllNeoStores();
					StoreAccess nativeStores;
					if ( _keepStatistics )
					{
						 AccessStatistics accessStatistics = new AccessStatistics();
						 _statistics = new VerboseStatistics( accessStatistics, new DefaultCounts( defaultConsistencyCheckThreadsNumber() ), NullLog.Instance );
						 nativeStores = new AccessStatsKeepingStoreAccess( _neoStore, accessStatistics );
					}
					else
					{
						 _statistics = Statistics.NONE;
						 nativeStores = new StoreAccess( _neoStore );
					}
					nativeStores.Initialize();

					IndexStoreView indexStoreView = new NeoStoreIndexStoreView( LockService.NO_LOCK_SERVICE, nativeStores.RawNeoStores );

					Monitors monitors = new Monitors();
					LabelScanStore labelScanStore = StartLabelScanStore( pageCache, indexStoreView, monitors, readOnly );
					IndexProviderMap indexes = CreateIndexes( pageCache, _fileSystem, _directory.databaseDir(), config, scheduler, logProvider, monitors );
					TokenHolders tokenHolders = new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );
					tokenHolders.PropertyKeyTokens().InitialTokens = _neoStore.PropertyKeyTokenStore.Tokens;
					tokenHolders.LabelTokens().InitialTokens = _neoStore.LabelTokenStore.Tokens;
					tokenHolders.RelationshipTypeTokens().InitialTokens = _neoStore.RelationshipTypeTokenStore.Tokens;
					_directStoreAccess = new DirectStoreAccess( nativeStores, labelScanStore, indexes, tokenHolders );
			  }
			  return _directStoreAccess;
		 }

		 private LabelScanStore StartLabelScanStore( PageCache pageCache, IndexStoreView indexStoreView, Monitors monitors, bool readOnly )
		 {
			  NativeLabelScanStore labelScanStore = new NativeLabelScanStore( pageCache, _directory.databaseLayout(), _fileSystem, new FullLabelStream(indexStoreView), readOnly, monitors, RecoveryCleanupWorkCollector.immediate() );
			  try
			  {
					labelScanStore.Init();
					labelScanStore.Start();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  return labelScanStore;
		 }

		 private IndexProviderMap CreateIndexes( PageCache pageCache, FileSystemAbstraction fileSystem, File storeDir, Config config, IJobScheduler scheduler, LogProvider logProvider, Monitors monitors )
		 {
			  LogService logService = new SimpleLogService( logProvider, logProvider );
			  TokenHolders tokenHolders = new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );
			  DatabaseKernelExtensions extensions = _life.add( instantiateKernelExtensions( storeDir, fileSystem, config, logService, pageCache, scheduler, RecoveryCleanupWorkCollector.ignore(), DatabaseInfo.COMMUNITY, monitors, tokenHolders ) );
			  return _life.add( new DefaultIndexProviderMap( extensions, config ) );
		 }

		 public virtual DatabaseLayout DatabaseLayout()
		 {
			  return _directory.databaseLayout();
		 }

		 public virtual Statistics AccessStatistics
		 {
			 get
			 {
				  return _statistics;
			 }
		 }

		 public abstract class Transaction
		 {
			  internal readonly long StartTimestamp = currentTimeMillis();

			  protected internal abstract void TransactionData( TransactionDataBuilder tx, IdGenerator next );

			  public virtual TransactionRepresentation Representation( IdGenerator idGenerator, int masterId, int authorId, long lastCommittedTx, NeoStores neoStores )
			  {
					TransactionWriter writer = new TransactionWriter( neoStores );
					TransactionData( new TransactionDataBuilder( writer, neoStores.NodeStore ), idGenerator );
					idGenerator.UpdateCorrespondingIdGenerators( neoStores );
					return writer.Representation( new sbyte[0], masterId, authorId, StartTimestamp, lastCommittedTx, currentTimeMillis() );
			  }
		 }

		 public virtual IdGenerator IdGenerator()
		 {
			  return new IdGenerator( this );
		 }

		 public class IdGenerator
		 {
			 private readonly GraphStoreFixture _outerInstance;

			 public IdGenerator( GraphStoreFixture outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public virtual long Schema()
			  {
					return outerInstance.schemaId++;
			  }

			  public virtual long Node()
			  {
					return outerInstance.nodeId++;
			  }

			  public virtual int Label()
			  {
					return outerInstance.labelId++;
			  }

			  public virtual long NodeLabel()
			  {
					return outerInstance.nodeLabelsId++;
			  }

			  public virtual long Relationship()
			  {
					return outerInstance.relId++;
			  }

			  public virtual long RelationshipGroup()
			  {
					return outerInstance.relGroupId++;
			  }

			  public virtual long Property()
			  {
					return outerInstance.propId++;
			  }

			  public virtual long StringProperty()
			  {
					return outerInstance.stringPropId++;
			  }

			  public virtual long ArrayProperty()
			  {
					return outerInstance.arrayPropId++;
			  }

			  public virtual int RelationshipType()
			  {
					return outerInstance.relTypeId++;
			  }

			  public virtual int PropertyKey()
			  {
					return outerInstance.propKeyId++;
			  }

			  internal virtual void UpdateCorrespondingIdGenerators( NeoStores neoStores )
			  {
					neoStores.NodeStore.HighestPossibleIdInUse = outerInstance.nodeId;
					neoStores.RelationshipStore.HighestPossibleIdInUse = outerInstance.relId;
					neoStores.RelationshipGroupStore.HighestPossibleIdInUse = outerInstance.relGroupId;
			  }
		 }

		 public sealed class TransactionDataBuilder
		 {
			  internal readonly TransactionWriter Writer;
			  internal readonly NodeStore Nodes;

			  internal TransactionDataBuilder( TransactionWriter writer, NodeStore nodes )
			  {
					this.Writer = writer;
					this.Nodes = nodes;
			  }

			  public void CreateSchema( ICollection<DynamicRecord> beforeRecords, ICollection<DynamicRecord> afterRecords, SchemaRule rule )
			  {
					Writer.createSchema( beforeRecords, afterRecords, rule );
			  }

			  // In the following three methods there's an assumption that all tokens use one dynamic record
			  // and since the first record in a dynamic store the id starts at 1 instead of 0... hence the +1

			  public void PropertyKey( int id, string key )
			  {
					Writer.propertyKey( id, key, id + 1 );
			  }

			  public void NodeLabel( int id, string name )
			  {
					Writer.label( id, name, id + 1 );
			  }

			  public void RelationshipType( int id, string relationshipType )
			  {
					Writer.relationshipType( id, relationshipType, id + 1 );
			  }

			  public void Update( NeoStoreRecord before, NeoStoreRecord after )
			  {
					Writer.update( before, after );
			  }

			  public void Create( NodeRecord node )
			  {
					UpdateCounts( node, 1 );
					Writer.create( node );
			  }

			  public void Update( NodeRecord before, NodeRecord after )
			  {
					UpdateCounts( before, -1 );
					UpdateCounts( after, 1 );
					Writer.update( before, after );
			  }

			  public void Delete( NodeRecord node )
			  {
					UpdateCounts( node, -1 );
					Writer.delete( node );
			  }

			  public void Create( RelationshipRecord relationship )
			  {
					Writer.create( relationship );
			  }

			  public void Update( RelationshipRecord before, RelationshipRecord after )
			  {
					Writer.update( before, after );
			  }

			  public void Delete( RelationshipRecord relationship )
			  {
					Writer.delete( relationship );
			  }

			  public void Create( RelationshipGroupRecord group )
			  {
					Writer.create( group );
			  }

			  public void Update( RelationshipGroupRecord before, RelationshipGroupRecord after )
			  {
					Writer.update( before, after );
			  }

			  public void Delete( RelationshipGroupRecord group )
			  {
					Writer.delete( group );
			  }

			  public void Create( PropertyRecord property )
			  {
					Writer.create( property );
			  }

			  public void Update( PropertyRecord before, PropertyRecord property )
			  {
					Writer.update( before, property );
			  }

			  public void Delete( PropertyRecord before, PropertyRecord property )
			  {
					Writer.delete( before, property );
			  }

			  internal void UpdateCounts( NodeRecord node, int delta )
			  {
					Writer.incrementNodeCount( StatementConstants.ANY_LABEL, delta );
					foreach ( long label in NodeLabelsField.parseLabelsField( node ).get( Nodes ) )
					{
						 Writer.incrementNodeCount( ( int )label, delta );
					}
			  }

			  public void IncrementNodeCount( int labelId, long delta )
			  {
					Writer.incrementNodeCount( labelId, delta );
			  }

			  public void IncrementRelationshipCount( int startLabelId, int typeId, int endLabelId, long delta )
			  {
					Writer.incrementRelationshipCount( startLabelId, typeId, endLabelId, delta );
			  }
		 }

		 protected internal abstract void GenerateInitialData( GraphDatabaseService graphDb );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected void start(@SuppressWarnings("UnusedParameters") java.io.File storeDir)
		 protected internal virtual void Start( File storeDir )
		 {
			  // allow for override
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void stop() throws Throwable
		 protected internal virtual void Stop()
		 {
			  if ( _directStoreAccess != null )
			  {
					_neoStore.close();
					_directStoreAccess.Dispose();
					_directStoreAccess = null;
			  }
		 }

		 private int MyId()
		 {
			  return 1;
		 }

		 private int MasterId()
		 {
			  return -1;
		 }

		 public class Applier : IDisposable
		 {
			 private readonly GraphStoreFixture _outerInstance;

			  internal readonly GraphDatabaseAPI Database;
			  internal readonly TransactionRepresentationCommitProcess CommitProcess;
			  internal readonly TransactionIdStore TransactionIdStore;
			  internal readonly NeoStores NeoStores;

			  internal Applier( GraphStoreFixture outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Database = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(outerInstance.directory.DatabaseDir()).setConfig("dbms.backup.enabled", "false").newGraphDatabase();
					DependencyResolver dependencyResolver = Database.DependencyResolver;

					CommitProcess = new TransactionRepresentationCommitProcess( dependencyResolver.ResolveDependency( typeof( TransactionAppender ) ), dependencyResolver.ResolveDependency( typeof( StorageEngine ) ) );
					TransactionIdStore = Database.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );

					NeoStores = Database.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(Transaction transaction) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  public virtual void Apply( Transaction transaction )
			  {
					TransactionRepresentation representation = transaction.Representation( outerInstance.IdGenerator(), outerInstance.masterId(), outerInstance.myId(), TransactionIdStore.LastCommittedTransactionId, NeoStores );
					CommitProcess.commit( new TransactionToApply( representation ), CommitEvent.NULL, TransactionApplicationMode.EXTERNAL );
			  }

			  public override void Close()
			  {
					Database.shutdown();
			  }
		 }

		 public virtual Applier CreateApplier()
		 {
			  return new Applier( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyTransaction(Transaction transaction) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void ApplyTransaction( Transaction transaction )
		 {
			  // TODO you know... we could have just appended the transaction representation to the log
			  // and the next startup of the store would do recovery where the transaction would have been
			  // applied and all would have been well.

			  using ( Applier applier = CreateApplier() )
			  {
					applier.Apply( transaction );
			  }
		 }

		 private void GenerateInitialData()
		 {
			  GraphDatabaseBuilder builder = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_directory.databaseDir());
			  GraphDatabaseAPI graphDb = ( GraphDatabaseAPI ) builder.SetConfig( GraphDatabaseSettings.record_format, _formatName ).setConfig( GraphDatabaseSettings.label_block_size, "60" ).setConfig( "dbms.backup.enabled", "false" ).newGraphDatabase();
			  try
			  {
					GenerateInitialData( graphDb );
					StoreAccess stores = ( new StoreAccess( graphDb.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores() ) ).initialize();
					_schemaId = stores.SchemaStore.HighId;
					_nodeId = stores.NodeStore.HighId;
					_labelId = ( int ) stores.LabelTokenStore.HighId;
					_nodeLabelsId = stores.NodeDynamicLabelStore.HighId;
					_relId = stores.RelationshipStore.HighId;
					_relGroupId = stores.RelationshipGroupStore.HighId;
					_propId = ( int ) stores.PropertyStore.HighId;
					_stringPropId = stores.StringStore.HighId;
					_arrayPropId = stores.ArrayStore.HighId;
					_relTypeId = ( int ) stores.RelationshipTypeTokenStore.HighId;
					_propKeyId = ( int ) stores.PropertyKeyNameStore.HighId;
			  }
			  finally
			  {
					graphDb.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(description.getTestClass());
			  TestDirectory directory = TestDirectory.testDirectory( description.TestClass );
			  return base.apply(directory.apply(new StatementAnonymousInnerClass(this, @base, directory)
			 , description), description);
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly GraphStoreFixture _outerInstance;

			 private Statement @base;
			 private TestDirectory _directory;

			 public StatementAnonymousInnerClass( GraphStoreFixture outerInstance, Statement @base, TestDirectory directory )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._directory = directory;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  _outerInstance.directory = _directory;
				  try
				  {
						outerInstance.generateInitialData();
						_outerInstance.start( _outerInstance.directory.databaseDir() );
						try
						{
							 @base.evaluate();
						}
						finally
						{
							 outerInstance.Stop();
						}
				  }
				  finally
				  {
						_outerInstance.directory = null;
				  }
			 }
		 }
	}

}