using System;
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
namespace Neo4Net.Consistency
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Result = Neo4Net.Consistency.ConsistencyCheckService.Result;
	using GraphStoreFixture = Neo4Net.Consistency.checking.GraphStoreFixture;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Strings = Neo4Net.Helpers.Strings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.LUCENE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.property;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.set;

	public class ConsistencyCheckServiceIntegrationTest
	{
		private bool InstanceFieldsInitialized = false;

		public ConsistencyCheckServiceIntegrationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			Chain = RuleChain.outerRule( _testDirectory ).around( fixture );
		}

		 private GraphStoreFixture fixture = new GraphStoreFixtureAnonymousInnerClass( RecordFormatName );

		 private class GraphStoreFixtureAnonymousInnerClass : GraphStoreFixture
		 {
			 public GraphStoreFixtureAnonymousInnerClass( string getRecordFormatName ) : base( getRecordFormatName )
			 {
			 }

			 protected internal override void generateInitialData( GraphDatabaseService graphDb )
			 {
				  using ( Transaction tx = graphDb.BeginTx() )
				  {
						Node node1 = set( graphDb.CreateNode() );
						Node node2 = set( graphDb.CreateNode(), property("key", "exampleValue") );
						node1.CreateRelationshipTo( node2, RelationshipType.withName( "C" ) );
						tx.Success();
				  }
			 }
		 }

		 private readonly DefaultFileSystemAbstraction _fs = new DefaultFileSystemAbstraction();
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fixture);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportNotUsedRelationshipReferencedInChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportNotUsedRelationshipReferencedInChain()
		 {
			  PrepareDbWithDeletedRelationshipPartOfTheChain();

			  DateTime timestamp = DateTime.Now;
			  ConsistencyCheckService service = new ConsistencyCheckService( timestamp );
			  Config configuration = Config.defaults( Settings() );

			  ConsistencyCheckService.Result result = RunFullConsistencyCheck( service, configuration );

			  assertFalse( result.Successful );

			  File reportFile = result.ReportFile();
			  assertTrue( "Consistency check report file should be generated.", reportFile.exists() );
			  assertThat( "Expected to see report about not deleted relationship record present as part of a chain", Files.readAllLines( reportFile.toPath() ).ToString(), containsString("The relationship record is not in use, but referenced from relationships chain.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnDatabaseInNeedOfRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnDatabaseInNeedOfRecovery()
		 {
			  NonRecoveredDatabase();
			  ConsistencyCheckService service = new ConsistencyCheckService();
			  try
			  {
					IDictionary<string, string> settings = settings();
					Config defaults = Config.defaults( settings );
					RunFullConsistencyCheck( service, defaults );
					fail();
			  }
			  catch ( ConsistencyCheckIncompleteException e )
			  {
					assertEquals( e.InnerException.Message, Strings.joinAsLines( "Active logical log detected, this might be a source of inconsistencies.", "Please recover database.", "To perform recovery please start database in single mode and perform clean shutdown." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ableToDeleteDatabaseDirectoryAfterConsistencyCheckRun() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AbleToDeleteDatabaseDirectoryAfterConsistencyCheckRun()
		 {
			  PrepareDbWithDeletedRelationshipPartOfTheChain();
			  ConsistencyCheckService service = new ConsistencyCheckService();
			  Result consistencyCheck = RunFullConsistencyCheck( service, Config.defaults( Settings() ) );
			  assertFalse( consistencyCheck.Successful );
			  // using commons file utils since they do not forgive not closed file descriptors on windows
			  org.apache.commons.io.FileUtils.deleteDirectory( fixture.databaseLayout().databaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedIfStoreIsConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedIfStoreIsConsistent()
		 {
			  // given
			  DateTime timestamp = DateTime.Now;
			  ConsistencyCheckService service = new ConsistencyCheckService( timestamp );
			  Config configuration = Config.defaults( Settings() );

			  // when
			  ConsistencyCheckService.Result result = RunFullConsistencyCheck( service, configuration );

			  // then
			  assertTrue( result.Successful );
			  File reportFile = result.ReportFile();
			  assertFalse( "Unexpected generation of consistency check report file: " + reportFile, reportFile.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTheStoreInNotConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfTheStoreInNotConsistent()
		 {
			  // given
			  BreakNodeStore();
			  DateTime timestamp = DateTime.Now;
			  ConsistencyCheckService service = new ConsistencyCheckService( timestamp );
			  string logsDir = _testDirectory.directory().Path;
			  Config configuration = Config.defaults( Settings( GraphDatabaseSettings.logs_directory.name(), logsDir ) );

			  // when
			  ConsistencyCheckService.Result result = RunFullConsistencyCheck( service, configuration );

			  // then
			  assertFalse( result.Successful );
			  string reportFile = format( "inconsistencies-%s.report", ( new SimpleDateFormat( "yyyy-MM-dd.HH.mm.ss" ) ).format( timestamp ) );
			  assertEquals( new File( logsDir, reportFile ), result.ReportFile() );
			  assertTrue( "Inconsistency report file not generated", result.ReportFile().exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportDuplicateForHugeLongValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportDuplicateForHugeLongValues()
		 {
			  // given
			  ConsistencyCheckService service = new ConsistencyCheckService();
			  Config configuration = Config.defaults( Settings() );
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_testDirectory.storeDir()).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig("dbms.backup.enabled", "false").newGraphDatabase();

			  string propertyKey = "itemId";
			  Label label = Label.label( "Item" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					set( Db.createNode( label ), property( propertyKey, 973305894188596880L ) );
					set( Db.createNode( label ), property( propertyKey, 973305894188596864L ) );
					tx.Success();
			  }
			  Db.shutdown();

			  // when
			  Result result = RunFullConsistencyCheck( service, configuration );

			  // then
			  assertTrue( result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowGraphCheckDisabled() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowGraphCheckDisabled()
		 {
			  GraphDatabaseService gds = GraphDatabaseService;

			  using ( Transaction tx = gds.BeginTx() )
			  {
					gds.CreateNode();
					tx.Success();
			  }

			  gds.Shutdown();

			  ConsistencyCheckService service = new ConsistencyCheckService();
			  Config configuration = Config.defaults( Settings( ConsistencyCheckSettings.ConsistencyCheckGraph.name(), Settings.FALSE ) );

			  // when
			  Result result = RunFullConsistencyCheck( service, configuration );

			  // then
			  assertTrue( result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMissingSchemaIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMissingSchemaIndex()
		 {
			  // given
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  GraphDatabaseService gds = GetGraphDatabaseService( databaseLayout.DatabaseDirectory() );

			  Label label = Label.label( "label" );
			  string propKey = "propKey";
			  CreateIndex( gds, label, propKey );

			  gds.Shutdown();

			  // when
			  File schemaDir = FindFile( databaseLayout, "schema" );
			  FileUtils.deleteRecursively( schemaDir );

			  ConsistencyCheckService service = new ConsistencyCheckService();
			  Config configuration = Config.defaults( Settings() );
			  Result result = RunFullConsistencyCheck( service, configuration, databaseLayout );

			  // then
			  assertTrue( result.Successful );
			  File reportFile = result.ReportFile();
			  assertTrue( "Consistency check report file should be generated.", reportFile.exists() );
			  assertThat( "Expected to see report about schema index not being online", Files.readAllLines( reportFile.toPath() ).ToString(), allOf(containsString("schema rule"), containsString("not online")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oldLuceneSchemaIndexShouldBeConsideredConsistentWithFusionProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OldLuceneSchemaIndexShouldBeConsideredConsistentWithFusionProvider()
		 {
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  string defaultSchemaProvider = GraphDatabaseSettings.default_schema_provider.name();
			  Label label = Label.label( "label" );
			  string propKey = "propKey";

			  // Given a lucene index
			  GraphDatabaseService db = GetGraphDatabaseService( databaseLayout.DatabaseDirectory(), defaultSchemaProvider, LUCENE10.providerName() );
			  CreateIndex( db, label, propKey );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label ).setProperty( propKey, 1 );
					Db.createNode( label ).setProperty( propKey, "string" );
					tx.Success();
			  }
			  Db.shutdown();

			  ConsistencyCheckService service = new ConsistencyCheckService();
			  Config configuration = Config.defaults( Settings( defaultSchemaProvider, NATIVE20.providerName() ) );
			  Result result = RunFullConsistencyCheck( service, configuration, databaseLayout );
			  assertTrue( result.Successful );
		 }

		 private static void CreateIndex( GraphDatabaseService gds, Label label, string propKey )
		 {
			  IndexDefinition indexDefinition;

			  using ( Transaction tx = gds.BeginTx() )
			  {
					indexDefinition = gds.Schema().indexFor(label).on(propKey).create();
					tx.Success();
			  }

			  using ( Transaction tx = gds.BeginTx() )
			  {
					gds.Schema().awaitIndexOnline(indexDefinition, 1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private static File FindFile( DatabaseLayout databaseLayout, string targetFile )
		 {
			  File file = databaseLayout.File( targetFile );
			  if ( !file.exists() )
			  {
					fail( "Could not find file " + targetFile );
			  }
			  return file;
		 }

		 private GraphDatabaseService GraphDatabaseService
		 {
			 get
			 {
				  return GetGraphDatabaseService( _testDirectory.absolutePath() );
			 }
		 }

		 private GraphDatabaseService getGraphDatabaseService( File storeDir )
		 {
			  return GetGraphDatabaseService( storeDir, new string[0] );
		 }

		 private GraphDatabaseService getGraphDatabaseService( File storeDir, params string[] settings )
		 {
			  GraphDatabaseBuilder builder = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir);
			  builder.Config = settings( settings );

			  return builder.NewGraphDatabase();
		 }

		 private void PrepareDbWithDeletedRelationshipPartOfTheChain()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_testDirectory.databaseDir()).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig("dbms.backup.enabled", "false").newGraphDatabase();
			  try
			  {

					RelationshipType relationshipType = RelationshipType.withName( "testRelationshipType" );
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node1 = set( Db.createNode() );
						 Node node2 = set( Db.createNode(), property("key", "value") );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 node1.CreateRelationshipTo( node2, relationshipType );
						 tx.Success();
					}

					RecordStorageEngine recordStorageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );

					NeoStores neoStores = recordStorageEngine.TestAccessNeoStores();
					RelationshipStore relationshipStore = neoStores.RelationshipStore;
					RelationshipRecord relationshipRecord = new RelationshipRecord( -1 );
					RelationshipRecord record = relationshipStore.GetRecord( 4, relationshipRecord, RecordLoad.FORCE );
					record.InUse = false;
					relationshipStore.UpdateRecord( relationshipRecord );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void nonRecoveredDatabase() throws java.io.IOException
		 private void NonRecoveredDatabase()
		 {
			  File tmpLogDir = new File( _testDirectory.directory(), "logs" );
			  _fs.mkdir( tmpLogDir );
			  File storeDir = _testDirectory.databaseDir();
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig("dbms.backup.enabled", "false").newGraphDatabase();

			  RelationshipType relationshipType = RelationshipType.withName( "testRelationshipType" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node1 = set( Db.createNode() );
					Node node2 = set( Db.createNode(), property("key", "value") );
					node1.CreateRelationshipTo( node2, relationshipType );
					tx.Success();
			  }
			  File[] txLogs = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDir, _fs ).build().logFiles();
			  foreach ( File file in txLogs )
			  {
					_fs.copyToDirectory( file, tmpLogDir );
			  }
			  Db.shutdown();
			  foreach ( File txLog in txLogs )
			  {
					_fs.deleteFile( txLog );
			  }

			  foreach ( File file in LogFilesBuilder.logFilesBasedOnlyBuilder( tmpLogDir, _fs ).build().logFiles() )
			  {
					_fs.moveToDirectory( file, storeDir );
			  }
		 }

		 protected internal virtual IDictionary<string, string> Settings( params string[] strings )
		 {
			  IDictionary<string, string> defaults = new Dictionary<string, string>();
			  defaults[GraphDatabaseSettings.pagecache_memory.name()] = "8m";
			  defaults[GraphDatabaseSettings.record_format.name()] = RecordFormatName;
			  defaults["dbms.backup.enabled"] = "false";
			  return stringMap( defaults, strings );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void breakNodeStore() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void BreakNodeStore()
		 {
			  fixture.apply( new TransactionAnonymousInnerClass( this ) );
		 }

		 private class TransactionAnonymousInnerClass : GraphStoreFixture.Transaction
		 {
			 private readonly ConsistencyCheckServiceIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass( ConsistencyCheckServiceIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.Create( new NodeRecord( next.Node(), false, next.Relationship(), -1 ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.consistency.ConsistencyCheckService.Result runFullConsistencyCheck(ConsistencyCheckService service, org.neo4j.kernel.configuration.Config configuration) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private Result RunFullConsistencyCheck( ConsistencyCheckService service, Config configuration )
		 {
			  return RunFullConsistencyCheck( service, configuration, fixture.databaseLayout() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.consistency.ConsistencyCheckService.Result runFullConsistencyCheck(ConsistencyCheckService service, org.neo4j.kernel.configuration.Config configuration, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private static Result RunFullConsistencyCheck( ConsistencyCheckService service, Config configuration, DatabaseLayout databaseLayout )
		 {
			  return service.RunFullConsistencyCheck( databaseLayout, configuration, ProgressMonitorFactory.NONE, NullLogProvider.Instance, false );
		 }

		 protected internal virtual string RecordFormatName
		 {
			 get
			 {
				  return StringUtils.EMPTY;
			 }
		 }
	}

}