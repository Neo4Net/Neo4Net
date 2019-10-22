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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.transaction.log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.IOUtils.closeAllUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.concurrent.Runnables.runAll;

	/// <summary>
	/// Not thread safe, except for <seealso cref="count()"/> which does not support calls concurrent with <seealso cref="add(object)"/>.
	/// 
	/// Storage that store <seealso cref="ENTRY entries"/> in a file by simply appending them.
	/// Entries can be read back, in the order they where added, through a <seealso cref="CURSOR"/>.
	/// This storage is useful when we don't want to hold all entries in memory.
	/// 
	/// Extending classes are responsible for serializing and deserializing entries.
	/// 
	/// On close, file will be deleted but provided <seealso cref="ByteBufferFactory"/> will not be closed.
	/// </summary>
	/// @param <ENTRY> Type of entry we are storing. </param>
	/// @param <CURSOR> Cursor type responsible for deserializing what we have stored. </param>
	public abstract class SimpleEntryStorage<ENTRY, CURSOR> : System.IDisposable
	{
		 internal static readonly int TypeSize = Byte.BYTES;
		 internal const sbyte STOP_TYPE = -1;
		 private static readonly sbyte[] _noEntries = new sbyte[] { STOP_TYPE };
		 private readonly File _file;
		 private readonly FileSystemAbstraction _fs;
		 private readonly int _blockSize;
		 private readonly ByteBufferFactory.Allocator _byteBufferFactory;

		 // Resources allocated lazily upon add
		 private bool _allocated;
		 private ByteBuffer _buffer;
		 private ByteArrayPageCursor _pageCursor;
		 private StoreChannel _storeChannel;

		 private volatile long _count;

		 internal SimpleEntryStorage( FileSystemAbstraction fs, File file, ByteBufferFactory.Allocator byteBufferFactory, int blockSize )
		 {
			  this._fs = fs;
			  this._file = file;
			  this._byteBufferFactory = byteBufferFactory;
			  this._blockSize = blockSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(ENTRY entry) throws java.io.IOException
		 internal virtual void Add( ENTRY entry )
		 {
			  AllocateResources();
			  Add( entry, _pageCursor );
			  // a single thread, and the same thread every time, increments this count
			  _count++;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CURSOR reader() throws java.io.IOException
		 internal virtual CURSOR Reader()
		 {
			  if ( !_allocated )
			  {
					return Reader( new ByteArrayPageCursor( _noEntries ) );
			  }

			  // Reuse the existing buffer because we're not writing while reading anyway
			  _buffer.clear();
			  ReadAheadChannel<StoreChannel> channel = new ReadAheadChannel<StoreChannel>( _fs.open( _file, OpenMode.READ ), _buffer );
			  PageCursor pageCursor = new ReadableChannelPageCursor( channel );
			  return Reader( pageCursor );
		 }

		 internal virtual long Count()
		 {
			  return _count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void doneAdding() throws java.io.IOException
		 internal virtual void DoneAdding()
		 {
			  if ( !_allocated )
			  {
					return;
			  }
			  if ( _buffer.remaining() < TypeSize )
			  {
					Flush();
			  }
			  _pageCursor.putByte( STOP_TYPE );
			  Flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _allocated )
			  {
					runAll( "Failed while trying to close " + this.GetType().Name, () => closeAllUnchecked(_pageCursor, _storeChannel), () => _fs.deleteFile(_file) );
			  }
			  else
			  {
					_fs.deleteFile( _file );
			  }
		 }

		 /// <summary>
		 /// DON'T CALL THIS METHOD DIRECTLY. Instead, use <seealso cref="add(object)"/>.
		 /// Write entry to pageCursor. Implementor of this method is responsible for calling <seealso cref="prepareWrite(int)"/> before actually start writing.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void add(ENTRY entry, org.Neo4Net.io.pagecache.PageCursor pageCursor) throws java.io.IOException;
		 internal abstract void Add( ENTRY entry, PageCursor pageCursor );

		 /// <summary>
		 /// DON'T CALL THIS METHOD DIRECTLY. Instead use <seealso cref="reader()"/>.
		 /// Return <seealso cref="CURSOR"/> responsible for deserializing wrapping provided <seealso cref="PageCursor"/>, pointing to head of file.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract CURSOR reader(org.Neo4Net.io.pagecache.PageCursor pageCursor) throws java.io.IOException;
		 internal abstract CURSOR Reader( PageCursor pageCursor );

		 /// <summary>
		 /// DON'T CALL THIS METHOD DIRECTLY. Only used by subclasses.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void prepareWrite(int entrySize) throws java.io.IOException
		 internal virtual void PrepareWrite( int entrySize )
		 {
			  if ( entrySize > _buffer.remaining() )
			  {
					Flush();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flush() throws java.io.IOException
		 private void Flush()
		 {
			  _buffer.flip();
			  _storeChannel.write( _buffer );
			  _buffer.clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void allocateResources() throws java.io.IOException
		 private void AllocateResources()
		 {
			  if ( !_allocated )
			  {
					this._buffer = _byteBufferFactory.allocate( _blockSize );
					this._pageCursor = new ByteArrayPageCursor( _buffer );
					this._storeChannel = _fs.create( _file );
					this._allocated = true;
			  }
		 }
	}

}