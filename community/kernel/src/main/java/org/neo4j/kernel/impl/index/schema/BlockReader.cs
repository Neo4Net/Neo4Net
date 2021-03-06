﻿/*
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
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Org.Neo4j.Kernel.impl.transaction.log;

	/// <summary>
	/// Hands out <seealso cref="BlockEntryReader"/> for each Block in file in sequential order. The resulting readers have their own <seealso cref="StoreChannel"/> that they read
	/// from and they need to be closed separately, this class does not take responsibility for created readers. This also mean it's safe to close this
	/// <seealso cref="BlockReader"/> even if there are still child readers alive.
	/// </summary>
	public class BlockReader<KEY, VALUE> : System.IDisposable
	{
		 private readonly StoreChannel _channel;
		 private readonly FileSystemAbstraction _fs;
		 private readonly File _file;
		 private readonly Layout<KEY, VALUE> _layout;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BlockReader(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file, org.neo4j.index.internal.gbptree.Layout<KEY,VALUE> layout) throws java.io.IOException
		 internal BlockReader( FileSystemAbstraction fs, File file, Layout<KEY, VALUE> layout )
		 {
			  this._fs = fs;
			  this._file = file;
			  this._layout = layout;
			  this._channel = fs.Open( file, OpenMode.READ );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BlockEntryReader<KEY,VALUE> nextBlock(ByteBuffer blockBuffer) throws java.io.IOException
		 internal virtual BlockEntryReader<KEY, VALUE> NextBlock( ByteBuffer blockBuffer )
		 {
			  long position = _channel.position();
			  if ( position >= _channel.size() )
			  {
					return null;
			  }
			  StoreChannel blockChannel = _fs.open( _file, OpenMode.READ );
			  blockChannel.Position( position );
			  PageCursor pageCursor = new ReadableChannelPageCursor( new ReadAheadChannel<>( blockChannel, blockBuffer ) );
			  BlockEntryReader<KEY, VALUE> blockEntryReader = new BlockEntryReader<KEY, VALUE>( pageCursor, _layout );
			  long blockSize = blockEntryReader.BlockSize();
			  _channel.position( position + blockSize );
			  return blockEntryReader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.close();
		 }
	}

}