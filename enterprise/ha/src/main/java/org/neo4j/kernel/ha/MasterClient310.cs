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
	using Org.Neo4j.com;
	using Protocol = Org.Neo4j.com.Protocol;
	using Protocol310 = Org.Neo4j.com.Protocol310;
	using ProtocolVersion = Org.Neo4j.com.ProtocolVersion;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ResponseUnpacker = Org.Neo4j.com.storecopy.ResponseUnpacker;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ProtocolVersion.INTERNAL_PROTOCOL_VERSION;

	public class MasterClient310 : MasterClient214
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public new static readonly ProtocolVersion ProtocolVersionConflict = new ProtocolVersion( ( sbyte ) 9, INTERNAL_PROTOCOL_VERSION );

		 public MasterClient310( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, long readTimeoutMillis, long lockReadTimeout, int maxConcurrentChannels, int chunkSize, ResponseUnpacker unpacker, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, readTimeoutMillis, lockReadTimeout, maxConcurrentChannels, chunkSize, unpacker, byteCounterMonitor, requestMonitor, entryReader )
		 {
		 }

		 protected internal override Protocol CreateProtocol( int chunkSize, sbyte applicationProtocolVersion )
		 {
			  return new Protocol310( chunkSize, applicationProtocolVersion, InternalProtocolVersion );
		 }

		 public override ProtocolVersion ProtocolVersion
		 {
			 get
			 {
				  return ProtocolVersionConflict;
			 }
		 }

		 protected internal override Deserializer<Void> CreateFileStreamDeserializer( StoreWriter writer )
		 {
			  return new Protocol.FileStreamsDeserializer310( writer );
		 }
	}

}