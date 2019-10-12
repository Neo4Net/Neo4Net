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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// Reads <seealso cref="BlockEntry"/> from a block in sequential order. Key and value instances are handed out though <seealso cref="key()"/> and <seealso cref="value()"/> but are reused
	/// internally so consumer need to either create a copy or finish all operations on key and value before progressing reader.
	/// Reader will figure out when to stop reading based on Block header wish contains total size of this Block in bytes and total number of entries in Block.
	/// </summary>
	public class BlockEntryReader<KEY, VALUE> : BlockEntryCursor<KEY, VALUE>
	{
		 private readonly long _blockSize;
		 private readonly long _entryCount;
		 private readonly PageCursor _pageCursor;
		 private readonly KEY _key;
		 private readonly VALUE _value;
		 private readonly Layout<KEY, VALUE> _layout;
		 private long _readEntries;

		 internal BlockEntryReader( PageCursor pageCursor, Layout<KEY, VALUE> layout )
		 {
			  this._pageCursor = pageCursor;
			  this._blockSize = pageCursor.Long;
			  this._entryCount = pageCursor.Long;
			  this._layout = layout;
			  this._key = layout.NewKey();
			  this._value = layout.NewValue();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public virtual bool Next()
		 {
			  if ( _readEntries >= _entryCount )
			  {
					return false;
			  }
			  BlockEntry.Read( _pageCursor, _layout, _key, _value );
			  _readEntries++;
			  return true;
		 }

		 public virtual long BlockSize()
		 {
			  return _blockSize;
		 }

		 public virtual long EntryCount()
		 {
			  return _entryCount;
		 }

		 public virtual KEY Key()
		 {
			  return _key;
		 }

		 public virtual VALUE Value()
		 {
			  return _value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _pageCursor.close();
		 }
	}

}