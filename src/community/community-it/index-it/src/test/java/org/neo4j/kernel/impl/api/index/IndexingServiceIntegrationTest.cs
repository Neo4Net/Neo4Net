using System;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forRelType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexingServiceIntegrationTest
	public class IndexingServiceIntegrationTest
	{
		 private const string FOOD_LABEL = "food";
		 private const string CLOTHES_LABEL = "clothes";
		 private const string WEATHER_LABEL = "weather";
		 private const string PROPERTY_NAME = "name";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();
		 private IGraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex[] parameters()
		 public static GraphDatabaseSettings.SchemaIndex[] Parameters()
		 {
			  return GraphDatabaseSettings.SchemaIndex.values();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex;
		 public GraphDatabaseSettings.SchemaIndex SchemaIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  EphemeralFileSystemAbstraction fileSystem = FileSystemRule.get();
			  _database = ( new TestGraphDatabaseFactory() ).setFileSystem(fileSystem).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.default_schema_provider, SchemaIndex.providerName()).newGraphDatabase();
			  CreateData( _database, 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  try
			  {
					_database.shutdown();
			  }
			  catch ( Exception )
			  {
					//ignore
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testManualIndexPopulation() throws InterruptedException, org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestManualIndexPopulation()
		 {
			  using ( Transaction tx = _database.beginTx() )
			  {
					_database.schema().indexFor(Label.label(FOOD_LABEL)).on(PROPERTY_NAME).create();
					tx.Success();
			  }

			  int labelId = GetLabelId( FOOD_LABEL );
			  int propertyKeyId = GetPropertyKeyId( PROPERTY_NAME );

			  IndexingService indexingService = GetIndexingService( _database );
			  IndexProxy indexProxy = indexingService.getIndexProxy( forLabel( labelId, propertyKeyId ) );

			  WaitIndexOnline( indexProxy );
			  assertEquals( InternalIndexState.ONLINE, indexProxy.State );
			  PopulationProgress progress = indexProxy.IndexPopulationProgress;
			  assertEquals( progress.Completed, progress.Total );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testManualRelationshipIndexPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestManualRelationshipIndexPopulation()
		 {
			  RelationTypeSchemaDescriptor descriptor;
			  using ( Neo4Net.Kernel.Api.Internal.Transaction tx = ( ( GraphDatabaseAPI ) _database ).DependencyResolver.resolveDependency( typeof( Kernel ) ).BeginTransaction( @explicit, AUTH_DISABLED ) )
			  {
					int foodId = tx.TokenWrite().relationshipTypeGetOrCreateForName(FOOD_LABEL);
					int propertyId = tx.TokenWrite().propertyKeyGetOrCreateForName(PROPERTY_NAME);
					descriptor = forRelType( foodId, propertyId );
					tx.SchemaWrite().indexCreate(descriptor);
					tx.Success();
			  }

			  IndexingService indexingService = GetIndexingService( _database );
			  IndexProxy indexProxy = indexingService.getIndexProxy( descriptor );

			  WaitIndexOnline( indexProxy );
			  assertEquals( InternalIndexState.ONLINE, indexProxy.State );
			  PopulationProgress progress = indexProxy.IndexPopulationProgress;
			  assertEquals( progress.Completed, progress.Total );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaIndexMatchIndexingService() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSchemaIndexMatchIndexingService()
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					_database.schema().constraintFor(Label.label(CLOTHES_LABEL)).assertPropertyIsUnique(PROPERTY_NAME).create();
					_database.schema().indexFor(Label.label(WEATHER_LABEL)).on(PROPERTY_NAME).create();

					transaction.Success();
			  }

			  using ( Transaction ignored = _database.beginTx() )
			  {
					_database.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }

			  IndexingService indexingService = GetIndexingService( _database );
			  int clothedLabelId = GetLabelId( CLOTHES_LABEL );
			  int weatherLabelId = GetLabelId( WEATHER_LABEL );
			  int propertyId = GetPropertyKeyId( PROPERTY_NAME );

			  IndexProxy clothesIndex = indexingService.getIndexProxy( forLabel( clothedLabelId, propertyId ) );
			  IndexProxy weatherIndex = indexingService.getIndexProxy( forLabel( weatherLabelId, propertyId ) );
			  assertEquals( InternalIndexState.ONLINE, clothesIndex.State );
			  assertEquals( InternalIndexState.ONLINE, weatherIndex.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failForceIndexesWhenOneOfTheIndexesIsBroken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailForceIndexesWhenOneOfTheIndexesIsBroken()
		 {
			  string constraintLabelPrefix = "ConstraintLabel";
			  string constraintPropertyPrefix = "ConstraintProperty";
			  string indexLabelPrefix = "Label";
			  string indexPropertyPrefix = "Property";
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction transaction = _database.beginTx() )
					{
						 _database.schema().constraintFor(Label.label(constraintLabelPrefix + i)).assertPropertyIsUnique(constraintPropertyPrefix + i).create();
						 _database.schema().indexFor(Label.label(indexLabelPrefix + i)).on(indexPropertyPrefix + i).create();
						 transaction.Success();
					}
			  }

			  using ( Transaction ignored = _database.beginTx() )
			  {
					_database.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }

			  IndexingService indexingService = GetIndexingService( _database );

			  int indexLabel7 = GetLabelId( indexLabelPrefix + 7 );
			  int indexProperty7 = GetPropertyKeyId( indexPropertyPrefix + 7 );

			  IndexProxy index = indexingService.GetIndexProxy( TestIndexDescriptorFactory.forLabel( indexLabel7, indexProperty7 ).schema() );

			  index.Drop();

			  ExpectedException.expect( typeof( UnderlyingStorageException ) );
			  ExpectedException.expectMessage( "Unable to force" );
			  indexingService.ForceAll( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitIndexOnline(IndexProxy indexProxy) throws InterruptedException
		 private void WaitIndexOnline( IndexProxy indexProxy )
		 {
			  while ( InternalIndexState.ONLINE != indexProxy.State )
			  {
					Thread.Sleep( 10 );
			  }
		 }

		 private IndexingService GetIndexingService( IGraphDatabaseService database )
		 {
			  return GetDependencyResolver( database ).resolveDependency( typeof( IndexingService ) );
		 }

		 private DependencyResolver GetDependencyResolver( IGraphDatabaseService database )
		 {
			  return ( ( GraphDatabaseAPI )database ).DependencyResolver;
		 }

		 private void CreateData( IGraphDatabaseService database, int numberOfNodes )
		 {
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 Node node = database.CreateNode( Label.label( FOOD_LABEL ), Label.label( CLOTHES_LABEL ), Label.label( WEATHER_LABEL ) );
						 node.SetProperty( PROPERTY_NAME, "Node" + i );
						 Relationship relationship = node.CreateRelationshipTo( node, RelationshipType.withName( FOOD_LABEL ) );
						 relationship.SetProperty( PROPERTY_NAME, "Relationship" + i );
						 transaction.Success();
					}
			  }
		 }

		 private int GetPropertyKeyId( string name )
		 {
			  using ( Transaction tx = _database.beginTx() )
			  {
					KernelTransaction transaction = ( ( GraphDatabaseAPI ) _database ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					return transaction.TokenRead().propertyKey(name);
			  }
		 }

		 private int GetLabelId( string name )
		 {
			  using ( Transaction tx = _database.beginTx() )
			  {
					KernelTransaction transaction = ( ( GraphDatabaseAPI ) _database ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					return transaction.TokenRead().nodeLabel(name);
			  }
		 }
	}

}