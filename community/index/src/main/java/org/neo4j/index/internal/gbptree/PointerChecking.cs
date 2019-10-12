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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// Methods for ensuring a read <seealso cref="GenerationSafePointer GSP pointer"/> is valid.
	/// </summary>
	internal class PointerChecking
	{
		 internal static readonly string WriterTraverseOldStateMessage = "Writer traversed to a tree node that has a valid successor, " +
							  "This is most likely due to failure to checkpoint the tree before shutdown and/or tree state " +
							  "being out of date.";

		 private PointerChecking()
		 {
		 }

		 /// <summary>
		 /// Checks a read pointer for success/failure and throws appropriate exception with failure information
		 /// if failure. Must be called after a consistent read from page cache (after <seealso cref="PageCursor.shouldRetry()"/>.
		 /// </summary>
		 /// <param name="result"> result from <seealso cref="GenerationSafePointerPair.FLAG_READ"/> or
		 /// <seealso cref="GenerationSafePointerPair.write(PageCursor, long, long, long)"/>. </param>
		 /// <param name="allowNoNode"> If <seealso cref="TreeNode.NO_NODE_FLAG"/> is allowed as pointer value. </param>
		 internal static void CheckPointer( long result, bool allowNoNode )
		 {
			  GenerationSafePointerPair.AssertSuccess( result );
			  if ( allowNoNode && !TreeNode.IsNode( result ) )
			  {
					return;
			  }
			  if ( result < IdSpace.MIN_TREE_NODE_ID )
			  {
					throw new TreeInconsistencyException( "Pointer to id " + result + " not allowed. Minimum node id allowed is " + IdSpace.MIN_TREE_NODE_ID );
			  }
		 }

		 /// <summary>
		 /// Assert cursor rest on a node that does not have a valid (not crashed) successor.
		 /// </summary>
		 /// <param name="cursor"> PageCursor resting on a tree node. </param>
		 /// <param name="stableGeneration"> Current stable generation of tree. </param>
		 /// <param name="unstableGeneration"> Current unstable generation of tree. </param>
		 internal static bool AssertNoSuccessor( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  long successor = TreeNode.Successor( cursor, stableGeneration, unstableGeneration );
			  if ( TreeNode.IsNode( successor ) )
			  {
					throw new TreeInconsistencyException( WriterTraverseOldStateMessage );
			  }
			  return true;
		 }
	}

}