using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus
{

	using Message = Neo4Net.causalclustering.messaging.Message;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class DirectNetworking
	{
		 private readonly IDictionary<MemberId, Neo4Net.causalclustering.messaging.Inbound_MessageHandler> _handlers = new Dictionary<MemberId, Neo4Net.causalclustering.messaging.Inbound_MessageHandler>();
		 private readonly IDictionary<MemberId, LinkedList<Message>> _messageQueues = new Dictionary<MemberId, LinkedList<Message>>();
		 private readonly ISet<MemberId> _disconnectedMembers = Collections.newSetFromMap( new ConcurrentDictionary<MemberId>() );

		 public virtual void ProcessMessages()
		 {
			  while ( MessagesToBeProcessed() )
			  {
					foreach ( KeyValuePair<MemberId, LinkedList<Message>> entry in _messageQueues.SetOfKeyValuePairs() )
					{
						 MemberId id = entry.Key;
						 LinkedList<Message> queue = entry.Value;
						 if ( queue.Count > 0 )
						 {
							  Message message = queue.RemoveFirst();
							  _handlers[id].handle( message );
						 }
					}
			  }
		 }

		 private bool MessagesToBeProcessed()
		 {
			  foreach ( LinkedList<Message> queue in _messageQueues.Values )
			  {
					if ( queue.Count > 0 )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual void Disconnect( MemberId id )
		 {
			  _disconnectedMembers.Add( id );
		 }

		 public virtual void Reconnect( MemberId id )
		 {
			  _disconnectedMembers.remove( id );
		 }

		 public class Outbound : Neo4Net.causalclustering.messaging.Outbound<MemberId, RaftMessages_RaftMessage>
		 {
			 private readonly DirectNetworking _outerInstance;

			  internal readonly MemberId Me;

			  public Outbound( DirectNetworking outerInstance, MemberId me )
			  {
				  this._outerInstance = outerInstance;
					this.Me = me;
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public synchronized void send(org.neo4j.causalclustering.identity.MemberId to, final RaftMessages_RaftMessage message, boolean block)
			  public override void Send( MemberId to, RaftMessages_RaftMessage message, bool block )
			  {
				  lock ( this )
				  {
						if ( CanDeliver( to ) )
						{
							 outerInstance.messageQueues[to].AddLast( message );
						}
				  }
			  }

			  internal virtual bool CanDeliver( MemberId to )
			  {
					return outerInstance.messageQueues.ContainsKey( to ) && !outerInstance.disconnectedMembers.Contains( to ) && !outerInstance.disconnectedMembers.Contains( Me );
			  }
		 }

		 public class Inbound<M> : Neo4Net.causalclustering.messaging.Inbound<M> where M : Neo4Net.causalclustering.messaging.Message
		 {
			 private readonly DirectNetworking _outerInstance;

			  internal readonly MemberId Id;

			  public Inbound( DirectNetworking outerInstance, MemberId id )
			  {
				  this._outerInstance = outerInstance;
					this.Id = id;
			  }

			  public override void RegisterHandler( Neo4Net.causalclustering.messaging.Inbound_MessageHandler handler )
			  {
					outerInstance.handlers[Id] = handler;
					outerInstance.messageQueues[Id] = new LinkedList<Message>();
			  }
		 }
	}

}