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
namespace Neo4Net.causalclustering.messaging
{
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.logging;

	public class LoggingInbound<M> : Inbound<M> where M : Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage
	{
		 private readonly Inbound<M> _inbound;
		 private readonly MessageLogger<MemberId> _messageLogger;
		 private readonly MemberId _me;

		 public LoggingInbound( Inbound<M> inbound, MessageLogger<MemberId> messageLogger, MemberId me )
		 {
			  this._inbound = inbound;
			  this._messageLogger = messageLogger;
			  this._me = me;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void registerHandler(final Inbound_MessageHandler<M> handler)
		 public override void RegisterHandler( Inbound_MessageHandler<M> handler )
		 {
			  _inbound.registerHandler( new Inbound_MessageHandlerAnonymousInnerClass( this, handler ) );
		 }

		 private class Inbound_MessageHandlerAnonymousInnerClass : Inbound_MessageHandler<M>
		 {
			 private readonly LoggingInbound<M> _outerInstance;

			 private Neo4Net.causalclustering.messaging.Inbound_MessageHandler<M> _handler;

			 public Inbound_MessageHandlerAnonymousInnerClass( LoggingInbound<M> outerInstance, Neo4Net.causalclustering.messaging.Inbound_MessageHandler<M> handler )
			 {
				 this.outerInstance = outerInstance;
				 this._handler = handler;
			 }

			 public void handle( M message )
			 {
				 lock ( this )
				 {
					  _outerInstance.messageLogger.logInbound( message.from(), message, _outerInstance.me );
					  _handler.handle( message );
				 }
			 }
		 }
	}

}