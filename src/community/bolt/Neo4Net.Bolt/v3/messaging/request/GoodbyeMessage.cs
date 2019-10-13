/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.v3.messaging.request
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;

	/// <summary>
	/// On decoding of a <seealso cref="GoodbyeMessage"/>, we immediately stop whatever the connection is doing and shut down this connection.
	/// As the <seealso cref="BoltStateMachine"/> with this connection will also shut down at the same time,
	/// this message will actually NEVER be handled by <seealso cref="BoltStateMachine"/>.
	/// </summary>
	public class GoodbyeMessage : RequestMessage
	{
		 public const sbyte SIGNATURE = 0x02;
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static readonly GoodbyeMessage GoodbyeMessageConflict = new GoodbyeMessage();

		 private GoodbyeMessage()
		 {
			  // left empty on purpose
		 }

		 public override bool SafeToProcessInAnyState()
		 {
			  return true;
		 }

		 public override string ToString()
		 {
			  return "GOODBYE";
		 }
	}

}