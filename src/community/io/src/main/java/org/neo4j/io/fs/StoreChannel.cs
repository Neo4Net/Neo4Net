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
namespace Neo4Net.Io.fs
{

	public interface IStoreChannel : Flushable, SeekableByteChannel, GatheringByteChannel, ScatteringByteChannel, InterruptibleChannel
	{
		 /// <summary>
		 /// Attempts to acquire an exclusive lock on this channel's file. </summary>
		 /// <returns> A lock object representing the newly-acquired lock, or null if the lock could not be acquired. </returns>
		 /// <exception cref="IOException"> If an I/O error occurs. </exception>
		 /// <exception cref="java.nio.channels.ClosedChannelException"> if the channel is closed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.nio.channels.FileLock tryLock() throws java.io.IOException;
		 FileLock TryLock();

		 /// <summary>
		 /// Same as #write(), except this method will write the full contents of the buffer in chunks if the OS fails to
		 /// write it all at once.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeAll(ByteBuffer src, long position) throws java.io.IOException;
		 void WriteAll( ByteBuffer src, long position );

		 /// <summary>
		 /// Same as #write(), except this method will write the full contents of the buffer in chunks if the OS fails to
		 /// write it all at once.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeAll(ByteBuffer src) throws java.io.IOException;
		 void WriteAll( ByteBuffer src );

		 /// <seealso cref= java.nio.channels.FileChannel#read(java.nio.ByteBuffer, long) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int read(ByteBuffer dst, long position) throws java.io.IOException;
		 int Read( ByteBuffer dst, long position );

		 /// <summary>
		 /// Try to Read a sequence of bytes from channel into the given buffer, till the buffer will be full.
		 /// In case if end of channel will be reached during reading <seealso cref="System.InvalidOperationException"/> will be thrown.
		 /// </summary>
		 /// <param name="dst"> destination buffer. </param>
		 /// <exception cref="IOException"> if an I/O exception occurs. </exception>
		 /// <exception cref="IllegalStateException"> if end of stream reached during reading. </exception>
		 /// <seealso cref= ReadableByteChannel#read(ByteBuffer) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void readAll(ByteBuffer dst) throws java.io.IOException;
		 void ReadAll( ByteBuffer dst );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void force(boolean metaData) throws java.io.IOException;
		 void Force( bool metaData );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreChannel position(long newPosition) throws java.io.IOException;
		 StoreChannel Position( long newPosition );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreChannel truncate(long size) throws java.io.IOException;
		 StoreChannel Truncate( long size );
	}

}