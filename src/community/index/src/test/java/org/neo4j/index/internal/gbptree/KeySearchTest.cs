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
namespace Neo4Net.Index.Internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GBPTreeTestUtil.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.KeySearch.search;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.SimpleLongLayout.longLayout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Overflow.NO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.LEAF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.ByteArrayPageCursor.wrap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class KeySearchTest
	internal class KeySearchTest
	{
		private bool InstanceFieldsInitialized = false;

		public KeySearchTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_node = new TreeNodeFixedSize<MutableLong, MutableLong>( PAGE_SIZE, _layout );
			_readKey = _layout.newKey();
			_searchKey = _layout.newKey();
			_insertKey = _layout.newKey();
			_dummyValue = _layout.newValue();
		}

		 private const int STABLE_GENERATION = 1;
		 private const int UNSTABLE_GENERATION = 2;

		 private const int KEY_COUNT = 10;
		 private const int PAGE_SIZE = 512;
		 private readonly PageCursor _cursor = wrap( new sbyte[PAGE_SIZE], 0, PAGE_SIZE );
		 private readonly Layout<MutableLong, MutableLong> _layout = longLayout().build();
		 private TreeNode<MutableLong, MutableLong> _node;
		 private MutableLong _readKey;
		 private MutableLong _searchKey;
		 private MutableLong _insertKey;
		 private MutableLong _dummyValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchEmptyLeaf()
		 internal virtual void SearchEmptyLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  int keyCount = TreeNode.KeyCount( _cursor );

			  // then
			  int result = search( _cursor, _node, LEAF, _searchKey, _readKey, keyCount );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchEmptyInternal()
		 internal virtual void SearchEmptyInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  int keyCount = TreeNode.KeyCount( _cursor );

			  // then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int result = search(cursor, node, INTERNAL, searchKey, readKey, keyCount);
			  int result = search( _cursor, _node, INTERNAL, _searchKey, _readKey, keyCount );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitLessThanWithOneKeyInLeaf()
		 internal virtual void SearchNoHitLessThanWithOneKeyInLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( 1L );

			  // then
			  int result = SearchKey( 0L );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitLessThanWithOneKeyInInternal()
		 internal virtual void SearchNoHitLessThanWithOneKeyInInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( 1L );

			  // then
			  int result = SearchKey( 0L );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitWithOneKeyInLeaf()
		 internal virtual void SearchHitWithOneKeyInLeaf()
		 {
			  // given
			  long key = 1L;
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( key );

			  // then
			  int result = SearchKey( key );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitWithOneKeyInInternal()
		 internal virtual void SearchHitWithOneKeyInInternal()
		 {
			  // given
			  long key = 1L;
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( key );

			  // then
			  int result = SearchKey( key );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitGreaterThanWithOneKeyInLeaf()
		 internal virtual void SearchNoHitGreaterThanWithOneKeyInLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( 1L );

			  // then
			  int result = SearchKey( 2L );
			  AssertSearchResult( false, 1, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitGreaterThanWithOneKeyInInternal()
		 internal virtual void SearchNoHitGreaterThanWithOneKeyInInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  AppendKey( 1L );

			  // then
			  int result = SearchKey( 2L );
			  AssertSearchResult( false, 1, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitGreaterThanWithFullLeaf()
		 internal virtual void SearchNoHitGreaterThanWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( KEY_COUNT );
			  AssertSearchResult( false, KEY_COUNT, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitGreaterThanWithFullInternal()
		 internal virtual void SearchNoHitGreaterThanWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( KEY_COUNT );
			  AssertSearchResult( false, KEY_COUNT, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnLastWithFullLeaf()
		 internal virtual void SearchHitOnLastWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( KEY_COUNT - 1 );
			  AssertSearchResult( true, KEY_COUNT - 1, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnLastWithFullInternal()
		 internal virtual void SearchHitOnLastWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( KEY_COUNT - 1 );
			  AssertSearchResult( true, KEY_COUNT - 1, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnFirstWithFullLeaf()
		 internal virtual void SearchHitOnFirstWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( 0 );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnFirstWithFullInternal()
		 internal virtual void SearchHitOnFirstWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int result = SearchKey( 0 );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitLessThanWithFullLeaf()
		 internal virtual void SearchNoHitLessThanWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i + 1 );
			  }

			  // then
			  int result = SearchKey( 0 );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitLessThanWithFullInternal()
		 internal virtual void SearchNoHitLessThanWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i + 1 );
			  }

			  // then
			  int result = SearchKey( 0 );
			  AssertSearchResult( false, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnMiddleWithFullLeaf()
		 internal virtual void SearchHitOnMiddleWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int middle = KEY_COUNT / 2;
			  int result = SearchKey( middle );
			  AssertSearchResult( true, middle, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnMiddleWithFullInternal()
		 internal virtual void SearchHitOnMiddleWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i );
			  }

			  // then
			  int middle = KEY_COUNT / 2;
			  int result = SearchKey( middle );
			  AssertSearchResult( true, middle, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitInMiddleWithFullLeaf()
		 internal virtual void SearchNoHitInMiddleWithFullLeaf()
		 {
			  // given
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i * 2 );
			  }

			  // then
			  int middle = KEY_COUNT / 2;
			  int result = SearchKey( ( middle * 2 ) - 1 );
			  AssertSearchResult( false, middle, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchNoHitInMiddleWithFullInternal()
		 internal virtual void SearchNoHitInMiddleWithFullInternal()
		 {
			  // given
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					AppendKey( i * 2 );
			  }

			  // then
			  int middle = KEY_COUNT / 2;
			  int result = SearchKey( ( middle * 2 ) - 1 );
			  AssertSearchResult( false, middle, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnFirstNonUniqueKeysLeaf()
		 internal virtual void SearchHitOnFirstNonUniqueKeysLeaf()
		 {
			  // given
			  long first = 1L;
			  long second = 2L;
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					long key = i < KEY_COUNT / 2 ? first : second;
					AppendKey( key );
			  }

			  // then
			  int result = SearchKey( first );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnFirstNonUniqueKeysInternal()
		 internal virtual void SearchHitOnFirstNonUniqueKeysInternal()
		 {
			  // given
			  long first = 1L;
			  long second = 2L;
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					long key = i < KEY_COUNT / 2 ? first : second;
					AppendKey( key );
			  }

			  // then
			  int result = SearchKey( first );
			  AssertSearchResult( true, 0, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnMiddleNonUniqueKeysLeaf()
		 internal virtual void SearchHitOnMiddleNonUniqueKeysLeaf()
		 {
			  // given
			  long first = 1L;
			  long second = 2L;
			  int middle = KEY_COUNT / 2;
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					long key = i < middle ? first : second;
					AppendKey( key );
			  }

			  // then
			  int result = SearchKey( second );
			  AssertSearchResult( true, middle, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void searchHitOnMiddleNonUniqueKeysInternal()
		 internal virtual void SearchHitOnMiddleNonUniqueKeysInternal()
		 {
			  // given
			  long first = 1L;
			  long second = 2L;
			  int middle = KEY_COUNT / 2;
			  _node.initializeInternal( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					long key = i < middle ? first : second;
					AppendKey( key );
			  }

			  // then
			  int result = SearchKey( second );
			  AssertSearchResult( true, middle, result );
		 }

		 /* Below are more thorough tests that look at all keys in node */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindExistingKey()
		 internal virtual void ShouldFindExistingKey()
		 {
			  // GIVEN
			  FullLeafWithUniqueKeys();

			  // WHEN
			  MutableLong key = _layout.newKey();
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					key.Value = key( i );
					int result = search( _cursor, _node, LEAF, key, _readKey, KEY_COUNT );

					// THEN
					AssertSearchResult( true, i, result );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectIndexesForKeysInBetweenExisting()
		 internal virtual void ShouldReturnCorrectIndexesForKeysInBetweenExisting()
		 {
			  // GIVEN
			  FullLeafWithUniqueKeys();

			  // WHEN
			  MutableLong key = _layout.newKey();
			  for ( int i = 1; i < KEY_COUNT - 1; i++ )
			  {
					key.Value = key( i ) - 1;
					int result = search( _cursor, _node, LEAF, key, _readKey, KEY_COUNT );

					// THEN
					AssertSearchResult( false, i, result );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSearchAndFindOnRandomData()
		 internal virtual void ShouldSearchAndFindOnRandomData()
		 {
			  // GIVEN a leaf node with random, although sorted (as of course it must be to binary-search), data
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  IList<MutableLong> keys = new List<MutableLong>();
			  int currentKey = _random.Next( 10_000 );
			  MutableLong key = _layout.newKey();

			  int keyCount = 0;
			  while ( true )
			  {
					MutableLong expectedKey = _layout.newKey();
					key.Value = currentKey;
					if ( _node.leafOverflow( _cursor, keyCount, key, _dummyValue ) != NO )
					{
						 break;
					}
					_layout.copyKey( key, expectedKey );
					keys.Insert( keyCount, expectedKey );
					_node.insertKeyValueAt( _cursor, key, _dummyValue, keyCount, keyCount );
					currentKey += _random.Next( 100 ) + 10;
					keyCount++;
			  }
			  TreeNode.SetKeyCount( _cursor, keyCount );

			  // WHEN searching for random keys within that general range
			  MutableLong searchKey = _layout.newKey();
			  for ( int i = 0; i < 1_000; i++ )
			  {
					searchKey.Value = _random.Next( currentKey + 10 );
					int searchResult = search( _cursor, _node, LEAF, searchKey, _readKey, keyCount );

					// THEN position should be as expected
					bool exists = contains( keys, searchKey, _layout );
					int position = KeySearch.PositionOf( searchResult );
					assertEquals( exists, KeySearch.IsHit( searchResult ) );
					if ( _layout.Compare( searchKey, keys[0] ) <= 0 )
					{ // Our search key was lower than any of our keys, expect 0
						 assertEquals( 0, position );
					}
					else
					{ // step backwards through our expected keys and see where it should fit, assert that fact
						 bool found = false;
						 for ( int j = keyCount - 1; j >= 0; j-- )
						 {
							  if ( _layout.Compare( searchKey, keys[j] ) > 0 )
							  {
									assertEquals( j + 1, position );
									found = true;
									break;
							  }
						 }

						 assertTrue( found );
					}
			  }
		 }

		 /* Helper */

		 private int SearchKey( long key )
		 {
			  int keyCount = TreeNode.KeyCount( _cursor );
			  TreeNode.Type type = TreeNode.IsInternal( _cursor ) ? INTERNAL : LEAF;
			  _searchKey.Value = key;
			  return search( _cursor, _node, type, _searchKey, _readKey, keyCount );
		 }

		 private void AppendKey( long key )
		 {
			  _insertKey.Value = key;
			  int keyCount = TreeNode.KeyCount( _cursor );
			  if ( TreeNode.IsInternal( _cursor ) )
			  {
					long dummyChild = 10;
					_node.insertKeyAndRightChildAt( _cursor, _insertKey, dummyChild, keyCount, keyCount, STABLE_GENERATION, UNSTABLE_GENERATION );
			  }
			  else
			  {
					_node.insertKeyValueAt( _cursor, _insertKey, _dummyValue, keyCount, keyCount );
			  }
			  TreeNode.SetKeyCount( _cursor, keyCount + 1 );
		 }

		 private void AssertSearchResult( bool hit, int position, int searchResult )
		 {
			  assertEquals( hit, KeySearch.IsHit( searchResult ) );
			  assertEquals( position, KeySearch.PositionOf( searchResult ) );
		 }

		 private void FullLeafWithUniqueKeys()
		 {
			  // [2,4,8,16,32,64,128,512,1024,2048]
			  _node.initializeLeaf( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  MutableLong key = _layout.newKey();
			  for ( int i = 0; i < KEY_COUNT; i++ )
			  {
					key.Value = key( i );
					_node.insertKeyValueAt( _cursor, key, _dummyValue, i, i );
			  }
			  TreeNode.SetKeyCount( _cursor, KEY_COUNT );
		 }

		 private int Key( int i )
		 {
			  return 2 << i;
		 }
	}

}