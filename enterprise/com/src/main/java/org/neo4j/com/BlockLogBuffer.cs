using System;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;


	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;

	/// <summary>
	/// Implementation of a LogBuffer over a ChannelBuffer. Maintains a byte buffer
	/// of content which is flushed to the underlying channel when a maximum size is
	/// reached. It is supposed to be used with <seealso cref="BlockLogReader"/>.
	/// <para>
	/// Every chunk is exactly 256 bytes in length, except for the last one which can
	/// be anything greater than one and up to 256. This is signaled via the first
	/// byte which is 0 for every non-last chunk and the actual number of bytes for
	/// the last one (always &gt; 0).
	/// </para>
	/// </summary>
	public class BlockLogBuffer : System.IDisposable
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_byteBuffer = ByteBuffer.wrap( _byteArray );
		}

		 // First byte of every chunk that is not the last one
		 internal const sbyte FULL_BLOCK_AND_MORE = 0;
		 internal const int MAX_SIZE = 256; // soft limit, incl. header
		 internal static readonly int DataSize = MAX_SIZE - 1;

		 private readonly ChannelBuffer _target;
		 private readonly ByteCounterMonitor _monitor;
		 // MAX_SIZE can be overcome by one primitive put(), the largest is 8 bytes
		 private readonly sbyte[] _byteArray = new sbyte[MAX_SIZE + 8];
		 private ByteBuffer _byteBuffer;

		 public BlockLogBuffer( ChannelBuffer target, ByteCounterMonitor monitor )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._target = target;
			  this._monitor = monitor;
			  ClearInternalBuffer();
		 }

		 private void ClearInternalBuffer()
		 {
			  _byteBuffer.clear();
			  // reserve space for size - assume we are going to fill the buffer
			  _byteBuffer.put( FULL_BLOCK_AND_MORE );
		 }

		 /// <summary>
		 /// If the position of the byteBuffer is larger than MAX_SIZE then
		 /// MAX_SIZE bytes are flushed to the underlying channel. The remaining
		 /// bytes (1 up to and including 8 - see the byteArray field initializer)
		 /// are moved over at the beginning of the cleared buffer.
		 /// </summary>
		 /// <returns> the buffer </returns>
		 private BlockLogBuffer CheckFlush()
		 {
			  if ( _byteBuffer.position() > MAX_SIZE )
			  {
					Flush();
			  }
			  return this;
		 }

		 private void Flush()
		 {
			  int howManyBytesToWrite = MAX_SIZE;
			  _target.writeBytes( _byteArray, 0, howManyBytesToWrite );
			  _monitor.bytesWritten( howManyBytesToWrite );
			  int pos = _byteBuffer.position();
			  ClearInternalBuffer();
			  _byteBuffer.put( _byteArray, howManyBytesToWrite, pos - howManyBytesToWrite );
		 }

		 public virtual BlockLogBuffer Put( sbyte b )
		 {
			  _byteBuffer.put( b );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer PutShort( short s )
		 {
			  _byteBuffer.putShort( s );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer PutInt( int i )
		 {
			  _byteBuffer.putInt( i );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer PutLong( long l )
		 {
			  _byteBuffer.putLong( l );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer PutFloat( float f )
		 {
			  _byteBuffer.putFloat( f );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer PutDouble( double d )
		 {
			  _byteBuffer.putDouble( d );
			  return CheckFlush();
		 }

		 public virtual BlockLogBuffer Put( sbyte[] bytes, int length )
		 {
			  for ( int pos = 0; pos < length; )
			  {
					int toWrite = Math.Min( _byteBuffer.remaining(), length - pos );
					_byteBuffer.put( bytes, pos, toWrite );
					CheckFlush();
					pos += toWrite;
			  }
			  return this;
		 }

		 /// <summary>
		 /// Signals the end of use for this buffer over this channel - first byte of
		 /// the chunk is set to the position of the buffer ( != 0, instead of
		 /// FULL_BLOCK_AND_MORE) and it is written to the channel.
		 /// </summary>
		 public override void Close()
		 {
			  Debug.Assert( _byteBuffer.position() > 1, "buffer should contain more than the header" );
			  Debug.Assert( _byteBuffer.position() <= MAX_SIZE, "buffer should not be over full" );
			  long howManyBytesToWrite = _byteBuffer.position();
			  _byteBuffer.put( 0, ( sbyte )( _byteBuffer.position() - 1 ) );
			  _byteBuffer.flip();
			  _target.writeBytes( _byteBuffer );
			  _monitor.bytesWritten( howManyBytesToWrite );
			  ClearInternalBuffer();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(java.nio.channels.ReadableByteChannel data) throws java.io.IOException
		 public virtual int Write( ReadableByteChannel data )
		 {
			  int result = 0;
			  int bytesRead;
			  while ( ( bytesRead = data.read( _byteBuffer ) ) >= 0 )
			  {
					CheckFlush();
					result += bytesRead;
			  }
			  return result;
		 }
	}

}