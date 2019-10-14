using System;

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
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTreeGenerationTarget_Fields.NO_GENERATION_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.BYTE_POS_KEYCOUNT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.BYTE_POS_LEFTSIBLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.BYTE_POS_RIGHTSIBLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.BYTE_POS_SUCCESSOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.goTo;

	/// <summary>
	/// Use together with <seealso cref="GBPTree.unsafe(GBPTreeUnsafe)"/>
	/// </summary>
	public sealed class GBPTreeCorruption
	{
		 private GBPTreeCorruption()
		 {
		 }

		 /* PageCorruption */
		 public static PageCorruption<KEY, VALUE> Crashed<KEY, VALUE>( GBPTreePointerType gbpTreePointerType )
		 {
			  return ( pageCursor, layout, node, treeState ) =>
			  {
				int offset = gbpTreePointerType.Offset( node );
				long stableGeneration = treeState.stableGeneration();
				long unstableGeneration = treeState.unstableGeneration();
				long crashGeneration = crashGeneration( treeState );
				pageCursor.Offset = offset;
				long pointer = pointer( GenerationSafePointerPair.Read( pageCursor, stableGeneration, unstableGeneration, NO_GENERATION_TARGET ) );
				OverwriteGSPP( pageCursor, offset, crashGeneration, pointer );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> Broken<KEY, VALUE>( GBPTreePointerType gbpTreePointerType )
		 {
			  return ( pageCursor, layout, node, treeState ) =>
			  {
				int offset = gbpTreePointerType.Offset( node );
				pageCursor.Offset = offset;
				pageCursor.putInt( int.MaxValue );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> SetPointer<KEY, VALUE>( GBPTreePointerType pointerType, long pointer )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				OverwriteGSPP( cursor, pointerType.Offset( node ), treeState.stableGeneration(), pointer );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> NotATreeNode<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) => cursor.putByte( TreeNode.BYTE_POS_NODE_TYPE, sbyte.MaxValue );
		 }

		 public static PageCorruption<KEY, VALUE> UnknownTreeNodeType<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) => cursor.putByte( TreeNode.BytePosType, sbyte.MaxValue );
		 }

		 public static PageCorruption<KEY, VALUE> RightSiblingPointToNonExisting<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) => overwriteGSPP( cursor, GBPTreePointerType.rightSibling().offset(node), treeState.stableGeneration(), GenerationSafePointer.MAX_POINTER );
		 }

		 public static PageCorruption<KEY, VALUE> LeftSiblingPointToNonExisting<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) => overwriteGSPP( cursor, GBPTreePointerType.leftSibling().offset(node), treeState.stableGeneration(), GenerationSafePointer.MAX_POINTER );
		 }

		 public static PageCorruption<KEY, VALUE> RightSiblingPointerHasTooLowGeneration<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				long rightSibling = pointer( TreeNode.RightSibling( cursor, treeState.stableGeneration(), treeState.unstableGeneration() ) );
				OverwriteGSPP( cursor, BYTE_POS_RIGHTSIBLING, GenerationSafePointer.MIN_GENERATION, rightSibling );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> LeftSiblingPointerHasTooLowGeneration<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				long leftSibling = pointer( TreeNode.LeftSibling( cursor, treeState.stableGeneration(), treeState.unstableGeneration() ) );
				OverwriteGSPP( cursor, BYTE_POS_LEFTSIBLING, GenerationSafePointer.MIN_GENERATION, leftSibling );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> ChildPointerHasTooLowGeneration<KEY, VALUE>( int childPos )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				long child = pointer( node.childAt( cursor, childPos, treeState.stableGeneration(), treeState.unstableGeneration() ) );
				OverwriteGSPP( cursor, node.childOffset( childPos ), GenerationSafePointer.MIN_GENERATION, child );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> SetChild<KEY, VALUE>( int childPos, long childPointer )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				GenerationKeeper childGeneration = new GenerationKeeper();
				node.childAt( cursor, childPos, treeState.stableGeneration(), treeState.unstableGeneration(), childGeneration );
				OverwriteGSPP( cursor, GBPTreePointerType.child( childPos ).offset( node ), childGeneration.Generation, childPointer );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> HasSuccessor<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) => overwriteGSPP( cursor, BYTE_POS_SUCCESSOR, treeState.unstableGeneration(), GenerationSafePointer.MAX_POINTER );
		 }

		 public static PageCorruption<KEY, VALUE> SwapKeyOrderLeaf<KEY, VALUE>( int firstKeyPos, int secondKeyPos, int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				// Remove key from higher position and insert into lower position
				int lowerKeyPos = firstKeyPos < secondKeyPos ? firstKeyPos : secondKeyPos;
				int higherKeyPos = firstKeyPos == lowerKeyPos ? secondKeyPos : firstKeyPos;

				// Record key and value on higher position
				KEY key = layout.newKey();
				VALUE value = layout.newValue();
				node.keyAt( cursor, key, higherKeyPos, TreeNode.Type.Leaf );
				node.valueAt( cursor, value, higherKeyPos );

				// Remove key and value, may need to defragment node to make sure we have room for insert later
				node.removeKeyValueAt( cursor, higherKeyPos, keyCount );
				node.defragmentLeaf( cursor );

				// Insert key and value in lower position
				node.insertKeyValueAt( cursor, key, value, lowerKeyPos, keyCount - 1 );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> SwapKeyOrderInternal<KEY, VALUE>( int firstKeyPos, int secondKeyPos, int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				// Remove key from higher position and insert into lower position
				int lowerKeyPos = firstKeyPos < secondKeyPos ? firstKeyPos : secondKeyPos;
				int higherKeyPos = firstKeyPos == lowerKeyPos ? secondKeyPos : firstKeyPos;

				// Record key and right child on higher position together with generation of child pointer
				KEY key = layout.newKey();
				node.keyAt( cursor, key, higherKeyPos, TreeNode.Type.Internal );
				GenerationKeeper childPointerGeneration = new GenerationKeeper();
				long rightChild = node.childAt( cursor, higherKeyPos + 1, treeState.stableGeneration(), treeState.unstableGeneration(), childPointerGeneration );

				// Remove key and right child, may need to defragment node to make sure we have room for insert later
				node.removeKeyAndRightChildAt( cursor, higherKeyPos, keyCount );
				node.defragmentLeaf( cursor );

				// Insert key and right child in lower position
				node.insertKeyAndRightChildAt( cursor, key, rightChild, lowerKeyPos, keyCount - 1, treeState.stableGeneration(), treeState.unstableGeneration() );

				// Overwrite the newly inserted child to reset the generation
				int childOffset = node.childOffset( lowerKeyPos + 1 );
				OverwriteGSPP( cursor, childOffset, childPointerGeneration.Generation, rightChild );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> SwapChildOrder<KEY, VALUE>( int firstChildPos, int secondChildPos, int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				// Read first and second child together with generation
				GenerationKeeper firstChildGeneration = new GenerationKeeper();
				long firstChild = node.childAt( cursor, firstChildPos, treeState.stableGeneration(), treeState.unstableGeneration(), firstChildGeneration );
				GenerationKeeper secondChildGeneration = new GenerationKeeper();
				long secondChild = node.childAt( cursor, secondChildPos, treeState.stableGeneration(), treeState.unstableGeneration(), secondChildGeneration );

				// Overwrite respective child with the other
				OverwriteGSPP( cursor, GBPTreePointerType.child( firstChildPos ).offset( node ), secondChildGeneration.Generation, secondChild );
				OverwriteGSPP( cursor, GBPTreePointerType.child( secondChildPos ).offset( node ), firstChildGeneration.Generation, firstChild );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> OverwriteKeyAtPosLeaf<KEY, VALUE>( KEY key, int keyPos, int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				// Record value so that we can reinsert it together with key later
				VALUE value = layout.newValue();
				node.valueAt( cursor, value, keyPos );

				// Remove key and value, may need to defragment node to make sure we have room for insert later
				node.removeKeyValueAt( cursor, keyPos, keyCount );
				TreeNode.SetKeyCount( cursor, keyCount - 1 );
				node.defragmentLeaf( cursor );

				// Insert new key and value
				node.insertKeyValueAt( cursor, key, value, keyPos, keyCount - 1 );
				TreeNode.SetKeyCount( cursor, keyCount );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> OverwriteKeyAtPosInternal<KEY, VALUE>( KEY key, int keyPos, int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				// Record rightChild so that we can reinsert it together with key later
				long rightChild = node.childAt( cursor, keyPos + 1, treeState.stableGeneration(), treeState.unstableGeneration() );

				// Remove key and right child, may need to defragment node to make sure we have room for insert later
				node.removeKeyAndRightChildAt( cursor, keyPos, keyCount );
				TreeNode.SetKeyCount( cursor, keyCount - 1 );
				node.defragmentInternal( cursor );

				// Insert key and right child
				node.insertKeyAndRightChildAt( cursor, key, rightChild, keyPos, keyCount - 1, treeState.stableGeneration(), treeState.unstableGeneration() );
				TreeNode.SetKeyCount( cursor, keyCount );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> MaximizeAllocOffsetInDynamicNode<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				TreeNodeDynamicSize dynamicNode = AssertDynamicNode( node );
				dynamicNode.setAllocOffset( cursor, cursor.CurrentPageSize ); // Clear alloc space
			  };
		 }

		 public static PageCorruption<KEY, VALUE> MinimizeAllocOffsetInDynamicNode<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				TreeNodeDynamicSize dynamicNode = AssertDynamicNode( node );
				dynamicNode.setAllocOffset( cursor, TreeNodeDynamicSize.HeaderLengthDynamic );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> DecrementAllocOffsetInDynamicNode<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				TreeNodeDynamicSize dynamicNode = AssertDynamicNode( node );
				int allocOffset = dynamicNode.getAllocOffset( cursor );
				dynamicNode.setAllocOffset( cursor, allocOffset - 1 );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> IncrementDeadSpaceInDynamicNode<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				TreeNodeDynamicSize dynamicNode = AssertDynamicNode( node );
				int deadSpace = dynamicNode.getDeadSpace( cursor );
				dynamicNode.setDeadSpace( cursor, deadSpace + 1 );
			  };
		 }

		 public static IndexCorruption<KEY, VALUE> DecrementFreelistWritePos<KEY, VALUE>()
		 {
			  return ( pagedFile, layout, node, treeState ) =>
			  {
				using ( PageCursor cursor = pagedFile.io( 0, PagedFile.PF_SHARED_WRITE_LOCK ) )
				{
					 goTo( cursor, "", treeState.pageId() );
					 int decrementedWritePos = treeState.freeListWritePos() - 1;
					 TreeState.Write( cursor, treeState.stableGeneration(), treeState.unstableGeneration(), treeState.rootId(), treeState.rootGeneration(), treeState.lastId(), treeState.freeListWritePageId(), treeState.freeListReadPageId(), decrementedWritePos, treeState.freeListReadPos(), treeState.Clean );
				}
			  };
		 }

		 public static IndexCorruption<KEY, VALUE> AddFreelistEntry<KEY, VALUE>( long releasedId )
		 {
			  return ( pagedFile, layout, node, treeState ) =>
			  {
				FreeListIdProvider freelist = GetFreelist( pagedFile, treeState );
				freelist.ReleaseId( treeState.stableGeneration(), treeState.unstableGeneration(), releasedId );
				using ( PageCursor cursor = pagedFile.io( 0, PagedFile.PF_SHARED_WRITE_LOCK ) )
				{
					 goTo( cursor, "", treeState.pageId() );
					 TreeState.Write( cursor, treeState.stableGeneration(), treeState.unstableGeneration(), treeState.rootId(), treeState.rootGeneration(), freelist.LastId(), freelist.WritePageId(), freelist.ReadPageId(), freelist.WritePos(), freelist.ReadPos(), treeState.Clean );
				}
			  };
		 }

		 public static IndexCorruption<KEY, VALUE> setTreeState<KEY, VALUE>( TreeState target )
		 {
			  return ( pagedFile, layout, node, treeState ) =>
			  {
				using ( PageCursor cursor = pagedFile.io( 0, PagedFile.PF_SHARED_WRITE_LOCK ) )
				{
					 goTo( cursor, "", treeState.pageId() ); // Write new tree state to current tree states page
					 TreeState.Write( cursor, target.StableGeneration(), target.UnstableGeneration(), target.RootId(), target.RootGeneration(), target.LastId(), target.FreeListWritePageId(), target.FreeListReadPageId(), target.FreeListWritePos(), target.FreeListReadPos(), target.Clean );
				}
			  };
		 }

		 public static PageCorruption<KEY, VALUE> setKeyCount<KEY, VALUE>( int keyCount )
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				cursor.putInt( BYTE_POS_KEYCOUNT, keyCount );
			  };
		 }

		 public static PageCorruption<KEY, VALUE> SetHighestReasonableKeyCount<KEY, VALUE>()
		 {
			  return ( cursor, layout, node, treeState ) =>
			  {
				int keyCount = 0;
				while ( node.reasonableKeyCount( keyCount + 1 ) )
				{
					 keyCount++;
				}
				cursor.putInt( BYTE_POS_KEYCOUNT, keyCount );
			  };
		 }

		 public static IndexCorruption<KEY, VALUE> PageSpecificCorruption<KEY, VALUE>( long targetPage, PageCorruption<KEY, VALUE> corruption )
		 {
			  return ( pagedFile, layout, node, treeState ) =>
			  {
				using ( PageCursor cursor = pagedFile.io( 0, PagedFile.PF_SHARED_WRITE_LOCK ) )
				{
					 goTo( cursor, "", targetPage );
					 corruption.Corrupt( cursor, layout, node, treeState );
				}
			  };
		 }

		 private static FreeListIdProvider GetFreelist( PagedFile pagedFile, TreeState treeState )
		 {
			  FreeListIdProvider freelist = new FreeListIdProvider( pagedFile, pagedFile.PageSize(), treeState.LastId(), FreeListIdProvider.NO_MONITOR );
			  freelist.Initialize( treeState.LastId(), treeState.FreeListWritePageId(), treeState.FreeListReadPageId(), treeState.FreeListWritePos(), freelist.ReadPos() );
			  return freelist;
		 }

		 private static TreeNodeDynamicSize AssertDynamicNode<KEY, VALUE>( TreeNode<KEY, VALUE> node )
		 {
			  if ( !( node is TreeNodeDynamicSize ) )
			  {
					throw new Exception( "Can not use this corruption if node is not of type " + typeof( TreeNodeDynamicSize ).Name );
			  }
			  return ( TreeNodeDynamicSize ) node;
		 }

		 private static void OverwriteGSPP( PageCursor cursor, int gsppOffset, long generation, long pointer )
		 {
			  cursor.Offset = gsppOffset;
			  GenerationSafePointer.Write( cursor, generation, pointer );
			  GenerationSafePointer.Clean( cursor );
		 }

		 private static long CrashGeneration( TreeState treeState )
		 {
			  if ( treeState.UnstableGeneration() - treeState.StableGeneration() < 2 )
			  {
					throw new System.InvalidOperationException( "Need stable and unstable generation to have a crash gap but was stableGeneration=" + treeState.StableGeneration() + " and unstableGeneration=" + treeState.UnstableGeneration() );
			  }
			  return treeState.UnstableGeneration() - 1;
		 }

		 internal interface PageCorruption<KEY, VALUE>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void corrupt(org.neo4j.io.pagecache.PageCursor pageCursor, Layout<KEY,VALUE> layout, TreeNode<KEY,VALUE> node, TreeState treeState) throws java.io.IOException;
			  void Corrupt( PageCursor pageCursor, Layout<KEY, VALUE> layout, TreeNode<KEY, VALUE> node, TreeState treeState );
		 }

		 internal interface IndexCorruption<KEY, VALUE> : GBPTreeUnsafe<KEY, VALUE>
		 {
		 }
	}

}