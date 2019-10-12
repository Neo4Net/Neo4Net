using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus
{

	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Message = Org.Neo4j.causalclustering.messaging.Message;
	using Org.Neo4j.causalclustering.messaging;

	public class OutboundMessageCollector : Outbound<MemberId, RaftMessages_RaftMessage>
	{
		 private IDictionary<MemberId, IList<RaftMessages_RaftMessage>> _sentMessages = new Dictionary<MemberId, IList<RaftMessages_RaftMessage>>();

		 public virtual void Clear()
		 {
			  _sentMessages.Clear();
		 }

		 public override void Send( MemberId to, RaftMessages_RaftMessage message, bool block )
		 {
			  RaftMessages( to ).Add( message );
		 }

		 private IList<RaftMessages_RaftMessage> RaftMessages( MemberId to )
		 {
			  return _sentMessages.computeIfAbsent( to, k => new List<>() );
		 }

		 public virtual IList<RaftMessages_RaftMessage> SentTo( MemberId member )
		 {
			  IList<RaftMessages_RaftMessage> messages = _sentMessages[member];

			  if ( messages == null )
			  {
					messages = new List<RaftMessages_RaftMessage>();
			  }

			  return messages;
		 }

		 public virtual bool HasAnyEntriesTo( MemberId member )
		 {
			  IList<RaftMessages_RaftMessage> messages = _sentMessages[member];
			  return messages != null && messages.Count != 0;
		 }

		 public virtual bool HasEntriesTo( MemberId member, params RaftLogEntry[] expectedMessages )
		 {
			  IList<RaftLogEntry> actualMessages = new List<RaftLogEntry>();

			  foreach ( Message message in SentTo( member ) )
			  {
					if ( message is RaftMessages_AppendEntries_Request )
					{
						 Collections.addAll( actualMessages, ( ( RaftMessages_AppendEntries_Request ) message ).Entries() );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  return actualMessages.containsAll( Arrays.asList( expectedMessages ) );
		 }
	}

}