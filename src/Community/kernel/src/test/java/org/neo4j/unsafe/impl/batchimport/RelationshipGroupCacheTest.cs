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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ByteUnit = Neo4Net.Io.ByteUnit;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;

	public class RelationshipGroupCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPutGroupsOnlyWithinPreparedRange()
		 public virtual void ShouldPutGroupsOnlyWithinPreparedRange()
		 {
			  // GIVEN
			  int nodeCount = 1000;
			  RelationshipGroupCache cache = new RelationshipGroupCache( HEAP, ByteUnit.kibiBytes( 4 ), nodeCount );
			  int[] counts = new int[nodeCount];
			  for ( int nodeId = 0; nodeId < Counts.Length; nodeId++ )
			  {
					counts[nodeId] = Random.Next( 10 );
					SetCount( cache, nodeId, counts[nodeId] );
			  }

			  long toNodeId = cache.Prepare( 0 );
			  assertTrue( toNodeId < nodeCount );

			  // WHEN
			  bool thereAreMoreGroups = true;
			  int cachedCount = 0;
			  while ( thereAreMoreGroups )
			  {
					thereAreMoreGroups = false;
					for ( int nodeId = 0; nodeId < nodeCount; nodeId++ )
					{
						 if ( counts[nodeId] > 0 )
						 {
							  thereAreMoreGroups = true;
							  int typeId = counts[nodeId]--;
							  if ( cache.Put( ( new RelationshipGroupRecord( nodeId ) ).initialize( true, typeId, -1, -1, -1, nodeId, -1 ) ) )
							  {
									cachedCount++;
							  }
						 }
					}
			  }
			  assertTrue( cachedCount >= toNodeId );

			  // THEN the relationship groups we get back are only for those we prepared for
			  int readCount = 0;
			  foreach ( RelationshipGroupRecord cachedGroup in cache )
			  {
					assertTrue( cachedGroup.OwningNode >= 0 && cachedGroup.OwningNode < toNodeId );
					readCount++;
			  }
			  assertEquals( cachedCount, readCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindSpaceToPutMoreGroupsThanSpecifiedForANode()
		 public virtual void ShouldNotFindSpaceToPutMoreGroupsThanSpecifiedForANode()
		 {
			  // GIVEN
			  int nodeCount = 10;
			  RelationshipGroupCache cache = new RelationshipGroupCache( HEAP, ByteUnit.kibiBytes( 4 ), nodeCount );
			  SetCount( cache, 1, 7 );
			  assertEquals( nodeCount, cache.Prepare( 0 ) );

			  // WHEN
			  for ( int i = 0; i < 7; i++ )
			  {
					cache.Put( ( new RelationshipGroupRecord( i + 1 ) ).initialize( true, i, -1, -1, -1, 1, -1 ) );
			  }
			  try
			  {
					cache.Put( ( new RelationshipGroupRecord( 8 ) ).initialize( true, 8, -1, -1, -1, 1, -1 ) );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortOutOfOrderTypes()
		 public virtual void ShouldSortOutOfOrderTypes()
		 {
			  // GIVEN
			  int nodeCount = 100;
			  RelationshipGroupCache cache = new RelationshipGroupCache( HEAP, ByteUnit.kibiBytes( 40 ), nodeCount );
			  int[] counts = new int[nodeCount];
			  int groupCount = 0;
			  for ( int nodeId = 0; nodeId < Counts.Length; nodeId++ )
			  {
					counts[nodeId] = Random.Next( 10 );
					SetCount( cache, nodeId, counts[nodeId] );
					groupCount += counts[nodeId];
			  }
			  assertEquals( nodeCount, cache.Prepare( 0 ) );
			  bool thereAreMoreGroups = true;
			  int cachedCount = 0;
			  int[] types = ScrambledTypes( 10 );
			  for ( int i = 0; thereAreMoreGroups; i++ )
			  {
					int typeId = types[i];
					thereAreMoreGroups = false;
					for ( int nodeId = 0; nodeId < nodeCount; nodeId++ )
					{
						 if ( counts[nodeId] > 0 )
						 {
							  thereAreMoreGroups = true;
							  if ( cache.Put( ( new RelationshipGroupRecord( nodeId ) ).initialize( true, typeId, -1, -1, -1, nodeId, -1 ) ) )
							  {
									cachedCount++;
									counts[nodeId]--;
							  }
						 }
					}
			  }
			  assertEquals( groupCount, cachedCount );

			  // WHEN/THEN
			  long currentNodeId = -1;
			  int currentTypeId = -1;
			  int readCount = 0;
			  foreach ( RelationshipGroupRecord group in cache )
			  {
					assertTrue( group.OwningNode >= currentNodeId );
					if ( group.OwningNode > currentNodeId )
					{
						 currentNodeId = group.OwningNode;
						 currentTypeId = -1;
					}
					assertTrue( group.Type > currentTypeId );
					readCount++;
			  }
			  assertEquals( cachedCount, readCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleGroupCountBeyondSignedShortRange()
		 public virtual void ShouldHandleGroupCountBeyondSignedShortRange()
		 {
			  // GIVEN
			  long nodeId = 0;
			  int limit = short.MaxValue + 10;
			  RelationshipGroupCache cache = new RelationshipGroupCache( HEAP, ByteUnit.kibiBytes( 100 ), nodeId + 1 );

			  // WHEN first counting all groups per node
			  for ( int type = 0; type < limit; type++ )
			  {
					cache.IncrementGroupCount( nodeId );
			  }
			  // and WHEN later putting group records into the cache
			  RelationshipGroupRecord group = new RelationshipGroupRecord( -1 );
			  group.OwningNode = nodeId;
			  for ( int type = 0; type < limit; type++ )
			  {
					group.Id = type;
					group.FirstOut = type; // just some relationship
					group.Type = type;
					cache.Put( group );
			  }
			  long prepared = cache.Prepare( nodeId );

			  // THEN that should work, because it used to fail inside prepare, but we can also ask
			  // the groupCount method to be sure
			  assertEquals( nodeId, prepared );
			  assertEquals( limit, cache.GroupCount( nodeId ) );
		 }

		 private int[] ScrambledTypes( int count )
		 {
			  int[] types = new int[count];
			  for ( int i = 0; i < count; i++ )
			  {
					types[i] = i + short.MaxValue;
			  }

			  for ( int i = 0; i < 10; i++ )
			  {
					Swap( types, i, Random.Next( count ) );
			  }
			  return types;
		 }

		 private void Swap( int[] types, int a, int b )
		 {
			  int temp = types[a];
			  types[a] = types[b];
			  types[b] = temp;
		 }

		 private void SetCount( RelationshipGroupCache cache, int nodeId, int count )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					cache.IncrementGroupCount( nodeId );
			  }
		 }
	}

}