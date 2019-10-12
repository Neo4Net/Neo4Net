using System;

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
namespace Neo4Net.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;


	/// <summary>
	/// The counterpart of <seealso cref="BlockLogBuffer"/>, sits on the receiving end and
	/// reads chunks of log. It is provided with a <seealso cref="ChannelBuffer"/> which feeds
	/// a series of chunks formatted as follows:
	/// <ul>
	/// <li>If the first byte is 0, then it is 256 bytes in total size (including the first byte) AND there are more
	/// coming.</li>
	/// <li>If the first byte is not 0, then its value cast as an
	/// integer is the total size of the chunk AND there are no more - the stream is complete</li>
	/// </ul>
	/// </summary>
	public class BlockLogReader : ReadableByteChannel
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_byteBuffer = ByteBuffer.wrap( _byteArray );
		}

		 private readonly ChannelBuffer _source;
		 private readonly sbyte[] _byteArray = new sbyte[BlockLogBuffer.MAX_SIZE];
		 private ByteBuffer _byteBuffer;
		 private bool _moreBlocks;

		 public BlockLogReader( ChannelBuffer source )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._source = source;
			  ReadNextBlock();
		 }

		 /// <summary>
		 /// Read a block from the channel. Read the first byte, determine size and if
		 /// more are coming, set state accordingly and store content. NOTE: After
		 /// this op the buffer is flipped, ready to read.
		 /// </summary>
		 private void ReadNextBlock()
		 {
			  int blockSize = _source.readUnsignedByte();
			  _byteBuffer.clear();
			  _moreBlocks = blockSize == BlockLogBuffer.FULL_BLOCK_AND_MORE;
			  int limit = _moreBlocks ? BlockLogBuffer.DataSize : blockSize;
			  _byteBuffer.limit( limit );
			  _source.readBytes( _byteBuffer );
			  _byteBuffer.flip();
		 }

		 public override bool Open
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override void Close()
		 {
			  // This is to make sure that reader index in the ChannelBuffer is left
			  // in the right place even if this reader wasn't completely read through.
			  ReadToTheEnd();
		 }

		 public override int Read( ByteBuffer dst )
		 {
			  /*
			   * Fill up dst with what comes from the channel, until dst is full.
			   * readAsMuchAsPossible() is constantly called reading essentially
			   * one chunk at a time until either it runs out of stuff coming
			   * from the channel or the actual target buffer is filled.
			   */
			  int bytesWanted = dst.limit();
			  int bytesRead = 0;
			  while ( bytesWanted > 0 )
			  {
					int bytesReadThisTime = ReadAsMuchAsPossible( dst, bytesWanted );
					if ( bytesReadThisTime == 0 )
					{
						 break;
					}
					bytesRead += bytesReadThisTime;
					bytesWanted -= bytesReadThisTime;
			  }
			  return bytesRead == 0 && !_moreBlocks ? -1 : bytesRead;
		 }

		 /// <summary>
		 /// Reads in at most {@code maxBytesWanted} in {@code dst} but never more
		 /// than a chunk.
		 /// </summary>
		 /// <param name="dst"> The buffer to write the reads bytes to </param>
		 /// <param name="maxBytesWanted"> The maximum number of bytes to read. </param>
		 /// <returns> The number of bytes actually read </returns>
		 private int ReadAsMuchAsPossible( ByteBuffer dst, int maxBytesWanted )
		 {
			  if ( _byteBuffer.remaining() == 0 && _moreBlocks )
			  {
					ReadNextBlock();
			  }

			  int bytesToRead = Math.Min( maxBytesWanted, _byteBuffer.remaining() );
			  dst.put( _byteArray, _byteBuffer.position(), bytesToRead );
			  _byteBuffer.position( _byteBuffer.position() + bytesToRead );
			  return bytesToRead;
		 }

		 /// <summary>
		 /// Reads everything that can be read from the channel. Stops when a chunk
		 /// starting with a non zero byte is met.
		 /// </summary>
		 private void ReadToTheEnd()
		 {
			  while ( _moreBlocks )
			  {
					ReadNextBlock();
			  }
		 }
	}

}