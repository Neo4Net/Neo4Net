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

	using MadeUpRequestType = Neo4Net.com.MadeUpServer.MadeUpRequestType;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.MadeUpServer.FRAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.writeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.RequestContext.EMPTY;

	public abstract class MadeUpClient : Client<MadeUpCommunicationInterface>, MadeUpCommunicationInterface
	{
		 public MadeUpClient( int port, StoreId storeIdToExpect, int chunkSize, ResponseUnpacker responseUnpacker ) : base( Localhost(), port, null, NullLogProvider.Instance, storeIdToExpect, FRAME_LENGTH, Client.DEFAULT_READ_RESPONSE_TIMEOUT_SECONDS * 1000, Client.DEFAULT_MAX_NUMBER_OF_CONCURRENT_CHANNELS_PER_CLIENT, chunkSize, responseUnpacker, (new Monitors()).newMonitor(typeof(ByteCounterMonitor)), (new Monitors()).newMonitor(typeof(RequestMonitor)), new VersionAwareLogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>() )
		 {
		 }

		 private static string Localhost()
		 {
			  try
			  {
					return InetAddress.LocalHost.HostAddress;
			  }
			  catch ( UnknownHostException e )
			  {
					throw new Exception( e );
			  }
		 }

		 protected internal override sbyte InternalProtocolVersion
		 {
			 get
			 {
				  return ProtocolVersion.InternalProtocol;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<int> multiply(final int value1, final int value2)
		 public override Response<int> Multiply( int value1, int value2 )
		 {
			  Serializer serializer = buffer =>
			  {
				buffer.writeInt( value1 );
				buffer.writeInt( value2 );
			  };
			  return SendRequest( MadeUpServer.MadeUpRequestType.Multiply, RequestContext, serializer, Protocol.IntegerDeserializer );
		 }

		 private RequestContext RequestContext
		 {
			 get
			 {
				  return new RequestContext( EMPTY.Epoch, EMPTY.machineId(), EMPTY.EventIdentifier, 2, EMPTY.Checksum );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<Void> fetchDataStream(final MadeUpWriter writer, final int dataSize)
		 public override Response<Void> FetchDataStream( MadeUpWriter writer, int dataSize )
		 {
			  Serializer serializer = buffer => buffer.writeInt( dataSize );
			  Deserializer<Void> deserializer = ( buffer, temporaryBuffer ) =>
			  {
				writer.Write( new BlockLogReader( buffer ) );
				return null;
			  };
			  return SendRequest( MadeUpServer.MadeUpRequestType.FetchDataStream, RequestContext, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<Void> sendDataStream(final java.nio.channels.ReadableByteChannel data)
		 public override Response<Void> SendDataStream( ReadableByteChannel data )
		 {
			  Serializer serializer = buffer =>
			  {
				using ( BlockLogBuffer writer = new BlockLogBuffer( buffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) ) )
				{
					 writer.Write( data );
				}
			  };
			  return SendRequest( MadeUpServer.MadeUpRequestType.SendDataStream, RequestContext, serializer, Protocol.VoidDeserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<int> throwException(final String messageInException)
		 public override Response<int> ThrowException( string messageInException )
		 {
			  Serializer serializer = buffer => writeString( buffer, messageInException );
			  Deserializer<int> deserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( MadeUpServer.MadeUpRequestType.ThrowException, RequestContext, serializer, deserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<int> streamBackTransactions(final int responseToSendBack, final int txCount)
		 public override Response<int> StreamBackTransactions( int responseToSendBack, int txCount )
		 {
			  Serializer serializer = buffer =>
			  {
				buffer.writeInt( responseToSendBack );
				buffer.writeInt( txCount );
			  };
			  Deserializer<int> integerDeserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( MadeUpRequestType.STREAM_BACK_TRANSACTIONS, RequestContext, serializer, integerDeserializer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<int> informAboutTransactionObligations(final int responseToSendBack, final long desiredObligation)
		 public override Response<int> InformAboutTransactionObligations( int responseToSendBack, long desiredObligation )
		 {
			  Serializer serializer = buffer =>
			  {
				buffer.writeInt( responseToSendBack );
				buffer.writeLong( desiredObligation );
			  };
			  Deserializer<int> deserializer = ( buffer, temporaryBuffer ) => buffer.readInt();
			  return SendRequest( MadeUpRequestType.INFORM_ABOUT_TX_OBLIGATIONS, RequestContext, serializer, deserializer );
		 }
	}

}