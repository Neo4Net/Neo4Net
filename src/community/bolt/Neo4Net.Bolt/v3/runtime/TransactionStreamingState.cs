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
namespace Neo4Net.Bolt.v3.runtime
{
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;

	public class TransactionStreamingState : AbstractStreamingState
	{
		 public override string Name()
		 {
			  return "TX_STREAMING";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.bolt.runtime.BoltStateMachineState processStreamResultMessage(boolean pull, org.Neo4Net.bolt.runtime.StateMachineContext context) throws Throwable
		 protected internal override BoltStateMachineState ProcessStreamResultMessage( bool pull, StateMachineContext context )
		 {
			  context.ConnectionState().StatementProcessor.streamResult(recordStream => context.ConnectionState().ResponseHandler.onRecords(recordStream, pull));
			  return ReadyStateConflict;
		 }
	}

}