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
namespace Neo4Net.@unsafe.Batchinsert.Internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Neo4Net.Collections.Helpers;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using SpatialIndexValueTestUtil = Neo4Net.Kernel.Impl.Index.Schema.config.SpatialIndexValueTestUtil;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestLabels = Neo4Net.Test.TestLabels;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BatchInsertIndexTest
	public class BatchInsertIndexTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _storeDir ).around( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly GraphDatabaseSettings.SchemaIndex _schemaIndex;
		 private DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private TestDirectory _storeDir = TestDirectory.testDirectory();
		 private PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(storeDir).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex[] data()
		 public static GraphDatabaseSettings.SchemaIndex[] Data()
		 {
			  return GraphDatabaseSettings.SchemaIndex.values();
		 }

		 public BatchInsertIndexTest( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._schemaIndex = schemaIndex;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void batchInserterShouldUseConfiguredIndexProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BatchInserterShouldUseConfiguredIndexProvider()
		 {
			  Config config = Config.defaults( stringMap( default_schema_provider.name(), _schemaIndex.providerName() ) );
			  BatchInserter inserter = NewBatchInserter( config );
			  inserter.CreateDeferredSchemaIndex( TestLabels.LABEL_ONE ).on( "key" ).create();
			  inserter.Shutdown();
			  IGraphDatabaseService db = IGraphDatabaseService( config );
			  AwaitIndexesOnline( db );
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						DependencyResolver dependencyResolver = ( ( GraphDatabaseAPI ) db ).DependencyResolver;
						ThreadToStatementContextBridge threadToStatementContextBridge = dependencyResolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
						KernelTransaction kernelTransaction = threadToStatementContextBridge.GetKernelTransactionBoundToThisThread( true );
						TokenRead tokenRead = kernelTransaction.TokenRead();
						SchemaRead schemaRead = kernelTransaction.SchemaRead();
						int labelId = tokenRead.NodeLabel( TestLabels.LABEL_ONE.name() );
						int propertyId = tokenRead.PropertyKey( "key" );
						IndexReference index = schemaRead.Index( labelId, propertyId );
						assertTrue( UnexpectedIndexProviderMessage( index ), _schemaIndex.providerName().contains(index.ProviderKey()) );
						assertTrue( UnexpectedIndexProviderMessage( index ), _schemaIndex.providerName().contains(index.ProviderVersion()) );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndexWithUniquePointsThatCollideOnSpaceFillingCurve() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateIndexWithUniquePointsThatCollideOnSpaceFillingCurve()
		 {
			  Config config = Config.defaults( stringMap( default_schema_provider.name(), _schemaIndex.providerName() ) );
			  BatchInserter inserter = NewBatchInserter( config );
			  Pair<PointValue, PointValue> collidingPoints = SpatialIndexValueTestUtil.pointsWithSameValueOnSpaceFillingCurve( config );
			  inserter.createNode( MapUtil.map( "prop", collidingPoints.First() ), TestLabels.LABEL_ONE );
			  inserter.createNode( MapUtil.map( "prop", collidingPoints.Other() ), TestLabels.LABEL_ONE );
			  inserter.CreateDeferredConstraint( TestLabels.LABEL_ONE ).assertPropertyIsUnique( "prop" ).create();
			  inserter.Shutdown();

			  IGraphDatabaseService db = IGraphDatabaseService( config );
			  try
			  {
					AwaitIndexesOnline( db );
					using ( Transaction tx = Db.beginTx() )
					{
						 AssertSingleCorrectHit( db, collidingPoints.First() );
						 AssertSingleCorrectHit( db, collidingPoints.Other() );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenPopulatingWithNonUniquePoints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenPopulatingWithNonUniquePoints()
		 {
			  Config config = Config.defaults( stringMap( default_schema_provider.name(), _schemaIndex.providerName() ) );
			  BatchInserter inserter = NewBatchInserter( config );
			  PointValue point = Values.pointValue( CoordinateReferenceSystem.WGS84, 0.0, 0.0 );
			  inserter.createNode( MapUtil.map( "prop", point ), TestLabels.LABEL_ONE );
			  inserter.createNode( MapUtil.map( "prop", point ), TestLabels.LABEL_ONE );
			  inserter.CreateDeferredConstraint( TestLabels.LABEL_ONE ).assertPropertyIsUnique( "prop" ).create();
			  inserter.Shutdown();

			  IGraphDatabaseService db = IGraphDatabaseService( config );
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						IEnumerator<IndexDefinition> indexes = Db.schema().Indexes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertTrue( indexes.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						IndexDefinition index = indexes.next();
						Neo4Net.GraphDb.Schema.Schema_IndexState indexState = Db.schema().getIndexState(index);
						assertEquals( Neo4Net.GraphDb.Schema.Schema_IndexState.Failed, indexState );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertFalse( indexes.hasNext() );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private void AssertSingleCorrectHit( IGraphDatabaseService db, PointValue point )
		 {
			  ResourceIterator<Node> nodes = Db.findNodes( TestLabels.LABEL_ONE, "prop", point );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( nodes.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Node node = nodes.next();
			  object prop = node.GetProperty( "prop" );
			  assertEquals( point, prop );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( nodes.hasNext() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.unsafe.batchinsert.BatchInserter newBatchInserter(org.Neo4Net.kernel.configuration.Config config) throws Exception
		 private BatchInserter NewBatchInserter( Config config )
		 {
			  return BatchInserters.inserter( _storeDir.databaseDir(), _fileSystemRule.get(), config.Raw );
		 }

		 private IGraphDatabaseService IGraphDatabaseService( Config config )
		 {
			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  factory.FileSystem = _fileSystemRule.get();
			  return factory.NewImpermanentDatabaseBuilder( _storeDir.databaseDir() ).setConfig(config.Raw).newGraphDatabase();
		 }

		 private void AwaitIndexesOnline( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

		 private static string UnexpectedIndexProviderMessage( IndexReference index )
		 {
			  return "Unexpected provider: key=" + index.ProviderKey() + ", version=" + index.ProviderVersion();
		 }
	}

}