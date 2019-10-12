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
namespace Org.Neo4j.Kernel.ha.com.slave
{
	using ProtocolVersion = Org.Neo4j.com.ProtocolVersion;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using RequestType = Org.Neo4j.com.RequestType;
	using Org.Neo4j.com;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using Slave = Org.Neo4j.Kernel.ha.com.master.Slave;
	using SlaveRequestType = Org.Neo4j.Kernel.ha.com.master.SlaveClient.SlaveRequestType;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.DEFAULT_FRAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ProtocolVersion.INTERNAL_PROTOCOL_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.TxChecksumVerifier_Fields.ALWAYS_MATCH;

	public class SlaveServer : Server<Slave, Void>
	{
		 public const sbyte APPLICATION_PROTOCOL_VERSION = 1;
		 public static readonly ProtocolVersion SlaveProtocolVersion = new ProtocolVersion( ( sbyte ) 1, INTERNAL_PROTOCOL_VERSION );

		 public SlaveServer( Slave requestTarget, Configuration config, LogProvider logProvider, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor ) : base( requestTarget, config, logProvider, DEFAULT_FRAME_LENGTH, SlaveProtocolVersion, ALWAYS_MATCH, Clocks.systemClock(), byteCounterMonitor, requestMonitor )
		 {
		 }

		 protected internal override RequestType GetRequestContext( sbyte id )
		 {
			  return SlaveRequestType.values()[id];
		 }

		 protected internal override void StopConversation( RequestContext context )
		 {
		 }
	}

}