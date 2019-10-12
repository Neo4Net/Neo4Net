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
namespace Neo4Net.cluster.statemachine
{
	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Implemented by states in a state machine. Each state must
	/// implement the handle method, to perform different things depending
	/// on the message that comes in. This should only be implemented as enums.
	/// <para>
	/// A state is guaranteed to only have one handle at a time, i.e. access is serialized.
	/// </para>
	/// </summary>
	public interface State<CONTEXT, MESSAGETYPE> where MESSAGETYPE : Neo4Net.cluster.com.message.MessageType
	{
		 /// <summary>
		 /// Handle a message. The state can use context for state storage/retrieval and it will also act
		 /// as a facade to the rest of the system. The MessageProcessor is used to trigger new messages.
		 /// When the handling is done the state returns the next state of the state machine.
		 /// 
		 /// </summary>
		 /// <param name="context">  that contains state and methods to access other parts of the system </param>
		 /// <param name="message">  that needs to be handled </param>
		 /// <param name="outgoing"> processor for new messages created by the handling of this message </param>
		 /// <returns> the new state </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: State<CONTEXT, MESSAGETYPE> handle(CONTEXT context, org.neo4j.cluster.com.message.Message<MESSAGETYPE> message, org.neo4j.cluster.com.message.MessageHolder outgoing) throws Throwable;
		 State<CONTEXT, MESSAGETYPE> Handle( CONTEXT context, Message<MESSAGETYPE> message, MessageHolder outgoing );
	}

}