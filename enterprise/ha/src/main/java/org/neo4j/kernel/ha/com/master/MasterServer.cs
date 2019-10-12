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
namespace Org.Neo4j.Kernel.ha.com.master
{
	using Protocol = Org.Neo4j.com.Protocol;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using RequestType = Org.Neo4j.com.RequestType;
	using Org.Neo4j.com;
	using TxChecksumVerifier = Org.Neo4j.com.TxChecksumVerifier;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.com.slave.MasterClient_Fields.CURRENT;

	/// <summary>
	/// Sits on the master side, receiving serialized requests from slaves (via
	/// <seealso cref="org.neo4j.kernel.ha.com.slave.MasterClient"/>). Delegates actual work to <seealso cref="MasterImpl"/>.
	/// </summary>
	public class MasterServer : Server<Master, Void>
	{
		 public static readonly int FrameLength = Protocol.DEFAULT_FRAME_LENGTH;
		 private readonly ConversationManager _conversationManager;
		 private readonly HaRequestType210 _requestTypes;

		 public MasterServer( Master requestTarget, LogProvider logProvider, Configuration config, TxChecksumVerifier txVerifier, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, ConversationManager conversationManager, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( requestTarget, config, logProvider, FrameLength, CURRENT, txVerifier, Clocks.systemClock(), byteCounterMonitor, requestMonitor )
		 {
			  this._conversationManager = conversationManager;
			  this._requestTypes = new HaRequestType210( entryReader, MasterClient320.LOCK_RESULT_OBJECT_SERIALIZER );
		 }

		 protected internal override RequestType GetRequestContext( sbyte id )
		 {
			  return _requestTypes.type( id );
		 }

		 protected internal override void StopConversation( RequestContext context )
		 {
			  _conversationManager.stop( context );
		 }
	}

}