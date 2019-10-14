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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Methods for (binary-)searching keys in a tree node.
	/// </summary>
	internal class KeySearch
	{
		 private const int POSITION_MASK = 0x3FFFFFFF;
		 private const int HIT_FLAG = unchecked( ( int )0x80000000 );
		 private const int NO_HIT_FLAG = 0x00000000;
		 private const int HIT_MASK = HIT_FLAG | NO_HIT_FLAG;
		 private const int SUCCESS_FLAG = 0x00000000;
		 private const int NO_SUCCESS_FLAG = 0x40000000;
		 private const int SUCCESS_MASK = SUCCESS_FLAG | NO_SUCCESS_FLAG;

		 private KeySearch()
		 {
		 }

		 /// <summary>
		 /// Search for left most pos such that keyAtPos obeys key <= keyAtPos.
		 /// Return pos (not offset) of keyAtPos, or key count if no such key exist.
		 /// <para>
		 /// On insert, key should be inserted at pos.
		 /// On seek in internal, child at pos should be followed from internal node.
		 /// On seek in leaf, value at pos is correct if keyAtPos is equal to key.
		 /// </para>
		 /// <para>
		 /// Implemented as binary search.
		 /// </para>
		 /// <para>
		 /// Leaves cursor on same page as when called. No guarantees on offset.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page with node (internal or leaf does not matter) </param>
		 /// <param name="bTreeNode"> <seealso cref="TreeNode"/> that knows how to operate on KEY and VALUE </param>
		 /// <param name="type"> <seealso cref="TreeNode.Type"/> of this tree node being searched </param>
		 /// <param name="key"> KEY to search for </param>
		 /// <param name="readKey"> KEY to use as temporary storage during calculation. </param> </param>
		 /// <param name="keyCount"> number of keys in node when starting search    <returns> search result where least significant 31 bits are first position i for which
		 /// bTreeNode.keyComparator().compare( key, bTreeNode.keyAt( i ) <= 0, or keyCount if no such key exists.
		 /// highest bit (sign bit) says whether or not the exact key was found in the node, if so set to 1, otherwise 0.
		 /// To extract position from the returned search result, then use <seealso cref="positionOf(int)"/>.
		 /// To extract whether or not the exact key was found, then use <seealso cref="isHit(int)"/>. </returns>
		 internal static int Search<KEY, VALUE>( PageCursor cursor, TreeNode<KEY, VALUE> bTreeNode, TreeNode.Type type, KEY key, KEY readKey, int keyCount )
		 {
			  if ( keyCount == 0 )
			  {
					return SearchResult( 0, false );
			  }

			  int lower = 0;
			  int higher = keyCount - 1;
			  int pos;
			  bool hit = false;

			  // Compare key with lower and higher and sort out special cases
			  IComparer<KEY> comparator = bTreeNode.KeyComparator();
			  int comparison;

			  // key greater than greatest key in node
			  if ( comparator.Compare( key, bTreeNode.KeyAt( cursor, readKey, higher, type ) ) > 0 )
			  {
					pos = keyCount;
			  }
			  // key smaller than or equal to smallest key in node
			  else if ( ( comparison = comparator.Compare( key, bTreeNode.KeyAt( cursor, readKey, lower, type ) ) ) <= 0 )
			  {
					if ( comparison == 0 )
					{
						 hit = true;
					}
					pos = 0;
			  }
			  else
			  {
					// Start binary search
					// If key <= keyAtPos -> move higher to pos
					// If key > keyAtPos -> move lower to pos+1
					// Terminate when lower == higher
					while ( lower < higher )
					{
						 pos = ( lower + higher ) / 2;
						 comparison = comparator.Compare( key, bTreeNode.KeyAt( cursor, readKey, pos, type ) );
						 if ( comparison <= 0 )
						 {
							  higher = pos;
						 }
						 else
						 {
							  lower = pos + 1;
						 }
					}
					if ( lower != higher )
					{
						 return NO_SUCCESS_FLAG;
					}
					pos = lower;

					hit = comparator.Compare( key, bTreeNode.KeyAt( cursor, readKey, pos, type ) ) == 0;
			  }
			  return SearchResult( pos, hit );
		 }

		 private static int SearchResult( int pos, bool hit )
		 {
			  return ( pos & POSITION_MASK ) | ( hit ? HIT_FLAG : NO_HIT_FLAG );
		 }

		 /// <summary>
		 /// Extracts the position from a search result from <seealso cref="search(PageCursor, TreeNode, TreeNode.Type, object, object, int)"/>.
		 /// </summary>
		 /// <param name="searchResult"> search result from <seealso cref="search(PageCursor, TreeNode, TreeNode.Type, object, object, int)"/>. </param>
		 /// <returns> position of the search result. </returns>
		 internal static int PositionOf( int searchResult )
		 {
			  return searchResult & POSITION_MASK;
		 }

		 /// <summary>
		 /// Extracts whether or not the searched key was found from search result from
		 /// <seealso cref="search(PageCursor, TreeNode, TreeNode.Type, object, object, int)"/>.
		 /// </summary>
		 /// <param name="searchResult"> search result form <seealso cref="search(PageCursor, TreeNode, TreeNode.Type, object, object, int)"/>. </param>
		 /// <returns> whether or not the searched key was found. </returns>
		 internal static bool IsHit( int searchResult )
		 {
			  return ( searchResult & HIT_MASK ) == HIT_FLAG;
		 }

		 internal static bool IsSuccess( int searchResult )
		 {
			  return ( searchResult & SUCCESS_MASK ) == SUCCESS_FLAG;
		 }

		 internal static void AssertSuccess( int searchResult )
		 {
			  if ( !IsSuccess( searchResult ) )
			  {
					throw new TreeInconsistencyException( "Search terminated in unexpected way" );
			  }
		 }
	}

}