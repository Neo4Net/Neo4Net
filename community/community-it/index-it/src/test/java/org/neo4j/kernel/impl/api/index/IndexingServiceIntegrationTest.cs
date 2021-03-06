﻿using System;
using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using RelationTypeSchemaDescriptor = Org.Neo4j.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forRelType;

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
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();
		 private GraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex[] parameters()
		 public static GraphDatabaseSettings.SchemaIndex[] Parameters()
		 {
			  return GraphDatabaseSettings.SchemaIndex.values();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex;
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
//ORIGINAL LINE: @Test public void testManualIndexPopulation() throws InterruptedException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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
			  using ( Org.Neo4j.@internal.Kernel.Api.Transaction tx = ( ( GraphDatabaseAPI ) _database ).DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
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
//ORIGINAL LINE: @Test public void testSchemaIndexMatchIndexingService() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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
			  indexingService.ForceAll( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
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

		 private IndexingService GetIndexingService( GraphDatabaseService database )
		 {
			  return GetDependencyResolver( database ).resolveDependency( typeof( IndexingService ) );
		 }

		 private DependencyResolver GetDependencyResolver( GraphDatabaseService database )
		 {
			  return ( ( GraphDatabaseAPI )database ).DependencyResolver;
		 }

		 private void CreateData( GraphDatabaseService database, int numberOfNodes )
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