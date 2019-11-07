using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Index.Internal.gbptree;
	using AllEntriesLabelScanReader = Neo4Net.Kernel.Api.LabelScan.AllEntriesLabelScanReader;
	using NodeLabelRange = Neo4Net.Kernel.Api.LabelScan.NodeLabelRange;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.labelscan.LabelScanValue.RANGE_SIZE;

	public class NativeAllEntriesLabelScanReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNonOverlappingRanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNonOverlappingRanges()
		 {
			  int rangeSize = 4;
			  // new ranges at: 0, 4, 8, 12 ...
			  ShouldIterateCorrectlyOver( Labels( 0, rangeSize, 0, 1, 2, 3 ), Labels( 1, rangeSize, 4, 6 ), Labels( 2, rangeSize, 12 ), Labels( 3, rangeSize, 17, 18 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeOverlappingRanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeOverlappingRanges()
		 {
			  int rangeSize = 4;
			  // new ranges at: 0, 4, 8, 12 ...
			  ShouldIterateCorrectlyOver( Labels( 0, rangeSize, 0, 1, 3, 55 ), Labels( 3, rangeSize, 1, 2, 5, 6, 43 ), Labels( 5, rangeSize, 8, 9, 15, 42 ), Labels( 6, rangeSize, 4, 8, 12 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRangesFromRandomData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRangesFromRandomData()
		 {
			  IList<Labels> labels = RandomData();

			  ShouldIterateCorrectlyOver( labels.ToArray() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldIterateCorrectlyOver(Labels... data) throws Exception
		 private void ShouldIterateCorrectlyOver( params Labels[] data )
		 {
			  // GIVEN
			  using ( AllEntriesLabelScanReader reader = new NativeAllEntriesLabelScanReader( Store( data ), HighestLabelId( data ) ) )
			  {
					// WHEN/THEN
					AssertRanges( reader, data );
			  }
		 }

		 private IList<Labels> RandomData()
		 {
			  IList<Labels> labels = new List<Labels>();
			  int labelCount = Random.intBetween( 30, 100 );
			  int labelId = 0;
			  for ( int i = 0; i < labelCount; i++ )
			  {
					labelId += Random.intBetween( 1, 20 );
					int nodeCount = Random.intBetween( 20, 100 );
					long[] nodeIds = new long[nodeCount];
					long nodeId = 0;
					for ( int j = 0; j < nodeCount; j++ )
					{
						 nodeId += Random.intBetween( 1, 100 );
						 nodeIds[j] = nodeId;
					}
					labels.Add( labels( labelId, nodeIds ) );
			  }
			  return labels;
		 }

		 private static int HighestLabelId( Labels[] data )
		 {
			  int highest = 0;
			  foreach ( Labels labels in data )
			  {
					highest = Integer.max( highest, labels.LabelId );
			  }
			  return highest;
		 }

		 private static void AssertRanges( AllEntriesLabelScanReader reader, Labels[] data )
		 {
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();
			  long highestRangeId = highestRangeId( data );
			  for ( long rangeId = 0; rangeId <= highestRangeId; rangeId++ )
			  {
					SortedDictionary<long, IList<long>> expected = RangeOf( data, rangeId );
					if ( expected != null )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( "Was expecting range " + expected, iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 NodeLabelRange range = iterator.next();

						 assertEquals( rangeId, range.Id() );
						 foreach ( KeyValuePair<long, IList<long>> expectedEntry in expected.SetOfKeyValuePairs() )
						 {
							  long[] labels = range.Labels( expectedEntry.Key );
							  assertArrayEquals( asArray( expectedEntry.Value.GetEnumerator() ), labels );
						 }
					}
					// else there was nothing in this range
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

		 private static SortedDictionary<long, IList<long>> RangeOf( Labels[] data, long rangeId )
		 {
			  SortedDictionary<long, IList<long>> result = new SortedDictionary<long, IList<long>>();
			  foreach ( Labels label in data )
			  {
					foreach ( Pair<LabelScanKey, LabelScanValue> entry in label.Entries )
					{
						 if ( entry.First().IdRange == rangeId )
						 {
							  long baseNodeId = entry.First().IdRange * RANGE_SIZE;
							  long bits = entry.Other().Bits;
							  while ( bits != 0 )
							  {
									long nodeId = baseNodeId + Long.numberOfTrailingZeros( bits );
									result.computeIfAbsent( nodeId, id => new List<>() ).add((long) label.LabelId);
									bits &= bits - 1;
							  }
						 }
					}
			  }
			  return result.Count == 0 ? null : result;
		 }

		 private static long HighestRangeId( Labels[] data )
		 {
			  long highest = 0;
			  foreach ( Labels labels in data )
			  {
					Pair<LabelScanKey, LabelScanValue> highestEntry = labels.Entries[labels.Entries.Count - 1];
					highest = max( highest, highestEntry.First().IdRange );
			  }
			  return highest;
		 }

		 private static System.Func<int, IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> Store( params Labels[] labels )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<Labels> labelsMap = new org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap<>(labels.length);
			  MutableIntObjectMap<Labels> labelsMap = new IntObjectHashMap<Labels>( labels.Length );
			  foreach ( Labels item in labels )
			  {
					labelsMap.put( item.LabelId, item );
			  }

			  return labelId =>
			  {
				Labels item = labelsMap.get( labelId );
				return item != null ? item.Cursor() : EMPTY_CURSOR;
			  };
		 }

		 private static Labels Labels( int labelId, params long[] nodeIds )
		 {
			  IList<Pair<LabelScanKey, LabelScanValue>> entries = new List<Pair<LabelScanKey, LabelScanValue>>();
			  long currentRange = 0;
			  LabelScanValue value = new LabelScanValue();
			  foreach ( long nodeId in nodeIds )
			  {
					long range = nodeId / RANGE_SIZE;
					if ( range != currentRange )
					{
						 if ( value.Bits != 0 )
						 {
							  entries.Add( Pair.of( ( new LabelScanKey() ).Set(labelId, currentRange), value ) );
							  value = new LabelScanValue();
						 }
					}
					value.Set( toIntExact( nodeId % RANGE_SIZE ) );
					currentRange = range;
			  }

			  if ( value.Bits != 0 )
			  {
					entries.Add( Pair.of( ( new LabelScanKey() ).Set(labelId, currentRange), value ) );
			  }

			  return new Labels( labelId, entries );
		 }

		 private class Labels
		 {
			  internal readonly int LabelId;
			  internal readonly IList<Pair<LabelScanKey, LabelScanValue>> Entries;

			  internal Labels( int labelId, IList<Pair<LabelScanKey, LabelScanValue>> entries )
			  {
					this.LabelId = labelId;
					this.Entries = entries;
			  }

			  internal virtual IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> Cursor()
			  {
					return new RawCursorAnonymousInnerClass( this );
			  }

			  private class RawCursorAnonymousInnerClass : IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>
			  {
				  private readonly Labels _outerInstance;

				  public RawCursorAnonymousInnerClass( Labels outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  cursor = -1;
				  }

				  internal int cursor;

				  public Hit<LabelScanKey, LabelScanValue> get()
				  {
						Debug.Assert( cursor >= 0 );
						Pair<LabelScanKey, LabelScanValue> entry = _outerInstance.entries[cursor];
						return new MutableHit<LabelScanKey, LabelScanValue>( entry.First(), entry.Other() );
				  }

				  public bool next()
				  {
						if ( cursor + 1 >= _outerInstance.entries.Count )
						{
							 return false;
						}
						cursor++;
						return true;
				  }

				  public void close()
				  { // Nothing to close
				  }
			  }
		 }

		 private static readonly IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> EMPTY_CURSOR = new RawCursorAnonymousInnerClass();

		 private class RawCursorAnonymousInnerClass : IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>
		 {
			 public Hit<LabelScanKey, LabelScanValue> get()
			 {
				  throw new System.InvalidOperationException();
			 }

			 public bool next()
			 {
				  return false;
			 }

			 public void close()
			 { // Nothing to close
			 }
		 }
	}

}