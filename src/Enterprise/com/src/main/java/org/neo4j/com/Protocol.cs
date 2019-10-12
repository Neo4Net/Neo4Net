using System.Collections.Generic;

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
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using LengthFieldBasedFrameDecoder = org.jboss.netty.handler.codec.frame.LengthFieldBasedFrameDecoder;
	using LengthFieldPrepender = org.jboss.netty.handler.codec.frame.LengthFieldPrepender;
	using BlockingReadHandler = org.jboss.netty.handler.queue.BlockingReadHandler;


	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Neo4Net.Kernel.impl.store.format;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Neo4Net.Kernel.impl.transaction.log;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// Contains the logic for serializing requests and deserializing responses. Still missing the inverse, serializing
	/// responses and deserializing requests, which is hard-coded in the server class. That should be moved over
	/// eventually.
	/// </summary>
	public abstract class Protocol
	{
		 public const int MEGA = 1024 * 1024;
		 public static readonly int DefaultFrameLength = 16 * MEGA;
		 public static readonly ObjectSerializer<int> IntegerSerializer = ( responseObject, result ) => result.writeInt( responseObject );
		 public static readonly ObjectSerializer<long> LongSerializer = ( responseObject, result ) => result.writeLong( responseObject );
		 public static readonly ObjectSerializer<Void> VoidSerializer = ( responseObject, result ) =>
		 {
		 };
		 public static readonly Deserializer<int> IntegerDeserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
		 public static readonly Deserializer<Void> VoidDeserializer = ( buffer, temporaryBuffer ) => null;
		 public static readonly Serializer EmptySerializer = buffer =>
		 {
		 };

		 public class TransactionRepresentationDeserializer : Deserializer<TransactionRepresentation>
		 {
			  internal readonly LogEntryReader<ReadableClosablePositionAwareChannel> Reader;

			  public TransactionRepresentationDeserializer( LogEntryReader<ReadableClosablePositionAwareChannel> reader )
			  {
					this.Reader = reader;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.TransactionRepresentation read(org.jboss.netty.buffer.ChannelBuffer buffer, ByteBuffer temporaryBuffer) throws java.io.IOException
			  public override TransactionRepresentation Read( ChannelBuffer buffer, ByteBuffer temporaryBuffer )
			  {
					NetworkReadableClosableChannel channel = new NetworkReadableClosableChannel( buffer );

					int authorId = channel.Int;
					int masterId = channel.Int;
					long latestCommittedTxWhenStarted = channel.Long;
					long timeStarted = channel.Long;
					long timeCommitted = channel.Long;

					int headerLength = channel.Int;
					sbyte[] header = new sbyte[headerLength];
					channel.Get( header, headerLength );

					LogEntryCommand entryRead;
					IList<StorageCommand> commands = new LinkedList<StorageCommand>();
					while ( ( entryRead = ( LogEntryCommand ) Reader.readLogEntry( channel ) ) != null )
					{
						 commands.Add( entryRead.Command );
					}

					PhysicalTransactionRepresentation toReturn = new PhysicalTransactionRepresentation( commands );
					toReturn.SetHeader( header, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, -1 );
					return toReturn;
			  }
		 }
		 private readonly int _chunkSize;

		 /* ========================
		    Static utility functions
		    ======================== */
		 private readonly sbyte _applicationProtocolVersion;
		 private readonly sbyte _internalProtocolVersion;

		 public Protocol( int chunkSize, sbyte applicationProtocolVersion, sbyte internalProtocolVersion )
		 {
			  this._chunkSize = chunkSize;
			  this._applicationProtocolVersion = applicationProtocolVersion;
			  this._internalProtocolVersion = internalProtocolVersion;
		 }

		 public static void AddLengthFieldPipes( ChannelPipeline pipeline, int frameLength )
		 {
			  pipeline.addLast( "frameDecoder", new LengthFieldBasedFrameDecoder( frameLength + 4, 0, 4, 0, 4 ) );
			  pipeline.addLast( "frameEncoder", new LengthFieldPrepender( 4 ) );
		 }

		 public static void WriteString( ChannelBuffer buffer, string name )
		 {
			  char[] chars = name.ToCharArray();
			  buffer.writeInt( chars.Length );
			  WriteChars( buffer, chars );
		 }

		 public static void WriteChars( ChannelBuffer buffer, char[] chars )
		 {
			  // TODO optimize?
			  foreach ( char ch in chars )
			  {
					buffer.writeChar( ch );
			  }
		 }

		 public static string ReadString( ChannelBuffer buffer )
		 {
			  return ReadString( buffer, buffer.readInt() );
		 }

		 public static bool ReadBoolean( ChannelBuffer buffer )
		 {
			  sbyte value = buffer.readByte();
			  switch ( value )
			  {
			  case 0:
					return false;
			  case 1:
					return true;
			  default:
					throw new ComException( "Invalid boolean value " + value );
			  }
		 }

		 public static string ReadString( ChannelBuffer buffer, int length )
		 {
			  char[] chars = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					chars[i] = buffer.readChar();
			  }
			  return new string( chars );
		 }

		 public static void AssertChunkSizeIsWithinFrameSize( int chunkSize, int frameLength )
		 {
			  if ( chunkSize > frameLength )
			  {
					throw new System.ArgumentException( "Chunk size " + chunkSize + " needs to be equal or less than frame length " + frameLength );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serializeRequest(org.jboss.netty.channel.Channel channel, org.jboss.netty.buffer.ChannelBuffer buffer, RequestType type, RequestContext ctx, Serializer payload) throws java.io.IOException
		 public virtual void SerializeRequest( Channel channel, ChannelBuffer buffer, RequestType type, RequestContext ctx, Serializer payload )
		 {
			  buffer.clear();
			  ChunkingChannelBuffer chunkingBuffer = new ChunkingChannelBuffer( buffer, channel, _chunkSize, _internalProtocolVersion, _applicationProtocolVersion );
			  chunkingBuffer.WriteByte( type.Id() );
			  WriteContext( ctx, chunkingBuffer );
			  payload.Write( chunkingBuffer );
			  chunkingBuffer.Done();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <PAYLOAD> Response<PAYLOAD> deserializeResponse(org.jboss.netty.handler.queue.BlockingReadHandler<org.jboss.netty.buffer.ChannelBuffer> reader, ByteBuffer input, long timeout, Deserializer<PAYLOAD> payloadDeserializer, ResourceReleaser channelReleaser, final org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> entryReader) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response<PAYLOAD> DeserializeResponse<PAYLOAD>( BlockingReadHandler<ChannelBuffer> reader, ByteBuffer input, long timeout, Deserializer<PAYLOAD> payloadDeserializer, ResourceReleaser channelReleaser, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DechunkingChannelBuffer dechunkingBuffer = new DechunkingChannelBuffer(reader, timeout, internalProtocolVersion, applicationProtocolVersion);
			  DechunkingChannelBuffer dechunkingBuffer = new DechunkingChannelBuffer( reader, timeout, _internalProtocolVersion, _applicationProtocolVersion );

			  PAYLOAD response = payloadDeserializer.Read( dechunkingBuffer, input );
			  StoreId storeId = ReadStoreId( dechunkingBuffer, input );

			  // Response type is what previously was a byte saying how many data sources there were in the
			  // coming transaction stream response. For backwards compatibility we keep it as a byte and we introduce
			  // the transaction obligation response type as -1
			  sbyte responseType = dechunkingBuffer.ReadByte();
			  if ( responseType == TransactionObligationResponse.RESPONSE_TYPE )
			  {
					// It is a transaction obligation response
					long obligationTxId = dechunkingBuffer.ReadLong();
					return new TransactionObligationResponse<PAYLOAD>( response, storeId, obligationTxId, channelReleaser );
			  }

			  // It's a transaction stream in this response
			  TransactionStream transactions = visitor =>
			  {
				NetworkReadableClosableChannel channel = new NetworkReadableClosableChannel( dechunkingBuffer );

				using ( PhysicalTransactionCursor<ReadableClosablePositionAwareChannel> cursor = new PhysicalTransactionCursor<ReadableClosablePositionAwareChannel>( channel, entryReader ) )
				{
					 while ( cursor.next() && !visitor.visit(cursor.get()) )
					 {
					 }
				}
			  };
			  return new TransactionStreamResponse<PAYLOAD>( response, storeId, transactions, channelReleaser );
		 }

		 protected internal abstract StoreId ReadStoreId( ChannelBuffer source, ByteBuffer byteBuffer );

		 private void WriteContext( RequestContext context, ChannelBuffer targetBuffer )
		 {
			  targetBuffer.writeLong( context.Epoch );
			  targetBuffer.writeInt( context.MachineId() );
			  targetBuffer.writeInt( context.EventIdentifier );
			  long tx = context.LastAppliedTransaction();
			  targetBuffer.writeLong( tx );
			  targetBuffer.writeLong( context.Checksum );
		 }

		 public class FileStreamsDeserializer210 : Deserializer<Void>
		 {
			  internal readonly StoreWriter Writer;

			  public FileStreamsDeserializer210( StoreWriter writer )
			  {
					this.Writer = writer;
			  }

			  // NOTICE: this assumes a "smart" ChannelBuffer that continues to next chunk
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void read(org.jboss.netty.buffer.ChannelBuffer buffer, ByteBuffer temporaryBuffer) throws java.io.IOException
			  public override Void Read( ChannelBuffer buffer, ByteBuffer temporaryBuffer )
			  {
					int pathLength;
					while ( 0 != ( pathLength = buffer.readUnsignedShort() ) )
					{
						 string path = ReadString( buffer, pathLength );
						 bool hasData = buffer.readByte() == 1;
						 Writer.write( path, hasData ? new BlockLogReader( buffer ) : null, temporaryBuffer, hasData, 1 );
					}
					Writer.Dispose();
					return null;
			  }
		 }

		 public class FileStreamsDeserializer310 : Deserializer<Void>
		 {
			  internal readonly StoreWriter Writer;

			  public FileStreamsDeserializer310( StoreWriter writer )
			  {
					this.Writer = writer;
			  }

			  // NOTICE: this assumes a "smart" ChannelBuffer that continues to next chunk
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void read(org.jboss.netty.buffer.ChannelBuffer buffer, ByteBuffer temporaryBuffer) throws java.io.IOException
			  public override Void Read( ChannelBuffer buffer, ByteBuffer temporaryBuffer )
			  {
					int pathLength;
					while ( 0 != ( pathLength = buffer.readUnsignedShort() ) )
					{
						 string path = ReadString( buffer, pathLength );
						 bool hasData = buffer.readByte() == 1;
						 int recordSize = hasData ? buffer.readInt() : Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE;
						 Writer.write( path, hasData ? new BlockLogReader( buffer ) : null, temporaryBuffer, hasData, recordSize );
					}
					Writer.Dispose();
					return null;
			  }
		 }

		 public class TransactionSerializer : Serializer
		 {
			  internal readonly TransactionRepresentation Tx;

			  public TransactionSerializer( TransactionRepresentation tx )
			  {
					this.Tx = tx;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.jboss.netty.buffer.ChannelBuffer buffer) throws java.io.IOException
			  public override void Write( ChannelBuffer buffer )
			  {
					NetworkFlushableChannel channel = new NetworkFlushableChannel( buffer );

					WriteString( buffer, NeoStoreDataSource.DEFAULT_DATA_SOURCE_NAME );
					channel.PutInt( Tx.AuthorId );
					channel.PutInt( Tx.MasterId );
					channel.PutLong( Tx.LatestCommittedTxWhenStarted );
					channel.PutLong( Tx.TimeStarted );
					channel.PutLong( Tx.TimeCommitted );
					channel.PutInt( Tx.additionalHeader().Length );
					channel.Put( Tx.additionalHeader(), Tx.additionalHeader().Length );
					( new LogEntryWriter( channel ) ).serialize( Tx );
			  }
		 }
	}

}