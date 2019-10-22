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
namespace Neo4Net.Kernel.ha.com.master
{
	using Protocol = Neo4Net.com.Protocol;
	using RequestContext = Neo4Net.com.RequestContext;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;
	using TxChecksumVerifier = Neo4Net.com.TxChecksumVerifier;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.com.slave.MasterClient_Fields.CURRENT;

	/// <summary>
	/// Sits on the master side, receiving serialized requests from slaves (via
	/// <seealso cref="org.Neo4Net.kernel.ha.com.slave.MasterClient"/>). Delegates actual work to <seealso cref="MasterImpl"/>.
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