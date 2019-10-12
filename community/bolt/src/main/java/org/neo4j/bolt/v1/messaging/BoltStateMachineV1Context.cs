using System;

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
namespace Org.Neo4j.Bolt.v1.messaging
{

	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Org.Neo4j.Bolt.runtime.BoltStateMachineSPI;
	using MutableConnectionState = Org.Neo4j.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;

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
//ORIGINAL LINE: public void handleFailure(Throwable cause, boolean fatal) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override void HandleFailure( Exception cause, bool fatal )
		 {
			  _machine.handleFailure( cause, fatal );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean resetMachine() throws org.neo4j.bolt.runtime.BoltConnectionFatality
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