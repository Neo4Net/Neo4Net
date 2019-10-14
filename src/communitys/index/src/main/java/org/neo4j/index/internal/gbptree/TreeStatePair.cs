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
namespace Neo4Net.Index.@internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.checkOutOfBounds;

	/// <summary>
	/// Pair of <seealso cref="TreeState"/>, ability to make decision about which of the two to read and write respectively,
	/// depending on the <seealso cref="TreeState.isValid() validity"/> and <seealso cref="TreeState.stableGeneration()"/> of each.
	/// </summary>
	internal class TreeStatePair
	{

		 private TreeStatePair()
		 {
		 }

		 /// <summary>
		 /// Initialize state pages because new pages are expected to be allocated directly after
		 /// the existing highest allocated page. Otherwise there'd be a hole between meta and root pages
		 /// until they would have been written, which isn't guaranteed to be handled correctly by the page cache.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> assumed to be opened with write capabilities. </param>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void initializeStatePages(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 internal static void InitializeStatePages( PageCursor cursor )
		 {
			  PageCursorUtil.GoTo( cursor, "State page A", IdSpace.STATE_PAGE_A );
			  PageCursorUtil.GoTo( cursor, "State page B", IdSpace.STATE_PAGE_B );
		 }

		 /// <summary>
		 /// Reads the tree state pair, one from each of {@code pageIdA} and {@code pageIdB}, deciding their validity
		 /// and returning them as a <seealso cref="Pair"/>.
		 /// do-shouldRetry is managed inside this method because data is read from two pages.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to use when reading. This cursor will be moved to the two pages
		 /// one after the other, to read their states. </param>
		 /// <param name="pageIdA"> page id containing the first state. </param>
		 /// <param name="pageIdB"> page id containing the second state. </param>
		 /// <returns> <seealso cref="Pair"/> of both tree states. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> reading error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.apache.commons.lang3.tuple.Pair<TreeState,TreeState> readStatePages(org.neo4j.io.pagecache.PageCursor cursor, long pageIdA, long pageIdB) throws java.io.IOException
		 internal static Pair<TreeState, TreeState> ReadStatePages( PageCursor cursor, long pageIdA, long pageIdB )
		 {
			  TreeState stateA = ReadStatePage( cursor, pageIdA );
			  TreeState stateB = ReadStatePage( cursor, pageIdB );
			  return Pair.of( stateA, stateB );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static TreeState readStatePage(org.neo4j.io.pagecache.PageCursor cursor, long pageIdA) throws java.io.IOException
		 private static TreeState ReadStatePage( PageCursor cursor, long pageIdA )
		 {
			  PageCursorUtil.GoTo( cursor, "state page", pageIdA );
			  TreeState state;
			  do
			  {
					state = TreeState.Read( cursor );
			  } while ( cursor.ShouldRetry() );
			  checkOutOfBounds( cursor );
			  return state;
		 }

		 /// <param name="states"> the two states to compare. </param>
		 /// <returns> newest (w/ regards to <seealso cref="TreeState.stableGeneration()"/>) <seealso cref="TreeState.isValid() valid"/>
		 /// <seealso cref="TreeState"/> of the two. </returns>
		 /// <exception cref="IllegalStateException"> if none were valid. </exception>
		 internal static TreeState SelectNewestValidState( Pair<TreeState, TreeState> states )
		 {
			  return SelectNewestValidStateOptionally( states ).orElseThrow( () => new TreeInconsistencyException("Unexpected combination of state.%n  STATE_A[%s]%n  STATE_B[%s]", states.Left, states.Right) );
		 }

		 /// <param name="states"> the two states to compare. </param>
		 /// <returns> oldest (w/ regards to <seealso cref="TreeState.stableGeneration()"/>) <seealso cref="TreeState.isValid() invalid"/>
		 /// <seealso cref="TreeState"/> of the two. If both are invalid then the <seealso cref="Pair.getLeft() first one"/> is returned. </returns>
		 internal static TreeState SelectOldestOrInvalid( Pair<TreeState, TreeState> states )
		 {
			  TreeState newestValidState = SelectNewestValidStateOptionally( states ).orElse( states.Right );
			  return newestValidState == states.Left ? states.Right : states.Left;
		 }

		 private static Optional<TreeState> SelectNewestValidStateOptionally( Pair<TreeState, TreeState> states )
		 {
			  TreeState stateA = states.Left;
			  TreeState stateB = states.Right;

			  if ( stateA.Valid != stateB.Valid )
			  {
					// return only valid
					return stateA.Valid ? stateA : stateB;
			  }
			  else if ( stateA.Valid && stateB.Valid )
			  {
					// return newest

					// compare unstable generations of A/B, if equal, compare clean flag (clean is newer than dirty)
					// and include sanity check for stable generations such that there cannot be a state S compared
					// to other state O where
					// S.unstableGeneration > O.unstableGeneration AND S.stableGeneration < O.stableGeneration

					if ( stateA.StableGeneration() == stateB.StableGeneration() && stateA.UnstableGeneration() == stateB.UnstableGeneration() && stateA.Clean != stateB.Clean )
					{
						 return ( stateA.Clean ? stateA : stateB );
					}
					else if ( stateA.StableGeneration() >= stateB.StableGeneration() && stateA.UnstableGeneration() > stateB.UnstableGeneration() )
					{
						 return stateA;
					}
					else if ( stateA.StableGeneration() <= stateB.StableGeneration() && stateA.UnstableGeneration() < stateB.UnstableGeneration() )
					{
						 return stateB;
					}
			  }

			  // return null communicating that this combination didn't result in any valid "newest" state
			  return null;
		 }
	}

}