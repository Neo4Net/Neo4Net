using System;
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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Timeout = org.junit.rules.Timeout;


	using Primitive = Org.Neo4j.Collection.primitive.Primitive;
	using PrimitiveLongSet = Org.Neo4j.Collection.primitive.PrimitiveLongSet;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using MultiTokenSchemaDescriptor = Org.Neo4j.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using KernelImpl = Org.Neo4j.Kernel.Impl.Api.KernelImpl;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using VerboseTimeout = Org.Neo4j.Test.rule.VerboseTimeout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
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
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexProviderFactory.DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.assertQueryFindsIds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken;

	public class FulltextIndexProviderTest
	{
		 private const string NAME = "fulltext";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.Timeout timeout = org.neo4j.test.rule.VerboseTimeout.builder().withTimeout(1, java.util.concurrent.TimeUnit.MINUTES).build();
		 public Timeout Timeout = VerboseTimeout.builder().withTimeout(1, TimeUnit.MINUTES).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public DatabaseRule Db = new EmbeddedDatabaseRule();

		 private Node _node1;
		 private Node _node2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepDB()
		 public virtual void PrepDB()
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					_node1 = Db.createNode( label( "hej" ), label( "ha" ), label( "he" ) );
					_node1.setProperty( "hej", "value" );
					_node1.setProperty( "ha", "value1" );
					_node1.setProperty( "he", "value2" );
					_node1.setProperty( "ho", "value3" );
					_node1.setProperty( "hi", "value4" );
					_node2 = Db.createNode();
					Relationship rel = _node1.createRelationshipTo( _node2, RelationshipType.withName( "hej" ) );
					rel.SetProperty( "hej", "valuuu" );
					rel.SetProperty( "ha", "value1" );
					rel.SetProperty( "he", "value2" );
					rel.SetProperty( "ho", "value3" );
					rel.SetProperty( "hi", "value4" );

					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createFulltextIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateFulltextIndex()
		 {
			  IndexReference fulltextIndex = CreateIndex( new int[]{ 7, 8, 9 }, new int[]{ 2, 3, 4 } );
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					IndexReference descriptor = transaction.SchemaRead().indexGetForName(NAME);
					assertEquals( descriptor.Schema(), fulltextIndex.Schema() );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndRetainFulltextIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndRetainFulltextIndex()
		 {
			  IndexReference fulltextIndex = CreateIndex( new int[]{ 7, 8, 9 }, new int[]{ 2, 3, 4 } );
			  Db.restartDatabase( DatabaseRule.RestartAction_Fields.EMPTY );

			  VerifyThatFulltextIndexIsPresent( fulltextIndex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndRetainRelationshipFulltextIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndRetainRelationshipFulltextIndex()
		 {
			  IndexReference indexReference;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					MultiTokenSchemaDescriptor multiTokenSchemaDescriptor = multiToken( new int[]{ 0, 1, 2 }, EntityType.RELATIONSHIP, 0, 1, 2, 3 );
					FulltextSchemaDescriptor schema = new FulltextSchemaDescriptor( multiTokenSchemaDescriptor, new Properties() );
					indexReference = transaction.SchemaWrite().indexCreate(schema, DESCRIPTOR.name(), "fulltext");
					transaction.Success();
			  }
			  Await( indexReference );
			  Db.restartDatabase( DatabaseRule.RestartAction_Fields.EMPTY );

			  VerifyThatFulltextIndexIsPresent( indexReference );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndQueryFulltextIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndQueryFulltextIndex()
		 {
			  IndexReference indexReference;
			  FulltextIndexProvider provider = ( FulltextIndexProvider ) Db.resolveDependency( typeof( IndexProviderMap ) ).lookup( DESCRIPTOR );
			  indexReference = CreateIndex( new int[]{ 0, 1, 2 }, new int[]{ 0, 1, 2, 3 } );
			  Await( indexReference );
			  long thirdNodeid;
			  thirdNodeid = CreateTheThirdNode();
			  VerifyNodeData( provider, thirdNodeid );
			  Db.restartDatabase( DatabaseRule.RestartAction_Fields.EMPTY );
			  provider = ( FulltextIndexProvider ) Db.resolveDependency( typeof( IndexProviderMap ) ).lookup( DESCRIPTOR );
			  VerifyNodeData( provider, thirdNodeid );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndQueryFulltextRelationshipIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndQueryFulltextRelationshipIndex()
		 {
			  FulltextIndexProvider provider = ( FulltextIndexProvider ) Db.resolveDependency( typeof( IndexProviderMap ) ).lookup( DESCRIPTOR );
			  IndexReference indexReference;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					MultiTokenSchemaDescriptor multiTokenSchemaDescriptor = multiToken( new int[]{ 0, 1, 2 }, EntityType.RELATIONSHIP, 0, 1, 2, 3 );
					FulltextSchemaDescriptor schema = new FulltextSchemaDescriptor( multiTokenSchemaDescriptor, new Properties() );
					indexReference = transaction.SchemaWrite().indexCreate(schema, DESCRIPTOR.name(), "fulltext");
					transaction.Success();
			  }
			  Await( indexReference );
			  long secondRelId;
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Relationship ho = _node1.createRelationshipTo( _node2, RelationshipType.withName( "ho" ) );
					secondRelId = ho.Id;
					ho.SetProperty( "hej", "villa" );
					ho.SetProperty( "ho", "value3" );
					transaction.Success();
			  }
			  VerifyRelationshipData( provider, secondRelId );
			  Db.restartDatabase( DatabaseRule.RestartAction_Fields.EMPTY );
			  provider = ( FulltextIndexProvider ) Db.resolveDependency( typeof( IndexProviderMap ) ).lookup( DESCRIPTOR );
			  VerifyRelationshipData( provider, secondRelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multiTokenFulltextIndexesMustShowUpInSchemaGetIndexes()
		 public virtual void MultiTokenFulltextIndexesMustShowUpInSchemaGetIndexes()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodeIndex", array( "Label1", "Label2" ), array( "prop1", "prop2" ) ) ).close();
					Db.execute( format( RELATIONSHIP_CREATE, "relIndex", array( "RelType1", "RelType2" ), array( "prop1", "prop2" ) ) ).close();
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( IndexDefinition index in Db.schema().Indexes )
					{
						 assertFalse( index.ConstraintIndex );
						 assertTrue( index.MultiTokenIndex );
						 assertTrue( index.CompositeIndex );
						 if ( index.NodeIndex )
						 {
							  assertFalse( index.RelationshipIndex );
							  assertThat( index.Labels, containsInAnyOrder( Label.label( "Label1" ), Label.label( "Label2" ) ) );
							  try
							  {
									index.Label;
									fail( "index.getLabel() on multi-token IndexDefinition should have thrown." );
							  }
							  catch ( System.InvalidOperationException )
							  {
							  }
							  try
							  {
									index.RelationshipTypes;
									fail( "index.getRelationshipTypes() on node IndexDefinition should have thrown." );
							  }
							  catch ( System.InvalidOperationException )
							  {
							  }
						 }
						 else
						 {
							  assertTrue( index.RelationshipIndex );
							  assertThat( index.RelationshipTypes, containsInAnyOrder( RelationshipType.withName( "RelType1" ), RelationshipType.withName( "RelType2" ) ) );
							  try
							  {
									index.RelationshipType;
									fail( "index.getRelationshipType() on multi-token IndexDefinition should have thrown." );
							  }
							  catch ( System.InvalidOperationException )
							  {
							  }
							  try
							  {
									index.Labels;
									fail( "index.getLabels() on node IndexDefinition should have thrown." );
							  }
							  catch ( System.InvalidOperationException )
							  {
							  }
						 }
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void awaitIndexesOnlineMustWorkOnFulltextIndexes()
		 public virtual void AwaitIndexesOnlineMustWorkOnFulltextIndexes()
		 {
			  string prop1 = "prop1";
			  string prop2 = "prop2";
			  string prop3 = "prop3";
			  string val1 = "foo foo";
			  string val2 = "bar bar";
			  string val3 = "baz baz";
			  Label label1 = Label.label( "FirstLabel" );
			  Label label2 = Label.label( "SecondLabel" );
			  Label label3 = Label.label( "ThirdLabel" );
			  RelationshipType relType1 = RelationshipType.withName( "FirstRelType" );
			  RelationshipType relType2 = RelationshipType.withName( "SecondRelType" );
			  RelationshipType relType3 = RelationshipType.withName( "ThirdRelType" );

			  LongHashSet nodes1 = new LongHashSet();
			  LongHashSet nodes2 = new LongHashSet();
			  LongHashSet nodes3 = new LongHashSet();
			  LongHashSet rels1 = new LongHashSet();
			  LongHashSet rels2 = new LongHashSet();
			  LongHashSet rels3 = new LongHashSet();

			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 Node node1 = Db.createNode( label1 );
						 node1.SetProperty( prop1, val1 );
						 nodes1.add( node1.Id );
						 Relationship rel1 = node1.CreateRelationshipTo( node1, relType1 );
						 rel1.SetProperty( prop1, val1 );
						 rels1.add( rel1.Id );

						 Node node2 = Db.createNode( label2 );
						 node2.SetProperty( prop2, val2 );
						 nodes2.add( node2.Id );
						 Relationship rel2 = node1.CreateRelationshipTo( node2, relType2 );
						 rel2.SetProperty( prop2, val2 );
						 rels2.add( rel2.Id );

						 Node node3 = Db.createNode( label3 );
						 node3.SetProperty( prop3, val3 );
						 nodes3.add( node3.Id );
						 Relationship rel3 = node1.CreateRelationshipTo( node3, relType3 );
						 rel3.SetProperty( prop3, val3 );
						 rels3.add( rel3.Id );
					}
					tx.Success();
			  }

			  // Test that multi-token node indexes can be waited for.
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( NODE_CREATE, "nodeIndex", array( label1.Name(), label2.Name(), label3.Name() ), array(prop1, prop2, prop3) ) ).close();
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertQueryFindsIds( Db, true, "nodeIndex", "foo", nodes1 );
					assertQueryFindsIds( Db, true, "nodeIndex", "bar", nodes2 );
					assertQueryFindsIds( Db, true, "nodeIndex", "baz", nodes3 );
					tx.Success();
			  }

			  // Test that multi-token relationship indexes can be waited for.
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( format( RELATIONSHIP_CREATE, "relIndex", array( relType1.Name(), relType2.Name(), relType3.Name() ), array(prop1, prop2, prop3) ) ).close();
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertQueryFindsIds( Db, false, "relIndex", "foo", rels1 );
					assertQueryFindsIds( Db, false, "relIndex", "bar", rels2 );
					assertQueryFindsIds( Db, false, "relIndex", "baz", rels3 );
					tx.Success();
			  }
		 }

		 private KernelTransactionImplementation KernelTransaction
		 {
			 get
			 {
				  try
				  {
						return ( KernelTransactionImplementation ) Db.resolveDependency( typeof( KernelImpl ) ).beginTransaction( Org.Neo4j.@internal.Kernel.Api.Transaction_Type.Explicit, LoginContext.AUTH_DISABLED );
				  }
				  catch ( TransactionFailureException )
				  {
						throw new Exception( "oops" );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createIndex(int[] entityTokens, int[] propertyIds) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
		 private IndexReference CreateIndex( int[] entityTokens, int[] propertyIds )

		 {
			  IndexReference fulltext;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					MultiTokenSchemaDescriptor multiTokenSchemaDescriptor = multiToken( entityTokens, EntityType.NODE, propertyIds );
					FulltextSchemaDescriptor schema = new FulltextSchemaDescriptor( multiTokenSchemaDescriptor, new Properties() );
					fulltext = transaction.SchemaWrite().indexCreate(schema, DESCRIPTOR.name(), NAME);
					transaction.Success();
			  }
			  return fulltext;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyThatFulltextIndexIsPresent(org.neo4j.internal.kernel.api.IndexReference fulltextIndexDescriptor) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void VerifyThatFulltextIndexIsPresent( IndexReference fulltextIndexDescriptor )
		 {
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					IndexReference descriptor = transaction.SchemaRead().indexGetForName(NAME);
					assertEquals( fulltextIndexDescriptor.Schema(), descriptor.Schema() );
					assertEquals( ( ( IndexDescriptor ) fulltextIndexDescriptor ).type(), ((IndexDescriptor) descriptor).type() );
					transaction.Success();
			  }
		 }

		 private long CreateTheThirdNode()
		 {
			  long secondNodeId;
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Node hej = Db.createNode( label( "hej" ) );
					secondNodeId = hej.Id;
					hej.SetProperty( "hej", "villa" );
					hej.SetProperty( "ho", "value3" );
					transaction.Success();
			  }
			  return secondNodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyNodeData(FulltextIndexProvider provider, long thirdNodeid) throws Exception
		 private void VerifyNodeData( FulltextIndexProvider provider, long thirdNodeid )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = LuceneFulltextTestSupport.KernelTransaction( tx );
					ScoreEntityIterator result = provider.Query( ktx, "fulltext", "value" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( 0L, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );

					result = provider.Query( ktx, "fulltext", "villa" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( thirdNodeid, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );

					result = provider.Query( ktx, "fulltext", "value3" );
					PrimitiveLongSet ids = Primitive.longSet();
					ids.Add( 0L );
					ids.Add( thirdNodeid );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( ids.Remove( result.Next().entityId() ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( ids.Remove( result.Next().entityId() ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRelationshipData(FulltextIndexProvider provider, long secondRelId) throws Exception
		 private void VerifyRelationshipData( FulltextIndexProvider provider, long secondRelId )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = LuceneFulltextTestSupport.KernelTransaction( tx );
					ScoreEntityIterator result = provider.Query( ktx, "fulltext", "valuuu" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( 0L, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );

					result = provider.Query( ktx, "fulltext", "villa" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( secondRelId, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );

					result = provider.Query( ktx, "fulltext", "value3" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( 0L, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( secondRelId, result.Next().entityId() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void await(org.neo4j.internal.kernel.api.IndexReference descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void Await( IndexReference descriptor )
		 {
			  try
			  {
					  using ( Transaction ignore = Db.beginTx() )
					  {
						while ( KernelTransaction.schemaRead().indexGetState(descriptor) != InternalIndexState.ONLINE )
						{
							 Thread.Sleep( 100 );
						}
					  }
			  }
			  catch ( InterruptedException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
		 }
	}

}