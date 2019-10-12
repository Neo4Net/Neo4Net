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
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.get6BLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.getUnsignedInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.put6BLong;

	/// <summary>
	/// Manages the physical format of a free-list node, i.e. how bytes about free-list pages
	/// are represented out in <seealso cref="PageCursor"/>. High-level view of the format:
	/// 
	/// <pre>
	/// [HEADER         ][RELEASED PAGE IDS...                         ]
	/// [NODE TYPE][NEXT][GENERATION,ID][GENERATION,ID][...............]
	/// </pre>
	/// 
	/// A free-list node is a page in the same <seealso cref="org.neo4j.io.pagecache.PagedFile mapped page cache file"/>
	/// as a <seealso cref="TreeNode"/>. They distinguish themselves from one another by a "node type" one-byte header.
	/// </summary>
	internal class FreelistNode
	{
		 private const int _pageIdSize = GenerationSafePointer.POINTER_SIZE;
		 private static readonly int _bytePosNext = TreeNode.BYTE_POS_NODE_TYPE + Byte.BYTES;
		 private static readonly int _headerLength = _bytePosNext + _pageIdSize;
		 private static readonly int _entrySize = GenerationSafePointer.GENERATION_SIZE + _pageIdSize;
		 internal const long NoPageId = TreeNode.NO_NODE_FLAG;

		 private readonly int _maxEntries;

		 internal FreelistNode( int pageSize )
		 {
			  this._maxEntries = ( pageSize - _headerLength ) / _entrySize;
		 }

		 internal static void Initialize( PageCursor cursor )
		 {
			  cursor.PutByte( TreeNode.BYTE_POS_NODE_TYPE, TreeNode.NODE_TYPE_FREE_LIST_NODE );
		 }

		 internal virtual void Write( PageCursor cursor, long unstableGeneration, long pageId, int pos )
		 {
			  if ( pageId == NoPageId )
			  {
					throw new System.ArgumentException( "Tried to write pageId " + pageId + " which means null" );
			  }
			  AssertPos( pos );
			  GenerationSafePointer.AssertGenerationOnWrite( unstableGeneration );
			  cursor.Offset = EntryOffset( pos );
			  cursor.PutInt( ( int ) unstableGeneration );
			  put6BLong( cursor, pageId );
		 }

		 private void AssertPos( int pos )
		 {
			  if ( pos >= _maxEntries )
			  {
					throw new System.ArgumentException( "Pos " + pos + " too big, max entries " + _maxEntries );
			  }
			  if ( pos < 0 )
			  {
					throw new System.ArgumentException( "Negative pos " + pos );
			  }
		 }

		 internal virtual long Read( PageCursor cursor, long stableGeneration, int pos )
		 {
			  return Read( cursor, stableGeneration, pos, GBPTreeGenerationTarget_Fields.NoGenerationTarget );
		 }

		 internal virtual long Read( PageCursor cursor, long stableGeneration, int pos, GBPTreeGenerationTarget target )
		 {
			  AssertPos( pos );
			  cursor.Offset = EntryOffset( pos );
			  long generation = getUnsignedInt( cursor );
			  target( generation );
			  return generation <= stableGeneration ? get6BLong( cursor ) : NoPageId;
		 }

		 private static int EntryOffset( int pos )
		 {
			  return _headerLength + pos * _entrySize;
		 }

		 internal virtual int MaxEntries()
		 {
			  return _maxEntries;
		 }

		 internal static void SetNext( PageCursor cursor, long nextFreelistPage )
		 {
			  cursor.Offset = _bytePosNext;
			  put6BLong( cursor, nextFreelistPage );
		 }

		 internal static long Next( PageCursor cursor )
		 {
			  cursor.Offset = _bytePosNext;
			  return get6BLong( cursor );
		 }
	}

}