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
	using IntStack = org.eclipse.collections.api.stack.primitive.IntStack;
	using MutableIntStack = org.eclipse.collections.api.stack.primitive.MutableIntStack;
	using IntArrayStack = org.eclipse.collections.impl.stack.mutable.primitive.IntArrayStack;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.SIZE_KEY_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.SIZE_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.SIZE_TOTAL_OVERHEAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.SIZE_VALUE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.extractKeySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.extractTombstone;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.extractValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.getOverhead;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.putKeyOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.putKeySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.putKeyValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.putTombstone;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.readKeyOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.DynamicSizeUtil.readKeyValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.PageCursorUtil.putUnsignedShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// # = empty space
	/// K* = offset to key or key and value
	/// 
	/// LEAF
	/// [                                   HEADER   86B                                                   ]|[KEY_OFFSETS]##########[KEYS_VALUES]
	/// [NODETYPE][TYPE][GENERATION][KEYCOUNT][RIGHTSIBLING][LEFTSIBLING][SUCCESSOR][ALLOCOFFSET][DEADSPACE]|[K0*,K1*,K2*]->      <-[KV0,KV2,KV1]
	///  0         1     2           6         10            34           58         82           84          86
	/// 
	///  INTERNAL
	/// [                                   HEADER   86B                                                   ]|[  KEY_OFFSET_CHILDREN  ]######[  KEYS  ]
	/// [NODETYPE][TYPE][GENERATION][KEYCOUNT][RIGHTSIBLING][LEFTSIBLING][SUCCESSOR][ALLOCOFFSET][DEADSPACE]|[C0,K0*,C1,K1*,C2,K2*,C3]->  <-[K2,K0,K1]
	///  0         1     2           6         10            34           58         82           84          86
	/// 
	/// See <seealso cref="DynamicSizeUtil"/> for more detailed layout for individual offset array entries and key / key_value entries.
	/// </summary>
	public class TreeNodeDynamicSize<KEY, VALUE> : TreeNode<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_maxKeyCount = PageSize / ( BytesKeyOffset() + SIZE_KEY_SIZE + SIZE_VALUE_SIZE );
			_oldOffset = new int[_maxKeyCount];
			_newOffset = new int[_maxKeyCount];
		}

		 internal const sbyte FORMAT_IDENTIFIER = 3;
		 internal const sbyte FORMAT_VERSION = 0;

		 /// <summary>
		 /// Concepts
		 /// Total space - The space available for data (pageSize - headerSize)
		 /// Active space - Space currently occupied by active data (not including dead keys)
		 /// Dead space - Space currently occupied by dead data that could be reclaimed by defragment
		 /// Alloc offset - Exact offset to leftmost key and thus the end of alloc space
		 /// Alloc space - The available space between offset array and data space
		 /// 
		 /// TotalSpace  |----------------------------------------|
		 /// ActiveSpace |-----------|   +    |---------|  + |----|
		 /// DeadSpace                                  |----|
		 /// AllocSpace              |--------|
		 /// AllocOffset                      v
		 ///     [Header][OffsetArray]........[_________,XXXX,____] (_ = alive key, X = dead key)
		 /// </summary>
		 private static readonly int _bytePosAllocoffset = BaseHeaderLength;
		 private static readonly int _bytePosDeadspace = _bytePosAllocoffset + BytesPageOffset();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting static final int HEADER_LENGTH_DYNAMIC = BYTE_POS_DEADSPACE + bytesPageOffset();
		 internal static readonly int HeaderLengthDynamic = _bytePosDeadspace + BytesPageOffset();

		 private const int LEAST_NUMBER_OF_ENTRIES_PER_PAGE = 2;
		 private static readonly int _minimumEntrySizeCap = ( sizeof( long ) * 8 );
		 private readonly int _keyValueSizeCap;
		 private readonly MutableIntStack _deadKeysOffset = new IntArrayStack();
		 private readonly MutableIntStack _aliveKeysOffset = new IntArrayStack();
		 private int _maxKeyCount;
		 private int[] _oldOffset;
		 private int[] _newOffset;
		 private readonly int _totalSpace;
		 private readonly int _halfSpace;
		 private readonly KEY _tmpKeyLeft;
		 private readonly KEY _tmpKeyRight;

		 internal TreeNodeDynamicSize( int pageSize, Layout<KEY, VALUE> layout ) : base( pageSize, layout )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  _totalSpace = pageSize - HeaderLengthDynamic;
			  _halfSpace = _totalSpace / 2;
			  _keyValueSizeCap = KeyValueSizeCapFromPageSize( pageSize );

			  if ( _keyValueSizeCap < _minimumEntrySizeCap )
			  {
					throw new MetadataMismatchException( "We need to fit at least %d key-value entries per page in leaves. To do that a key-value entry can be at most %dB " + "with current page size of %dB. We require this cap to be at least %dB.", LEAST_NUMBER_OF_ENTRIES_PER_PAGE, _keyValueSizeCap, pageSize, ( sizeof( long ) * 8 ) );
			  }

			  _tmpKeyLeft = layout.NewKey();
			  _tmpKeyRight = layout.NewKey();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static int keyValueSizeCapFromPageSize(int pageSize)
		 public static int KeyValueSizeCapFromPageSize( int pageSize )
		 {
			  return ( pageSize - HeaderLengthDynamic ) / LEAST_NUMBER_OF_ENTRIES_PER_PAGE - SIZE_TOTAL_OVERHEAD;
		 }

		 internal override void WriteAdditionalHeader( PageCursor cursor )
		 {
			  SetAllocOffset( cursor, PageSize );
			  SetDeadSpace( cursor, 0 );
		 }

		 internal override KEY KeyAt( PageCursor cursor, KEY into, int pos, Type type )
		 {
			  PlaceCursorAtActualKey( cursor, pos, type );

			  // Read key
			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );
			  if ( KeyValueSizeTooLarge( keySize, valueSize ) || keySize < 0 )
			  {
					ReadUnreliableKeyValueSize( cursor, keySize, valueSize, keyValueSize, pos );
					return into;
			  }
			  Layout.readKey( cursor, into, keySize );
			  return into;
		 }

		 internal override void KeyValueAt( PageCursor cursor, KEY intoKey, VALUE intoValue, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, LEAF );

			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );
			  if ( KeyValueSizeTooLarge( keySize, valueSize ) || keySize < 0 || valueSize < 0 )
			  {
					ReadUnreliableKeyValueSize( cursor, keySize, valueSize, keyValueSize, pos );
					return;
			  }
			  Layout.readKey( cursor, intoKey, keySize );
			  Layout.readValue( cursor, intoValue, valueSize );
		 }

		 internal override void InsertKeyAndRightChildAt( PageCursor cursor, KEY key, long child, int pos, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  // Where to write key?
			  int currentKeyOffset = GetAllocOffset( cursor );
			  int keySize = Layout.keySize( key );
			  int newKeyOffset = currentKeyOffset - keySize - getOverhead( keySize, 0 );

			  // Write key
			  cursor.Offset = newKeyOffset;
			  putKeySize( cursor, keySize );
			  Layout.writeKey( cursor, key );

			  // Update alloc space
			  SetAllocOffset( cursor, newKeyOffset );

			  // Write to offset array
			  InsertSlotsAt( cursor, pos, 1, keyCount, KeyPosOffsetInternal( 0 ), KeyChildSize() );
			  cursor.Offset = KeyPosOffsetInternal( pos );
			  putKeyOffset( cursor, newKeyOffset );
			  WriteChild( cursor, child, stableGeneration, unstableGeneration );
		 }

		 internal override void InsertKeyValueAt( PageCursor cursor, KEY key, VALUE value, int pos, int keyCount )
		 {
			  // Where to write key?
			  int currentKeyValueOffset = GetAllocOffset( cursor );
			  int keySize = Layout.keySize( key );
			  int valueSize = Layout.valueSize( value );
			  int newKeyValueOffset = currentKeyValueOffset - keySize - valueSize - getOverhead( keySize, valueSize );

			  // Write key and value
			  cursor.Offset = newKeyValueOffset;
			  putKeyValueSize( cursor, keySize, valueSize );
			  Layout.writeKey( cursor, key );
			  Layout.writeValue( cursor, value );

			  // Update alloc space
			  SetAllocOffset( cursor, newKeyValueOffset );

			  // Write to offset array
			  InsertSlotsAt( cursor, pos, 1, keyCount, KeyPosOffsetLeaf( 0 ), BytesKeyOffset() );
			  cursor.Offset = KeyPosOffsetLeaf( pos );
			  putKeyOffset( cursor, newKeyValueOffset );
		 }

		 internal override void RemoveKeyValueAt( PageCursor cursor, int pos, int keyCount )
		 {
			  // Kill actual key
			  PlaceCursorAtActualKey( cursor, pos, LEAF );
			  int keyOffset = cursor.Offset;
			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );
			  cursor.Offset = keyOffset;
			  putTombstone( cursor );

			  // Update dead space
			  int deadSpace = GetDeadSpace( cursor );
			  SetDeadSpace( cursor, deadSpace + keySize + valueSize + getOverhead( keySize, valueSize ) );

			  // Remove from offset array
			  RemoveSlotAt( cursor, pos, keyCount, KeyPosOffsetLeaf( 0 ), BytesKeyOffset() );
		 }

		 internal override void RemoveKeyAndRightChildAt( PageCursor cursor, int keyPos, int keyCount )
		 {
			  // Kill actual key
			  PlaceCursorAtActualKey( cursor, keyPos, INTERNAL );
			  int keyOffset = cursor.Offset;
			  int keySize = extractKeySize( readKeyValueSize( cursor ) );
			  cursor.Offset = keyOffset;
			  putTombstone( cursor );

			  // Update dead space
			  int deadSpace = GetDeadSpace( cursor );
			  SetDeadSpace( cursor, deadSpace + keySize + getOverhead( keySize, 0 ) );

			  // Remove for offsetArray
			  RemoveSlotAt( cursor, keyPos, keyCount, KeyPosOffsetInternal( 0 ), KeyChildSize() );

			  // Zero pad empty area
			  ZeroPad( cursor, KeyPosOffsetInternal( keyCount - 1 ), BytesKeyOffset() + ChildSize() );
		 }

		 internal override void RemoveKeyAndLeftChildAt( PageCursor cursor, int keyPos, int keyCount )
		 {
			  // Kill actual key
			  PlaceCursorAtActualKey( cursor, keyPos, INTERNAL );
			  int keyOffset = cursor.Offset;
			  int keySize = extractKeySize( readKeyValueSize( cursor ) );
			  cursor.Offset = keyOffset;
			  putTombstone( cursor );

			  // Update dead space
			  int deadSpace = GetDeadSpace( cursor );
			  SetDeadSpace( cursor, deadSpace + keySize + getOverhead( keySize, 0 ) );

			  // Remove for offsetArray
			  RemoveSlotAt( cursor, keyPos, keyCount, KeyPosOffsetInternal( 0 ) - ChildSize(), KeyChildSize() );

			  // Move last child
			  cursor.CopyTo( ChildOffset( keyCount ), cursor, ChildOffset( keyCount - 1 ), ChildSize() );

			  // Zero pad empty area
			  ZeroPad( cursor, KeyPosOffsetInternal( keyCount - 1 ), BytesKeyOffset() + ChildSize() );
		 }

		 internal override bool SetKeyAtInternal( PageCursor cursor, KEY key, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, INTERNAL );

			  long keyValueSize = readKeyValueSize( cursor );
			  int oldKeySize = extractKeySize( keyValueSize );
			  int oldValueSize = extractValueSize( keyValueSize );
			  if ( KeyValueSizeTooLarge( oldKeySize, oldValueSize ) )
			  {
					ReadUnreliableKeyValueSize( cursor, oldKeySize, oldValueSize, keyValueSize, pos );
			  }
			  int newKeySize = Layout.keySize( key );
			  if ( newKeySize == oldKeySize )
			  {
					// Fine, we can just overwrite
					Layout.writeKey( cursor, key );
					return true;
			  }
			  return false;
		 }

		 internal override VALUE ValueAt( PageCursor cursor, VALUE into, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, LEAF );

			  // Read value
			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );
			  if ( KeyValueSizeTooLarge( keySize, valueSize ) || keySize < 0 || valueSize < 0 )
			  {
					ReadUnreliableKeyValueSize( cursor, keySize, valueSize, keyValueSize, pos );
					return into;
			  }
			  ProgressCursor( cursor, keySize );
			  Layout.readValue( cursor, into, valueSize );
			  return into;
		 }

		 internal override bool SetValueAt( PageCursor cursor, VALUE value, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, LEAF );

			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int oldValueSize = extractValueSize( keyValueSize );
			  int newValueSize = Layout.valueSize( value );
			  if ( oldValueSize == newValueSize )
			  {
					// Fine we can just overwrite
					ProgressCursor( cursor, keySize );
					Layout.writeValue( cursor, value );
					return true;
			  }
			  return false;
		 }

		 private void ProgressCursor( PageCursor cursor, int delta )
		 {
			  cursor.Offset = cursor.Offset + delta;
		 }

		 internal override void SetChildAt( PageCursor cursor, long child, int pos, long stableGeneration, long unstableGeneration )
		 {
			  cursor.Offset = ChildOffset( pos );
			  WriteChild( cursor, child, stableGeneration, unstableGeneration );
		 }

		 public override int KeyValueSizeCap()
		 {
			  return _keyValueSizeCap;
		 }

		 internal override void ValidateKeyValueSize( KEY key, VALUE value )
		 {
			  int keySize = Layout.keySize( key );
			  int valueSize = Layout.valueSize( value );
			  if ( KeyValueSizeTooLarge( keySize, valueSize ) )
			  {
					throw new System.ArgumentException( "Index key-value size it to large. Please see index documentation for limitations." );
			  }
		 }

		 internal override bool ReasonableKeyCount( int keyCount )
		 {
			  return keyCount >= 0 && keyCount <= _totalSpace / SIZE_TOTAL_OVERHEAD;
		 }

		 internal override bool ReasonableChildCount( int childCount )
		 {
			  return ReasonableKeyCount( childCount );
		 }

		 internal override int ChildOffset( int pos )
		 {
			  // Child pointer to the left of key at pos
			  return KeyPosOffsetInternal( pos ) - ChildSize();
		 }

		 internal override Overflow InternalOverflow( PageCursor cursor, int currentKeyCount, KEY newKey )
		 {
			  // How much space do we have?
			  int allocSpace = GetAllocSpace( cursor, currentKeyCount, INTERNAL );
			  int deadSpace = GetDeadSpace( cursor );

			  // How much space do we need?
			  int neededSpace = TotalSpaceOfKeyChild( newKey );

			  // There is your answer!
			  return neededSpace <= allocSpace ? Overflow.No : neededSpace <= allocSpace + deadSpace ? Overflow.NoNeedDefrag : Overflow.Yes;
		 }

		 internal override Overflow LeafOverflow( PageCursor cursor, int currentKeyCount, KEY newKey, VALUE newValue )
		 {
			  // How much space do we have?
			  int deadSpace = GetDeadSpace( cursor );
			  int allocSpace = GetAllocSpace( cursor, currentKeyCount, LEAF );

			  // How much space do we need?
			  int neededSpace = TotalSpaceOfKeyValue( newKey, newValue );

			  // There is your answer!
			  return neededSpace <= allocSpace ? Overflow.No : neededSpace <= allocSpace + deadSpace ? Overflow.NoNeedDefrag : Overflow.Yes;
		 }

		 internal override void DefragmentLeaf( PageCursor cursor )
		 {
			  DoDefragment( cursor, LEAF );
		 }

		 internal override void DefragmentInternal( PageCursor cursor )
		 {
			  DoDefragment( cursor, INTERNAL );
		 }

		 private void DoDefragment( PageCursor cursor, Type type )
		 {
			  /*
			  The goal is to compact all alive keys in the node
			  by reusing the space occupied by dead keys.
	
			  BEFORE
			  [8][X][1][3][X][2][X][7][5]
	
			  AFTER
			  .........[8][1][3][2][7][5]
			      ^ Reclaimed space
	
			  It works like this:
			  Work from right to left.
			  For each dead space of size X (can be multiple consecutive dead keys)
			  Move all neighbouring alive keys to the left of that dead space X bytes to the right.
			  Can only move in blocks of size X at the time.
	
			  Step by step:
			  [8][X][1][3][X][2][X][7][5]
			  [8][X][1][3][X][X][2][7][5]
			  [8][X][X][X][1][3][2][7][5]
			  [X][X][X][8][1][3][2][7][5]
	
			  Here is how the offsets work
			  BEFORE MOVE
			                    v       aliveRangeOffset
			  [X][_][_][X][_][X][_][_]
			             ^   ^          deadRangeOffset
			             |_____________ moveRangeOffset
	
			  AFTER MOVE
			                 v          aliveRangeOffset
			  [X][_][_][X][X][_][_][_]
			           ^                 deadRangeOffset
			  */

			  // Mark all offsets
			  _deadKeysOffset.clear();
			  _aliveKeysOffset.clear();
			  if ( type == INTERNAL )
			  {
					RecordDeadAndAliveInternal( cursor, _deadKeysOffset, _aliveKeysOffset );
			  }
			  else
			  {
					RecordDeadAndAliveLeaf( cursor, _deadKeysOffset, _aliveKeysOffset );
			  }

			  // Cursors into field byte arrays
			  int oldOffsetCursor = 0;
			  int newOffsetCursor = 0;

			  int aliveRangeOffset = PageSize; // Everything after this point is alive
			  int deadRangeOffset; // Everything between this point and aliveRangeOffset is dead space

			  // Rightmost alive keys does not need to move
			  while ( Peek( _deadKeysOffset ) < Peek( _aliveKeysOffset ) )
			  {
					aliveRangeOffset = Poll( _aliveKeysOffset );
			  }

			  do
			  {
					// Locate next range of dead keys
					deadRangeOffset = aliveRangeOffset;
					while ( Peek( _aliveKeysOffset ) < Peek( _deadKeysOffset ) )
					{
						 deadRangeOffset = Poll( _deadKeysOffset );
					}

					// Locate next range of alive keys
					int moveOffset = deadRangeOffset;
					while ( Peek( _deadKeysOffset ) < Peek( _aliveKeysOffset ) )
					{
						 int moveKey = Poll( _aliveKeysOffset );
						 _oldOffset[oldOffsetCursor++] = moveKey;
						 moveOffset = moveKey;
					}

					// Update offset mapping
					int deadRangeSize = aliveRangeOffset - deadRangeOffset;
					while ( oldOffsetCursor > newOffsetCursor )
					{
						 _newOffset[newOffsetCursor] = _oldOffset[newOffsetCursor] + deadRangeSize;
						 newOffsetCursor++;
					}

					// Do move
					while ( moveOffset < ( deadRangeOffset - deadRangeSize ) )
					{
						 // Move one block
						 deadRangeOffset -= deadRangeSize;
						 aliveRangeOffset -= deadRangeSize;
						 cursor.CopyTo( deadRangeOffset, cursor, aliveRangeOffset, deadRangeSize );
					}
					// Move the last piece
					int lastBlockSize = deadRangeOffset - moveOffset;
					if ( lastBlockSize > 0 )
					{
						 deadRangeOffset -= lastBlockSize;
						 aliveRangeOffset -= lastBlockSize;
						 cursor.CopyTo( deadRangeOffset, cursor, aliveRangeOffset, lastBlockSize );
					}
			  } while ( !_aliveKeysOffset.Empty );
			  // Update allocOffset
			  int prevAllocOffset = GetAllocOffset( cursor );
			  SetAllocOffset( cursor, aliveRangeOffset );

			  // Zero pad reclaimed area
			  ZeroPad( cursor, prevAllocOffset, aliveRangeOffset - prevAllocOffset );

			  // Update offset array
			  int keyCount = keyCount( cursor );
			  for ( int pos = 0; pos < keyCount; pos++ )
			  {
					int keyPosOffset = keyPosOffset( pos, type );
					cursor.Offset = keyPosOffset;
					int keyOffset = readKeyOffset( cursor );
					for ( int index = 0; index < oldOffsetCursor; index++ )
					{
						 if ( keyOffset == _oldOffset[index] )
						 {
							  // Overwrite with new offset
							  cursor.Offset = keyPosOffset;
							  putKeyOffset( cursor, _newOffset[index] );
							  goto keyPosContinue;
						 }
					}
				  keyPosContinue:;
			  }
			  keyPosBreak:

			  // Update dead space
			  SetDeadSpace( cursor, 0 );
		 }

		 private static int Peek( IntStack stack )
		 {
			  return stack.Empty ? -1 : stack.peek();
		 }

		 private static int Poll( MutableIntStack stack )
		 {
			  return stack.Empty ? -1 : stack.pop();
		 }

		 internal override bool LeafUnderflow( PageCursor cursor, int keyCount )
		 {
			  int halfSpace = this._halfSpace;
			  int allocSpace = GetAllocSpace( cursor, keyCount, LEAF );
			  int deadSpace = GetDeadSpace( cursor );
			  int availableSpace = allocSpace + deadSpace;

			  return availableSpace > halfSpace;
		 }

		 internal override int CanRebalanceLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  int leftActiveSpace = TotalActiveSpace( leftCursor, leftKeyCount, LEAF );
			  int rightActiveSpace = TotalActiveSpace( rightCursor, rightKeyCount, LEAF );

			  if ( leftActiveSpace + rightActiveSpace < _totalSpace )
			  {
					// We can merge
					return -1;
			  }
			  if ( leftActiveSpace < rightActiveSpace )
			  {
					// Moving keys to the right will only create more imbalance
					return 0;
			  }

			  int prevDelta;
			  int currentDelta = Math.Abs( leftActiveSpace - rightActiveSpace );
			  int keysToMove = 0;
			  int lastChunkSize;
			  do
			  {
					keysToMove++;
					lastChunkSize = TotalSpaceOfKeyValue( leftCursor, leftKeyCount - keysToMove );
					leftActiveSpace -= lastChunkSize;
					rightActiveSpace += lastChunkSize;

					prevDelta = currentDelta;
					currentDelta = Math.Abs( leftActiveSpace - rightActiveSpace );
			  } while ( currentDelta < prevDelta );
			  keysToMove--; // Move back to optimal split
			  leftActiveSpace += lastChunkSize;
			  rightActiveSpace -= lastChunkSize;

			  int halfSpace = this._halfSpace;
			  bool canRebalance = leftActiveSpace > halfSpace && rightActiveSpace > halfSpace;
			  return canRebalance ? keysToMove : 0;
		 }

		 internal override bool CanMergeLeaves( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  int leftActiveSpace = TotalActiveSpace( leftCursor, leftKeyCount, LEAF );
			  int rightActiveSpace = TotalActiveSpace( rightCursor, rightKeyCount, LEAF );
			  int totalSpace = this._totalSpace;
			  return totalSpace >= leftActiveSpace + rightActiveSpace;
		 }

		 internal override void DoSplitLeaf( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, VALUE newValue, KEY newSplitter, double ratioToKeepInLeftOnSplit )
		 {
			  // Find split position
			  int keyCountAfterInsert = leftKeyCount + 1;
			  int splitPos = SplitPosInLeaf( leftCursor, insertPos, newKey, newValue, keyCountAfterInsert, ratioToKeepInLeftOnSplit );

			  KEY leftInSplit;
			  KEY rightInSplit;
			  if ( splitPos == insertPos )
			  {
					leftInSplit = KeyAt( leftCursor, _tmpKeyLeft, splitPos - 1, LEAF );
					rightInSplit = newKey;

			  }
			  else
			  {
					int rightPos = insertPos < splitPos ? splitPos - 1 : splitPos;
					rightInSplit = KeyAt( leftCursor, _tmpKeyRight, rightPos, LEAF );

					if ( rightPos == insertPos )
					{
						 leftInSplit = newKey;
					}
					else
					{
						 int leftPos = rightPos - 1;
						 leftInSplit = KeyAt( leftCursor, _tmpKeyLeft, leftPos, LEAF );
					}
			  }
			  Layout.minimalSplitter( leftInSplit, rightInSplit, newSplitter );

			  int rightKeyCount = keyCountAfterInsert - splitPos;

			  if ( insertPos < splitPos )
			  {
					//                v---------v       copy
					// before _,_,_,_,_,_,_,_,_,_
					// insert _,_,_,X,_,_,_,_,_,_,_
					// split            ^
					MoveKeysAndValues( leftCursor, splitPos - 1, rightCursor, 0, rightKeyCount );
					DefragmentLeaf( leftCursor );
					InsertKeyValueAt( leftCursor, newKey, newValue, insertPos, splitPos - 1 );
			  }
			  else
			  {
					//                  v---v           first copy
					//                        v-v       second copy
					// before _,_,_,_,_,_,_,_,_,_
					// insert _,_,_,_,_,_,_,_,X,_,_
					// split            ^

					// Copy everything in one go
					int newInsertPos = insertPos - splitPos;
					int keysToMove = leftKeyCount - splitPos;
					MoveKeysAndValues( leftCursor, splitPos, rightCursor, 0, keysToMove );
					DefragmentLeaf( leftCursor );
					InsertKeyValueAt( rightCursor, newKey, newValue, newInsertPos, keysToMove );
			  }
			  TreeNode.SetKeyCount( leftCursor, splitPos );
			  TreeNode.SetKeyCount( rightCursor, rightKeyCount );
		 }

		 internal override void DoSplitInternal( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int insertPos, KEY newKey, long newRightChild, long stableGeneration, long unstableGeneration, KEY newSplitter, double ratioToKeepInLeftOnSplit )
		 {
			  int keyCountAfterInsert = leftKeyCount + 1;
			  int splitPos = SplitPosInternal( leftCursor, insertPos, newKey, keyCountAfterInsert, ratioToKeepInLeftOnSplit );

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

					MoveKeysAndChildren( leftCursor, splitPos, rightCursor, 0, rightKeyCount, true );
					// Rightmost key in left is the one we send up to parent, remove it from here.
					RemoveKeyAndRightChildAt( leftCursor, splitPos - 1, splitPos );
					DefragmentInternal( leftCursor );
					InsertKeyAndRightChildAt( leftCursor, newKey, newRightChild, insertPos, splitPos - 1, stableGeneration, unstableGeneration );
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
					if ( insertPos == splitPos )
					{
						 int copyFrom = splitPos;
						 int copyCount = leftKeyCount - copyFrom;
						 MoveKeysAndChildren( leftCursor, copyFrom, rightCursor, 0, copyCount, false );
						 DefragmentInternal( leftCursor );
						 SetChildAt( rightCursor, newRightChild, 0, stableGeneration, unstableGeneration );
					}
					else
					{
						 int copyFrom = splitPos + 1;
						 int copyCount = leftKeyCount - copyFrom;
						 MoveKeysAndChildren( leftCursor, copyFrom, rightCursor, 0, copyCount, true );
						 // Rightmost key in left is the one we send up to parent, remove it from here.
						 RemoveKeyAndRightChildAt( leftCursor, splitPos, splitPos + 1 );
						 DefragmentInternal( leftCursor );
						 InsertKeyAndRightChildAt( rightCursor, newKey, newRightChild, insertPos - copyFrom, copyCount, stableGeneration, unstableGeneration );
					}
			  }
			  TreeNode.SetKeyCount( leftCursor, splitPos );
			  TreeNode.SetKeyCount( rightCursor, rightKeyCount );
		 }

		 internal override void MoveKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount, int fromPosInLeftNode )
		 {
			  DefragmentLeaf( rightCursor );
			  int numberOfKeysToMove = leftKeyCount - fromPosInLeftNode;

			  // Push keys and values in right sibling to the right
			  InsertSlotsAt( rightCursor, 0, numberOfKeysToMove, rightKeyCount, KeyPosOffsetLeaf( 0 ), BytesKeyOffset() );

			  // Move (also updates keyCount of left)
			  MoveKeysAndValues( leftCursor, fromPosInLeftNode, rightCursor, 0, numberOfKeysToMove );

			  // Right keyCount
			  SetKeyCount( rightCursor, rightKeyCount + numberOfKeysToMove );
		 }

		 // NOTE: Does update keyCount
		 private void MoveKeysAndValues( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toPos, int count )
		 {
			  int firstAllocOffset = GetAllocOffset( toCursor );
			  int toAllocOffset = firstAllocOffset;
			  for ( int i = 0; i < count; i++, toPos++ )
			  {
					toAllocOffset = MoveRawKeyValue( fromCursor, fromPos + i, toCursor, toAllocOffset );
					toCursor.Offset = KeyPosOffsetLeaf( toPos );
					putKeyOffset( toCursor, toAllocOffset );
			  }
			  SetAllocOffset( toCursor, toAllocOffset );

			  // Update deadspace
			  int deadSpace = GetDeadSpace( fromCursor );
			  int totalMovedBytes = firstAllocOffset - toAllocOffset;
			  SetDeadSpace( fromCursor, deadSpace + totalMovedBytes );

			  // Key count
			  SetKeyCount( fromCursor, fromPos );
		 }

		 /// <summary>
		 /// Transfer key and value from logical position in 'from' to physical position next to current alloc offset in 'to'.
		 /// Mark transferred key as dead. </summary>
		 /// <returns> new alloc offset in 'to' </returns>
		 private int MoveRawKeyValue( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toAllocOffset )
		 {
			  // What to copy?
			  PlaceCursorAtActualKey( fromCursor, fromPos, LEAF );
			  int fromKeyOffset = fromCursor.Offset;
			  long keyValueSize = readKeyValueSize( fromCursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );

			  // Copy
			  int toCopy = getOverhead( keySize, valueSize ) + keySize + valueSize;
			  int newRightAllocSpace = toAllocOffset - toCopy;
			  fromCursor.CopyTo( fromKeyOffset, toCursor, newRightAllocSpace, toCopy );

			  // Put tombstone
			  fromCursor.Offset = fromKeyOffset;
			  putTombstone( fromCursor );
			  return newRightAllocSpace;
		 }

		 internal override void CopyKeyValuesFromLeftToRight( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount )
		 {
			  DefragmentLeaf( rightCursor );

			  // Push keys and values in right sibling to the right
			  InsertSlotsAt( rightCursor, 0, leftKeyCount, rightKeyCount, KeyPosOffsetLeaf( 0 ), BytesKeyOffset() );

			  // Copy
			  CopyKeysAndValues( leftCursor, 0, rightCursor, 0, leftKeyCount );

			  // KeyCount
			  SetKeyCount( rightCursor, rightKeyCount + leftKeyCount );
		 }

		 private void CopyKeysAndValues( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toPos, int count )
		 {
			  int toAllocOffset = GetAllocOffset( toCursor );
			  for ( int i = 0; i < count; i++, toPos++ )
			  {
					toAllocOffset = CopyRawKeyValue( fromCursor, fromPos + i, toCursor, toAllocOffset );
					toCursor.Offset = KeyPosOffsetLeaf( toPos );
					putKeyOffset( toCursor, toAllocOffset );
			  }
			  SetAllocOffset( toCursor, toAllocOffset );
		 }

		 /// <summary>
		 /// Copy key and value from logical position in 'from' tp physical position next to current alloc offset in 'to'.
		 /// Does NOT mark transferred key as dead. </summary>
		 /// <returns> new alloc offset in 'to' </returns>
		 private int CopyRawKeyValue( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toAllocOffset )
		 {
			  // What to copy?
			  PlaceCursorAtActualKey( fromCursor, fromPos, LEAF );
			  int fromKeyOffset = fromCursor.Offset;
			  long keyValueSize = readKeyValueSize( fromCursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );

			  // Copy
			  int toCopy = getOverhead( keySize, valueSize ) + keySize + valueSize;
			  int newRightAllocSpace = toAllocOffset - toCopy;
			  fromCursor.CopyTo( fromKeyOffset, toCursor, newRightAllocSpace, toCopy );
			  return newRightAllocSpace;
		 }

		 private int GetAllocSpace( PageCursor cursor, int keyCount, Type type )
		 {
			  int allocOffset = GetAllocOffset( cursor );
			  int endOfOffsetArray = type == LEAF ? KeyPosOffsetLeaf( keyCount ) : KeyPosOffsetInternal( keyCount );
			  return allocOffset - endOfOffsetArray;
		 }

		 private void RecordDeadAndAliveLeaf( PageCursor cursor, MutableIntStack deadKeysOffset, MutableIntStack aliveKeysOffset )
		 {
			  int currentOffset = GetAllocOffset( cursor );
			  while ( currentOffset < PageSize )
			  {
					cursor.Offset = currentOffset;
					long keyValueSize = readKeyValueSize( cursor );
					int keySize = extractKeySize( keyValueSize );
					int valueSize = extractValueSize( keyValueSize );
					bool dead = extractTombstone( keyValueSize );

					if ( dead )
					{
						 deadKeysOffset.push( currentOffset );
					}
					else
					{
						 aliveKeysOffset.push( currentOffset );
					}
					currentOffset += keySize + valueSize + getOverhead( keySize, valueSize );
			  }
		 }

		 private void RecordDeadAndAliveInternal( PageCursor cursor, MutableIntStack deadKeysOffset, MutableIntStack aliveKeysOffset )
		 {
			  int currentOffset = GetAllocOffset( cursor );
			  while ( currentOffset < PageSize )
			  {
					cursor.Offset = currentOffset;
					long keyValueSize = readKeyValueSize( cursor );
					int keySize = extractKeySize( keyValueSize );
					bool dead = extractTombstone( keyValueSize );

					if ( dead )
					{
						 deadKeysOffset.push( currentOffset );
					}
					else
					{
						 aliveKeysOffset.push( currentOffset );
					}
					currentOffset += keySize + getOverhead( keySize, 0 );
			  }
		 }

		 // NOTE: Does NOT update keyCount
		 private void MoveKeysAndChildren( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toPos, int count, bool includeLeftMostChild )
		 {
			  if ( count == 0 && !includeLeftMostChild )
			  {
					// Nothing to move
					return;
			  }

			  // All children
			  // This will also copy key offsets but those will be overwritten below.
			  int childFromOffset = includeLeftMostChild ? ChildOffset( fromPos ) : ChildOffset( fromPos + 1 );
			  int childToOffset = ChildOffset( fromPos + count ) + ChildSize();
			  int lengthInBytes = childToOffset - childFromOffset;
			  int targetOffset = includeLeftMostChild ? ChildOffset( 0 ) : ChildOffset( 1 );
			  fromCursor.CopyTo( childFromOffset, toCursor, targetOffset, lengthInBytes );

			  // Move actual keys and update pointers
			  int toAllocOffset = GetAllocOffset( toCursor );
			  int firstAllocOffset = toAllocOffset;
			  for ( int i = 0; i < count; i++, toPos++ )
			  {
					// Key
					toAllocOffset = TransferRawKey( fromCursor, fromPos + i, toCursor, toAllocOffset );
					toCursor.Offset = KeyPosOffsetInternal( toPos );
					putKeyOffset( toCursor, toAllocOffset );
			  }
			  SetAllocOffset( toCursor, toAllocOffset );

			  // Update deadspace
			  int deadSpace = GetDeadSpace( fromCursor );
			  int totalMovedBytes = firstAllocOffset - toAllocOffset;
			  SetDeadSpace( fromCursor, deadSpace + totalMovedBytes );

			  // Zero pad empty area
			  ZeroPad( fromCursor, childFromOffset, lengthInBytes );
		 }

		 private void ZeroPad( PageCursor fromCursor, int fromOffset, int lengthInBytes )
		 {
			  fromCursor.Offset = fromOffset;
			  fromCursor.PutBytes( lengthInBytes, ( sbyte ) 0 );
		 }

		 private int TransferRawKey( PageCursor fromCursor, int fromPos, PageCursor toCursor, int toAllocOffset )
		 {
			  // What to copy?
			  PlaceCursorAtActualKey( fromCursor, fromPos, INTERNAL );
			  int fromKeyOffset = fromCursor.Offset;
			  int keySize = extractKeySize( readKeyValueSize( fromCursor ) );

			  // Copy
			  int toCopy = getOverhead( keySize, 0 ) + keySize;
			  toAllocOffset -= toCopy;
			  fromCursor.CopyTo( fromKeyOffset, toCursor, toAllocOffset, toCopy );

			  // Put tombstone
			  fromCursor.Offset = fromKeyOffset;
			  putTombstone( fromCursor );
			  return toAllocOffset;
		 }

		 /// <seealso cref= TreeNodeDynamicSize#splitPosInLeaf(PageCursor, int, Object, Object, int, double) </seealso>
		 private int SplitPosInternal( PageCursor cursor, int insertPos, KEY newKey, int keyCountAfterInsert, double ratioToKeepInLeftOnSplit )
		 {
			  int targetLeftSpace = ( int )( this._totalSpace * ratioToKeepInLeftOnSplit );
			  int splitPos = 0;
			  int currentPos = 0;
			  int accumulatedLeftSpace = ChildSize(); // Leftmost child will always be included in left side
			  int currentDelta = Math.Abs( accumulatedLeftSpace - targetLeftSpace );
			  int prevDelta;
			  int spaceOfNewKeyAndChild = TotalSpaceOfKeyChild( newKey );
			  int totalSpaceIncludingNewKeyAndChild = TotalActiveSpace( cursor, keyCountAfterInsert - 1, INTERNAL ) + spaceOfNewKeyAndChild;
			  bool includedNew = false;
			  bool prevPosPossible;
			  bool thisPosPossible = false;

			  do
			  {
					prevPosPossible = thisPosPossible;

					// We may come closer to split by keeping one more in left
					int space;
					if ( currentPos == insertPos & !includedNew )
					{
						 space = TotalSpaceOfKeyChild( newKey );
						 includedNew = true;
						 currentPos--;
					}
					else
					{
						 space = TotalSpaceOfKeyChild( cursor, currentPos );
					}
					accumulatedLeftSpace += space;
					prevDelta = currentDelta;
					currentDelta = Math.Abs( accumulatedLeftSpace - targetLeftSpace );
					splitPos++;
					currentPos++;
					thisPosPossible = totalSpaceIncludingNewKeyAndChild - accumulatedLeftSpace < _totalSpace;
			  } while ( ( currentDelta < prevDelta && splitPos < keyCountAfterInsert && accumulatedLeftSpace < _totalSpace ) || !thisPosPossible );
			  if ( prevPosPossible )
			  {
					splitPos--; // Step back to the pos that most equally divide the available space in two
			  }
			  return splitPos;
		 }

		 /// <summary>
		 /// Calculates a valid and as optimal as possible position where to split a leaf if inserting a key overflows, trying to come as close as possible to
		 /// ratioToKeepInLeftOnSplit. There are a couple of goals/conditions which drives the search for it:
		 /// <ul>
		 ///     <li>The returned position will be one where the keys ending up in the left and right leaves respectively are guaranteed to fit.</li>
		 ///     <li>Out of those possible positions the one will be selected which leaves left node filled with with space closest to "targetLeftSpace".</li>
		 /// </ul>
		 /// 
		 /// We loop over an imaginary range of keys where newKey has already been inserted at insertPos in the current node. splitPos point to position in the
		 /// imaginary range while currentPos point to the node. In the loop we "move" splitPos from left to right, accumulating space for left node as we go and
		 /// calculate delta towards targetLeftSpace. We want to continue loop as long as:
		 /// <ul>
		 ///     <li>We are still moving closer to optimal divide (currentDelta < prevDelta) and</li>
		 ///     <li>We are still inside end of range (splitPost < keyCountAfterInsert) and</li>
		 ///     <li>We have not accumulated to much space to fit in left node (accumulatedLeftSpace <= totalSpace).</li>
		 /// </ul>
		 /// But we also have to force loop to continue if the current position does not give a possible divide because right node will be given to much data to
		 /// fit (!thisPosPossible). Exiting loop means we've gone too far and thus we move one step back after loop, but only if the previous position gave us a
		 /// possible divide.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to use for reading sizes of existing entries. </param>
		 /// <param name="insertPos"> the pos which the new key will be inserted at. </param>
		 /// <param name="newKey"> key to be inserted. </param>
		 /// <param name="newValue"> value to be inserted. </param>
		 /// <param name="keyCountAfterInsert"> key count including the new key. </param>
		 /// <param name="ratioToKeepInLeftOnSplit"> What ratio of keys to try and keep in left node, 1=keep as much as possible, 0=move as much as possible to right </param>
		 /// <returns> the pos where to split. </returns>
		 private int SplitPosInLeaf( PageCursor cursor, int insertPos, KEY newKey, VALUE newValue, int keyCountAfterInsert, double ratioToKeepInLeftOnSplit )
		 {
			  int targetLeftSpace = ( int )( this._totalSpace * ratioToKeepInLeftOnSplit );
			  int splitPos = 0;
			  int currentPos = 0;
			  int accumulatedLeftSpace = 0;
			  int currentDelta = targetLeftSpace;
			  int prevDelta;
			  int spaceOfNewKey = TotalSpaceOfKeyValue( newKey, newValue );
			  int totalSpaceIncludingNewKey = TotalActiveSpace( cursor, keyCountAfterInsert - 1, LEAF ) + spaceOfNewKey;
			  bool includedNew = false;
			  bool prevPosPossible;
			  bool thisPosPossible = false;

			  if ( totalSpaceIncludingNewKey > _totalSpace * 2 )
			  {
					throw new System.InvalidOperationException( format( "There's not enough space to insert new key, even when splitting the leaf. Space needed:%d, max space allowed:%d", totalSpaceIncludingNewKey, _totalSpace * 2 ) );
			  }

			  do
			  {
					prevPosPossible = thisPosPossible;

					// We may come closer to split by keeping one more in left
					int currentSpace;
					if ( currentPos == insertPos & !includedNew )
					{
						 currentSpace = spaceOfNewKey;
						 includedNew = true;
						 currentPos--;
					}
					else
					{
						 currentSpace = TotalSpaceOfKeyValue( cursor, currentPos );
					}
					accumulatedLeftSpace += currentSpace;
					prevDelta = currentDelta;
					currentDelta = Math.Abs( accumulatedLeftSpace - targetLeftSpace );
					currentPos++;
					splitPos++;
					thisPosPossible = totalSpaceIncludingNewKey - accumulatedLeftSpace <= _totalSpace;
			  } while ( ( currentDelta < prevDelta && splitPos < keyCountAfterInsert && accumulatedLeftSpace <= _totalSpace ) || !thisPosPossible );
			  // If previous position is possible then step back one pos since it divides the space most equally
			  if ( prevPosPossible )
			  {
					splitPos--;
			  }
			  return splitPos;
		 }

		 private int TotalActiveSpace( PageCursor cursor, int keyCount, Type type )
		 {
			  int deadSpace = GetDeadSpace( cursor );
			  int allocSpace = GetAllocSpace( cursor, keyCount, type );
			  return _totalSpace - deadSpace - allocSpace;
		 }

		 private int TotalSpaceOfKeyValue( KEY key, VALUE value )
		 {
			  int keySize = Layout.keySize( key );
			  int valueSize = Layout.valueSize( value );
			  return BytesKeyOffset() + getOverhead(keySize, valueSize) + keySize + valueSize;
		 }

		 private int TotalSpaceOfKeyChild( KEY key )
		 {
			  int keySize = Layout.keySize( key );
			  return BytesKeyOffset() + getOverhead(keySize, 0) + ChildSize() + keySize;
		 }

		 private int TotalSpaceOfKeyValue( PageCursor cursor, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, LEAF );
			  long keyValueSize = readKeyValueSize( cursor );
			  int keySize = extractKeySize( keyValueSize );
			  int valueSize = extractValueSize( keyValueSize );
			  return BytesKeyOffset() + getOverhead(keySize, valueSize) + keySize + valueSize;
		 }

		 private int TotalSpaceOfKeyChild( PageCursor cursor, int pos )
		 {
			  PlaceCursorAtActualKey( cursor, pos, INTERNAL );
			  int keySize = extractKeySize( readKeyValueSize( cursor ) );
			  return BytesKeyOffset() + getOverhead(keySize, 0) + ChildSize() + keySize;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void setAllocOffset(org.neo4j.io.pagecache.PageCursor cursor, int allocOffset)
		 internal virtual void SetAllocOffset( PageCursor cursor, int allocOffset )
		 {
			  PageCursorUtil.PutUnsignedShort( cursor, _bytePosAllocoffset, allocOffset );
		 }

		 internal virtual int GetAllocOffset( PageCursor cursor )
		 {
			  return PageCursorUtil.GetUnsignedShort( cursor, _bytePosAllocoffset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void setDeadSpace(org.neo4j.io.pagecache.PageCursor cursor, int deadSpace)
		 internal virtual void SetDeadSpace( PageCursor cursor, int deadSpace )
		 {
			  putUnsignedShort( cursor, _bytePosDeadspace, deadSpace );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int getDeadSpace(org.neo4j.io.pagecache.PageCursor cursor)
		 internal virtual int GetDeadSpace( PageCursor cursor )
		 {
			  return PageCursorUtil.GetUnsignedShort( cursor, _bytePosDeadspace );
		 }

		 private void PlaceCursorAtActualKey( PageCursor cursor, int pos, Type type )
		 {
			  // Set cursor to correct place in offset array
			  int keyPosOffset = keyPosOffset( pos, type );
			  cursor.Offset = keyPosOffset;

			  // Read actual offset to key
			  int keyOffset = readKeyOffset( cursor );

			  // Verify offset is reasonable
			  if ( keyOffset >= PageSize || keyOffset < HeaderLengthDynamic )
			  {
					cursor.CursorException = format( "Tried to read key on offset=%d, headerLength=%d, pageSize=%d, pos=%d", keyOffset, HeaderLengthDynamic, PageSize, pos );
					return;
			  }

			  // Set cursor to actual offset
			  cursor.Offset = keyOffset;
		 }

		 private void ReadUnreliableKeyValueSize( PageCursor cursor, int keySize, int valueSize, long keyValueSize, int pos )
		 {
			  cursor.CursorException = format( "Read unreliable key, id=%d, keySize=%d, valueSize=%d, keyValueSizeCap=%d, keyHasTombstone=%b, pos=%d", cursor.CurrentPageId, keySize, valueSize, KeyValueSizeCap(), extractTombstone(keyValueSize), pos );
		 }

		 private bool KeyValueSizeTooLarge( int keySize, int valueSize )
		 {
			  return keySize + valueSize > KeyValueSizeCap();
		 }

		 private int KeyPosOffset( int pos, Type type )
		 {
			  if ( type == LEAF )
			  {
					return KeyPosOffsetLeaf( pos );
			  }
			  else
			  {
					return KeyPosOffsetInternal( pos );
			  }
		 }

		 private int KeyPosOffsetLeaf( int pos )
		 {
			  return HeaderLengthDynamic + pos * BytesKeyOffset();
		 }

		 private int KeyPosOffsetInternal( int pos )
		 {
			  // header + childPointer + pos * (keyPosOffsetSize + childPointer)
			  return HeaderLengthDynamic + ChildSize() + pos * KeyChildSize();
		 }

		 private int KeyChildSize()
		 {
			  return BytesKeyOffset() + SizePageReference;
		 }

		 private int ChildSize()
		 {
			  return SizePageReference;
		 }

		 private static int BytesKeyOffset()
		 {
			  return SIZE_OFFSET;
		 }

		 private static int BytesPageOffset()
		 {
			  return SIZE_OFFSET;
		 }

		 public override string ToString()
		 {
			  return "TreeNodeDynamicSize[pageSize:" + PageSize + ", keyValueSizeCap:" + KeyValueSizeCap() + "]";
		 }

		 private string AsString( PageCursor cursor, bool includeValue, bool includeAllocSpace, long stableGeneration, long unstableGeneration )
		 {
			  int currentOffset = cursor.Offset;
			  // [header] <- dont care
			  // LEAF:     [allocOffset=][child0,key0*,child1,...][keySize|key][keySize|key]
			  // INTERNAL: [allocOffset=][key0*,key1*,...][offset|keySize|valueSize|key][keySize|valueSize|key]

			  Type type = IsInternal( cursor ) ? INTERNAL : LEAF;

			  // HEADER
			  int allocOffset = GetAllocOffset( cursor );
			  int deadSpace = GetDeadSpace( cursor );
			  string additionalHeader = "{" + cursor.CurrentPageId + "} [allocOffset=" + allocOffset + " deadSpace=" + deadSpace + "] ";

			  // OFFSET ARRAY
			  string offsetArray = ReadOffsetArray( cursor, stableGeneration, unstableGeneration, type );

			  // ALLOC SPACE
			  string allocSpace = "";
			  if ( includeAllocSpace )
			  {
					allocSpace = ReadAllocSpace( cursor, allocOffset, type );
			  }

			  // KEYS
			  KEY readKey = Layout.newKey();
			  VALUE readValue = Layout.newValue();
			  StringJoiner keys = new StringJoiner( " " );
			  cursor.Offset = allocOffset;
			  while ( cursor.Offset < cursor.CurrentPageSize )
			  {
					StringJoiner singleKey = new StringJoiner( "|" );
					singleKey.add( Convert.ToString( cursor.Offset ) );
					long keyValueSize = readKeyValueSize( cursor );
					int keySize = extractKeySize( keyValueSize );
					int valueSize = 0;
					if ( type == LEAF )
					{
						 valueSize = extractValueSize( keyValueSize );
					}
					if ( DynamicSizeUtil.ExtractTombstone( keyValueSize ) )
					{
						 singleKey.add( "X" );
					}
					else
					{
						 singleKey.add( "_" );
					}
					Layout.readKey( cursor, readKey, keySize );
					if ( type == LEAF )
					{
						 Layout.readValue( cursor, readValue, valueSize );
					}
					singleKey.add( Convert.ToString( keySize ) );
					if ( type == LEAF && includeValue )
					{
						 singleKey.add( Convert.ToString( valueSize ) );
					}
					singleKey.add( readKey.ToString() );
					if ( type == LEAF && includeValue )
					{
						 singleKey.add( readValue.ToString() );
					}
					keys.add( singleKey.ToString() );
			  }

			  cursor.Offset = currentOffset;
			  return additionalHeader + offsetArray + " " + allocSpace + " " + keys;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Override void printNode(org.neo4j.io.pagecache.PageCursor cursor, boolean includeValue, boolean includeAllocSpace, long stableGeneration, long unstableGeneration)
		 internal override void PrintNode( PageCursor cursor, bool includeValue, bool includeAllocSpace, long stableGeneration, long unstableGeneration )
		 {
			  Console.WriteLine( AsString( cursor, includeValue, includeAllocSpace, stableGeneration, unstableGeneration ) );
		 }

		 internal override string CheckMetaConsistency( PageCursor cursor, int keyCount, Type type, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  // Reminder: Header layout
			  // TotalSpace  |----------------------------------------|
			  // ActiveSpace |-----------|   +    |---------|  + |----|
			  // DeadSpace                                  |----|
			  // AllocSpace              |--------|
			  // AllocOffset                      v
			  //     [Header][OffsetArray]........[_________,XXXX,____] (_ = alive key, X = dead key)

			  long nodeId = cursor.CurrentPageId;
			  StringJoiner joiner = new StringJoiner( ", ", "Meta data for tree node is inconsistent, id=" + nodeId + ": ", "" );
			  bool hasInconsistency = false;

			  // Verify allocOffset >= offsetArray
			  int allocOffset = GetAllocOffset( cursor );
			  int offsetArray = KeyPosOffset( keyCount, type );
			  if ( allocOffset < offsetArray )
			  {
					hasInconsistency = true;
					joiner.add( format( "Overlap between offsetArray and allocSpace, offsetArray=%d, allocOffset=%d", offsetArray, allocOffset ) );
			  }

			  // If keyCount is unreasonable we will likely go out of bounds in those checks
			  if ( ReasonableKeyCount( keyCount ) )
			  {
					// Verify activeSpace + deadSpace + allocSpace == totalSpace
					int activeSpace = TotalActiveSpaceRaw( cursor, keyCount, type );
					int deadSpace = GetDeadSpace( cursor );
					int allocSpace = GetAllocSpace( cursor, keyCount, type );
					if ( activeSpace + deadSpace + allocSpace != _totalSpace )
					{
						 hasInconsistency = true;
						 joiner.add( format( "Space areas did not sum to total space; activeSpace=%d, deadSpace=%d, allocSpace=%d, totalSpace=%d", activeSpace, deadSpace, allocSpace, _totalSpace ) );
					}

					// Verify no overlap between alloc space and active keys
					int lowestActiveKeyOffset = lowestActiveKeyOffset( cursor, keyCount, type );
					if ( lowestActiveKeyOffset < allocOffset )
					{
						 hasInconsistency = true;
						 joiner.add( format( "Overlap between allocSpace and active keys, allocOffset=%d, lowestActiveKeyOffset=%d", allocOffset, lowestActiveKeyOffset ) );
					}
			  }

			  if ( allocOffset < PageSize && allocOffset >= 0 )
			  {
					// Verify allocOffset point at start of key
					cursor.Offset = allocOffset;
					long keyValueAtAllocOffset = readKeyValueSize( cursor );
					if ( keyValueAtAllocOffset == 0 )
					{
						 hasInconsistency = true;
						 joiner.add( format( "Pointer to allocSpace is misplaced, it should point to start of key, allocOffset=%d", allocOffset ) );
					}
			  }

			  // Report inconsistencies as cursor exception
			  if ( hasInconsistency )
			  {
					return joiner.ToString();
			  }
			  return "";
		 }

		 private int LowestActiveKeyOffset( PageCursor cursor, int keyCount, Type type )
		 {
			  int lowestOffsetSoFar = PageSize;
			  for ( int pos = 0; pos < keyCount; pos++ )
			  {
					// Set cursor to correct place in offset array
					int keyPosOffset = keyPosOffset( pos, type );
					cursor.Offset = keyPosOffset;

					// Read actual offset to key
					int keyOffset = readKeyOffset( cursor );
					lowestOffsetSoFar = Math.Min( lowestOffsetSoFar, keyOffset );
			  }
			  return lowestOffsetSoFar;
		 }

		 // Calculated by reading data instead of extrapolate from allocSpace and deadSpace
		 private int TotalActiveSpaceRaw( PageCursor cursor, int keyCount, Type type )
		 {
			  // Offset array
			  int offsetArrayStart = HeaderLengthDynamic;
			  int offsetArrayEnd = KeyPosOffset( keyCount, type );
			  int offsetArraySize = offsetArrayEnd - offsetArrayStart;

			  // Alive keys
			  int aliveKeySize = 0;
			  int nextKeyOffset = GetAllocOffset( cursor );
			  while ( nextKeyOffset < PageSize )
			  {
					cursor.Offset = nextKeyOffset;
					long keyValueSize = readKeyValueSize( cursor );
					int keySize = extractKeySize( keyValueSize );
					int valueSize = extractValueSize( keyValueSize );
					bool tombstone = extractTombstone( keyValueSize );
					if ( !tombstone )
					{
						 aliveKeySize += getOverhead( keySize, valueSize ) + keySize + valueSize;
					}
					nextKeyOffset = cursor.Offset + keySize + valueSize;
			  }
			  return offsetArraySize + aliveKeySize;
		 }

		 private string ReadAllocSpace( PageCursor cursor, int allocOffset, Type type )
		 {
			  int keyCount = keyCount( cursor );
			  int endOfOffsetArray = type == INTERNAL ? KeyPosOffsetInternal( keyCount ) : KeyPosOffsetLeaf( keyCount );
			  cursor.Offset = endOfOffsetArray;
			  int bytesToRead = allocOffset - endOfOffsetArray;
			  sbyte[] allocSpace = new sbyte[bytesToRead];
			  cursor.GetBytes( allocSpace );
			  foreach ( sbyte b in allocSpace )
			  {
					if ( b != 0 )
					{
						 return "v" + endOfOffsetArray + ">" + bytesToRead + "|" + Arrays.ToString( allocSpace );
					}
			  }
			  return "v" + endOfOffsetArray + ">" + bytesToRead + "|[0...]";
		 }

		 private string ReadOffsetArray( PageCursor cursor, long stableGeneration, long unstableGeneration, Type type )
		 {
			  int keyCount = keyCount( cursor );
			  StringJoiner offsetArray = new StringJoiner( " " );
			  for ( int i = 0; i < keyCount; i++ )
			  {
					if ( type == INTERNAL )
					{
						 long childPointer = GenerationSafePointerPair.Pointer( ChildAt( cursor, i, stableGeneration, unstableGeneration ) );
						 offsetArray.add( "/" + Convert.ToString( childPointer ) + "\\" );
					}
					cursor.Offset = KeyPosOffset( i, type );
					offsetArray.add( Convert.ToString( DynamicSizeUtil.ReadKeyOffset( cursor ) ) );
			  }
			  if ( type == INTERNAL )
			  {
					long childPointer = GenerationSafePointerPair.Pointer( ChildAt( cursor, keyCount, stableGeneration, unstableGeneration ) );
					offsetArray.add( "/" + Convert.ToString( childPointer ) + "\\" );
			  }
			  return offsetArray.ToString();
		 }
	}

}