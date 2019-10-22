using System;

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
namespace Neo4Net.Bolt.v1.messaging
{

	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;

	public class BoltStateMachineV1Context : StateMachineContext
	{
		 private readonly BoltStateMachine _machine;
		 private readonly BoltChannel _boltChannel;
		 private readonly BoltStateMachineSPI _spi;
		 private readonly MutableConnectionState _connectionState;
		 private readonly Clock _clock;

		 public BoltStateMachineV1Context( BoltStateMachine machine, BoltChannel boltChannel, BoltStateMachineSPI spi, MutableConnectionState connectionState, Clock clock )
		 {
			  this._machine = machine;
			  this._boltChannel = boltChannel;
			  this._spi = spi;
			  this._connectionState = connectionState;
			  this._clock = clock;
		 }

		 public override void AuthenticatedAsUser( string username, string userAgent )
		 {
			  _boltChannel.updateUser( username, userAgent );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handleFailure(Throwable cause, boolean fatal) throws org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 public override void HandleFailure( Exception cause, bool fatal )
		 {
			  _machine.handleFailure( cause, fatal );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean resetMachine() throws org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 public override bool ResetMachine()
		 {
			  return _machine.reset();
		 }

		 public override BoltStateMachineSPI BoltSpi()
		 {
			  return _spi;
		 }

		 public override MutableConnectionState ConnectionState()
		 {
			  return _connectionState;
		 }

		 public override Clock Clock()
		 {
			  return _clock;
		 }

		 public override string ConnectionId()
		 {
			  return _machine.id();
		 }
	}

}