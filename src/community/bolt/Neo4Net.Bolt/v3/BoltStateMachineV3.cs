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
namespace Neo4Net.Bolt.v3
{

	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using BoltStateMachineV1 = Neo4Net.Bolt.v1.runtime.BoltStateMachineV1;
	using ConnectedState = Neo4Net.Bolt.v3.runtime.ConnectedState;
	using FailedState = Neo4Net.Bolt.v3.runtime.FailedState;
	using InterruptedState = Neo4Net.Bolt.v3.runtime.InterruptedState;
	using ReadyState = Neo4Net.Bolt.v3.runtime.ReadyState;
	using StreamingState = Neo4Net.Bolt.v3.runtime.StreamingState;
	using TransactionReadyState = Neo4Net.Bolt.v3.runtime.TransactionReadyState;
	using TransactionStreamingState = Neo4Net.Bolt.v3.runtime.TransactionStreamingState;

	public class BoltStateMachineV3 : BoltStateMachineV1
	{
		 public BoltStateMachineV3( BoltStateMachineSPI boltSPI, BoltChannel boltChannel, Clock clock ) : base( boltSPI, boltChannel, clock )
		 {
		 }

		 protected internal override States BuildStates()
		 {
			  ConnectedState connected = new ConnectedState();
			  ReadyState ready = new ReadyState();
			  StreamingState streaming = new StreamingState();
			  TransactionReadyState txReady = new TransactionReadyState();
			  TransactionStreamingState txStreaming = new TransactionStreamingState();
			  FailedState failed = new FailedState();
			  InterruptedState interrupted = new InterruptedState();

			  connected.ReadyState = ready;

			  ready.TransactionReadyState = txReady;
			  ready.StreamingState = streaming;
			  ready.FailedState = failed;
			  ready.InterruptedState = interrupted;

			  streaming.ReadyState = ready;
			  streaming.FailedState = failed;
			  streaming.InterruptedState = interrupted;

			  txReady.ReadyState = ready;
			  txReady.TransactionStreamingState = txStreaming;
			  txReady.FailedState = failed;
			  txReady.InterruptedState = interrupted;

			  txStreaming.ReadyState = txReady;
			  txStreaming.FailedState = failed;
			  txStreaming.InterruptedState = interrupted;

			  failed.InterruptedState = interrupted;

			  interrupted.ReadyState = ready;

			  return new States( connected, failed );
		 }
	}

}