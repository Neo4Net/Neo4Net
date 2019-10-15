﻿/*
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
namespace Neo4Net.cluster.statemachine
{

	/// <summary>
	/// Generate id's for state machine conversations. This should be shared between all state machines in a server.
	/// <para>
	/// These conversation id's can be used to uniquely identify conversations between distributed state machines.
	/// </para>
	/// </summary>
	public class StateMachineConversations
	{
		 private readonly AtomicLong _nextConversationId = new AtomicLong();
		 private readonly string _serverId;

		 public StateMachineConversations( InstanceId me )
		 {
			  _serverId = me.ToString();
		 }

		 public virtual string NextConversationId
		 {
			 get
			 {
				  return _serverId + "/" + _nextConversationId.incrementAndGet() + "#";
			 }
		 }
	}

}