using System;

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
namespace Neo4Net.Index.@internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Layout_Fields.FIXED_SIZE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Layout_Fields.FIXED_SIZE_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// <para>
	/// DESIGN
	/// </para>
	/// <para>
	/// Using Separate design the internal nodes should look like
	/// <pre>
	/// # = empty space
	/// 
	/// [                                   HEADER   82B                           ]|[   KEYS   ]|[     CHILDREN      ]
	/// [NODETYPE][TYPE][GENERATION][KEYCOUNT][RIGHTSIBLING][LEFTSIBLING][SUCCESSOR]|[[KEY]...##]|[[CHILD][CHILD]...##]
	///  0         1     2           6         10            34           58          82
	/// </pre>
	/// Calc offset for key i (starting from 0)
	/// HEADER_LENGTH + i * SIZE_KEY
	/// </para>
	/// <para>
	/// Calc offset for child i
	/// HEADER_LENGTH + SIZE_KEY * MAX_KEY_COUNT_INTERNAL + i * SIZE_CHILD
	/// </para>
	/// <para>
	/// Using Separate design the leaf nodes should look like
	/// 
	/// <pre>
	/// [                                   HEADER   82B                           ]|[    KEYS  ]|[   VALUES   ]
	/// [NODETYPE][TYPE][GENERATION][KEYCOUNT][RIGHTSIBLING][LEFTSIBLING][SUCCESSOR]|[[KEY]...##]|[[VALUE]...##]
	///  0         1     2           6         10            34           58          82
	/// </pre>
	/// 
	/// Calc offset for key i (starting from 0)
	/// HEADER_LENGTH + i * SIZE_KEY
	/// </para>
	/// <para>
	/// Calc offset for value i
	/// HEADER_LENGTH + SIZE_KEY * MAX_KEY_COUNT_LEAF + i * SIZE_VALUE
	/// 
	/// </para>
	/// </summary>
	/// @param <KEY> type of key </param>
	/// @param <VALUE> type of value </param>
	internal class TreeNodeFixedSize<KEY, VALUE> : TreeNode<KEY, VALUE>
	{
		 internal const sbyte FORMAT_IDENTIFIER = 2;
		 internal const sbyte FORMAT_VERSION = 0;

		 private readonly int _internalMaxKeyCount;
		 private readonly int _leafMaxKeyCount;
		 private readonly int _keySize;
		 private readonly int _valueSize;

		 internal TreeNodeFixedSize( int pageSize, Layout<KEY, VALUE> layout ) : base( pageSize, layout )
		 {
			  this._keySize = layout.KeySize( default( KEY ) );
			  this._valueSize = layout.ValueSize( default( VALUE ) );
			  this._internalMaxKeyCount = Math.floorDiv( pageSize - ( BaseHeaderLength + SizePageReference ), _keySize + SizePageReference );
			  this._leafMaxKeyCount = Math.floorDiv( pageSize - BaseHeaderLength, _keySize + _valueSize );

			  if ( _internalMaxKeyCount < 2 )
			  {
					throw new MetadataMismatchException( "For layout %s a page size of %d would only fit %d internal keys, minimum is 2", layout, pageSize, _internalMaxKeyCount );
			  }
			  if ( _leafMaxKeyCount < 2 )
			  {
					throw new MetadataMismatchException( "A page size of %d would only fit leaf keys, minimum is 2", pageSize, _leafMaxKeyCount );
			  }
		 }

		 internal override void WriteAdditionalHeader( PageCursor cursor )
		 { // no-op
		 }

		 private static int ChildSize()
		 {
			  return SizePageReference;
		 }

		 internal override KEY KeyAt( PageCursor cursor, KEY into, int pos, Type type )
		 {
			  cursor.Offset = KeyOffset( pos );
			  Layout.readKey( cursor, into, FIXED_SIZE_KEY );
			  return into;
		 }

		 internal override void KeyValueAt( PageCursor cursor, KEY intoKey, VALUE intoValue, int pos )
		 {
			  KeyAt( cursor, intoKey, pos, LEAF );
			  ValueAt( cursor, intoValue, pos );
		 }

		 internal override void InsertKeyAndRightChildAt( PageCursor cursor, KEY key, long child, int pos, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  InsertKeyAt( cursor, key, pos, keyCount );
			  InsertChildAt( cursor, child, pos + 1, keyCount, stableGeneration, unstableGeneration );
		 }

		 internal override void InsertKeyValueAt( PageCursor cursor, KEY key, VALUE value, int pos, int keyCount )
		 {
			  InsertKeyAt( cursor, key, pos, keyCount );
			  InsertValueAt( cursor, value, pos, keyCount );
		 }

		 internal override void RemoveKeyValueAt( PageCursor cursor, int pos, int keyCount )
		 {
			  RemoveKeyAt( cursor, pos, keyCount );
			  RemoveValueAt( cursor, pos, keyCount );
		 }

		 internal override void RemoveKeyAndLeftChildAt( PageCursor cursor, int keyPos, int keyCount )
		 {
			  RemoveKeyAt( cursor, keyPos, keyCount );
			  RemoveChildAt( cursor, keyPos, keyCount );
		 }

		 internal override void RemoveKeyAndRightChildAt( PageCursor cursor, int keyPos, int keyCount )
		 {
			  RemoveKeyAt( cursor, keyPos, keyCount );
			  RemoveChildAt( cursor, keyPos + 1, keyCount );
		 }

		 internal override bool SetKeyAtInternal( PageCursor cursor, KEY key, int pos )
		 {
			  cursor.Offset = KeyOffset( pos );
			  Layout.writeKey( cursor, key );
			  return true;
		 }

		 internal override VALUE ValueAt( PageCursor cursor, VALUE value, int pos )
		 {
			  cursor.Offset = ValueOffset( pos );
			  Layout.readValue( cursor, value, FIXED_SIZE_VALUE );
			  return value;
		 }

		 internal override bool SetValueAt( PageCursor cursor, VALUE value, int pos )
		 {
			  cursor.Offset = ValueOffset( pos );
			  Layout.writeValue( cursor, value );
			  return true;
		 }

		 internal override void SetChildAt( PageCursor cursor, long child, int pos, long stableGeneration, long unstableGeneration )
		 {
			  cursor.Offset = ChildOffset( pos );
			  WriteChild( cursor, child, stableGeneration, unstableGeneration );
		 }

		 internal override int KeyValueSizeCap()
		 {
			  return NO_KEY_VALUE_SIZE_CAP;
		 }

		 internal override void ValidateKeyValueSize( KEY key, VALUE value )
		 { // no-op for fixed size
		 }

		 internal override bool ReasonableKeyCount( int keyCount )
		 {
			  return keyCount >= 0 && keyCount <= Math.Max( InternalMaxKeyCount(), LeafMaxKeyCount() );
		 }

		 internal override bool ReasonableChildCount( int childCount )
		 {
			  return childCount >= 0 && childCount <= InternalMaxKeyCount();
		 }

		 internal override int ChildOffset( int pos )
		 {
			  return BaseHeaderLength + _internalMaxKeyCount * _keySize + pos * SizePageReference;
		 }

		 private int InternalMaxKeyCount()
		 {
			  return _internalMaxKeyCount;
		 }

		 private void InsertKeyAt( PageCursor cursor, KEY key, int pos, int keyCount )
		 {
			  InsertKeySlotsAt( cursor, pos, 1, keyCount );
			  cursor.Offset = KeyOffset( pos );
			  Layout.writeKey( cursor, key );
		 }

		 private int LeafMaxKeyCount()
		 {
			  return _leafMaxKeyCount;
		 }

		 private void RemoveKeyAt( PageCursor cursor, int pos, int keyCount )
		 {
			  RemoveSlotAt( cursor, pos, keyCount, KeyOffset( 0 ), _keySize );
		 }

		 private void InsertChildAt( PageCursor cursor, long child, int pos, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  InsertChildSlot( cursor, pos, keyCount );
			  SetChildAt( cursor, child, pos, stableGeneration, unstableGeneration );
		 }

		 private void RemoveChildAt( PageCursor cursor, int pos, int keyCount )
		 {
			  RemoveSlotAt( cursor, pos, keyCount + 1, ChildOffset( 0 ), ChildSize() );
		 }

		 private void InsertKeyValueSlots( PageCursor cursor, int numberOfSlots, int keyCount )
		 {
			  InsertKeySlotsAt( cursor, 0, numberOfSlots, keyCount );
			  InsertValueSlotsAt( cursor, 0, numberOfSlots, keyCount );
		 }

		 // Always insert together with key. Use insertKeyValueAt
		 private void InsertValueAt( PageCursor cursor, VALUE value, int pos, int keyCount )
		 {
			  InsertValueSlotsAt( cursor, pos, 1, keyCount );
			  SetValueAt( cursor, value, pos );
		 }

		 // Always insert together with key. Use removeKeyValueAt
		 private void RemoveValueAt( PageCursor cursor, int pos, int keyCount )
		 {
			  RemoveSlotAt( cursor, pos, keyCount, ValueOffset( 0 ), _valueSize );
		 }

		 private void InsertKeySlotsAt( PageCursor cursor, int pos, int numberOfSlots, int keyCount )
		 {
			  InsertSlotsAt( cursor, pos, numberOfSlots, keyCount, KeyOffset( 0 ), _keySize );
		 }

		 private void InsertValueSlotsAt( PageCursor cursor, int pos, int numberOfSlots, int keyCount )
		 {
			  InsertSlotsAt( cursor, pos, numberOfSlots, keyCount, ValueOffset( 0 ), _valueSize );
		 }

		 private void InsertChildSlot( PageCursor cursor, int pos, int keyCount )
		 {
			  InsertSlotsAt( cursor, pos, 1, keyCount + 1, ChildOffset( 0 ), ChildSize() );
		 }

		 private int KeyOffset( int pos )
		 {
			  return BaseHeaderLength + pos * _keySize;
		 }

		 private int ValueOffset( int pos )
		 {
			  return BaseHeaderLength + _leafMaxKeyCount * _keySize + pos * _valueSize;
		 }

		 private int KeySize()
		 {
			  return _keySize;
		 }

		 private int ValueSize()
		 {
			  return _valueSize;
		 }

		 /* SPLIT, MERGE and REBALANCE*/

		 internal override Overflow InternalOverflow( PageCursor cursor, int currentKeyCount, KEY newKey )
		 {
			  return currentKeyCount + 1 > InternalMaxKeyCount() ? Overflow.Yes : Overflow.No;
		 }

		 internal override Overflow LeafOverflow( PageCursor cursor, int currentKeyCount, KEY newKey, VALUE newValue )
		 {
			  return currentKeyCount + 1 > LeafMaxKeyCount() ? Overflow.Yes : Overflow.No;
		 }

		 internal override void DefragmentLeaf( PageCursor cursor )
		 { // no-op
		 }

		 internal override void DefragmentInternal( PageCursor cursor )
		 { // no-op
		 }

		 internal override bool LeafUnderflow( PageCursor cursor, int keyCount )
		 {
			  return keyCount < ( LeafMaxKeyCount() + 1 ) / 2;
		 }

		 internal override int CanRebalanceLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  if ( leftKeyCount + rightKeyCount >= LeafMaxKeyCount() )
			  {
					int totalKeyCount = rightKeyCount + leftKeyCount;
					int moveFromPosition = totalKeyCount / 2;
					return leftKeyCount - moveFromPosition;
			  }
			  return -1;
		 }

		 internal override bool CanMergeLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  return leftKeyCount + rightKeyCount <= LeafMaxKeyCount();
		 }

		 internal override void DoSplitLeaf( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, VALUE newValue, KEY newSplitter, double ratioToKeepInLeftOnSplit )
		 {
			  int keyCountAfterInsert = leftKeyCount + 1;
			  int splitPos = splitPos( keyCountAfterInsert, ratioToKeepInLeftOnSplit );

			  if ( splitPos == insertPos )
			  {
					Layout.copyKey( newKey, newSplitter );
			  }
			  else
			  {
					KeyAt( leftCursor, newSplitter, insertPos < splitPos ? splitPos - 1 : splitPos, LEAF );
			  }
			  int rightKeyCount = keyCountAfterInsert - splitPos;

			  if ( insertPos < splitPos )
			  {
					//                v---------v       copy
					// before _,_,_,_,_,_,_,_,_,_
					// insert _,_,_,X,_,_,_,_,_,_,_
					// split            ^
					CopyKeysAndValues( leftCursor, splitPos - 1, rightCursor, 0, rightKeyCount );
					InsertKeyValueAt( leftCursor, newKey, newValue, insertPos, splitPos - 1 );
			  }
			  else
			  {
					//                  v---v           first copy
					//                        v-v       second copy
					// before _,_,_,_,_,_,_,_,_,_
					// insert _,_,_,_,_,_,_,_,X,_,_
					// split            ^
					int countBeforePos = insertPos - splitPos;
					if ( countBeforePos > 0 )
					{
						 // first copy
						 CopyKeysAndValues( leftCursor, splitPos, rightCursor, 0, countBeforePos );
					}
					InsertKeyValueAt( rightCursor, newKey, newValue, countBeforePos, countBeforePos );
					int countAfterPos = leftKeyCount - insertPos;
					if ( countAfterPos > 0 )
					{
						 // second copy
						 CopyKeysAndValues( leftCursor, insertPos, rightCursor, countBeforePos + 1, countAfterPos );
					}
			  }
			  TreeNode.SetKeyCount( leftCursor, splitPos );
			  TreeNode.SetKeyCount( rightCursor, rightKeyCount );
		 }

		 /// <summary>
		 /// Given a range with keyCount number of fixed size keys,
		 /// then splitPos point to the first key that should be moved to right node.
		 /// Everything before splitPos will be kept in left node.
		 /// 
		 /// Middle split
		 ///       0,1,2,3,4
		 /// split     ^
		 /// left  0,1
		 /// right 2,3,4
		 /// 
		 /// Min split
		 ///       0,1,2,3,4
		 /// split   ^
		 /// left  0
		 /// right 1,2,3,4
		 /// 
		 /// Max split
		 ///       0,1,2,3,4
		 /// split         ^
		 /// left  0,1,2,3
		 /// right 4
		 /// 
		 /// Note that splitPos can not point past last position (keyCount - 1) or before pos 1.
		 /// This is because we need to split the range somewhere.
		 /// </summary>
		 /// <param name="keyCount"> number of keys in range. </param>
		 /// <param name="ratioToKeepInLeftOnSplit"> How large ratio of key range to try and keep in left node. </param>
		 /// <returns> position of first key to move to right node. </returns>
		 private static int SplitPos( int keyCount, double ratioToKeepInLeftOnSplit )
		 {
			  // Key
			  int minSplitPos = 1;
			  int maxSplitPos = keyCount - 1;
			  return Math.Max( minSplitPos, Math.Min( maxSplitPos, ( int )( ratioToKeepInLeftOnSplit * keyCount ) ) );
		 }

		 internal override void DoSplitInternal( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, long newRightChild, long stableGeneration, long unstableGeneration, KEY newSplitter, double ratioToKeepInLeftOnSplit )
		 {
			  int keyCountAfterInsert = leftKeyCount + 1;
			  int splitPos = splitPos( keyCountAfterInsert, ratioToKeepInLeftOnSplit );

			  if ( splitPos == insertPos )
			  {
					Layout.copyKey( newKey, newSplitter );
			  }
			  else
			  {
					KeyAt( leftCursor, newSplitter, insertPos < splitPos ? splitPos - 1 : splitPos, INTERNAL );
			  }
			  int rightKeyCount = keyCountAfterInsert - splitPos - 1; // -1 because don't keep prim key in internal

			  if ( insertPos < splitPos )
			  {
					//                         v-------v       copy
					// before key    _,_,_,_,_,_,_,_,_,_
					// before child -,-,-,-,-,-,-,-,-,-,-
					// insert key    _,_,X,_,_,_,_,_,_,_,_
					// insert child -,-,-,x,-,-,-,-,-,-,-,-
					// split key               ^

					leftCursor.CopyTo( KeyOffset( splitPos ), rightCursor, KeyOffset( 0 ), rightKeyCount * KeySize() );
					leftCursor.CopyTo( ChildOffset( splitPos ), rightCursor, ChildOffset( 0 ), ( rightKeyCount + 1 ) * ChildSize() );
					InsertKeyAt( leftCursor, newKey, insertPos, splitPos - 1 );
					InsertChildAt( leftCursor, newRightChild, insertPos + 1, splitPos - 1, stableGeneration, unstableGeneration );
			  }
			  else
			  {
					// pos > splitPos
					//                         v-v          first copy
					//                             v-v-v    second copy
					// before key    _,_,_,_,_,_,_,_,_,_
					// before child -,-,-,-,-,-,-,-,-,-,-
					// insert key    _,_,_,_,_,_,_,X,_,_,_
					// insert child -,-,-,-,-,-,-,-,x,-,-,-
					// split key               ^

					// pos == splitPos
					//                                      first copy
					//                         v-v-v-v-v    second copy
					// before key    _,_,_,_,_,_,_,_,_,_
					// before child -,-,-,-,-,-,-,-,-,-,-
					// insert key    _,_,_,_,_,X,_,_,_,_,_
					// insert child -,-,-,-,-,-,x,-,-,-,-,-
					// split key               ^

					// Keys
					int countBeforePos = insertPos - ( splitPos + 1 );
					// ... first copy
					if ( countBeforePos > 0 )
					{
						 leftCursor.CopyTo( KeyOffset( splitPos + 1 ), rightCursor, KeyOffset( 0 ), countBeforePos * KeySize() );
					}
					// ... insert
					if ( countBeforePos >= 0 )
					{
						 InsertKeyAt( rightCursor, newKey, countBeforePos, countBeforePos );
					}
					// ... second copy
					int countAfterPos = leftKeyCount - insertPos;
					if ( countAfterPos > 0 )
					{
						 leftCursor.CopyTo( KeyOffset( insertPos ), rightCursor, KeyOffset( countBeforePos + 1 ), countAfterPos * KeySize() );
					}

					// Children
					countBeforePos = insertPos - splitPos;
					// ... first copy
					if ( countBeforePos > 0 )
					{
						 // first copy
						 leftCursor.CopyTo( ChildOffset( splitPos + 1 ), rightCursor, ChildOffset( 0 ), countBeforePos * ChildSize() );
					}
					// ... insert
					InsertChildAt( rightCursor, newRightChild, countBeforePos, countBeforePos, stableGeneration, unstableGeneration );
					// ... second copy
					if ( countAfterPos > 0 )
					{
						 leftCursor.CopyTo( ChildOffset( insertPos + 1 ), rightCursor, ChildOffset( countBeforePos + 1 ), countAfterPos * ChildSize() );
					}
			  }
			  TreeNode.SetKeyCount( leftCursor, splitPos );
			  TreeNode.SetKeyCount( rightCursor, rightKeyCount );
		 }

		 internal override void MoveKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount, int fromPosInLeftNode )
		 {
			  int numberOfKeysToMove = leftKeyCount - fromPosInLeftNode;

			  // Push keys and values in right sibling to the right
			  InsertKeyValueSlots( rightCursor, numberOfKeysToMove, rightKeyCount );

			  // Move keys and values from left sibling to right sibling
			  CopyKeysAndValues( leftCursor, fromPosInLeftNode, rightCursor, 0, numberOfKeysToMove );

			  SetKeyCount( leftCursor, leftKeyCount - numberOfKeysToMove );
			  SetKeyCount( rightCursor, rightKeyCount + numberOfKeysToMove );
		 }

		 internal override void CopyKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  // Push keys and values in right sibling to the right
			  InsertKeyValueSlots( rightCursor, leftKeyCount, rightKeyCount );

			  // Move keys and values from left sibling to right sibling
			  CopyKeysAndValues( leftCursor, 0, rightCursor, 0, leftKeyCount );

			  // KeyCount
			  SetKeyCount( rightCursor, rightKeyCount + leftKeyCount );
		 }

		 internal override void PrintNode( PageCursor cursor, bool includeValue, bool includeAllocSpace, long stableGeneration, long unstableGeneration )
		 {
			  PrintingGBPTreeVisitor<KEY, VALUE> visitor = new PrintingGBPTreeVisitor<KEY, VALUE>( System.out, includeValue, false, false, false, false );
			  try
			  {
					( new GBPTreeStructure<>( this, Layout, stableGeneration, unstableGeneration ) ).visitTreeNode( cursor, visitor );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void CopyKeysAndValues( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toPos, int count )
		 {
			  fromCursor.CopyTo( KeyOffset( fromPos ), toCursor, KeyOffset( toPos ), count * KeySize() );
			  fromCursor.CopyTo( ValueOffset( fromPos ), toCursor, ValueOffset( toPos ),count * ValueSize() );
		 }

		 internal override string CheckMetaConsistency( PageCursor cursor, int keyCount, Type type, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  return "";
		 }

		 public override string ToString()
		 {
			  return "TreeNodeFixedSize[pageSize:" + PageSize + ", internalMax:" + InternalMaxKeyCount() + ", leafMax:" + LeafMaxKeyCount() + ", " +
						 "keySize:" + KeySize() + ", valueSize:" + _valueSize + "]";
		 }
	}

}