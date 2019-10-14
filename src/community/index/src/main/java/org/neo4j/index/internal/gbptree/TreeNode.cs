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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTreeGenerationTarget_Fields.NO_GENERATION_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GenerationSafePointerPair.read;

	/// <summary>
	/// Methods to manipulate single tree node such as set and get header fields,
	/// insert and fetch keys, values and children.
	/// </summary>
	internal abstract class TreeNode<KEY, VALUE>
	{
		 internal enum Type
		 {
			  Leaf,
			  Internal
		 }

		 internal enum Overflow
		 {
			  Yes,
			  No,
			  NoNeedDefrag
		 }

		 // Shared between all node types: TreeNode and FreelistNode
		 internal const int BYTE_POS_NODE_TYPE = 0;
		 internal const sbyte NODE_TYPE_TREE_NODE = 1;
		 internal const sbyte NODE_TYPE_FREE_LIST_NODE = 2;

		 internal static readonly int SizePageReference = GenerationSafePointerPair.Size;
		 internal static readonly int BytePosType = BYTE_POS_NODE_TYPE + Byte.BYTES;
		 internal static readonly int BytePosGeneration = BytePosType + Byte.BYTES;
		 internal static readonly int BytePosKeycount = BytePosGeneration + Integer.BYTES;
		 internal static readonly int BytePosRightsibling = BytePosKeycount + Integer.BYTES;
		 internal static readonly int BytePosLeftsibling = BytePosRightsibling + SizePageReference;
		 internal static readonly int BytePosSuccessor = BytePosLeftsibling + SizePageReference;
		 internal static readonly int BaseHeaderLength = BytePosSuccessor + SizePageReference;

		 internal const sbyte LEAF_FLAG = 1;
		 internal const sbyte INTERNAL_FLAG = 0;
		 internal const long NO_NODE_FLAG = 0;

		 internal const int NO_KEY_VALUE_SIZE_CAP = -1;

		 internal readonly Layout<KEY, VALUE> Layout;
		 internal readonly int PageSize;

		 internal TreeNode( int pageSize, Layout<KEY, VALUE> layout )
		 {
			  this.PageSize = pageSize;
			  this.Layout = layout;
		 }

		 internal static sbyte NodeType( PageCursor cursor )
		 {
			  return cursor.GetByte( BYTE_POS_NODE_TYPE );
		 }

		 private static void WriteBaseHeader( PageCursor cursor, sbyte type, long stableGeneration, long unstableGeneration )
		 {
			  cursor.PutByte( BYTE_POS_NODE_TYPE, NODE_TYPE_TREE_NODE );
			  cursor.PutByte( BytePosType, type );
			  SetGeneration( cursor, unstableGeneration );
			  SetKeyCount( cursor, 0 );
			  SetRightSibling( cursor, NO_NODE_FLAG, stableGeneration, unstableGeneration );
			  SetLeftSibling( cursor, NO_NODE_FLAG, stableGeneration, unstableGeneration );
			  SetSuccessor( cursor, NO_NODE_FLAG, stableGeneration, unstableGeneration );
		 }

		 internal virtual void InitializeLeaf( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  WriteBaseHeader( cursor, LEAF_FLAG, stableGeneration, unstableGeneration );
			  WriteAdditionalHeader( cursor );
		 }

		 internal virtual void InitializeInternal( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  WriteBaseHeader( cursor, INTERNAL_FLAG, stableGeneration, unstableGeneration );
			  WriteAdditionalHeader( cursor );
		 }

		 /// <summary>
		 /// Write additional header. When called, cursor should be located directly after base header.
		 /// Meaning at <seealso cref="BASE_HEADER_LENGTH"/>.
		 /// </summary>
		 internal abstract void WriteAdditionalHeader( PageCursor cursor );

		 // HEADER METHODS

		 internal static sbyte TreeNodeType( PageCursor cursor )
		 {
			  return cursor.GetByte( BytePosType );
		 }

		 internal static bool IsLeaf( PageCursor cursor )
		 {
			  return TreeNodeType( cursor ) == LEAF_FLAG;
		 }

		 internal static bool IsInternal( PageCursor cursor )
		 {
			  return TreeNodeType( cursor ) == INTERNAL_FLAG;
		 }

		 internal static long Generation( PageCursor cursor )
		 {
			  return cursor.GetInt( BytePosGeneration ) & GenerationSafePointer.GENERATION_MASK;
		 }

		 internal static int KeyCount( PageCursor cursor )
		 {
			  return cursor.GetInt( BytePosKeycount );
		 }

		 internal static long RightSibling( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return RightSibling( cursor, stableGeneration, unstableGeneration, NO_GENERATION_TARGET );
		 }

		 internal static long RightSibling( PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreeGenerationTarget generationTarget )
		 {
			  cursor.Offset = BytePosRightsibling;
			  return read( cursor, stableGeneration, unstableGeneration, generationTarget );
		 }

		 internal static long LeftSibling( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return LeftSibling( cursor, stableGeneration, unstableGeneration, NO_GENERATION_TARGET );
		 }

		 internal static long LeftSibling( PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreeGenerationTarget generationTarget )
		 {
			  cursor.Offset = BytePosLeftsibling;
			  return read( cursor, stableGeneration, unstableGeneration, generationTarget );
		 }

		 internal static long Successor( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return Successor( cursor, stableGeneration, unstableGeneration, NO_GENERATION_TARGET );
		 }

		 internal static long Successor( PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreeGenerationTarget generationTarget )
		 {
			  cursor.Offset = BytePosSuccessor;
			  return read( cursor, stableGeneration, unstableGeneration, generationTarget );
		 }

		 internal static void SetGeneration( PageCursor cursor, long generation )
		 {
			  GenerationSafePointer.AssertGenerationOnWrite( generation );
			  cursor.PutInt( BytePosGeneration, ( int ) generation );
		 }

		 internal static void SetKeyCount( PageCursor cursor, int count )
		 {
			  if ( count < 0 )
			  {
					throw new System.ArgumentException( "Invalid key count, " + count + ". On tree node " + cursor.CurrentPageId + "." );
			  }
			  cursor.PutInt( BytePosKeycount, count );
		 }

		 internal static void SetRightSibling( PageCursor cursor, long rightSiblingId, long stableGeneration, long unstableGeneration )
		 {
			  cursor.Offset = BytePosRightsibling;
			  long result = GenerationSafePointerPair.Write( cursor, rightSiblingId, stableGeneration, unstableGeneration );
			  GenerationSafePointerPair.AssertSuccess( result );
		 }

		 internal static void SetLeftSibling( PageCursor cursor, long leftSiblingId, long stableGeneration, long unstableGeneration )
		 {
			  cursor.Offset = BytePosLeftsibling;
			  long result = GenerationSafePointerPair.Write( cursor, leftSiblingId, stableGeneration, unstableGeneration );
			  GenerationSafePointerPair.AssertSuccess( result );
		 }

		 internal static void SetSuccessor( PageCursor cursor, long successorId, long stableGeneration, long unstableGeneration )
		 {
			  cursor.Offset = BytePosSuccessor;
			  long result = GenerationSafePointerPair.Write( cursor, successorId, stableGeneration, unstableGeneration );
			  GenerationSafePointerPair.AssertSuccess( result );
		 }

		 // BODY METHODS

		 /// <summary>
		 /// Moves data from left to right to open up a gap where data can later be written without overwriting anything.
		 /// Key count is NOT updated!
		 /// </summary>
		 /// <param name="cursor"> Write cursor on relevant page </param>
		 /// <param name="pos"> Logical position where slots should be inserted, pos is based on baseOffset and slotSize. </param>
		 /// <param name="numberOfSlots"> How many slots to be inserted. </param>
		 /// <param name="totalSlotCount"> How many slots there are in total. (Usually keyCount for keys and values or keyCount+1 for children). </param>
		 /// <param name="baseOffset"> Offset to slot in logical position 0. </param>
		 /// <param name="slotSize"> Size of one single slot. </param>
		 internal static void InsertSlotsAt( PageCursor cursor, int pos, int numberOfSlots, int totalSlotCount, int baseOffset, int slotSize )
		 {
			  cursor.ShiftBytes( baseOffset + pos * slotSize, ( totalSlotCount - pos ) * slotSize, numberOfSlots * slotSize );
		 }

		 /// <summary>
		 /// Moves data from right to left to remove a slot where data that should be deleted currently sits.
		 /// Key count is NOT updated!
		 /// </summary>
		 /// <param name="cursor"> Write cursor on relevant page </param>
		 /// <param name="pos"> Logical position where slots should be inserted, pos is based on baseOffset and slotSize. </param>
		 /// <param name="totalSlotCount"> How many slots there are in total. (Usually keyCount for keys and values or keyCount+1 for children). </param>
		 /// <param name="baseOffset"> Offset to slot in logical position 0. </param>
		 /// <param name="slotSize"> Size of one single slot. </param>
		 internal static void RemoveSlotAt( PageCursor cursor, int pos, int totalSlotCount, int baseOffset, int slotSize )
		 {
			  cursor.ShiftBytes( baseOffset + ( pos + 1 ) * slotSize, ( totalSlotCount - ( pos + 1 ) ) * slotSize, -slotSize );
		 }

		 internal abstract KEY KeyAt( PageCursor cursor, KEY into, int pos, Type type );

		 internal abstract void KeyValueAt( PageCursor cursor, KEY intoKey, VALUE intoValue, int pos );

		 internal abstract void InsertKeyAndRightChildAt( PageCursor cursor, KEY key, long child, int pos, int keyCount, long stableGeneration, long unstableGeneration );

		 internal abstract void InsertKeyValueAt( PageCursor cursor, KEY key, VALUE value, int pos, int keyCount );

		 internal abstract void RemoveKeyValueAt( PageCursor cursor, int pos, int keyCount );

		 internal abstract void RemoveKeyAndRightChildAt( PageCursor cursor, int keyPos, int keyCount );

		 internal abstract void RemoveKeyAndLeftChildAt( PageCursor cursor, int keyPos, int keyCount );

		 /// <summary>
		 /// Overwrite key at position with given key. </summary>
		 /// <returns> True if key was overwritten, false otherwise. </returns>
		 internal abstract bool SetKeyAtInternal( PageCursor cursor, KEY key, int pos );

		 internal abstract VALUE ValueAt( PageCursor cursor, VALUE value, int pos );

		 /// <summary>
		 /// Overwrite value at position with given value. </summary>
		 /// <returns> True if value was overwritten, false otherwise. </returns>
		 internal abstract bool SetValueAt( PageCursor cursor, VALUE value, int pos );

		 internal virtual long ChildAt( PageCursor cursor, int pos, long stableGeneration, long unstableGeneration )
		 {
			  return ChildAt( cursor, pos, stableGeneration, unstableGeneration, NO_GENERATION_TARGET );
		 }

		 internal virtual long ChildAt( PageCursor cursor, int pos, long stableGeneration, long unstableGeneration, GBPTreeGenerationTarget generationTarget )
		 {
			  cursor.Offset = ChildOffset( pos );
			  return read( cursor, stableGeneration, unstableGeneration, generationTarget );
		 }

		 internal abstract void SetChildAt( PageCursor cursor, long child, int pos, long stableGeneration, long unstableGeneration );

		 internal static void WriteChild( PageCursor cursor, long child, long stableGeneration, long unstableGeneration )
		 {
			  long write = GenerationSafePointerPair.Write( cursor, child, stableGeneration, unstableGeneration );
			  GenerationSafePointerPair.AssertSuccess( write );
		 }

		 // HELPERS

		 internal abstract int KeyValueSizeCap();

		 /// <summary>
		 /// This method can throw and should not be used on read path.
		 /// Throws <seealso cref="System.ArgumentException"/> if key and value combined violate key-value size limit.
		 /// </summary>
		 internal abstract void ValidateKeyValueSize( KEY key, VALUE value );

		 internal abstract bool ReasonableKeyCount( int keyCount );

		 internal abstract bool ReasonableChildCount( int childCount );

		 internal abstract int ChildOffset( int pos );

		 internal static bool IsNode( long node )
		 {
			  return GenerationSafePointerPair.Pointer( node ) != NO_NODE_FLAG;
		 }

		 internal virtual IComparer<KEY> KeyComparator()
		 {
			  return Layout;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void goTo(org.neo4j.io.pagecache.PageCursor cursor, String messageOnError, long nodeId) throws java.io.IOException
		 internal static void GoTo( PageCursor cursor, string messageOnError, long nodeId )
		 {
			  PageCursorUtil.GoTo( cursor, messageOnError, GenerationSafePointerPair.Pointer( nodeId ) );
		 }

		 /* SPLIT, MERGE AND REBALANCE */

		 /// <summary>
		 /// Will internal overflow if inserting new key? </summary>
		 /// <returns> true if leaf will overflow, else false. </returns>
		 internal abstract Overflow InternalOverflow( PageCursor cursor, int currentKeyCount, KEY newKey );

		 /// <summary>
		 /// Will leaf overflow if inserting new key and value? </summary>
		 /// <returns> true if leaf will overflow, else false. </returns>
		 internal abstract Overflow LeafOverflow( PageCursor cursor, int currentKeyCount, KEY newKey, VALUE newValue );

		 /// <summary>
		 /// Clean page with leaf node from garbage to make room for further insert without having to split.
		 /// </summary>
		 internal abstract void DefragmentLeaf( PageCursor cursor );

		 /// <summary>
		 /// Clean page with internal node from garbage to make room for further insert without having to split.
		 /// </summary>
		 internal abstract void DefragmentInternal( PageCursor cursor );

		 internal abstract bool LeafUnderflow( PageCursor cursor, int keyCount );

		 /// <summary>
		 /// How do we best rebalance left and right leaf?
		 /// Can we move keys from underflowing left to right so that none of them underflow? </summary>
		 /// <returns> 0, do nothing. -1, merge. 1-inf, move this number of keys from left to right. </returns>
		 internal abstract int CanRebalanceLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount );

		 internal abstract bool CanMergeLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount );

		 /// <summary>
		 /// Calculate where split should be done and move entries between leaves participating in split.
		 /// 
		 /// Keys and values from left are divide between left and right and the new key and value is inserted where it belongs.
		 /// 
		 /// Key count is updated.
		 /// </summary>
		 internal abstract void DoSplitLeaf( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, VALUE newValue, KEY newSplitter, double ratioToKeepInLeftOnSplit );

		 /// <summary>
		 /// Performs the entry moving part of split in internal.
		 /// 
		 /// Keys and children from left is divided between left and right and the new key and child is inserted where it belongs.
		 /// 
		 /// Key count is updated.
		 /// </summary>
		 internal abstract void DoSplitInternal( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, long newRightChild, long stableGeneration, long unstableGeneration, KEY newSplitter, double ratioToKeepInLeftOnSplit );

		 /// <summary>
		 /// Move all rightmost keys and values in left leaf from given position to right leaf.
		 /// 
		 /// Right leaf will be defragmented.
		 /// 
		 /// Update keyCount in left and right.
		 /// </summary>
		 internal abstract void MoveKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount, int fromPosInLeftNode );

		 /// <summary>
		 /// Copy all keys and values in left leaf and insert to the left in right leaf.
		 /// 
		 /// Right leaf will be defragmented.
		 /// 
		 /// Update keyCount in right
		 /// </summary>
		 internal abstract void CopyKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount );

		 // Useful for debugging
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") abstract void printNode(org.neo4j.io.pagecache.PageCursor cursor, boolean includeValue, boolean includeAllocSpace, long stableGeneration, long unstableGeneration);
		 internal abstract void PrintNode( PageCursor cursor, bool includeValue, bool includeAllocSpace, long stableGeneration, long unstableGeneration );

		 /// <returns> <seealso cref="string"/> describing inconsistency of empty string "" if no inconsistencies. </returns>
		 internal abstract string CheckMetaConsistency( PageCursor cursor, int keyCount, Type type, GBPTreeConsistencyCheckVisitor<KEY> visitor );
	}

}