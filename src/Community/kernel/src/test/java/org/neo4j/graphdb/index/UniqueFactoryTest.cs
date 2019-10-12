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
namespace Neo4Net.Graphdb.index
{
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Graphdb.index.UniqueFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class UniqueFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseConcurrentlyCreatedNode()
		 internal virtual void ShouldUseConcurrentlyCreatedNode()
		 {
			  // given
			  GraphDatabaseService graphdb = mock( typeof( GraphDatabaseService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Index<org.neo4j.graphdb.Node> index = mock(Index.class);
			  Index<Node> index = mock( typeof( Index ) );
			  Transaction tx = mock( typeof( Transaction ) );
			  when( graphdb.BeginTx() ).thenReturn(tx);
			  when( index.GraphDatabase ).thenReturn( graphdb );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") IndexHits<org.neo4j.graphdb.Node> getHits = mock(IndexHits.class);
			  IndexHits<Node> getHits = mock( typeof( IndexHits ) );
			  when( index.get( "key1", "value1" ) ).thenReturn( getHits );
			  Node createdNode = mock( typeof( Node ) );
			  when( graphdb.CreateNode() ).thenReturn(createdNode);
			  Node concurrentNode = mock( typeof( Node ) );
			  when( index.PutIfAbsent( createdNode, "key1", "value1" ) ).thenReturn( concurrentNode );
			  UniqueFactory.UniqueNodeFactory unique = new UniqueNodeFactoryAnonymousInnerClass( this, index );

			  // when
			  UniqueEntity<Node> node = unique.GetOrCreateWithOutcome( "key1", "value1" );

			  // then
			  assertSame( node.Entity(), concurrentNode );
			  assertFalse( node.WasCreated() );
			  verify( index ).get( "key1", "value1" );
			  verify( index ).putIfAbsent( createdNode, "key1", "value1" );
			  verify( graphdb, times( 1 ) ).createNode();
			  verify( tx ).success();
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly UniqueFactoryTest _outerInstance;

			 public UniqueNodeFactoryAnonymousInnerClass( UniqueFactoryTest outerInstance, Neo4Net.Graphdb.index.Index<Node> index ) : base( index )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void initialize( Node created, IDictionary<string, object> properties )
			 {
				  fail( "we did not create the node, so it should not be initialized" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateNodeAndIndexItIfMissing()
		 internal virtual void ShouldCreateNodeAndIndexItIfMissing()
		 {
			  // given
			  GraphDatabaseService graphdb = mock( typeof( GraphDatabaseService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Index<org.neo4j.graphdb.Node> index = mock(Index.class);
			  Index<Node> index = mock( typeof( Index ) );
			  Transaction tx = mock( typeof( Transaction ) );
			  when( graphdb.BeginTx() ).thenReturn(tx);
			  when( index.GraphDatabase ).thenReturn( graphdb );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") IndexHits<org.neo4j.graphdb.Node> indexHits = mock(IndexHits.class);
			  IndexHits<Node> indexHits = mock( typeof( IndexHits ) );

			  when( index.get( "key1", "value1" ) ).thenReturn( indexHits );
			  Node indexedNode = mock( typeof( Node ) );
			  when( graphdb.CreateNode() ).thenReturn(indexedNode);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean initializeCalled = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean initializeCalled = new AtomicBoolean( false );
			  UniqueFactory.UniqueNodeFactory unique = new UniqueNodeFactoryAnonymousInnerClass2( this, index, initializeCalled );

			  // when
			  Node node = unique.GetOrCreate( "key1", "value1" );

			  // then
			  assertSame( node, indexedNode );
			  verify( index ).get( "key1", "value1" );
			  verify( index ).putIfAbsent( indexedNode, "key1", "value1" );
			  verify( graphdb, times( 1 ) ).createNode();
			  verify( tx ).success();
			  assertTrue( initializeCalled.get(), "Node not initialized" );
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass2 : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly UniqueFactoryTest _outerInstance;

			 private AtomicBoolean _initializeCalled;

			 public UniqueNodeFactoryAnonymousInnerClass2( UniqueFactoryTest outerInstance, Neo4Net.Graphdb.index.Index<Node> index, AtomicBoolean initializeCalled ) : base( index )
			 {
				 this.outerInstance = outerInstance;
				 this._initializeCalled = initializeCalled;
			 }

			 protected internal override void initialize( Node created, IDictionary<string, object> properties )
			 {
				  _initializeCalled.set( true );
				  assertEquals( Collections.singletonMap( "key1", "value1" ), properties );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateNodeWithOutcomeAndIndexItIfMissing()
		 internal virtual void ShouldCreateNodeWithOutcomeAndIndexItIfMissing()
		 {
			  // given
			  GraphDatabaseService graphdb = mock( typeof( GraphDatabaseService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Index<org.neo4j.graphdb.Node> index = mock(Index.class);
			  Index<Node> index = mock( typeof( Index ) );
			  Transaction tx = mock( typeof( Transaction ) );
			  when( graphdb.BeginTx() ).thenReturn(tx);
			  when( index.GraphDatabase ).thenReturn( graphdb );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") IndexHits<org.neo4j.graphdb.Node> indexHits = mock(IndexHits.class);
			  IndexHits<Node> indexHits = mock( typeof( IndexHits ) );

			  when( index.get( "key1", "value1" ) ).thenReturn( indexHits );
			  Node indexedNode = mock( typeof( Node ) );
			  when( graphdb.CreateNode() ).thenReturn(indexedNode);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean initializeCalled = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean initializeCalled = new AtomicBoolean( false );
			  UniqueFactory.UniqueNodeFactory unique = new UniqueNodeFactoryAnonymousInnerClass3( this, index, initializeCalled );

			  // when
			  UniqueEntity<Node> node = unique.GetOrCreateWithOutcome( "key1", "value1" );

			  // then
			  assertSame( node.Entity(), indexedNode );
			  assertTrue( node.WasCreated() );
			  verify( index ).get( "key1", "value1" );
			  verify( index ).putIfAbsent( indexedNode, "key1", "value1" );
			  verify( graphdb, times( 1 ) ).createNode();
			  verify( tx ).success();
			  assertTrue( initializeCalled.get(), "Node not initialized" );
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass3 : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly UniqueFactoryTest _outerInstance;

			 private AtomicBoolean _initializeCalled;

			 public UniqueNodeFactoryAnonymousInnerClass3( UniqueFactoryTest outerInstance, Neo4Net.Graphdb.index.Index<Node> index, AtomicBoolean initializeCalled ) : base( index )
			 {
				 this.outerInstance = outerInstance;
				 this._initializeCalled = initializeCalled;
			 }

			 protected internal override void initialize( Node created, IDictionary<string, object> properties )
			 {
				  _initializeCalled.set( true );
				  assertEquals( Collections.singletonMap( "key1", "value1" ), properties );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotTouchTransactionsIfAlreadyInIndex()
		 internal virtual void ShouldNotTouchTransactionsIfAlreadyInIndex()
		 {
			  GraphDatabaseService graphdb = mock( typeof( GraphDatabaseService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Index<org.neo4j.graphdb.Node> index = mock(Index.class);
			  Index<Node> index = mock( typeof( Index ) );
			  when( index.GraphDatabase ).thenReturn( graphdb );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") IndexHits<org.neo4j.graphdb.Node> getHits = mock(IndexHits.class);
			  IndexHits<Node> getHits = mock( typeof( IndexHits ) );
			  when( index.get( "key1", "value1" ) ).thenReturn( getHits );
			  Node indexedNode = mock( typeof( Node ) );
			  when( getHits.Single ).thenReturn( indexedNode );

			  UniqueFactory.UniqueNodeFactory unique = new UniqueNodeFactoryAnonymousInnerClass4( this, index );

			  // when
			  Node node = unique.GetOrCreate( "key1", "value1" );

			  // then
			  assertSame( node, indexedNode );
			  verify( index ).get( "key1", "value1" );
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass4 : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly UniqueFactoryTest _outerInstance;

			 public UniqueNodeFactoryAnonymousInnerClass4( UniqueFactoryTest outerInstance, Neo4Net.Graphdb.index.Index<Node> index ) : base( index )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void initialize( Node created, IDictionary<string, object> properties )
			 {
				  fail( "we did not create the node, so it should not be initialized" );
			 }
		 }
	}

}