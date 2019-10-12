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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.FileChunk.MAX_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.FileSender.State.FINISHED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.FileSender.State.FULL_PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.FileSender.State.LAST_PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.FileSender.State.PRE_INIT;

	internal class FileSender : ChunkedInput<FileChunk>
	{
		 private readonly StoreResource _resource;
		 private readonly ByteBuffer _byteBuffer;

		 private ReadableByteChannel _channel;
		 private sbyte[] _nextBytes;
		 private State _state = PRE_INIT;

		 internal FileSender( StoreResource resource )
		 {
			  this._resource = resource;
			  this._byteBuffer = ByteBuffer.allocateDirect( MAX_SIZE );
		 }

		 public override bool EndOfInput
		 {
			 get
			 {
				  return _state == FINISHED;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  if ( _channel != null )
			  {
					_channel.close();
					_channel = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FileChunk readChunk(io.netty.buffer.ByteBufAllocator allocator) throws Exception
		 public override FileChunk ReadChunk( ByteBufAllocator allocator )
		 {
			  if ( _state == FINISHED )
			  {
					return null;
			  }
			  else if ( _state == PRE_INIT )
			  {
					_channel = _resource.open();
					_nextBytes = Prefetch();
					if ( _nextBytes == null )
					{
						 _state = FINISHED;
						 return FileChunk.Create( new sbyte[0], true );
					}
					else
					{
						 _state = _nextBytes.Length < MAX_SIZE ? LAST_PENDING : FULL_PENDING;
					}
			  }

			  if ( _state == FULL_PENDING )
			  {
					sbyte[] toSend = _nextBytes;
					_nextBytes = Prefetch();
					if ( _nextBytes == null )
					{
						 _state = FINISHED;
						 return FileChunk.Create( toSend, true );
					}
					else if ( _nextBytes.Length < MAX_SIZE )
					{
						 _state = LAST_PENDING;
						 return FileChunk.Create( toSend, false );
					}
					else
					{
						 return FileChunk.Create( toSend, false );
					}
			  }
			  else if ( _state == LAST_PENDING )
			  {
					_state = FINISHED;
					return FileChunk.Create( _nextBytes, true );
			  }
			  else
			  {
					throw new System.InvalidOperationException();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FileChunk readChunk(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		 public override FileChunk ReadChunk( ChannelHandlerContext ctx )
		 {
			  return ReadChunk( ctx.alloc() );
		 }

		 public override long Length()
		 {
			  return -1;
		 }

		 public override long Progress()
		 {
			  return 0;
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
			  FileSender that = ( FileSender ) o;
			  return Objects.Equals( _resource, that._resource );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _resource );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] prefetch() throws java.io.IOException
		 private sbyte[] Prefetch()
		 {
			  do
			  {
					int bytesRead = _channel.read( _byteBuffer );
					if ( bytesRead == -1 )
					{
						 break;
					}
			  } while ( _byteBuffer.hasRemaining() );

			  if ( _byteBuffer.position() > 0 )
			  {
					return CreateByteArray( _byteBuffer );
			  }
			  else
			  {
					return null;
			  }
		 }

		 private sbyte[] CreateByteArray( ByteBuffer buffer )
		 {
			  buffer.flip();
			  sbyte[] bytes = new sbyte[buffer.limit()];
			  buffer.get( bytes );
			  buffer.clear();
			  return bytes;
		 }

		 internal enum State
		 {
			  PreInit,
			  FullPending,
			  LastPending,
			  Finished
		 }
	}

}