/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Neo4Net.com;
	using Neo4Net.com;
	using Protocol = Neo4Net.com.Protocol;
	using Protocol320 = Neo4Net.com.Protocol320;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using LockStatus = Neo4Net.Kernel.ha.@lock.LockStatus;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.readString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.writeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.ProtocolVersion.INTERNAL_PROTOCOL_VERSION;

	public class MasterClient320 : MasterClient310
	{
		 /* Version 1 first version
		  * Version 2 since 2012-01-24
		  * Version 3 since 2012-02-16
		  * Version 4 since 2012-07-05
		  * Version 5 since ?
		  * Version 6 since 2014-01-07
		  * Version 7 since 2014-03-18
		  * Version 8 since 2014-08-27
		  * Version 9 since 3.1.0, 2016-09-20
		  * Version 10 since 3.2.0, 2016-12-07
		  */
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public new static readonly ProtocolVersion ProtocolVersionConflict = new ProtocolVersion( ( sbyte ) 10, INTERNAL_PROTOCOL_VERSION );

		 // From 3.2.0 and onwards, LockResult messages can have messages, or not, independently of their LockStatus.
		 public new static readonly ObjectSerializer<LockResult> LockResultObjectSerializer = ( responseObject, result ) =>
		 {
		  result.writeByte( responseObject.Status.ordinal() );
		  string message = responseObject.Message;
		  if ( !string.ReferenceEquals( message, null ) )
		  {
				writeString( result, message );
		  }
		  else
		  {
				result.writeInt( 0 );
		  }
		 };

		 public new static readonly Deserializer<LockResult> LockResultDeserializer = ( buffer, temporaryBuffer ) =>
		 {
		  sbyte statusOrdinal = buffer.readByte();
		  int messageLength = buffer.readInt();
		  LockStatus status;
		  try
		  {
				status = LockStatus.values()[statusOrdinal];
		  }
		  catch ( System.IndexOutOfRangeException e )
		  {
				throw WithInvalidOrdinalMessage( buffer, statusOrdinal, e );
		  }
		  if ( messageLength > 0 )
		  {
				return new LockResult( status, readString( buffer, messageLength ) );
		  }
		  else
		  {
				return new LockResult( status );
		  }
		 };

		 public MasterClient320( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, long readTimeoutMillis, long lockReadTimeout, int maxConcurrentChannels, int chunkSize, ResponseUnpacker unpacker, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, readTimeoutMillis, lockReadTimeout, maxConcurrentChannels, chunkSize, unpacker, byteCounterMonitor, requestMonitor, entryReader )
		 {
		 }

		 protected internal override Protocol CreateProtocol( int chunkSize, sbyte applicationProtocolVersion )
		 {
			  return new Protocol320( chunkSize, applicationProtocolVersion, InternalProtocolVersion );
		 }

		 public override ObjectSerializer<LockResult> CreateLockResultSerializer()
		 {
			  return LockResultObjectSerializer;
		 }

		 public override Deserializer<LockResult> CreateLockResultDeserializer()
		 {
			  return LockResultDeserializer;
		 }

		 public override ProtocolVersion ProtocolVersion
		 {
			 get
			 {
				  return ProtocolVersionConflict;
			 }
		 }
	}

}