﻿using System;

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
	using Channel = io.netty.channel.Channel;
	using Test = org.junit.jupiter.api.Test;

	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Org.Neo4j.Bolt.runtime.BoltStateMachineSPI;
	using MutableConnectionState = Org.Neo4j.Bolt.runtime.MutableConnectionState;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class BoltStateMachineV1ContextTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFailure() throws org.neo4j.bolt.runtime.BoltConnectionFatality
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleFailure()
		 {
			  BoltStateMachine machine = mock( typeof( BoltStateMachine ) );
			  BoltStateMachineV1Context context = NewContext( machine, mock( typeof( BoltStateMachineSPI ) ) );

			  Exception cause = new Exception();
			  context.HandleFailure( cause, true );

			  verify( machine ).handleFailure( cause, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetMachine() throws org.neo4j.bolt.runtime.BoltConnectionFatality
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldResetMachine()
		 {
			  BoltStateMachine machine = mock( typeof( BoltStateMachine ) );
			  BoltStateMachineV1Context context = NewContext( machine, mock( typeof( BoltStateMachineSPI ) ) );

			  context.ResetMachine();

			  verify( machine ).reset();
		 }

		 private static BoltStateMachineV1Context NewContext( BoltStateMachine machine, BoltStateMachineSPI boltSPI )
		 {
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "bolt", mock( typeof( Channel ) ) );
			  return new BoltStateMachineV1Context( machine, boltChannel, boltSPI, new MutableConnectionState(), Clock.systemUTC() );
		 }
	}

}