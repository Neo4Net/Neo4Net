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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SimpleLongLayout = Org.Neo4j.Index.@internal.gbptree.SimpleLongLayout;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using RandomExtension = Org.Neo4j.Test.extension.RandomExtension;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class MergingBlockEntryReaderTest
	internal class MergingBlockEntryReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.neo4j.test.rule.RandomRule rnd;
		 protected internal RandomRule Rnd;

		 private static readonly SimpleLongLayout _layout = SimpleLongLayout.longLayout().build();
		 private static readonly IComparer<BlockEntry<MutableLong, MutableLong>> _blockEntryComparator = ( b1, b2 ) => _layout.Compare( b1.key(), b2.key() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeSingleReader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeSingleReader()
		 {
			  // given
			  MergingBlockEntryReader<MutableLong, MutableLong> merger = new MergingBlockEntryReader<MutableLong, MutableLong>( _layout );
			  IList<BlockEntry<MutableLong, MutableLong>> data = SomeBlockEntries( new HashSet<BlockEntry<MutableLong, MutableLong>>() );

			  // when
			  merger.AddSource( NewReader( data ) );

			  // then
			  IList<BlockEntry<MutableLong, MutableLong>> expected = SortAll( singleton( data ) );
			  VerifyMerged( expected, merger );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeSingleEmptyReader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeSingleEmptyReader()
		 {
			  // given
			  MergingBlockEntryReader<MutableLong, MutableLong> merger = new MergingBlockEntryReader<MutableLong, MutableLong>( _layout );
			  IList<BlockEntry<MutableLong, MutableLong>> data = emptyList();

			  // when
			  merger.AddSource( NewReader( data ) );

			  // then
			  assertFalse( merger.Next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeMultipleReaders() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeMultipleReaders()
		 {
			  // given
			  MergingBlockEntryReader<MutableLong, MutableLong> merger = new MergingBlockEntryReader<MutableLong, MutableLong>( _layout );
			  IList<IList<BlockEntry<MutableLong, MutableLong>>> datas = new List<IList<BlockEntry<MutableLong, MutableLong>>>();
			  ISet<MutableLong> uniqueKeys = new HashSet<MutableLong>();
			  int nbrOfReaders = Rnd.Next( 10 ) + 1;
			  for ( int i = 0; i < nbrOfReaders; i++ )
			  {
					// when
					IList<BlockEntry<MutableLong, MutableLong>> data = SomeBlockEntries( uniqueKeys );
					datas.Add( data );
					merger.AddSource( NewReader( data ) );
			  }

			  // then
			  IList<BlockEntry<MutableLong, MutableLong>> expected = SortAll( datas );
			  VerifyMerged( expected, merger );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseAllReaderEvenEmpty() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseAllReaderEvenEmpty()
		 {
			  // given
			  MergingBlockEntryReader<MutableLong, MutableLong> merger = new MergingBlockEntryReader<MutableLong, MutableLong>( _layout );
			  CloseTrackingBlockEntryCursor empty = NewReader( emptyList() );
			  CloseTrackingBlockEntryCursor nonEmpty = NewReader( SomeBlockEntries( new HashSet<MutableLong>() ) );
			  merger.AddSource( empty );
			  merger.AddSource( nonEmpty );

			  // when
			  merger.Dispose();

			  // then
			  assertTrue( empty.Closed );
			  assertTrue( nonEmpty.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseAllReaderEvenEmptyAndExhausted() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseAllReaderEvenEmptyAndExhausted()
		 {
			  // given
			  MergingBlockEntryReader<MutableLong, MutableLong> merger = new MergingBlockEntryReader<MutableLong, MutableLong>( _layout );
			  CloseTrackingBlockEntryCursor empty = NewReader( emptyList() );
			  CloseTrackingBlockEntryCursor nonEmpty = NewReader( SomeBlockEntries( new HashSet<MutableLong>() ) );
			  merger.AddSource( empty );
			  merger.AddSource( nonEmpty );

			  // when
			  while ( merger.Next() )
			  { // exhaust
			  }
			  merger.Dispose();

			  // then
			  assertTrue( empty.Closed );
			  assertTrue( nonEmpty.Closed );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyMerged(java.util.List<BlockEntry<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>> expected, MergingBlockEntryReader<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> merger) throws java.io.IOException
		 private static void VerifyMerged( IList<BlockEntry<MutableLong, MutableLong>> expected, MergingBlockEntryReader<MutableLong, MutableLong> merger )
		 {
			  foreach ( BlockEntry<MutableLong, MutableLong> expectedEntry in expected )
			  {
					assertTrue( merger.Next() );
					assertEquals( 0, _layout.Compare( expectedEntry.Key(), merger.Key() ) );
					assertEquals( expectedEntry.Value(), merger.Value() );
			  }
			  assertFalse( merger.Next() );
		 }

		 private static IList<BlockEntry<MutableLong, MutableLong>> SortAll( IEnumerable<IList<BlockEntry<MutableLong, MutableLong>>> data )
		 {
			  IList<BlockEntry<MutableLong, MutableLong>> result = new List<BlockEntry<MutableLong, MutableLong>>();
			  foreach ( IList<BlockEntry<MutableLong, MutableLong>> list in data )
			  {
					( ( IList<BlockEntry<MutableLong, MutableLong>> )result ).AddRange( list );
			  }
			  result.sort( _blockEntryComparator );
			  return result;
		 }

		 private static CloseTrackingBlockEntryCursor NewReader( IList<BlockEntry<MutableLong, MutableLong>> expected )
		 {
			  return new CloseTrackingBlockEntryCursor( expected );
		 }

		 private IList<BlockEntry<MutableLong, MutableLong>> SomeBlockEntries( ISet<MutableLong> uniqueKeys )
		 {
			  IList<BlockEntry<MutableLong, MutableLong>> entries = new List<BlockEntry<MutableLong, MutableLong>>();
			  int size = Rnd.Next( 10 );
			  for ( int i = 0; i < size; i++ )
			  {
					MutableLong key;
					do
					{
						 key = _layout.key( Rnd.nextLong( 10_000 ) );
					} while ( !uniqueKeys.Add( key ) );
					MutableLong value = _layout.value( Rnd.nextLong( 10_000 ) );
					entries.Add( new BlockEntry<>( key, value ) );
			  }
			  entries.sort( _blockEntryComparator );
			  return entries;
		 }

		 private class CloseTrackingBlockEntryCursor : ListBasedBlockEntryCursor<MutableLong, MutableLong>
		 {
			  internal bool Closed;

			  internal CloseTrackingBlockEntryCursor( IEnumerable<BlockEntry<MutableLong, MutableLong>> blockEntries ) : base( blockEntries )
			  {
			  }

			  public override void Close()
			  {
					base.Close();
					Closed = true;
			  }
		 }
	}

}