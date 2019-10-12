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
	using Pair = org.apache.commons.lang3.tuple.Pair;


	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// Utility class for printing a <seealso cref="GBPTree"/>, either whole or sub-tree.
	/// </summary>
	/// @param <KEY> type of keys in the tree. </param>
	/// @param <VALUE> type of values in the tree. </param>
	public class GBPTreeStructure<KEY, VALUE>
	{
		 private readonly TreeNode<KEY, VALUE> _node;
		 private readonly Layout<KEY, VALUE> _layout;
		 private readonly long _stableGeneration;
		 private readonly long _unstableGeneration;

		 internal GBPTreeStructure( TreeNode<KEY, VALUE> node, Layout<KEY, VALUE> layout, long stableGeneration, long unstableGeneration )
		 {
			  this._node = node;
			  this._layout = layout;
			  this._stableGeneration = stableGeneration;
			  this._unstableGeneration = unstableGeneration;
		 }

		 /// <summary>
		 /// Visit the header, that is tree state and meta information, about the tree present in the given {@code file}.
		 /// </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> able to map tree contained in {@code file}. </param>
		 /// <param name="file"> <seealso cref="File"/> containing the tree to print header for. </param>
		 /// <param name="visitor"> <seealso cref="GBPTreeVisitor"/> that shall visit header. </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void visitHeader(org.neo4j.io.pagecache.PageCache pageCache, java.io.File file, GBPTreeVisitor visitor) throws java.io.IOException
		 public static void VisitHeader( PageCache pageCache, File file, GBPTreeVisitor visitor )
		 {
			  using ( PagedFile pagedFile = pageCache.Map( file, pageCache.PageSize(), StandardOpenOption.READ ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( IdSpace.STATE_PAGE_A, Org.Neo4j.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
					{
						 VisitMeta( cursor, visitor );
						 VisitTreeState( cursor, visitor );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void visitMeta(org.neo4j.io.pagecache.PageCursor cursor, GBPTreeVisitor visitor) throws java.io.IOException
		 private static void VisitMeta( PageCursor cursor, GBPTreeVisitor visitor )
		 {
			  PageCursorUtil.GoTo( cursor, "meta page", IdSpace.META_PAGE_ID );
			  Meta meta = Meta.Read( cursor, null );
			  visitor.meta( meta );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void visitTreeState(org.neo4j.io.pagecache.PageCursor cursor, GBPTreeVisitor visitor) throws java.io.IOException
		 internal static void VisitTreeState( PageCursor cursor, GBPTreeVisitor visitor )
		 {
			  Pair<TreeState, TreeState> statePair = TreeStatePair.ReadStatePages( cursor, IdSpace.STATE_PAGE_A, IdSpace.STATE_PAGE_B );
			  visitor.treeState( statePair );
		 }

		 /// <summary>
		 /// Let the passed in {@code cursor} point to the root or sub-tree (internal node) of what to visit.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> placed at root of tree or sub-tree. </param>
		 /// <param name="writeCursor"> Currently active <seealso cref="PageCursor write cursor"/> in tree. </param>
		 /// <param name="visitor"> <seealso cref="GBPTreeVisitor"/> that should visit the tree. </param>
		 /// <exception cref="IOException"> on page cache access error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitTree(org.neo4j.io.pagecache.PageCursor cursor, org.neo4j.io.pagecache.PageCursor writeCursor, GBPTreeVisitor<KEY,VALUE> visitor) throws java.io.IOException
		 internal virtual void VisitTree( PageCursor cursor, PageCursor writeCursor, GBPTreeVisitor<KEY, VALUE> visitor )
		 {
			  // TreeState
			  long currentPage = cursor.CurrentPageId;
			  Pair<TreeState, TreeState> statePair = TreeStatePair.ReadStatePages( cursor, IdSpace.STATE_PAGE_A, IdSpace.STATE_PAGE_B );
			  visitor.TreeState( statePair );
			  TreeNode.GoTo( cursor, "back to tree node from reading state", currentPage );

			  AssertOnTreeNode( Select( cursor, writeCursor ) );

			  // Traverse the tree
			  int level = 0;
			  do
			  {
					// One level at the time
					visitor.BeginLevel( level );
					long leftmostSibling = cursor.CurrentPageId;

					// Go right through all siblings
					VisitLevel( cursor, writeCursor, visitor );

					visitor.EndLevel( level );
					level++;

					// Then go back to the left-most node on this level
					TreeNode.GoTo( cursor, "back", leftmostSibling );
			  } while ( GoToLeftmostChild( cursor, writeCursor ) );
			  // And continue down to next level if this level was an internal level
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertOnTreeNode(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private static void AssertOnTreeNode( PageCursor cursor )
		 {
			  sbyte nodeType;
			  bool isInternal;
			  bool isLeaf;
			  do
			  {
					nodeType = TreeNode.NodeType( cursor );
					isInternal = TreeNode.IsInternal( cursor );
					isLeaf = TreeNode.IsLeaf( cursor );
			  } while ( cursor.ShouldRetry() );

			  if ( nodeType != TreeNode.NODE_TYPE_TREE_NODE )
			  {
					throw new System.ArgumentException( "Cursor is not pinned to a tree node page. pageId:" + cursor.CurrentPageId );
			  }
			  if ( !isInternal && !isLeaf )
			  {
					throw new System.ArgumentException( "Cursor is not pinned to a page containing a tree node. pageId:" + cursor.CurrentPageId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitTreeNode(org.neo4j.io.pagecache.PageCursor cursor, GBPTreeVisitor<KEY,VALUE> visitor) throws java.io.IOException
		 internal virtual void VisitTreeNode( PageCursor cursor, GBPTreeVisitor<KEY, VALUE> visitor )
		 {
			  //[TYPE][GEN][KEYCOUNT] ([RIGHTSIBLING][LEFTSIBLING][SUCCESSOR]))
			  bool isLeaf;
			  int keyCount;
			  long generation = -1;
			  do
			  {
					isLeaf = TreeNode.IsLeaf( cursor );
					keyCount = TreeNode.KeyCount( cursor );
					if ( !_node.reasonableKeyCount( keyCount ) )
					{
						 cursor.CursorException = "Unexpected keyCount " + keyCount;
					}
					generation = TreeNode.Generation( cursor );
			  } while ( cursor.ShouldRetry() );
			  visitor.BeginNode( cursor.CurrentPageId, isLeaf, generation, keyCount );

			  KEY key = _layout.newKey();
			  VALUE value = _layout.newValue();
			  for ( int i = 0; i < keyCount; i++ )
			  {
					long child = -1;
					do
					{
						 _node.keyAt( cursor, key, i, isLeaf ? LEAF : INTERNAL );
						 if ( isLeaf )
						 {
							  _node.valueAt( cursor, value, i );
						 }
						 else
						 {
							  child = pointer( _node.childAt( cursor, i, _stableGeneration, _unstableGeneration ) );
						 }
					} while ( cursor.ShouldRetry() );

					visitor.Position( i );
					if ( isLeaf )
					{
						 visitor.Key( key, isLeaf );
						 visitor.Value( value );
					}
					else
					{
						 visitor.Child( child );
						 visitor.Key( key, isLeaf );
					}
			  }
			  if ( !isLeaf )
			  {
					long child;
					do
					{
						 child = pointer( _node.childAt( cursor, keyCount, _stableGeneration, _unstableGeneration ) );
					} while ( cursor.ShouldRetry() );
					visitor.Position( keyCount );
					visitor.Child( child );
			  }
			  visitor.EndNode( cursor.CurrentPageId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean goToLeftmostChild(org.neo4j.io.pagecache.PageCursor readCursor, org.neo4j.io.pagecache.PageCursor writeCursor) throws java.io.IOException
		 private bool GoToLeftmostChild( PageCursor readCursor, PageCursor writeCursor )
		 {
			  bool isInternal;
			  long leftmostSibling = -1;
			  PageCursor cursor = Select( readCursor, writeCursor );
			  do
			  {
					isInternal = TreeNode.IsInternal( cursor );
					if ( isInternal )
					{
						 leftmostSibling = _node.childAt( cursor, 0, _stableGeneration, _unstableGeneration );
					}
			  } while ( cursor.ShouldRetry() );

			  if ( isInternal )
			  {
					TreeNode.GoTo( readCursor, "child", leftmostSibling );
			  }
			  return isInternal;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void visitLevel(org.neo4j.io.pagecache.PageCursor readCursor, org.neo4j.io.pagecache.PageCursor writeCursor, GBPTreeVisitor<KEY,VALUE> visitor) throws java.io.IOException
		 private void VisitLevel( PageCursor readCursor, PageCursor writeCursor, GBPTreeVisitor<KEY, VALUE> visitor )
		 {
			  long rightSibling = -1;
			  do
			  {
					PageCursor cursor = Select( readCursor, writeCursor );
					VisitTreeNode( cursor, visitor );

					do
					{
						 rightSibling = TreeNode.RightSibling( cursor, _stableGeneration, _unstableGeneration );
					} while ( cursor.ShouldRetry() );

					if ( TreeNode.IsNode( rightSibling ) )
					{
						 TreeNode.GoTo( readCursor, "right sibling", rightSibling );
					}
			  } while ( TreeNode.IsNode( rightSibling ) );
		 }

		 private static PageCursor Select( PageCursor readCursor, PageCursor writeCursor )
		 {
			  return writeCursor == null ? readCursor : readCursor.CurrentPageId == writeCursor.CurrentPageId ? writeCursor : readCursor;
		 }

	}

}