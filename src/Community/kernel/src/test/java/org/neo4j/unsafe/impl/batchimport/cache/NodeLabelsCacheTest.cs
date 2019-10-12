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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using Test = org.junit.Test;


	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	public class NodeLabelsCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCacheSmallSetOfLabelsPerNode()
		 public virtual void ShouldCacheSmallSetOfLabelsPerNode()
		 {
			  // GIVEN
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 5, CHUNK_SIZE );
			  NodeLabelsCache.Client client = cache.NewClient();
			  long nodeId = 0;

			  // WHEN
			  cache.Put( nodeId, new long[] { 1, 2, 3 } );

			  // THEN
			  int[] readLabels = new int[3];
			  cache.Get( client, nodeId, readLabels );
			  assertArrayEquals( new int[] { 1, 2, 3 }, readLabels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLargeAmountOfLabelsPerNode()
		 public virtual void ShouldHandleLargeAmountOfLabelsPerNode()
		 {
			  // GIVEN
			  int highLabelId = 1000;
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, highLabelId, CHUNK_SIZE );
			  NodeLabelsCache.Client client = cache.NewClient();
			  long nodeId = 0;

			  // WHEN
			  int[] labels = RandomLabels( 200, 1000 );
			  cache.Put( nodeId, AsLongArray( labels ) );

			  // THEN
			  int[] readLabels = new int[labels.Length];
			  cache.Get( client, nodeId, readLabels );
			  assertArrayEquals( labels, readLabels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLabelsForManyNodes()
		 public virtual void ShouldHandleLabelsForManyNodes()
		 {
			  // GIVEN a really weird scenario where we have 5000 different labels
			  int highLabelId = 1_000;
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, highLabelId, 1_000_000 );
			  NodeLabelsCache.Client client = cache.NewClient();
			  int numberOfNodes = 100_000;
			  int[][] expectedLabels = new int[numberOfNodes][];
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					int[] labels = RandomLabels( _random.Next( 30 ) + 1, highLabelId );
					expectedLabels[i] = labels;
					cache.Put( i, AsLongArray( labels ) );
			  }

			  // THEN
			  int[] forceCreationOfNewIntArray = new int[0];
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					int[] labels = cache.Get( client, i, forceCreationOfNewIntArray );
					assertArrayEquals( "For node " + i, expectedLabels[i], labels );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEndTargetArrayWithMinusOne()
		 public virtual void ShouldEndTargetArrayWithMinusOne()
		 {
			  // GIVEN
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 10 );
			  NodeLabelsCache.Client client = cache.NewClient();
			  cache.Put( 10, new long[] { 5, 6, 7, 8 } );

			  // WHEN
			  int[] target = new int[20];
			  assertSame( target, cache.Get( client, 10, target ) );
			  assertEquals( 5, target[0] );
			  assertEquals( 6, target[1] );
			  assertEquals( 7, target[2] );
			  assertEquals( 8, target[3] );

			  // THEN
			  assertEquals( -1, target[4] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyArrayForNodeWithNoLabelsAndNoLabelsWhatsoever()
		 public virtual void ShouldReturnEmptyArrayForNodeWithNoLabelsAndNoLabelsWhatsoever()
		 {
			  // GIVEN
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, 0 );
			  NodeLabelsCache.Client client = cache.NewClient();

			  // WHEN
			  int[] target = new int[3];
			  cache.Get( client, 0, target );

			  // THEN
			  assertEquals( -1, target[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportConcurrentGet() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportConcurrentGet()
		 {
			  // GIVEN
			  int highLabelId = 10;
			  int numberOfNodes = 100;
			  int[][] expectedLabels = new int[numberOfNodes][];
			  NodeLabelsCache cache = new NodeLabelsCache( NumberArrayFactory_Fields.AutoWithoutPagecache, highLabelId );
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					cache.Put( i, AsLongArray( expectedLabels[i] = RandomLabels( _random.Next( 5 ), highLabelId ) ) );
			  }

			  // WHEN
			  Race getRace = new Race();
			  for ( int i = 0; i < 10; i++ )
			  {
					getRace.AddContestant( new LabelGetter( cache, expectedLabels, numberOfNodes ) );
			  }

			  // THEN expected labels should be had (asserted in LabelGetter), and no exceptions (propagated by go())
			  getRace.Go();
		 }

		 private class LabelGetter : ThreadStart
		 {
			  internal readonly NodeLabelsCache Cache;
			  internal readonly int[][] ExpectedLabels;
			  internal readonly NodeLabelsCache.Client Client;
			  internal readonly int NumberOfNodes;
			  internal int[] Scratch = new int[10];

			  internal LabelGetter( NodeLabelsCache cache, int[][] expectedLabels, int numberOfNodes )
			  {
					this.Cache = cache;
					this.Client = cache.NewClient();
					this.ExpectedLabels = expectedLabels;
					this.NumberOfNodes = numberOfNodes;
			  }

			  public override void Run()
			  {
					for ( int i = 0; i < 1_000; i++ )
					{
						 int nodeId = ThreadLocalRandom.current().Next(NumberOfNodes);
						 Scratch = Cache.get( Client, nodeId, Scratch );
						 AssertCorrectLabels( nodeId, Scratch );
					}
			  }

			  internal virtual void AssertCorrectLabels( int nodeId, int[] gotten )
			  {
					int[] expected = ExpectedLabels[nodeId];
					for ( int i = 0; i < expected.Length; i++ )
					{
						 assertEquals( expected[i], gotten[i] );
					}

					if ( gotten.Length != expected.Length )
					{
						 // gotten is a "scratch" array, i.e. reused and not resized all the time, instead ended with -1 value.
						 assertEquals( -1, gotten[expected.Length] );
					}
			  }
		 }

		 private long[] AsLongArray( int[] labels )
		 {
			  long[] result = new long[labels.Length];
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					result[i] = labels[i];
			  }
			  return result;
		 }

		 private int[] RandomLabels( int count, int highId )
		 {
			  int[] result = new int[count];
			  for ( int i = 0; i < count; i++ )
			  {
					result[i] = _random.Next( highId );
			  }
			  return result;
		 }

		 private const int CHUNK_SIZE = 100;
		 private readonly Random _random = new Random( 1234 );
	}

}