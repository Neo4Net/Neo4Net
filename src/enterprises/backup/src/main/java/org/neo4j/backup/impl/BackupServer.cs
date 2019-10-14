/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup.impl
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using Channel = org.jboss.netty.channel.Channel;

	using BackupRequestType = Neo4Net.backup.impl.BackupClient.BackupRequestType;
	using ChunkingChannelBuffer = Neo4Net.com.ChunkingChannelBuffer;
	using Neo4Net.com;
	using Protocol = Neo4Net.com.Protocol;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestContext = Neo4Net.com.RequestContext;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ProtocolVersion.INTERNAL_PROTOCOL_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.TxChecksumVerifier_Fields.ALWAYS_MATCH;

	public class BackupServer : Server<TheBackupInterface, object>
	{
		 private static readonly long _defaultOldChannelThreshold = Client.DEFAULT_READ_RESPONSE_TIMEOUT_SECONDS * 1000;
		 private const int DEFAULT_MAX_CONCURRENT_TX = 3;

		 private static readonly BackupRequestType[] _contexts = BackupRequestType.values();

		 /// <summary>
		 /// Protocol Version : Product Version
		 ///                1 : * to 3.0.x
		 ///                2 : 3.1.x
		 /// </summary>
		 public static readonly ProtocolVersion BackupProtocolVersion = new ProtocolVersion( ( sbyte ) 2, INTERNAL_PROTOCOL_VERSION );

		 public const int DEFAULT_PORT = 6362;
		 internal static readonly int FrameLength = Protocol.MEGA * 4;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public BackupServer(org.neo4j.backup.TheBackupInterface requestTarget, final org.neo4j.helpers.HostnamePort server, org.neo4j.logging.LogProvider logProvider, org.neo4j.kernel.monitoring.ByteCounterMonitor byteCounterMonitor, org.neo4j.com.monitor.RequestMonitor requestMonitor)
		 public BackupServer( TheBackupInterface requestTarget, HostnamePort server, LogProvider logProvider, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor ) : base( requestTarget, NewBackupConfig( FrameLength, server ), logProvider, FrameLength, BackupProtocolVersion, ALWAYS_MATCH, Clocks.systemClock(), byteCounterMonitor, requestMonitor )
		 {
		 }

		 protected internal override ChunkingChannelBuffer NewChunkingBuffer( ChannelBuffer bufferToWriteTo, Channel channel, int capacity, sbyte internalProtocolVersion, sbyte applicationProtocolVersion )
		 {
			  return new BufferReusingChunkingChannelBuffer( bufferToWriteTo, channel, capacity, internalProtocolVersion, applicationProtocolVersion );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Configuration newBackupConfig(final int chunkSize, final org.neo4j.helpers.HostnamePort server)
		 private static Configuration NewBackupConfig( int chunkSize, HostnamePort server )
		 {
			  return new ConfigurationAnonymousInnerClass( chunkSize, server );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private int _chunkSize;
			 private HostnamePort _server;

			 public ConfigurationAnonymousInnerClass( int chunkSize, HostnamePort server )
			 {
				 this._chunkSize = chunkSize;
				 this._server = server;
			 }

			 public long OldChannelThreshold
			 {
				 get
				 {
					  return _defaultOldChannelThreshold;
				 }
			 }

			 public int MaxConcurrentTransactions
			 {
				 get
				 {
					  return DEFAULT_MAX_CONCURRENT_TX;
				 }
			 }

			 public int ChunkSize
			 {
				 get
				 {
					  return _chunkSize;
				 }
			 }

			 public HostnamePort ServerAddress
			 {
				 get
				 {
					  return _server;
				 }
			 }
		 }

		 protected internal override void ResponseWritten( RequestType type, Channel channel, RequestContext context )
		 {
		 }

		 protected internal override RequestType GetRequestContext( sbyte id )
		 {
			  return _contexts[id];
		 }

		 protected internal override void StopConversation( RequestContext context )
		 {
		 }
	}

}