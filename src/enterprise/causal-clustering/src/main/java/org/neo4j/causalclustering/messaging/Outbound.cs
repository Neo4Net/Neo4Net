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
namespace Neo4Net.causalclustering.messaging
{
	/// <summary>
	/// A best effort service for delivery of messages to members. No guarantees are made about any of the methods
	/// in terms of eventual delivery. The only non trivial promises is that no messages get duplicated and nothing gets
	/// delivered to the wrong host.
	/// </summary>
	/// @param <MEMBER> The type of members that messages will be sent to. </param>
	public interface Outbound<MEMBER, MESSAGE> where MESSAGE : Message
	{
		 /// <summary>
		 /// Asynchronous, best effort delivery to destination.
		 /// </summary>
		 /// <param name="to"> destination </param>
		 /// <param name="message"> The message to send </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void send(MEMBER to, MESSAGE message)
	//	 {
	//		  send(to, message, false);
	//	 }

		 /// <summary>
		 /// Best effort delivery to destination.
		 /// <para>
		 /// Blocking waits at least until the I/O operation
		 /// completes, but it might still have failed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="to"> destination </param>
		 /// <param name="message"> the message to send </param>
		 /// <param name="block"> whether to block until I/O completion </param>
		 void Send( MEMBER to, MESSAGE message, bool block );
	}

}