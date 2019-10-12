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
namespace Org.Neo4j.Bolt.v3.runtime
{
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;

	/// <summary>
	/// When STREAMING, additionally attach bookmark to PULL_ALL, DISCARD_ALL result
	/// </summary>
	public class StreamingState : AbstractStreamingState
	{
		 public override string Name()
		 {
			  return "STREAMING";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.bolt.runtime.BoltStateMachineState processStreamResultMessage(boolean pull, org.neo4j.bolt.runtime.StateMachineContext context) throws Throwable
		 protected internal override BoltStateMachineState ProcessStreamResultMessage( bool pull, StateMachineContext context )
		 {
			  Bookmark bookmark = context.ConnectionState().StatementProcessor.streamResult(recordStream => context.ConnectionState().ResponseHandler.onRecords(recordStream, pull));
			  bookmark.AttachTo( context.ConnectionState() );
			  return ReadyStateConflict;
		 }
	}

}