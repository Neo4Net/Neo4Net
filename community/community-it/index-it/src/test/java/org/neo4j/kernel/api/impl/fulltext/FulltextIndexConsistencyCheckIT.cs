﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Org.Neo4j.Consistency.checking.full.ConsistencyFlags;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexDefinitionImpl = Org.Neo4j.Kernel.impl.coreapi.schema.IndexDefinitionImpl;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.first;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.array;

	public class FulltextIndexConsistencyCheckIT
	{
		private bool InstanceFieldsInitialized = false;

		public FulltextIndexConsistencyCheckIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _expectedException ).around( _cleanup );
		}

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly CleanupRule _cleanup = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(expectedException).around(cleanup);
		 public RuleChain Rules;

		 private GraphDatabaseBuilder _builder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  GraphDatabaseFactory factory = new GraphDatabaseFactory();
			  _builder = factory.NewEmbeddedDatabaseBuilder( _testDirectory.databaseDir() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckEmptyDatabaseWithFulltextIndexingEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckEmptyDatabaseWithFulltextIndexingEnabled()
		 {
			  CreateDatabase().shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexWithOneLabelAndOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexWithOneLabelAndOneProperty()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "Label" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Db.createNode( Label.label( "Label" ) ).setProperty( "prop", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexWithOneLabelAndMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexWithOneLabelAndMultipleProperties()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "Label" ), array( "p1", "p2" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode( Label.label( "Label" ) );
					node.SetProperty( "p1", "value" );
					node.SetProperty( "p2", "value" );
					Db.createNode( Label.label( "Label" ) ).setProperty( "p1", "value" );
					Db.createNode( Label.label( "Label" ) ).setProperty( "p2", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexWithMultipleLabelsAndOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexWithMultipleLabelsAndOneProperty()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "L1", "L2" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Db.createNode( Label.label( "L1" ), Label.label( "L2" ) ).setProperty( "prop", "value" );
					Db.createNode( Label.label( "L2" ) ).setProperty( "prop", "value" );
					Db.createNode( Label.label( "L1" ) ).setProperty( "prop", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexWithManyLabelsAndOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexWithManyLabelsAndOneProperty()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  // Enough labels to prevent inlining them into the node record, and instead require a dynamic label record to be allocated.
			  string[] labels = new string[] { "L1", "L2", "L3", "L4", "L5", "L6", "L7", "L8", "L9", "L10", "L11", "L12", "L13", "L14", "L15", "L16" };
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( labels ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					Db.createNode( Stream.of( labels ).map( Label::label ).toArray( Label[]::new ) ).setProperty( "prop", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexWithMultipleLabelsAndMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexWithMultipleLabelsAndMultipleProperties()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "L1", "L2" ), array( "p1", "p2" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node n1 = Db.createNode( Label.label( "L1" ), Label.label( "L2" ) );
					n1.SetProperty( "p1", "value" );
					n1.SetProperty( "p2", "value" );
					Node n2 = Db.createNode( Label.label( "L1" ), Label.label( "L2" ) );
					n2.SetProperty( "p1", "value" );
					Node n3 = Db.createNode( Label.label( "L1" ), Label.label( "L2" ) );
					n3.SetProperty( "p2", "value" );
					Node n4 = Db.createNode( Label.label( "L1" ) );
					n4.SetProperty( "p1", "value" );
					n4.SetProperty( "p2", "value" );
					Node n5 = Db.createNode( Label.label( "L1" ) );
					n5.SetProperty( "p1", "value" );
					Node n6 = Db.createNode( Label.label( "L1" ) );
					n6.SetProperty( "p2", "value" );
					Node n7 = Db.createNode( Label.label( "L2" ) );
					n7.SetProperty( "p1", "value" );
					n7.SetProperty( "p2", "value" );
					Node n8 = Db.createNode( Label.label( "L2" ) );
					n8.SetProperty( "p1", "value" );
					Node n9 = Db.createNode( Label.label( "L2" ) );
					n9.SetProperty( "p2", "value" );
					Db.createNode( Label.label( "L2" ) ).setProperty( "p1", "value" );
					Db.createNode( Label.label( "L2" ) ).setProperty( "p2", "value" );
					Db.createNode( Label.label( "L1" ) ).setProperty( "p1", "value" );
					Db.createNode( Label.label( "L1" ) ).setProperty( "p2", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckRelationshipIndexWithOneRelationshipTypeAndOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckRelationshipIndexWithOneRelationshipTypeAndOneProperty()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  RelationshipType relationshipType = RelationshipType.withName( "R1" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1" ), array( "p1" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode();
					node.CreateRelationshipTo( node, relationshipType ).setProperty( "p1", "value" );
					node.CreateRelationshipTo( node, relationshipType ).setProperty( "p1", "value" ); // This relationship will have a different id value than the node.
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckRelationshipIndexWithOneRelationshipTypeAndMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckRelationshipIndexWithOneRelationshipTypeAndMultipleProperties()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  RelationshipType relationshipType = RelationshipType.withName( "R1" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1" ), array( "p1", "p2" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode();
					Relationship r1 = node.CreateRelationshipTo( node, relationshipType );
					r1.SetProperty( "p1", "value" );
					r1.SetProperty( "p2", "value" );
					Relationship r2 = node.CreateRelationshipTo( node, relationshipType ); // This relationship will have a different id value than the node.
					r2.SetProperty( "p1", "value" );
					r2.SetProperty( "p2", "value" );
					node.CreateRelationshipTo( node, relationshipType ).setProperty( "p1", "value" );
					node.CreateRelationshipTo( node, relationshipType ).setProperty( "p2", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckRelationshipIndexWithMultipleRelationshipTypesAndOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckRelationshipIndexWithMultipleRelationshipTypesAndOneProperty()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  RelationshipType relType1 = RelationshipType.withName( "R1" );
			  RelationshipType relType2 = RelationshipType.withName( "R2" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1", "R2" ), array( "p1" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node n1 = Db.createNode();
					Node n2 = Db.createNode();
					n1.CreateRelationshipTo( n1, relType1 ).setProperty( "p1", "value" );
					n1.CreateRelationshipTo( n1, relType2 ).setProperty( "p1", "value" );
					n2.CreateRelationshipTo( n2, relType1 ).setProperty( "p1", "value" );
					n2.CreateRelationshipTo( n2, relType2 ).setProperty( "p1", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckRelationshipIndexWithMultipleRelationshipTypesAndMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckRelationshipIndexWithMultipleRelationshipTypesAndMultipleProperties()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  RelationshipType relType1 = RelationshipType.withName( "R1" );
			  RelationshipType relType2 = RelationshipType.withName( "R2" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1", "R2" ), array( "p1", "p2" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node n1 = Db.createNode();
					Node n2 = Db.createNode();
					Relationship r1 = n1.CreateRelationshipTo( n1, relType1 );
					r1.SetProperty( "p1", "value" );
					r1.SetProperty( "p2", "value" );
					Relationship r2 = n1.CreateRelationshipTo( n1, relType2 );
					r2.SetProperty( "p1", "value" );
					r2.SetProperty( "p2", "value" );
					Relationship r3 = n2.CreateRelationshipTo( n2, relType1 );
					r3.SetProperty( "p1", "value" );
					r3.SetProperty( "p2", "value" );
					Relationship r4 = n2.CreateRelationshipTo( n2, relType2 );
					r4.SetProperty( "p1", "value" );
					r4.SetProperty( "p2", "value" );
					n1.CreateRelationshipTo( n2, relType1 ).setProperty( "p1", "value" );
					n1.CreateRelationshipTo( n2, relType2 ).setProperty( "p1", "value" );
					n1.CreateRelationshipTo( n2, relType1 ).setProperty( "p2", "value" );
					n1.CreateRelationshipTo( n2, relType2 ).setProperty( "p2", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeAndRelationshipIndexesAtTheSameTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeAndRelationshipIndexesAtTheSameTime()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "L1", "L2", "L3" ), array( "p1", "p2" ) ) ).close();
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1", "R2" ), array( "p1", "p2" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node n1 = Db.createNode( Label.label( "L1" ), Label.label( "L3" ) );
					n1.SetProperty( "p1", "value" );
					n1.SetProperty( "p2", "value" );
					n1.CreateRelationshipTo( n1, RelationshipType.withName( "R2" ) ).setProperty( "p1", "value" );
					Node n2 = Db.createNode( Label.label( "L2" ) );
					n2.SetProperty( "p2", "value" );
					Relationship r1 = n2.CreateRelationshipTo( n2, RelationshipType.withName( "R1" ) );
					r1.SetProperty( "p1", "value" );
					r1.SetProperty( "p2", "value" );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencyCheckNodeIndexThatIsMissingNodesBecauseTheirPropertyValuesAreNotStrings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencyCheckNodeIndexThatIsMissingNodesBecauseTheirPropertyValuesAreNotStrings()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "L1" ), array( "p1" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Db.createNode( Label.label( "L1" ) ).setProperty( "p1", 1 );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToConsistencycheckRelationshipIndexThatIsMissingRelationshipsBecauseTheirPropertyValuesaAreNotStrings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeAbleToConsistencycheckRelationshipIndexThatIsMissingRelationshipsBecauseTheirPropertyValuesaAreNotStrings()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "R1" ), array( "p1" ) ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode();
					node.CreateRelationshipTo( node, RelationshipType.withName( "R1" ) ).setProperty( "p1", 1 );
					tx.Success();
			  }
			  Db.shutdown();
			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consistencyCheckerMustBeAbleToRunOnStoreWithFulltextIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConsistencyCheckerMustBeAbleToRunOnStoreWithFulltextIndexes()
		 {
			  GraphDatabaseService db = CreateDatabase();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  Label[] labels = IntStream.range( 1, 7 ).mapToObj( i => Label.label( "LABEL" + i ) ).toArray( Label[]::new );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  RelationshipType[] relTypes = IntStream.range( 1, 5 ).mapToObj( i => RelationshipType.withName( "REL" + i ) ).toArray( RelationshipType[]::new );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] propertyKeys = IntStream.range( 1, 7 ).mapToObj( i => "PROP" + i ).toArray( string[]::new );
			  RandomValues randomValues = RandomValues.create();

			  using ( Transaction tx = Db.beginTx() )
			  {
					ThreadLocalRandom rng = ThreadLocalRandom.current();
					int nodeCount = 1000;
					IList<Node> nodes = new List<Node>( nodeCount );
					for ( int i = 0; i < nodeCount; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Label[] nodeLabels = rng.ints( rng.Next( labels.Length ), 0, labels.Length ).distinct().mapToObj(x => labels[x]).toArray(Label[]::new);
						 Node node = Db.createNode( nodeLabels );
						 Stream.of( propertyKeys ).forEach( p => node.setProperty( p, rng.nextBoolean() ? p : randomValues.NextValue().asObject() ) );
						 nodes.Add( node );
						 int localRelCount = Math.Min( nodes.Count, 5 );
						 rng.ints( localRelCount, 0, localRelCount ).distinct().mapToObj(x => node.CreateRelationshipTo(nodes[x], relTypes[rng.Next(relTypes.Length)])).forEach(r => Stream.of(propertyKeys).forEach(p => r.setProperty(p, rng.nextBoolean() ? p : randomValues.NextValue().asObject())));
					}
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 1; i < labels.Length; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Db.execute( format( NODE_CREATE, "nodes" + i, array( java.util.labels.Take( i ).Select( Label::name ).ToArray( string[]::new ) ), array( Arrays.copyOf( propertyKeys, i ) ) ) ).close();
					}
					for ( int i = 1; i < relTypes.Length; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Db.execute( format( RELATIONSHIP_CREATE, "rels" + i, array( java.util.relTypes.Take( i ).Select( RelationshipType::name ).ToArray( string[]::new ) ), array( Arrays.copyOf( propertyKeys, i ) ) ) ).close();
					}
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }

			  Db.shutdown();

			  AssertIsConsistent( CheckConsistency() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustDiscoverNodeInStoreMissingFromIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDiscoverNodeInStoreMissingFromIndex()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "Label" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  StoreIndexDescriptor indexDescriptor;
			  long nodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					indexDescriptor = GetIndexDescriptor( first( Db.schema().Indexes ) );
					Node node = Db.createNode( Label.label( "Label" ) );
					node.SetProperty( "prop", "value" );
					nodeId = node.Id;
					tx.Success();
			  }
			  IndexingService indexes = GetIndexingService( db );
			  IndexProxy indexProxy = indexes.GetIndexProxy( indexDescriptor.Schema() );
			  using ( IndexUpdater updater = indexProxy.NewUpdater( IndexUpdateMode.ONLINE ) )
			  {
					updater.Process( IndexEntryUpdate.remove( nodeId, indexDescriptor, Values.stringValue( "value" ) ) );
			  }

			  Db.shutdown();

			  ConsistencyCheckService.Result result = CheckConsistency();
			  assertFalse( result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Turns out that this is not something that the consistency checker actually looks for, currently. " + "The test is disabled until the consistency checker is extended with checks that will discover this sort of inconsistency.") @Test public void mustDiscoverNodeInIndexMissingFromStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDiscoverNodeInIndexMissingFromStore()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodes", array( "Label" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  long nodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode( Label.label( "Label" ) );
					nodeId = node.Id;
					node.SetProperty( "prop", "value" );
					tx.Success();
			  }
			  NeoStores stores = GetNeoStores( db );
			  NodeRecord record = stores.NodeStore.newRecord();
			  record = stores.NodeStore.getRecord( nodeId, record, RecordLoad.NORMAL );
			  long propId = record.NextProp;
			  record.NextProp = AbstractBaseRecord.NO_ID;
			  stores.NodeStore.updateRecord( record );
			  PropertyRecord propRecord = stores.PropertyStore.getRecord( propId, stores.PropertyStore.newRecord(), RecordLoad.NORMAL );
			  propRecord.InUse = false;
			  stores.PropertyStore.updateRecord( propRecord );
			  Db.shutdown();

			  ConsistencyCheckService.Result result = CheckConsistency();
			  assertFalse( result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustDiscoverRelationshipInStoreMissingFromIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDiscoverRelationshipInStoreMissingFromIndex()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "REL" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  StoreIndexDescriptor indexDescriptor;
			  long relId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					indexDescriptor = GetIndexDescriptor( first( Db.schema().Indexes ) );
					Node node = Db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, RelationshipType.withName( "REL" ) );
					rel.SetProperty( "prop", "value" );
					relId = rel.Id;
					tx.Success();
			  }
			  IndexingService indexes = GetIndexingService( db );
			  IndexProxy indexProxy = indexes.GetIndexProxy( indexDescriptor.Schema() );
			  using ( IndexUpdater updater = indexProxy.NewUpdater( IndexUpdateMode.ONLINE ) )
			  {
					updater.Process( IndexEntryUpdate.remove( relId, indexDescriptor, Values.stringValue( "value" ) ) );
			  }

			  Db.shutdown();

			  ConsistencyCheckService.Result result = CheckConsistency();
			  assertFalse( result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Turns out that this is not something that the consistency checker actually looks for, currently. " + "The test is disabled until the consistency checker is extended with checks that will discover this sort of inconsistency.") @Test public void mustDiscoverRelationshipInIndexMissingFromStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDiscoverRelationshipInIndexMissingFromStore()
		 {
			  GraphDatabaseService db = CreateDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "rels", array( "REL" ), array( "prop" ) ) ).close();
					tx.Success();
			  }
			  long relId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					Node node = Db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, RelationshipType.withName( "REL" ) );
					relId = rel.Id;
					rel.SetProperty( "prop", "value" );
					tx.Success();
			  }
			  NeoStores stores = GetNeoStores( db );
			  RelationshipRecord record = stores.RelationshipStore.newRecord();
			  record = stores.RelationshipStore.getRecord( relId, record, RecordLoad.NORMAL );
			  long propId = record.NextProp;
			  record.NextProp = AbstractBaseRecord.NO_ID;
			  stores.RelationshipStore.updateRecord( record );
			  PropertyRecord propRecord = stores.PropertyStore.getRecord( propId, stores.PropertyStore.newRecord(), RecordLoad.NORMAL );
			  propRecord.InUse = false;
			  stores.PropertyStore.updateRecord( propRecord );
			  Db.shutdown();

			  ConsistencyCheckService.Result result = CheckConsistency();
			  assertFalse( result.Successful );
		 }

		 private GraphDatabaseService CreateDatabase()
		 {
			  return _cleanup.add( _builder.newGraphDatabase() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.consistency.ConsistencyCheckService.Result checkConsistency() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private ConsistencyCheckService.Result CheckConsistency()
		 {
			  Config config = Config.defaults();
			  ConsistencyCheckService consistencyCheckService = new ConsistencyCheckService( DateTime.Now );
			  ConsistencyFlags checkConsistencyConfig = new ConsistencyFlags( config );
			  return consistencyCheckService.runFullConsistencyCheck( _testDirectory.databaseLayout(), config, ProgressMonitorFactory.NONE, NullLogProvider.Instance, true, checkConsistencyConfig );
		 }

		 private static StoreIndexDescriptor GetIndexDescriptor( IndexDefinition definition )
		 {
			  StoreIndexDescriptor indexDescriptor;
			  IndexDefinitionImpl indexDefinition = ( IndexDefinitionImpl ) definition;
			  indexDescriptor = ( StoreIndexDescriptor ) indexDefinition.IndexReference;
			  return indexDescriptor;
		 }

		 private static IndexingService GetIndexingService( GraphDatabaseService db )
		 {
			  DependencyResolver dependencyResolver = GetDependencyResolver( db );
			  return dependencyResolver.ResolveDependency( typeof( IndexingService ), Org.Neo4j.Graphdb.DependencyResolver_SelectionStrategy.ONLY );
		 }

		 private static NeoStores GetNeoStores( GraphDatabaseService db )
		 {
			  DependencyResolver dependencyResolver = GetDependencyResolver( db );
			  return dependencyResolver.ResolveDependency( typeof( RecordStorageEngine ), Org.Neo4j.Graphdb.DependencyResolver_SelectionStrategy.ONLY ).testAccessNeoStores();
		 }

		 private static DependencyResolver GetDependencyResolver( GraphDatabaseService db )
		 {
			  GraphDatabaseAPI api = ( GraphDatabaseAPI ) db;
			  return api.DependencyResolver;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertIsConsistent(org.neo4j.consistency.ConsistencyCheckService.Result result) throws java.io.IOException
		 private static void AssertIsConsistent( ConsistencyCheckService.Result result )
		 {
			  if ( !result.Successful )
			  {
					PrintReport( result );
					fail( "Expected consistency check to be successful." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void printReport(org.neo4j.consistency.ConsistencyCheckService.Result result) throws java.io.IOException
		 private static void PrintReport( ConsistencyCheckService.Result result )
		 {
			  Files.readAllLines( result.ReportFile().toPath() ).forEach(System.err.println);
		 }
	}

}