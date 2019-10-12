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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using Org.Neo4j.Helpers.Collection;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using GroupVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache.GroupVisitor;
	using NodeChangeVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache.NodeChangeVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NodeRelationshipCacheTest
	public class NodeRelationshipCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public long super;
		 public long Base;
		 private NodeRelationshipCache _cache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _cache.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> data = new List<object[]>();
			  data.Add( new object[]{ 0L } );
			  data.Add( new object[]{ ( long ) int.MaxValue * 2 } );
			  return data;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectNumberOfDenseNodes()
		 public virtual void ShouldReportCorrectNumberOfDenseNodes()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 5, 100, Base );
			  _cache.NodeCount = 26;
			  Increment( _cache, 2, 10 );
			  Increment( _cache, 5, 2 );
			  Increment( _cache, 7, 12 );
			  Increment( _cache, 23, 4 );
			  Increment( _cache, 24, 5 );
			  Increment( _cache, 25, 6 );

			  // THEN
			  assertFalse( _cache.isDense( 0 ) );
			  assertTrue( _cache.isDense( 2 ) );
			  assertFalse( _cache.isDense( 5 ) );
			  assertTrue( _cache.isDense( 7 ) );
			  assertFalse( _cache.isDense( 23 ) );
			  assertTrue( _cache.isDense( 24 ) );
			  assertTrue( _cache.isDense( 25 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoThroughThePhases()
		 public virtual void ShouldGoThroughThePhases()
		 {
			  // GIVEN
			  int nodeCount = 10;
			  _cache = new NodeRelationshipCache( NumberArrayFactory.OFF_HEAP, 20, 100, Base );
			  _cache.NodeCount = nodeCount;
			  IncrementRandomCounts( _cache, nodeCount, nodeCount * 20 );

			  {
			  // Test sparse node semantics
					long node = FindNode( _cache, nodeCount, false );
					TestNode( _cache, node, null );
			  }

			  {
			  // Test dense node semantics
					long node = FindNode( _cache, nodeCount, true );
					TestNode( _cache, node, Direction.OUTGOING );
					TestNode( _cache, node, Direction.INCOMING );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldObserveFirstRelationshipAsEmptyInEachDirection()
		 public virtual void ShouldObserveFirstRelationshipAsEmptyInEachDirection()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 1, 100, Base );
			  int nodes = 100;
			  int typeId = 5;
			  Direction[] directions = Direction.values();
			  GroupVisitor groupVisitor = mock( typeof( GroupVisitor ) );
			  _cache.setForwardScan( true, true );
			  _cache.NodeCount = nodes + 1;
			  for ( int i = 0; i < nodes; i++ )
			  {
					assertEquals( -1L, _cache.getFirstRel( nodes, groupVisitor ) );
					_cache.incrementCount( i );
					long previous = _cache.getAndPutRelationship( i, typeId, directions[i % directions.Length], Random.Next( 1_000_000 ), true );
					assertEquals( -1L, previous );
			  }

			  // WHEN
			  _cache.setForwardScan( false, true );
			  for ( int i = 0; i < nodes; i++ )
			  {
					long previous = _cache.getAndPutRelationship( i, typeId, directions[i % directions.Length], Random.Next( 1_000_000 ), false );
					assertEquals( -1L, previous );
			  }

			  // THEN
			  _cache.setForwardScan( true, true );
			  for ( int i = 0; i < nodes; i++ )
			  {
					assertEquals( -1L, _cache.getFirstRel( nodes, groupVisitor ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetCountAfterGetOnDenseNodes()
		 public virtual void ShouldResetCountAfterGetOnDenseNodes()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 1, 100, Base );
			  long nodeId = 0;
			  int typeId = 3;
			  _cache.NodeCount = 1;
			  _cache.incrementCount( nodeId );
			  _cache.incrementCount( nodeId );
			  _cache.getAndPutRelationship( nodeId, typeId, OUTGOING, 10, true );
			  _cache.getAndPutRelationship( nodeId, typeId, OUTGOING, 12, true );
			  assertTrue( _cache.isDense( nodeId ) );

			  // WHEN
			  long count = _cache.getCount( nodeId, typeId, OUTGOING );
			  assertEquals( 2, count );

			  // THEN
			  assertEquals( 0, _cache.getCount( nodeId, typeId, OUTGOING ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAndPutRelationshipAroundChunkEdge()
		 public virtual void ShouldGetAndPutRelationshipAroundChunkEdge()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 10 );

			  // WHEN
			  long nodeId = 1_000_000 - 1;
			  int typeId = 10;
			  _cache.NodeCount = nodeId + 1;
			  Direction direction = Direction.OUTGOING;
			  long relId = 10;
			  _cache.getAndPutRelationship( nodeId, typeId, direction, relId, false );

			  // THEN
			  assertEquals( relId, _cache.getFirstRel( nodeId, mock( typeof( GroupVisitor ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPutRandomStuff()
		 public virtual void ShouldPutRandomStuff()
		 {
			  // GIVEN
			  int typeId = 10;
			  int nodes = 10_000;
			  MutableLongObjectMap<long[]> key = new LongObjectHashMap<long[]>( nodes );
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 1000, Base );

			  // mark random nodes as dense (dense node threshold is 1 so enough with one increment
			  _cache.NodeCount = nodes;
			  for ( long nodeId = 0; nodeId < nodes; nodeId++ )
			  {
					if ( Random.nextBoolean() )
					{
						 _cache.incrementCount( nodeId );
					}
			  }

			  // WHEN
			  for ( int i = 0; i < 100_000; i++ )
			  {
					long nodeId = Random.nextLong( nodes );
					bool dense = _cache.isDense( nodeId );
					Direction direction = Random.among( Direction.values() );
					long relationshipId = Random.nextLong( 1_000_000 );
					long previousHead = _cache.getAndPutRelationship( nodeId, typeId, direction, relationshipId, false );
					long[] keyIds = key.get( nodeId );
					int keyIndex = dense ? direction.ordinal() : 0;
					if ( keyIds == null )
					{
						 key.put( nodeId, keyIds = MinusOneLongs( Direction.values().length ) );
					}
					assertEquals( keyIds[keyIndex], previousHead );
					keyIds[keyIndex] = relationshipId;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPut6ByteRelationshipIds()
		 public virtual void ShouldPut6ByteRelationshipIds()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 100, Base );
			  long sparseNode = 0;
			  long denseNode = 1;
			  long relationshipId = ( 1L << 48 ) - 2;
			  int typeId = 10;
			  _cache.NodeCount = 2;
			  _cache.incrementCount( denseNode );

			  // WHEN
			  assertEquals( -1L, _cache.getAndPutRelationship( sparseNode, typeId, OUTGOING, relationshipId, false ) );
			  assertEquals( -1L, _cache.getAndPutRelationship( denseNode, typeId, OUTGOING, relationshipId, false ) );

			  // THEN
			  assertEquals( relationshipId, _cache.getAndPutRelationship( sparseNode, typeId, OUTGOING, 1, false ) );
			  assertEquals( relationshipId, _cache.getAndPutRelationship( denseNode, typeId, OUTGOING, 1, false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailFastIfTooBigRelationshipId()
		 public virtual void ShouldFailFastIfTooBigRelationshipId()
		 {
			  // GIVEN
			  int typeId = 10;
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 100, Base );
			  _cache.NodeCount = 1;

			  // WHEN
			  _cache.getAndPutRelationship( 0, typeId, OUTGOING, ( 1L << 48 ) - 2, false );
			  try
			  {
					_cache.getAndPutRelationship( 0, typeId, OUTGOING, ( 1L << 48 ) - 1, false );
					fail( "Should fail" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN Good
					assertTrue( e.Message.contains( "max" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVisitChangedNodes()
		 public virtual void ShouldVisitChangedNodes()
		 {
			  // GIVEN
			  int nodes = 10;
			  int typeId = 10;
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 2, 100, Base );
			  _cache.NodeCount = nodes;
			  for ( long nodeId = 0; nodeId < nodes; nodeId++ )
			  {
					_cache.incrementCount( nodeId );
					if ( Random.nextBoolean() )
					{
						 _cache.incrementCount( nodeId );
					}
			  }
			  MutableLongSet keySparseChanged = new LongHashSet();
			  MutableLongSet keyDenseChanged = new LongHashSet();
			  for ( int i = 0; i < nodes / 2; i++ )
			  {
					long nodeId = Random.nextLong( nodes );
					_cache.getAndPutRelationship( nodeId, typeId, Direction.OUTGOING, Random.nextLong( 1_000_000 ), false );
					bool dense = _cache.isDense( nodeId );
					( dense ? keyDenseChanged : keySparseChanged ).add( nodeId );
			  }

			  {
					// WHEN (sparse)
					NodeChangeVisitor visitor = ( nodeId, array ) =>
					{
					 // THEN (sparse)
					 assertTrue( "Unexpected sparse change reported for " + nodeId, keySparseChanged.remove( nodeId ) );
					};
					_cache.visitChangedNodes( visitor, NodeType.NODE_TYPE_SPARSE );
					assertTrue( "There was " + keySparseChanged.size() + " expected sparse changes that weren't reported", keySparseChanged.Empty );
			  }

			  {
					// WHEN (dense)
					NodeChangeVisitor visitor = ( nodeId, array ) =>
					{
					 // THEN (dense)
					 assertTrue( "Unexpected dense change reported for " + nodeId, keyDenseChanged.remove( nodeId ) );
					};
					_cache.visitChangedNodes( visitor, NodeType.NODE_TYPE_DENSE );
					assertTrue( "There was " + keyDenseChanged.size() + " expected dense changes that weren reported", keyDenseChanged.Empty );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailFastOnTooHighCountOnNode()
		 public virtual void ShouldFailFastOnTooHighCountOnNode()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 10, 100, Base );
			  long nodeId = 5;
			  long count = NodeRelationshipCache.MaxCount - 1;
			  int typeId = 10;
			  _cache.NodeCount = 10;
			  _cache.setCount( nodeId, count, typeId, OUTGOING );

			  // WHEN
			  _cache.incrementCount( nodeId );
			  try
			  {
					_cache.incrementCount( nodeId );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepNextGroupIdForNextRound()
		 public virtual void ShouldKeepNextGroupIdForNextRound()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 100, Base );
			  long nodeId = 0;
			  int typeId = 10;
			  _cache.NodeCount = nodeId + 1;
			  _cache.incrementCount( nodeId );
			  GroupVisitor groupVisitor = mock( typeof( GroupVisitor ) );
			  when( groupVisitor.Visit( anyLong(), anyInt(), anyLong(), anyLong(), anyLong() ) ).thenReturn(1L, 2L, 3L);

			  long firstRelationshipGroupId;
			  {
					// WHEN importing the first type
					long relationshipId = 10;
					_cache.getAndPutRelationship( nodeId, typeId, OUTGOING, relationshipId, true );
					firstRelationshipGroupId = _cache.getFirstRel( nodeId, groupVisitor );

					// THEN
					assertEquals( 1L, firstRelationshipGroupId );
					verify( groupVisitor ).visit( nodeId, typeId, relationshipId, -1L, -1L );

					// Also simulate going back again ("clearing" of the cache requires this)
					_cache.setForwardScan( false, true );
					_cache.getAndPutRelationship( nodeId, typeId, OUTGOING, relationshipId, false );
					_cache.setForwardScan( true, true );
			  }

			  long secondRelationshipGroupId;
			  {
					// WHEN importing the second type
					long relationshipId = 11;
					_cache.getAndPutRelationship( nodeId, typeId, INCOMING, relationshipId, true );
					secondRelationshipGroupId = _cache.getFirstRel( nodeId, groupVisitor );

					// THEN
					assertEquals( 2L, secondRelationshipGroupId );
					verify( groupVisitor ).visit( nodeId, typeId, -1, relationshipId, -1L );

					// Also simulate going back again ("clearing" of the cache requires this)
					_cache.setForwardScan( false, true );
					_cache.getAndPutRelationship( nodeId, typeId, OUTGOING, relationshipId, false );
					_cache.setForwardScan( true, true );
			  }

			  {
					// WHEN importing the third type
					long relationshipId = 10;
					_cache.getAndPutRelationship( nodeId, typeId, BOTH, relationshipId, true );
					long thirdRelationshipGroupId = _cache.getFirstRel( nodeId, groupVisitor );
					assertEquals( 3L, thirdRelationshipGroupId );
					verify( groupVisitor ).visit( nodeId, typeId, -1L, -1L, relationshipId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveDenseNodesWithBigCounts()
		 public virtual void ShouldHaveDenseNodesWithBigCounts()
		 {
			  // A count of a dense node follow a different path during import, first there's counting per node
			  // then import goes into actual import of relationships where individual chain degrees are
			  // kept. So this test will first set a total count, then set count for a specific chain

			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 100, Base );
			  long nodeId = 1;
			  int typeId = 10;
			  _cache.NodeCount = nodeId + 1;
			  _cache.setCount( nodeId, 2, typeId, OUTGOING ); // surely dense now
			  _cache.getAndPutRelationship( nodeId, typeId, OUTGOING, 1, true );
			  _cache.getAndPutRelationship( nodeId, typeId, INCOMING, 2, true );

			  // WHEN
			  long highCountOut = NodeRelationshipCache.MaxCount - 100;
			  long highCountIn = NodeRelationshipCache.MaxCount - 50;
			  _cache.setCount( nodeId, highCountOut, typeId, OUTGOING );
			  _cache.setCount( nodeId, highCountIn, typeId, INCOMING );
			  _cache.getAndPutRelationship( nodeId, typeId, OUTGOING, 1, true );
			  _cache.getAndPutRelationship( nodeId, typeId, INCOMING, 2, true );

			  // THEN
			  assertEquals( highCountOut + 1, _cache.getCount( nodeId, typeId, OUTGOING ) );
			  assertEquals( highCountIn + 1, _cache.getCount( nodeId, typeId, INCOMING ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCacheMultipleDenseNodeRelationshipHeads()
		 public virtual void ShouldCacheMultipleDenseNodeRelationshipHeads()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1 );
			  _cache.NodeCount = 10;
			  long nodeId = 3;
			  _cache.setCount( nodeId, 10, 0, OUTGOING );

			  // WHEN
			  IDictionary<Pair<int, Direction>, long> firstRelationshipIds = new Dictionary<Pair<int, Direction>, long>();
			  int typeCount = 3;
			  for ( int typeId = 0, relationshipId = 0; typeId < typeCount; typeId++ )
			  {
					foreach ( Direction direction in Direction.values() )
					{
						 long firstRelationshipId = relationshipId++;
						 _cache.getAndPutRelationship( nodeId, typeId, direction, firstRelationshipId, true );
						 firstRelationshipIds[Pair.of( typeId, direction )] = firstRelationshipId;
					}
			  }
			  AtomicInteger visitCount = new AtomicInteger();
			  GroupVisitor visitor = ( nodeId1, typeId, @out, @in, loop ) =>
			  {
				visitCount.incrementAndGet();
				assertEquals( firstRelationshipIds[Pair.of( typeId, OUTGOING )], @out );
				assertEquals( firstRelationshipIds[Pair.of( typeId, INCOMING )], @in );
				assertEquals( firstRelationshipIds[Pair.of( typeId, BOTH )], loop );
				return 0;
			  };
			  _cache.getFirstRel( nodeId, visitor );

			  // THEN
			  assertEquals( typeCount, visitCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSparseNodesWithBigCounts()
		 public virtual void ShouldHaveSparseNodesWithBigCounts()
		 {
			  // GIVEN
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1, 100, Base );
			  long nodeId = 1;
			  int typeId = 10;
			  _cache.NodeCount = nodeId + 1;

			  // WHEN
			  long highCount = NodeRelationshipCache.MaxCount - 100;
			  _cache.setCount( nodeId, highCount, typeId, OUTGOING );
			  long nextHighCount = _cache.incrementCount( nodeId );

			  // THEN
			  assertEquals( highCount + 1, nextHighCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailFastOnTooHighNodeCount()
		 public virtual void ShouldFailFastOnTooHighNodeCount()
		 {
			  // given
			  _cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, 1 );

			  try
			  {
					// when
					_cache.NodeCount = 2L << ( 5 * ( sizeof( sbyte ) * 8 ) );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  {
					// then good
			  }
		 }

		 private void TestNode( NodeRelationshipCache link, long node, Direction direction )
		 {
			  int typeId = 0; // doesn't matter here because it's all sparse
			  long count = link.GetCount( node, typeId, direction );
			  assertEquals( -1, link.GetAndPutRelationship( node, typeId, direction, 5, false ) );
			  assertEquals( 5, link.GetAndPutRelationship( node, typeId, direction, 10, false ) );
			  assertEquals( count, link.GetCount( node, typeId, direction ) );
		 }

		 private long FindNode( NodeRelationshipCache link, long nodeCount, bool isDense )
		 {
			  for ( long i = 0; i < nodeCount; i++ )
			  {
					if ( link.IsDense( i ) == isDense )
					{
						 return i;
					}
			  }
			  throw new System.ArgumentException( "No dense node found" );
		 }

		 private long IncrementRandomCounts( NodeRelationshipCache link, int nodeCount, int i )
		 {
			  long highestSeenCount = 0;
			  while ( i-- > 0 )
			  {
					long node = Random.Next( nodeCount );
					highestSeenCount = max( highestSeenCount, link.IncrementCount( node ) );
			  }
			  return highestSeenCount;
		 }

		 private void Increment( NodeRelationshipCache cache, long node, int count )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					cache.IncrementCount( node );
			  }
		 }

		 private long[] MinusOneLongs( int length )
		 {
			  long[] array = new long[length];
			  Arrays.fill( array, -1 );
			  return array;
		 }
	}

}