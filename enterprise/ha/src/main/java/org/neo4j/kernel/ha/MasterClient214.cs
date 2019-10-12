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
namespace Org.Neo4j.Kernel.ha
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Protocol = Org.Neo4j.com.Protocol;
	using Protocol214 = Org.Neo4j.com.Protocol214;
	using ProtocolVersion = Org.Neo4j.com.ProtocolVersion;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using RequestType = Org.Neo4j.com.RequestType;
	using Org.Neo4j.com;
	using Serializer = Org.Neo4j.com.Serializer;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ResponseUnpacker = Org.Neo4j.com.storecopy.ResponseUnpacker;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using HandshakeResult = Org.Neo4j.Kernel.ha.com.master.HandshakeResult;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using MasterServer = Org.Neo4j.Kernel.ha.com.master.MasterServer;
	using MasterClient = Org.Neo4j.Kernel.ha.com.slave.MasterClient;
	using IdAllocation = Org.Neo4j.Kernel.ha.id.IdAllocation;
	using LockResult = Org.Neo4j.Kernel.ha.@lock.LockResult;
	using LockStatus = Org.Neo4j.Kernel.ha.@lock.LockStatus;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.EMPTY_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.VOID_DESERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.readString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.writeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ProtocolVersion.INTERNAL_PROTOCOL_VERSION;

	/// <summary>
	/// The <seealso cref="org.neo4j.kernel.ha.com.master.Master"/> a slave should use to communicate with its master. It
	/// serializes requests and sends them to the master, more specifically
	/// <seealso cref="org.neo4j.kernel.ha.com.master.MasterServer"/> (which delegates to
	/// <seealso cref="org.neo4j.kernel.ha.com.master.MasterImpl"/>
	/// on the master side.
	/// </summary>
	public class MasterClient214 : Client<Master>, MasterClient
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public static readonly ProtocolVersion ProtocolVersionConflict = new ProtocolVersion( ( sbyte ) 8, INTERNAL_PROTOCOL_VERSION );

		 public static readonly ObjectSerializer<LockResult> LockResultObjectSerializer = ( responseObject, result ) =>
		 {
		  result.writeByte( responseObject.Status.ordinal() );
		  if ( responseObject.Status == LockStatus.DEAD_LOCKED )
		  {
				writeString( result, responseObject.Message );
		  }
		 };

		 public static readonly Deserializer<LockResult> LockResultDeserializer = ( buffer, temporaryBuffer ) =>
		 {
		  sbyte statusOrdinal = buffer.readByte();
		  LockStatus status;
		  try
		  {
				status = LockStatus.values()[statusOrdinal];
		  }
		  catch ( System.IndexOutOfRangeException e )
		  {
				throw WithInvalidOrdinalMessage( buffer, statusOrdinal, e );
		  }
		  return status == LockStatus.DEAD_LOCKED ? new LockResult( LockStatus.DEAD_LOCKED, readString( buffer ) ) : new LockResult( status );
		 };

		 protected internal static System.IndexOutOfRangeException WithInvalidOrdinalMessage( ChannelBuffer buffer, sbyte statusOrdinal, System.IndexOutOfRangeException e )
		 {
			  int maxBytesToPrint = 1024 * 40;
			  return Exceptions.withMessage( e, format( "%s | read invalid ordinal %d. First %db of this channel buffer is:%n%s", e.Message, statusOrdinal, maxBytesToPrint, BeginningOfBufferAsHexString( buffer, maxBytesToPrint ) ) );
		 }

		 private readonly long _lockReadTimeoutMillis;
		 private readonly HaRequestTypes _requestTypes;
		 private readonly Deserializer<LockResult> _lockResultDeserializer;

		 public MasterClient214( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, long readTimeoutMillis, long lockReadTimeoutMillis, int maxConcurrentChannels, int chunkSize, ResponseUnpacker responseUnpacker, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, MasterServer.FRAME_LENGTH, readTimeoutMillis, maxConcurrentChannels, chunkSize, responseUnpacker, byteCounterMonitor, requestMonitor, entryReader )
		 {
			  this._lockReadTimeoutMillis = lockReadTimeoutMillis;
			  this._requestTypes = new HaRequestType210( entryReader, CreateLockResultSerializer() );
			  this._lockResultDeserializer = CreateLockResultDeserializer();
		 }

		 protected internal override Protocol CreateProtocol( int chunkSize, sbyte applicationProtocolVersion )
		 {
			  return new Protocol214( chunkSize, applicationProtocolVersion, InternalProtocolVersion );
		 }

		 public override ProtocolVersion ProtocolVersion
		 {
			 get
			 {
				  return ProtocolVersionConflict;
			 }
		 }

		 public override ObjectSerializer<LockResult> CreateLockResultSerializer()
		 {
			  return LockResultObjectSerializer;
		 }

		 public override Deserializer<LockResult> CreateLockResultDeserializer()
		 {
			  return LockResultDeserializer;
		 }

		 protected internal override long GetReadTimeout( RequestType type, long readTimeout )
		 {
			  if ( HaRequestTypes_Type.AcquireExclusiveLock.@is( type ) || HaRequestTypes_Type.AcquireSharedLock.@is( type ) )
			  {
					return _lockReadTimeoutMillis;
			  }
			  if ( HaRequestTypes_Type.CopyStore.@is( type ) )
			  {
					return readTimeout * 2;
			  }
			  return readTimeout;
		 }

		 protected internal override bool ShouldCheckStoreId( RequestType type )
		 {
			  return !HaRequestTypes_Type.CopyStore.@is( type );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<org.neo4j.kernel.ha.id.IdAllocation> allocateIds(org.neo4j.com.RequestContext context, final org.neo4j.kernel.impl.store.id.IdType idType)
		 public override Response<IdAllocation> AllocateIds( RequestContext context, IdType idType )
		 {
			  Serializer serializer = buffer => buffer.writeByte( ( int )idType );
			  Deserializer<IdAllocation> deserializer = ( buffer, temporaryBuffer ) => ReadIdAllocation( buffer );
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.AllocateIds ), context, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<int> createRelationshipType(org.neo4j.com.RequestContext context, final String name)
		 public override Response<int> CreateRelationshipType( RequestContext context, string name )
		 {
			  Serializer serializer = buffer => writeString( buffer, name );
			  Deserializer<int> deserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.CreateRelationshipType ), context, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<int> createPropertyKey(org.neo4j.com.RequestContext context, final String name)
		 public override Response<int> CreatePropertyKey( RequestContext context, string name )
		 {
			  Serializer serializer = buffer => writeString( buffer, name );
			  Deserializer<int> deserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.CreatePropertyKey ), context, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<int> createLabel(org.neo4j.com.RequestContext context, final String name)
		 public override Response<int> CreateLabel( RequestContext context, string name )
		 {
			  Serializer serializer = buffer => writeString( buffer, name );
			  Deserializer<int> deserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.CreateLabel ), context, serializer, deserializer );
		 }

		 public override Response<Void> NewLockSession( RequestContext context )
		 {
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.NewLockSession ), context, EMPTY_SERIALIZER, VOID_DESERIALIZER );
		 }

		 public override Response<LockResult> AcquireSharedLock( RequestContext context, ResourceType type, params long[] resourceIds )
		 {
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.AcquireSharedLock ), context, new AcquireLockSerializer( type, resourceIds ), _lockResultDeserializer );
		 }

		 public override Response<LockResult> AcquireExclusiveLock( RequestContext context, ResourceType type, params long[] resourceIds )
		 {
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.AcquireExclusiveLock ), context, new AcquireLockSerializer( type, resourceIds ), _lockResultDeserializer );
		 }

		 public override Response<long> Commit( RequestContext context, TransactionRepresentation tx )
		 {
			  Serializer serializer = new Protocol.TransactionSerializer( tx );
			  Deserializer<long> deserializer = ( buffer, temporaryBuffer ) => buffer.readLong();
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.Commit ), context, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<Void> endLockSession(org.neo4j.com.RequestContext context, final boolean success)
		 public override Response<Void> EndLockSession( RequestContext context, bool success )
		 {
			  Serializer serializer = buffer => buffer.writeByte( success ? 1 : 0 );
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.EndLockSession ), context, serializer, VOID_DESERIALIZER );
		 }

		 public override Response<Void> PullUpdates( RequestContext context )
		 {
			  return PullUpdates( context, Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler_Fields.NoOpTxHandler );
		 }

		 public override Response<Void> PullUpdates( RequestContext context, Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler txHandler )
		 {
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.PullUpdates ), context, EMPTY_SERIALIZER, VOID_DESERIALIZER, null, txHandler );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<org.neo4j.kernel.ha.com.master.HandshakeResult> handshake(final long txId, org.neo4j.storageengine.api.StoreId storeId)
		 public override Response<HandshakeResult> Handshake( long txId, StoreId storeId )
		 {
			  Serializer serializer = buffer => buffer.writeLong( txId );
			  Deserializer<HandshakeResult> deserializer = ( buffer, temporaryBuffer ) => new HandshakeResult( buffer.readLong(), buffer.readLong() );
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.Handshake ), RequestContext.EMPTY, serializer, deserializer, storeId, Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler_Fields.NoOpTxHandler );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<Void> copyStore(org.neo4j.com.RequestContext context, final org.neo4j.com.storecopy.StoreWriter writer)
		 public override Response<Void> CopyStore( RequestContext context, StoreWriter writer )
		 {
			  context = StripFromTransactions( context );
			  return SendRequest( _requestTypes.type( HaRequestTypes_Type.CopyStore ), context, EMPTY_SERIALIZER, CreateFileStreamDeserializer( writer ) );
		 }

		 protected internal virtual Deserializer<Void> CreateFileStreamDeserializer( StoreWriter writer )
		 {
			  return new Protocol.FileStreamsDeserializer210( writer );
		 }

		 private RequestContext StripFromTransactions( RequestContext context )
		 {
			  return new RequestContext( context.Epoch, context.MachineId(), context.EventIdentifier, 0, context.Checksum );
		 }

		 private static IdAllocation ReadIdAllocation( ChannelBuffer buffer )
		 {
			  int numberOfDefragIds = buffer.readInt();
			  long[] defragIds = new long[numberOfDefragIds];
			  for ( int i = 0; i < numberOfDefragIds; i++ )
			  {
					defragIds[i] = buffer.readLong();
			  }
			  long rangeStart = buffer.readLong();
			  int rangeLength = buffer.readInt();
			  long highId = buffer.readLong();
			  long defragCount = buffer.readLong();
			  return new IdAllocation( new IdRange( defragIds, rangeStart, rangeLength ), highId, defragCount );
		 }

		 private class AcquireLockSerializer : Serializer
		 {
			  internal readonly ResourceType Type;
			  internal readonly long[] ResourceIds;

			  internal AcquireLockSerializer( ResourceType type, params long[] resourceIds )
			  {
					this.Type = type;
					this.ResourceIds = resourceIds;
			  }

			  public override void Write( ChannelBuffer buffer )
			  {
					buffer.writeInt( Type.typeId() );
					buffer.writeInt( ResourceIds.Length );
					foreach ( long entity in ResourceIds )
					{
						 buffer.writeLong( entity );
					}
			  }
		 }
	}

}