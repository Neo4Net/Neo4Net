using System.Collections.Generic;
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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using IdContextFactoryBuilder = Neo4Net.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using LabelSet = Neo4Net.@internal.Kernel.Api.LabelSet;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfiguration = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using ImpermanentGraphDatabase = Neo4Net.Test.ImpermanentGraphDatabase;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestGraphDatabaseFactoryState = Neo4Net.Test.TestGraphDatabaseFactoryState;
	using EphemeralIdGenerator = Neo4Net.Test.impl.EphemeralIdGenerator;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
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
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasNoLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasNoNodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasNodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class LabelsAcceptanceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private enum Labels
		 {
			  MyLabel,
			  MyOtherLabel
		 }

		 /// <summary>
		 /// https://github.com/neo4j/neo4j/issues/1279 </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInsertLabelsWithoutDuplicatingThem()
		 public virtual void ShouldInsertLabelsWithoutDuplicatingThem()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Node node = dbRule.executeAndCommit((System.Func<GraphDatabaseService,Node>) GraphDatabaseService::createNode);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Node node = DbRule.executeAndCommit( ( System.Func<GraphDatabaseService, Node> ) GraphDatabaseService::createNode );
			  // POST "FOOBAR"
			  DbRule.executeAndCommit(db =>
			  {
				node.AddLabel( label( "FOOBAR" ) );
			  });
			  // POST ["BAZQUX"]
			  DbRule.executeAndCommit(db =>
			  {
				node.AddLabel( label( "BAZQUX" ) );
			  });
			  // PUT ["BAZQUX"]
			  DbRule.executeAndCommit(db =>
			  {
				foreach ( Label label in node.Labels )
				{
					 node.RemoveLabel( label );
				}
				node.AddLabel( label( "BAZQUX" ) );
			  });
			  // GET
			  IList<Label> labels = DbRule.executeAndCommit(db =>
			  {
				IList<Label> labels1 = new List<Label>();
				foreach ( Label label in node.Labels )
				{
					 labels1.add( label );
				}
				return labels1;
			  });
			  assertEquals( labels.ToString(), 1, labels.Count );
			  assertEquals( "BAZQUX", labels[0].Name() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingALabelUsingAValidIdentifierShouldSucceed()
		 public virtual void AddingALabelUsingAValidIdentifierShouldSucceed()
		 {
			  // Given
			  GraphDatabaseService graphDatabase = DbRule.GraphDatabaseAPI;
			  Node myNode;

			  // When
			  using ( Transaction tx = graphDatabase.BeginTx() )
			  {
					myNode = graphDatabase.CreateNode();
					myNode.AddLabel( Labels.MyLabel );

					tx.Success();
			  }

			  // Then
			  assertThat( "Label should have been added to node", myNode, inTx( graphDatabase, hasLabel( Labels.MyLabel ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingALabelUsingAnInvalidIdentifierShouldFail()
		 public virtual void AddingALabelUsingAnInvalidIdentifierShouldFail()
		 {
			  // Given
			  GraphDatabaseService graphDatabase = DbRule.GraphDatabaseAPI;

			  // When I set an empty label
			  try
			  {
					  using ( Transaction ignored = graphDatabase.BeginTx() )
					  {
						graphDatabase.CreateNode().addLabel(label(""));
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( ConstraintViolationException )
			  { // Happy
			  }

			  // And When I set a null label
			  try
			  {
					  using ( Transaction ignored = graphDatabase.BeginTx() )
					  {
						graphDatabase.CreateNode().addLabel(() => null);
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( ConstraintViolationException )
			  { // Happy
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingALabelThatAlreadyExistsBehavesAsNoOp()
		 public virtual void AddingALabelThatAlreadyExistsBehavesAsNoOp()
		 {
			  // Given
			  GraphDatabaseService graphDatabase = DbRule.GraphDatabaseAPI;
			  Node myNode;

			  // When
			  using ( Transaction tx = graphDatabase.BeginTx() )
			  {
					myNode = graphDatabase.CreateNode();
					myNode.AddLabel( Labels.MyLabel );
					myNode.AddLabel( Labels.MyLabel );

					tx.Success();
			  }

			  // Then
			  assertThat( "Label should have been added to node", myNode, inTx( graphDatabase, hasLabel( Labels.MyLabel ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oversteppingMaxNumberOfLabelsShouldFailGracefully()
		 public virtual void OversteppingMaxNumberOfLabelsShouldFailGracefully()
		 {
			  // Given
			  GraphDatabaseService graphDatabase = BeansAPIWithNoMoreLabelIds();

			  // When
			  try
			  {
					  using ( Transaction ignored = graphDatabase.BeginTx() )
					  {
						graphDatabase.CreateNode().addLabel(Labels.MyLabel);
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( ConstraintViolationException )
			  { // Happy
			  }

			  graphDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingCommittedLabel()
		 public virtual void RemovingCommittedLabel()
		 {
			  // Given
			  GraphDatabaseService graphDatabase = DbRule.GraphDatabaseAPI;
			  Label label = Labels.MyLabel;
			  Node myNode = CreateNode( graphDatabase, label );

			  // When
			  using ( Transaction tx = graphDatabase.BeginTx() )
			  {
					myNode.RemoveLabel( label );
					tx.Success();
			  }

			  // Then
			  assertThat( myNode, not( inTx( graphDatabase, hasLabel( label ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createNodeWithLabels()
		 public virtual void CreateNodeWithLabels()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  // WHEN
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode( Enum.GetValues( typeof( Labels ) ) );
					tx.Success();
			  }

			  // THEN

			  ISet<string> names = Stream.of( Enum.GetValues( typeof( Labels ) ) ).map( Labels.name ).collect( toSet() );
			  assertThat( node, inTx( db, hasLabels( names ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingNonExistentLabel()
		 public virtual void RemovingNonExistentLabel()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Label label = Labels.MyLabel;

			  // When
			  Node myNode;
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					myNode = beansAPI.CreateNode();
					myNode.RemoveLabel( label );
					tx.Success();
			  }

			  // THEN
			  assertThat( myNode, not( inTx( beansAPI, hasLabel( label ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingExistingLabelFromUnlabeledNode()
		 public virtual void RemovingExistingLabelFromUnlabeledNode()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Label label = Labels.MyLabel;
			  CreateNode( beansAPI, label );
			  Node myNode = CreateNode( beansAPI );

			  // When
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					myNode.RemoveLabel( label );
					tx.Success();
			  }

			  // THEN
			  assertThat( myNode, not( inTx( beansAPI, hasLabel( label ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingUncommittedLabel()
		 public virtual void RemovingUncommittedLabel()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Label label = Labels.MyLabel;

			  // When
			  Node myNode;
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					myNode = beansAPI.CreateNode();
					myNode.AddLabel( label );
					myNode.RemoveLabel( label );

					// THEN
					assertFalse( myNode.HasLabel( label ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToListLabelsForANode()
		 public virtual void ShouldBeAbleToListLabelsForANode()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node node;
			  ISet<string> expected = asSet( Labels.MyLabel.name(), Labels.MyOtherLabel.name() );
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					node = beansAPI.CreateNode();
					foreach ( string label in expected )
					{
						 node.AddLabel( label( label ) );
					}
					tx.Success();
			  }

			  assertThat( node, inTx( beansAPI, hasLabels( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyListIfNoLabels()
		 public virtual void ShouldReturnEmptyListIfNoLabels()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node node = CreateNode( beansAPI );

			  // WHEN THEN
			  assertThat( node, inTx( beansAPI, hasNoLabels() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNodesWithLabelCommitted()
		 public virtual void getNodesWithLabelCommitted()
		 {
			  // Given
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;

			  // When
			  Node node;
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					node = beansAPI.CreateNode();
					node.AddLabel( Labels.MyLabel );
					tx.Success();
			  }

			  // THEN
			  assertThat( beansAPI, inTx( beansAPI, hasNodes( Labels.MyLabel, node ) ) );
			  assertThat( beansAPI, inTx( beansAPI, hasNoNodes( Labels.MyOtherLabel ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNodesWithLabelsWithTxAddsAndRemoves()
		 public virtual void getNodesWithLabelsWithTxAddsAndRemoves()
		 {
			  // GIVEN
			  GraphDatabaseService beansAPI = DbRule.GraphDatabaseAPI;
			  Node node1 = CreateNode( beansAPI, Labels.MyLabel, Labels.MyOtherLabel );
			  Node node2 = CreateNode( beansAPI, Labels.MyLabel, Labels.MyOtherLabel );

			  // WHEN
			  Node node3;
			  ISet<Node> nodesWithMyLabel;
			  ISet<Node> nodesWithMyOtherLabel;
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					node3 = beansAPI.CreateNode( Labels.MyLabel );
					node2.RemoveLabel( Labels.MyLabel );
					// extracted here to be asserted below
					nodesWithMyLabel = asSet( beansAPI.FindNodes( Labels.MyLabel ) );
					nodesWithMyOtherLabel = asSet( beansAPI.FindNodes( Labels.MyOtherLabel ) );
					tx.Success();
			  }

			  // THEN
			  assertEquals( asSet( node1, node3 ), nodesWithMyLabel );
			  assertEquals( asSet( node1, node2 ), nodesWithMyOtherLabel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllExistingLabels()
		 public virtual void ShouldListAllExistingLabels()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  CreateNode( db, Labels.MyLabel, Labels.MyOtherLabel );
			  IList<Label> labels;

			  // When
			  using ( Transaction ignored = Db.beginTx() )
			  {
					labels = new IList<Label> { Db.AllLabels };
			  }

			  // Then
			  assertEquals( 2, labels.Count );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( map( Label::name, labels ), hasItems( Labels.MyLabel.name(), Labels.MyOtherLabel.name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllLabelsInUse()
		 public virtual void ShouldListAllLabelsInUse()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  CreateNode( db, Labels.MyLabel );
			  Node node = CreateNode( db, Labels.MyOtherLabel );
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }

			  // When
			  IList<Label> labels;
			  using ( Transaction ignored = Db.beginTx() )
			  {
					labels = new IList<Label> { Db.AllLabelsInUse };
			  }

			  // Then
			  assertEquals( 1, labels.Count );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( map( Label::name, labels ), hasItems( Labels.MyLabel.name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void shouldListAllLabelsInUseEvenWhenExclusiveLabelLocksAreTaken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllLabelsInUseEvenWhenExclusiveLabelLocksAreTaken()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  CreateNode( db, Labels.MyLabel );
			  Node node = CreateNode( db, Labels.MyOtherLabel );
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }

			  BinaryLatch indexCreateStarted = new BinaryLatch();
			  BinaryLatch indexCreateAllowToFinish = new BinaryLatch();
			  Thread indexCreator = new Thread(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.schema().indexFor(Labels.MyLabel).on("prop").create();
					 indexCreateStarted.Release();
					 indexCreateAllowToFinish.Await();
					 tx.Success();
				}
			  });
			  indexCreator.Start();

			  // When
			  indexCreateStarted.Await();
			  IList<Label> labels;
			  using ( Transaction ignored = Db.beginTx() )
			  {
					labels = new IList<Label> { Db.AllLabelsInUse };
			  }
			  indexCreateAllowToFinish.Release();
			  indexCreator.Join();

			  // Then
			  assertEquals( 1, labels.Count );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( map( Label::name, labels ), hasItems( Labels.MyLabel.name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void shouldListAllRelationshipTypesInUseEvenWhenExclusiveRelationshipTypeLocksAreTaken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllRelationshipTypesInUseEvenWhenExclusiveRelationshipTypeLocksAreTaken()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  RelationshipType relType = RelationshipType.withName( "REL" );
			  Node node = CreateNode( db, Labels.MyLabel );
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.CreateRelationshipTo( node, relType ).setProperty( "prop", "val" );
					tx.Success();
			  }

			  BinaryLatch indexCreateStarted = new BinaryLatch();
			  BinaryLatch indexCreateAllowToFinish = new BinaryLatch();
			  Thread indexCreator = new Thread(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.execute( "CALL db.index.fulltext.createRelationshipIndex('myIndex', ['REL'], ['prop'] )" ).close();
					 indexCreateStarted.Release();
					 indexCreateAllowToFinish.Await();
					 tx.Success();
				}
			  });
			  indexCreator.Start();

			  // When
			  indexCreateStarted.Await();
			  IList<RelationshipType> relTypes;
			  using ( Transaction ignored = Db.beginTx() )
			  {
					relTypes = new IList<RelationshipType> { Db.AllRelationshipTypesInUse };
			  }
			  indexCreateAllowToFinish.Release();
			  indexCreator.Join();

			  // Then
			  assertEquals( 1, relTypes.Count );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( map( RelationshipType::name, relTypes ), hasItems( relType.Name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteAllNodesAndTheirLabels()
		 public virtual void DeleteAllNodesAndTheirLabels()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Label label = label("A");
			  Label label = label( "A" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.AddLabel( label );
					node.SetProperty( "name", "bla" );
					tx.Success();
			  }

			  // WHEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 node.removeLabel( label ); // remove Label ...
						 node.delete(); // ... and afterwards the node
					}
					tx.Success();
			  } // tx.close(); - here comes the exception

			  // THEN
			  using ( Transaction ignored = Db.beginTx() )
			  {
					assertEquals( 0, Iterables.count( Db.AllNodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingLabelDoesNotBreakPreviouslyCreatedLabelsIterator()
		 public virtual void RemovingLabelDoesNotBreakPreviouslyCreatedLabelsIterator()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Label label1 = label( "A" );
			  Label label2 = label( "B" );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label1, label2 );

					foreach ( Label next in node.Labels )
					{
						 node.RemoveLabel( next );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingPropertyDoesNotBreakPreviouslyCreatedNodePropertyKeysIterator()
		 public virtual void RemovingPropertyDoesNotBreakPreviouslyCreatedNodePropertyKeysIterator()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "name", "Horst" );
					node.SetProperty( "age", "72" );

					foreach ( string key in node.PropertyKeys )
					{
						 node.RemoveProperty( key );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeWithLotsOfLabelsAndThenRemoveMostOfThem()
		 public virtual void ShouldCreateNodeWithLotsOfLabelsAndThenRemoveMostOfThem()
		 {
			  // given
			  const int totalNumberOfLabels = 200;
			  const int numberOfPreservedLabels = 20;
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < totalNumberOfLabels; i++ )
					{
						 node.AddLabel( label( "label:" + i ) );
					}

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = numberOfPreservedLabels; i < totalNumberOfLabels; i++ )
					{
						 node.RemoveLabel( label( "label:" + i ) );
					}

					tx.Success();
			  }

			  // then
			  using ( Transaction ignored = Db.beginTx() )
			  {
					IList<string> labels = new List<string>();
					foreach ( Label label in node.Labels )
					{
						 labels.Add( label.Name() );
					}
					assertEquals( "labels on node: " + labels, numberOfPreservedLabels, labels.Count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowManyLabelsAndPropertyCursor()
		 public virtual void ShouldAllowManyLabelsAndPropertyCursor()
		 {
			  int propertyCount = 10;
			  int labelCount = 15;

			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < propertyCount; i++ )
					{
						 node.SetProperty( "foo" + i, "bar" );
					}
					for ( int i = 0; i < labelCount; i++ )
					{
						 node.AddLabel( label( "label" + i ) );
					}
					tx.Success();
			  }

			  ISet<int> seenProperties = new HashSet<int>();
			  ISet<int> seenLabels = new HashSet<int>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					DependencyResolver resolver = Db.DependencyResolver;
					ThreadToStatementContextBridge bridge = resolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
					KernelTransaction ktx = bridge.GetKernelTransactionBoundToThisThread( true );
					using ( NodeCursor nodes = ktx.Cursors().allocateNodeCursor(), PropertyCursor propertyCursor = ktx.Cursors().allocatePropertyCursor() )
					{
						 ktx.DataRead().singleNode(node.Id, nodes);
						 while ( nodes.Next() )
						 {
							  nodes.Properties( propertyCursor );
							  while ( propertyCursor.Next() )
							  {
									seenProperties.Add( propertyCursor.PropertyKey() );
							  }

							  LabelSet labels = nodes.Labels();
							  for ( int i = 0; i < labels.NumberOfLabels(); i++ )
							  {
									seenLabels.Add( labels.Label( i ) );
							  }
						 }
					}
					tx.Success();
			  }

			  assertEquals( propertyCount, seenProperties.Count );
			  assertEquals( labelCount, seenLabels.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeWithManyLabels()
		 public virtual void NodeWithManyLabels()
		 {
			  int labels = 500;
			  int halveLabels = labels / 2;
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  long nodeId = CreateNode( db ).Id;

			  AddLabels( nodeId, 0, halveLabels );
			  AddLabels( nodeId, halveLabels, halveLabels );

			  VerifyLabels( nodeId, 0, labels );

			  RemoveLabels( nodeId, halveLabels, halveLabels );
			  VerifyLabels( nodeId, 0, halveLabels );

			  RemoveLabels( nodeId, 0, halveLabels - 2 );
			  VerifyLabels( nodeId, halveLabels - 2, 2 );
		 }

		 private void AddLabels( long nodeId, int startLabelIndex, int count )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node node = DbRule.getNodeById( nodeId );
					int endLabelIndex = startLabelIndex + count;
					for ( int i = startLabelIndex; i < endLabelIndex; i++ )
					{
						 node.AddLabel( LabelWithIndex( i ) );
					}
					tx.Success();
			  }
		 }

		 private void VerifyLabels( long nodeId, int startLabelIndex, int count )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node node = DbRule.getNodeById( nodeId );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<string> labelNames = Iterables.asList( node.Labels ).Select( Label::name ).collect( toSet() );

					assertEquals( count, labelNames.Count );
					int endLabelIndex = startLabelIndex + count;
					for ( int i = startLabelIndex; i < endLabelIndex; i++ )
					{
						 assertTrue( labelNames.Contains( LabelName( i ) ) );
					}
					tx.Success();
			  }
		 }

		 private void RemoveLabels( long nodeId, int startLabelIndex, int count )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node node = DbRule.getNodeById( nodeId );
					int endLabelIndex = startLabelIndex + count;
					for ( int i = startLabelIndex; i < endLabelIndex; i++ )
					{
						 node.RemoveLabel( LabelWithIndex( i ) );
					}
					tx.Success();
			  }
		 }

		 private static Label LabelWithIndex( int index )
		 {
			  return label( LabelName( index ) );
		 }

		 private static string LabelName( int index )
		 {
			  return "Label-" + index;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") private GraphDatabaseService beansAPIWithNoMoreLabelIds()
		 private GraphDatabaseService BeansAPIWithNoMoreLabelIds()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.impl.EphemeralIdGenerator.Factory idFactory = new org.neo4j.test.impl.EphemeralIdGenerator.Factory()
			  EphemeralIdGenerator.Factory idFactory = new FactoryAnonymousInnerClass( this );

			  TestGraphDatabaseFactory dbFactory = new TestGraphDatabaseFactoryAnonymousInnerClass( this, idFactory );

			  return dbFactory.NewImpermanentDatabase( TestDirectory.directory( "impermanent-directory" ) );
		 }

		 private class FactoryAnonymousInnerClass : EphemeralIdGenerator.Factory
		 {
			 private readonly LabelsAcceptanceTest _outerInstance;

			 public FactoryAnonymousInnerClass( LabelsAcceptanceTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 idTypeConfigurationProvider = new CommunityIdTypeConfigurationProvider();
			 }

			 private IdTypeConfigurationProvider idTypeConfigurationProvider;

			 public override IdGenerator open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
			 {
				  if ( idType == IdType.LABEL_TOKEN )
				  {
						IdGenerator generator = generators.get( idType );
						if ( generator == null )
						{
							 IdTypeConfiguration idTypeConfiguration = idTypeConfigurationProvider.getIdTypeConfiguration( idType );
							 generator = new EphemeralIdGeneratorAnonymousInnerClass( this, idType, idTypeConfiguration );
							 generators.put( idType, generator );
						}
						return generator;
				  }
				  return base.open( fileName, grabSize, idType, () => long.MaxValue, long.MaxValue );
			 }

			 private class EphemeralIdGeneratorAnonymousInnerClass : EphemeralIdGenerator
			 {
				 private readonly FactoryAnonymousInnerClass _outerInstance;

				 public EphemeralIdGeneratorAnonymousInnerClass( FactoryAnonymousInnerClass outerInstance, IdType idType, IdTypeConfiguration idTypeConfiguration ) : base( idType, idTypeConfiguration )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override long nextId()
				 {
					  // Same exception as the one thrown by IdGeneratorImpl
					  throw new UnderlyingStorageException( "Id capacity exceeded" );
				 }
			 }
		 }

		 private class TestGraphDatabaseFactoryAnonymousInnerClass : TestGraphDatabaseFactory
		 {
			 private readonly LabelsAcceptanceTest _outerInstance;

			 private EphemeralIdGenerator.Factory _idFactory;

			 public TestGraphDatabaseFactoryAnonymousInnerClass( LabelsAcceptanceTest outerInstance, EphemeralIdGenerator.Factory idFactory )
			 {
				 this.outerInstance = outerInstance;
				 this._idFactory = idFactory;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.graphdb.factory.GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator(final java.io.File storeDir, final org.neo4j.test.TestGraphDatabaseFactoryState state)
			 protected internal override GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
			 {
				  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			 }

			 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			 {
				 private readonly TestGraphDatabaseFactoryAnonymousInnerClass _outerInstance;

				 private File _storeDir;
				 private TestGraphDatabaseFactoryState _state;

				 public DatabaseCreatorAnonymousInnerClass( TestGraphDatabaseFactoryAnonymousInnerClass outerInstance, File storeDir, TestGraphDatabaseFactoryState state )
				 {
					 this.outerInstance = outerInstance;
					 this._storeDir = storeDir;
					 this._state = state;
				 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public GraphDatabaseService newDatabase(@Nonnull Config config)
				 public GraphDatabaseService newDatabase( Config config )
				 {
					  return new ImpermanentGraphDatabaseAnonymousInnerClass( this, _storeDir, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
				 }

				 private class ImpermanentGraphDatabaseAnonymousInnerClass : ImpermanentGraphDatabase
				 {
					 private readonly DatabaseCreatorAnonymousInnerClass _outerInstance;

					 private Config _config;

					 public ImpermanentGraphDatabaseAnonymousInnerClass( DatabaseCreatorAnonymousInnerClass outerInstance, File storeDir, Config config, GraphDatabaseDependencies newDependencies ) : base( storeDir, config, newDependencies )
					 {
						 this.outerInstance = outerInstance;
						 this._config = config;
					 }

					 protected internal void create( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
					 {
						  System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new CommunityEditionModuleWithCustomIdContextFactory( platformModule, _outerInstance.outerInstance.idFactory );
						  new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, factory, storeDir, config, dependencies )
						  .initFacade( storeDir, config, dependencies, this );
					 }

					 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
					 {
						 private readonly ImpermanentGraphDatabaseAnonymousInnerClass _outerInstance;

						 private File _storeDir;
						 private Config _config;
						 private GraphDatabaseFacadeFactory.Dependencies _dependencies;

						 public GraphDatabaseFacadeFactoryAnonymousInnerClass( ImpermanentGraphDatabaseAnonymousInnerClass outerInstance, DatabaseInfo community, System.Func<PlatformModule, AbstractEditionModule> factory, File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, factory )
						 {
							 this.outerInstance = outerInstance;
							 this._storeDir = storeDir;
							 this._config = config;
							 this._dependencies = dependencies;
						 }


						 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
						 {
							  return new ImpermanentPlatformModule( storeDir, config, databaseInfo, dependencies );
						 }
					 }
				 }
			 }
		 }

		 private Node CreateNode( GraphDatabaseService db, params Label[] labels )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( labels );
					tx.Success();
					return node;
			  }
		 }

		 private class CommunityEditionModuleWithCustomIdContextFactory : CommunityEditionModule
		 {
			  internal CommunityEditionModuleWithCustomIdContextFactory( PlatformModule platformModule, EphemeralIdGenerator.Factory idFactory ) : base( platformModule )
			  {
					IdContextFactoryConflict = IdContextFactoryBuilder.of( platformModule.FileSystem, platformModule.JobScheduler ).withIdGenerationFactoryProvider( any => idFactory ).build();
			  }
		 }
	}

}