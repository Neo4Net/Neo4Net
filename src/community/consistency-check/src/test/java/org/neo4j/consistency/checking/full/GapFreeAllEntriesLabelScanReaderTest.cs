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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using AllEntriesLabelScanReader = Neo4Net.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class GapFreeAllEntriesLabelScanReaderTest
	internal class GapFreeAllEntriesLabelScanReaderTest
	{
		 private const int EMPTY_RANGE = 0;
		 private static readonly int _nonEmptyRange = 0b10101; // 0, 2, 4
		 private const int RANGE_SIZE = 10;
		 private static readonly long[] _labelIds = new long[] { 1 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFillGapInBeginning()
		 internal virtual void ShouldFillGapInBeginning()
		 {
			  // given
			  int[] ranges = Array( EMPTY_RANGE, EMPTY_RANGE, _nonEmptyRange );
			  GapFreeAllEntriesLabelScanReader reader = NewGapFreeAllEntriesLabelScanReader( ranges );

			  // when
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();

			  // then
			  AssertRanges( iterator, ranges );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFillGapInEnd()
		 internal virtual void ShouldFillGapInEnd()
		 {
			  // given
			  int[] ranges = Array( _nonEmptyRange, EMPTY_RANGE, EMPTY_RANGE );
			  GapFreeAllEntriesLabelScanReader reader = NewGapFreeAllEntriesLabelScanReader( ranges );

			  // when
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();

			  // then
			  AssertRanges( iterator, ranges );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFillGapInMiddle()
		 internal virtual void ShouldFillGapInMiddle()
		 {
			  // given
			  int[] ranges = Array( EMPTY_RANGE, _nonEmptyRange, EMPTY_RANGE );
			  GapFreeAllEntriesLabelScanReader reader = NewGapFreeAllEntriesLabelScanReader( ranges );

			  // when
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();

			  // then
			  AssertRanges( iterator, ranges );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFillRandomGaps()
		 internal virtual void ShouldFillRandomGaps()
		 {
			  // given
			  int numberOfRanges = _random.intBetween( 50, 100 );
			  int[] ranges = new int[numberOfRanges];
			  for ( int rangeId = 0; rangeId < numberOfRanges; rangeId++ )
			  {
					ranges[rangeId] = _random.Next( 1 << RANGE_SIZE );
			  }
			  GapFreeAllEntriesLabelScanReader reader = NewGapFreeAllEntriesLabelScanReader( ranges );

			  // when
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();

			  // then
			  AssertRanges( iterator, ranges );
		 }

		 private static void AssertRanges( IEnumerator<NodeLabelRange> iterator, int[] expectedRanges )
		 {
			  for ( int expectedRangeId = 0; expectedRangeId < expectedRanges.Length; expectedRangeId++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					NodeLabelRange actualRange = iterator.next();
					assertEquals( expectedRangeId, actualRange.Id() );
					int expectedRange = expectedRanges[expectedRangeId];
					long baseNodeId = expectedRangeId * RANGE_SIZE;
					for ( int i = 0; i < RANGE_SIZE; i++ )
					{
						 long nodeId = baseNodeId + i;
						 long[] expectedLabelIds = ( expectedRange & ( 1 << i ) ) == 0 ? EMPTY_LONG_ARRAY : _labelIds;
						 assertArrayEquals( expectedLabelIds, actualRange.Labels( nodeId ) );
						 assertEquals( nodeId, actualRange.Nodes()[i] );
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

		 private static GapFreeAllEntriesLabelScanReader NewGapFreeAllEntriesLabelScanReader( params int[] ranges )
		 {
			  return new GapFreeAllEntriesLabelScanReader( ranges( RANGE_SIZE, ranges ), RANGE_SIZE * ranges.Length );
		 }

		 private static AllEntriesLabelScanReader Ranges( int rangeSize, params int[] ranges )
		 {
			  IList<NodeLabelRange> rangeList = new List<NodeLabelRange>();
			  for ( int rangeId = 0; rangeId < ranges.Length; rangeId++ )
			  {
					rangeList.Add( new NodeLabelRange( rangeId, LabelsPerNode( ranges[rangeId] ) ) );
			  }

			  return new AllEntriesLabelScanReaderAnonymousInnerClass( rangeSize, ranges, rangeList );
		 }

		 private class AllEntriesLabelScanReaderAnonymousInnerClass : AllEntriesLabelScanReader
		 {
			 private int _rangeSize;
			 private int[] _ranges;
			 private IList<NodeLabelRange> _rangeList;

			 public AllEntriesLabelScanReaderAnonymousInnerClass( int rangeSize, int[] ranges, IList<NodeLabelRange> rangeList )
			 {
				 this._rangeSize = rangeSize;
				 this._ranges = ranges;
				 this._rangeList = rangeList;
			 }

			 public void close()
			 { // Nothing to close
			 }

			 public IEnumerator<NodeLabelRange> iterator()
			 {
				  return _rangeList.GetEnumerator();
			 }

			 public long maxCount()
			 {
				  return _ranges.Length * _rangeSize;
			 }

			 public int rangeSize()
			 {
				  return RANGE_SIZE;
			 }
		 }

		 private static long[][] LabelsPerNode( int relativeNodeIds )
		 {
			  long[][] result = new long[RANGE_SIZE][];
			  for ( int i = 0; i < RANGE_SIZE; i++ )
			  {
					if ( ( relativeNodeIds & ( 1 << i ) ) != 0 )
					{
						 result[i] = _labelIds;
					}
			  }
			  return result;
		 }

		 private static int[] Array( params int[] relativeNodeIds )
		 {
			  return relativeNodeIds;
		 }
	}

}