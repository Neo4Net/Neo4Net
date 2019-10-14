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

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Tree state is defined as top level tree meta data which changes as the tree and its constructs changes, such as:
	/// <ul>
	/// <li>stable/unstable generation numbers</li>
	/// <li>root id, the page id containing the root of the tree</li>
	/// <li>last id, the page id which is the highest allocated in the store</li>
	/// <li>pointers into free-list (page id + offset)</li>
	/// </ul>
	/// This class also knows how to
	/// <seealso cref="write(PageCursor, long, long, long, long, long, long, long, int, int, bool) write"/> and
	/// <seealso cref="read(PageCursor) read"/> tree state to and from a <seealso cref="PageCursor"/>, although doesn't care where
	/// in the store that is.
	/// </summary>
	internal class TreeState
	{
		 /// <summary>
		 /// Size of one set of tree-state fields.
		 /// </summary>
		 private static readonly int _treeStateFieldsSize = Integer.BYTES * 2 + Long.BYTES + Long.BYTES + Long.BYTES + Long.BYTES + Long.BYTES + Long.BYTES + Long.BYTES + Byte.BYTES; // clean

		 /// <summary>
		 /// Size of a tree-state altogether, which consists of two sets of tree-state fields.
		 /// </summary>
		 internal static readonly int Size = _treeStateFieldsSize * 2;

		 private const sbyte CLEAN_BYTE = 0x01;
		 private const sbyte DIRTY_BYTE = 0x00;

		 /// <summary>
		 /// Page id this tree state has been read from.
		 /// </summary>
		 private readonly long _pageId;

		 /// <summary>
		 /// Stable generation of the tree.
		 /// </summary>
		 private readonly long _stableGeneration;

		 /// <summary>
		 /// Unstable generation of the tree.
		 /// </summary>
		 private readonly long _unstableGeneration;

		 /// <summary>
		 /// Page id which is the root of the tree.
		 /// </summary>
		 private readonly long _rootId;

		 /// <summary>
		 /// Generation of <seealso cref="rootId"/>.
		 /// </summary>
		 private readonly long _rootGeneration;

		 /// <summary>
		 /// Highest allocated page id in the store. This id may not be in use currently and cannot decrease
		 /// since <seealso cref="PageCache"/> doesn't allow shrinking files.
		 /// </summary>
		 private readonly long _lastId;

		 /// <summary>
		 /// Page id to write new released tree node ids into.
		 /// </summary>
		 private readonly long _freeListWritePageId;

		 /// <summary>
		 /// Page id to read released tree node ids from, when acquiring ids.
		 /// </summary>
		 private readonly long _freeListReadPageId;

		 /// <summary>
		 /// Offset in page <seealso cref="freeListWritePageId"/> to write new released tree node ids at.
		 /// </summary>
		 private readonly int _freeListWritePos;

		 /// <summary>
		 /// Offset in page <seealso cref="freeListReadPageId"/> to read released tree node ids from, when acquiring ids.
		 /// </summary>
		 private readonly int _freeListReadPos;

		 /// <summary>
		 /// Due to writing with potential concurrent page flushing tree state is written twice, the second
		 /// state acting as checksum. If both states match this variable should be set to {@code true},
		 /// otherwise to {@code false}.
		 /// </summary>
		 private bool _valid;

		 /// <summary>
		 /// Is tree clean or dirty. Clean means it was closed without any non-checkpointed changes.
		 /// </summary>
		 private readonly bool _clean;

		 internal TreeState( long pageId, long stableGeneration, long unstableGeneration, long rootId, long rootGeneration, long lastId, long freeListWritePageId, long freeListReadPageId, int freeListWritePos, int freeListReadPos, bool clean, bool valid )
		 {
			  this._pageId = pageId;
			  this._stableGeneration = stableGeneration;
			  this._unstableGeneration = unstableGeneration;
			  this._rootId = rootId;
			  this._rootGeneration = rootGeneration;
			  this._lastId = lastId;
			  this._freeListWritePageId = freeListWritePageId;
			  this._freeListReadPageId = freeListReadPageId;
			  this._freeListWritePos = freeListWritePos;
			  this._freeListReadPos = freeListReadPos;
			  this._clean = clean;
			  this._valid = valid;
		 }

		 internal virtual long PageId()
		 {
			  return _pageId;
		 }

		 internal virtual long StableGeneration()
		 {
			  return _stableGeneration;
		 }

		 internal virtual long UnstableGeneration()
		 {
			  return _unstableGeneration;
		 }

		 internal virtual long RootId()
		 {
			  return _rootId;
		 }

		 internal virtual long RootGeneration()
		 {
			  return _rootGeneration;
		 }

		 internal virtual long LastId()
		 {
			  return _lastId;
		 }

		 internal virtual long FreeListWritePageId()
		 {
			  return _freeListWritePageId;
		 }

		 internal virtual long FreeListReadPageId()
		 {
			  return _freeListReadPageId;
		 }

		 internal virtual int FreeListWritePos()
		 {
			  return _freeListWritePos;
		 }

		 internal virtual int FreeListReadPos()
		 {
			  return _freeListReadPos;
		 }

		 internal virtual bool Valid
		 {
			 get
			 {
				  return _valid;
			 }
		 }

		 /// <summary>
		 /// Writes provided tree state to {@code cursor} at its current offset. Two versions of the state
		 /// are written after each other, the second one acting as checksum for the first, see <seealso cref="valid"/> field.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write into, at its current offset. </param>
		 /// <param name="stableGeneration"> stable generation. </param>
		 /// <param name="unstableGeneration"> unstable generation. </param>
		 /// <param name="rootId"> root id. </param>
		 /// <param name="rootGeneration"> root generation. </param>
		 /// <param name="lastId"> last id. </param>
		 /// <param name="freeListWritePageId"> free-list page id to write released ids into. </param>
		 /// <param name="freeListReadPageId"> free-list page id to read released ids from. </param>
		 /// <param name="freeListWritePos"> offset into free-list write page id to write released ids into. </param>
		 /// <param name="freeListReadPos"> offset into free-list read page id to read released ids from. </param>
		 /// <param name="clean"> is tree clean or dirty </param>
		 internal static void Write( PageCursor cursor, long stableGeneration, long unstableGeneration, long rootId, long rootGeneration, long lastId, long freeListWritePageId, long freeListReadPageId, int freeListWritePos, int freeListReadPos, bool clean )
		 {
			  GenerationSafePointer.AssertGenerationOnWrite( stableGeneration );
			  GenerationSafePointer.AssertGenerationOnWrite( unstableGeneration );

			  WriteStateOnce( cursor, stableGeneration, unstableGeneration, rootId, rootGeneration, lastId, freeListWritePageId, freeListReadPageId, freeListWritePos, freeListReadPos, clean ); // Write state
			  WriteStateOnce( cursor, stableGeneration, unstableGeneration, rootId, rootGeneration, lastId, freeListWritePageId, freeListReadPageId, freeListWritePos, freeListReadPos, clean ); // Write checksum
		 }

		 /// <summary>
		 /// Reads tree state from {@code cursor} at its current offset. If checksum matches then <seealso cref="valid"/>
		 /// is set to {@code true}, otherwise {@code false}.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read tree state from, at its current offset. </param>
		 /// <returns> <seealso cref="TreeState"/> instance containing read tree state. </returns>
		 internal static TreeState Read( PageCursor cursor )
		 {
			  TreeState state = ReadStateOnce( cursor );
			  TreeState checksumState = ReadStateOnce( cursor );

			  bool valid = state.Equals( checksumState );

			  bool isEmpty = state.Empty;
			  valid &= !isEmpty;

			  return state.setValid( valid );
		 }

		 private TreeState setValid( bool valid )
		 {
			  this._valid = valid;
			  return this;
		 }

		 internal virtual bool Empty
		 {
			 get
			 {
				  return _stableGeneration == 0L && _unstableGeneration == 0L && _rootId == 0L && _lastId == 0L && _freeListWritePageId == 0L && _freeListReadPageId == 0L && _freeListWritePos == 0 && _freeListReadPos == 0;
			 }
		 }

		 private static TreeState ReadStateOnce( PageCursor cursor )
		 {
			  long pageId = cursor.CurrentPageId;
			  long stableGeneration = cursor.Int & GenerationSafePointer.GENERATION_MASK;
			  long unstableGeneration = cursor.Int & GenerationSafePointer.GENERATION_MASK;
			  long rootId = cursor.Long;
			  long rootGeneration = cursor.Long;
			  long lastId = cursor.Long;
			  long freeListWritePageId = cursor.Long;
			  long freeListReadPageId = cursor.Long;
			  int freeListWritePos = cursor.Int;
			  int freeListReadPos = cursor.Int;
			  bool clean = cursor.Byte == CLEAN_BYTE;
			  return new TreeState( pageId, stableGeneration, unstableGeneration, rootId, rootGeneration, lastId, freeListWritePageId, freeListReadPageId, freeListWritePos, freeListReadPos, clean, true );
		 }

		 private static void WriteStateOnce( PageCursor cursor, long stableGeneration, long unstableGeneration, long rootId, long rootGeneration, long lastId, long freeListWritePageId, long freeListReadPageId, int freeListWritePos, int freeListReadPos, bool clean )
		 {
			  cursor.PutInt( ( int ) stableGeneration );
			  cursor.PutInt( ( int ) unstableGeneration );
			  cursor.PutLong( rootId );
			  cursor.PutLong( rootGeneration );
			  cursor.PutLong( lastId );
			  cursor.PutLong( freeListWritePageId );
			  cursor.PutLong( freeListReadPageId );
			  cursor.PutInt( freeListWritePos );
			  cursor.PutInt( freeListReadPos );
			  cursor.PutByte( clean ? CLEAN_BYTE : DIRTY_BYTE );
		 }

		 public override string ToString()
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("pageId=%d, stableGeneration=%d, unstableGeneration=%d, rootId=%d, rootGeneration=%d, " + "lastId=%d, freeListWritePageId=%d, freeListReadPageId=%d, freeListWritePos=%d, freeListReadPos=%d, " + "clean=%b, valid=%b", pageId, stableGeneration, unstableGeneration, rootId, rootGeneration, lastId, freeListWritePageId, freeListReadPageId, freeListWritePos, freeListReadPos, clean, valid);
			  return string.Format( "pageId=%d, stableGeneration=%d, unstableGeneration=%d, rootId=%d, rootGeneration=%d, " + "lastId=%d, freeListWritePageId=%d, freeListReadPageId=%d, freeListWritePos=%d, freeListReadPos=%d, " + "clean=%b, valid=%b", _pageId, _stableGeneration, _unstableGeneration, _rootId, _rootGeneration, _lastId, _freeListWritePageId, _freeListReadPageId, _freeListWritePos, _freeListReadPos, _clean, _valid );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  TreeState treeState = ( TreeState ) o;
			  return _pageId == treeState._pageId && _stableGeneration == treeState._stableGeneration && _unstableGeneration == treeState._unstableGeneration && _rootId == treeState._rootId && _rootGeneration == treeState._rootGeneration && _lastId == treeState._lastId && _freeListWritePageId == treeState._freeListWritePageId && _freeListReadPageId == treeState._freeListReadPageId && _freeListWritePos == treeState._freeListWritePos && _freeListReadPos == treeState._freeListReadPos && _clean == treeState._clean && _valid == treeState._valid;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _pageId, _stableGeneration, _unstableGeneration, _rootId, _rootGeneration, _lastId, _freeListWritePageId, _freeListReadPageId, _freeListWritePos, _freeListReadPos, _clean, _valid );
		 }

		 public virtual bool Clean
		 {
			 get
			 {
				  return this._clean;
			 }
		 }
	}

}