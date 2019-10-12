/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.v3.messaging.request
{
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;

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