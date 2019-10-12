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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.Test;

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.asArray;

	public class NativeLabelScanReaderTest
	{
		 private const int LABEL_ID = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldFindMultipleNodesInEachRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindMultipleNodesInEachRange()
		 {
			  // GIVEN
			  GBPTree<LabelScanKey, LabelScanValue> index = mock( typeof( GBPTree ) );
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = mock( typeof( RawCursor ) );
			  when( cursor.Next() ).thenReturn(true, true, true, false);
			  when( cursor.get() ).thenReturn(Hit(0, 0b1000_1000__1100_0010L), Hit(1, 0b0000_0010__0000_1000L), Hit(3, 0b0010_0000__1010_0001L), null);
			  when( index.Seek( any( typeof( LabelScanKey ) ), any( typeof( LabelScanKey ) ) ) ).thenReturn( cursor );
			  using ( NativeLabelScanReader reader = new NativeLabelScanReader( index ) )
			  {
					// WHEN
					LongIterator iterator = reader.NodesWithLabel( LABEL_ID );

					// THEN
					assertArrayEquals( new long[] { 1, 6, 7, 11, 15, 64 + 3, 64 + 9, 192 + 0, 192 + 5, 192 + 7, 192 + 13 }, asArray( iterator ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportMultipleOpenCursorsConcurrently() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportMultipleOpenCursorsConcurrently()
		 {
			  // GIVEN
			  GBPTree<LabelScanKey, LabelScanValue> index = mock( typeof( GBPTree ) );
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor1 = mock( typeof( RawCursor ) );
			  when( cursor1.Next() ).thenReturn(false);
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor2 = mock( typeof( RawCursor ) );
			  when( cursor2.Next() ).thenReturn(false);
			  when( index.Seek( any( typeof( LabelScanKey ) ), any( typeof( LabelScanKey ) ) ) ).thenReturn( cursor1, cursor2 );

			  // WHEN
			  using ( NativeLabelScanReader reader = new NativeLabelScanReader( index ) )
			  {
					// first check test invariants
					verify( cursor1, never() ).close();
					verify( cursor2, never() ).close();
					LongIterator first = reader.NodesWithLabel( LABEL_ID );
					LongIterator second = reader.NodesWithLabel( LABEL_ID );

					// getting the second iterator should not have closed the first one
					verify( cursor1, never() ).close();
					verify( cursor2, never() ).close();

					// exhausting the first one should have closed only the first one
					Exhaust( first );
					verify( cursor1, times( 1 ) ).close();
					verify( cursor2, never() ).close();

					// exhausting the second one should close it
					Exhaust( second );
					verify( cursor1, times( 1 ) ).close();
					verify( cursor2, times( 1 ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseUnexhaustedCursorsOnReaderClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseUnexhaustedCursorsOnReaderClose()
		 {
			  // GIVEN
			  GBPTree<LabelScanKey, LabelScanValue> index = mock( typeof( GBPTree ) );
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor1 = mock( typeof( RawCursor ) );
			  when( cursor1.Next() ).thenReturn(false);
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor2 = mock( typeof( RawCursor ) );
			  when( cursor2.Next() ).thenReturn(false);
			  when( index.Seek( any( typeof( LabelScanKey ) ), any( typeof( LabelScanKey ) ) ) ).thenReturn( cursor1, cursor2 );

			  // WHEN
			  using ( NativeLabelScanReader reader = new NativeLabelScanReader( index ) )
			  {
					// first check test invariants
					reader.NodesWithLabel( LABEL_ID );
					reader.NodesWithLabel( LABEL_ID );
					verify( cursor1, never() ).close();
					verify( cursor2, never() ).close();
			  }

			  // THEN
			  verify( cursor1, times( 1 ) ).close();
			  verify( cursor2, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromGivenId() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartFromGivenId()
		 {
			  // given
			  GBPTree<LabelScanKey, LabelScanValue> index = mock( typeof( GBPTree ) );
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = mock( typeof( RawCursor ) );
			  when( cursor.Next() ).thenReturn(true, true, false);
			  when( cursor.get() ).thenReturn(Hit(1, 0b0001_1000__0101_1110L), Hit(3, 0b0010_0000__1010_0001L), null);
			  when( index.Seek( any( typeof( LabelScanKey ) ), any( typeof( LabelScanKey ) ) ) ).thenReturn( cursor );

			  // when
			  long fromId = LabelScanValue.RangeSize + 3;
			  using ( NativeLabelScanReader reader = new NativeLabelScanReader( index ), PrimitiveLongResourceIterator iterator = reader.NodesWithAnyOfLabels( fromId, LABEL_ID ) )
			  {
					// then
					assertArrayEquals( new long[] { 64 + 4, 64 + 6, 64 + 11, 64 + 12, 192 + 0, 192 + 5, 192 + 7, 192 + 13 }, asArray( iterator ) );
			  }
		 }

		 private static Hit<LabelScanKey, LabelScanValue> Hit( long baseNodeId, long bits )
		 {
			  LabelScanKey key = new LabelScanKey( LABEL_ID, baseNodeId );
			  LabelScanValue value = new LabelScanValue();
			  value.Bits = bits;
			  return new MutableHit<LabelScanKey, LabelScanValue>( key, value );
		 }

		 private void Exhaust( LongIterator iterator )
		 {
			  while ( iterator.hasNext() )
			  {
					iterator.next();
			  }
		 }
	}

}