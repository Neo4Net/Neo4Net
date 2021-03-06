﻿/*
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
namespace Org.Neo4j.Bolt.v1.runtime
{

	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Org.Neo4j.Bolt.runtime.BoltStateMachineSPI;
	using TransactionStateMachineSPI = Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationException = Org.Neo4j.Bolt.security.auth.AuthenticationException;
	using BoltTestUtil = Org.Neo4j.Bolt.testing.BoltTestUtil;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.hasTransaction;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;

	/// <summary>
	/// Helpers for testing the <seealso cref="BoltStateMachine"/>.
	/// </summary>
	public class MachineRoom
	{
		 internal static readonly MapValue EmptyParams = VirtualValues.EMPTY_MAP;
		 internal const string USER_AGENT = "BoltStateMachineV1Test/0.0";

		 private MachineRoom()
		 {
		 }

		 public static BoltStateMachine NewMachine()
		 {
			  return NewMachine( mock( typeof( BoltStateMachineV1SPI ), RETURNS_MOCKS ) );
		 }

		 public static BoltStateMachine NewMachine( BoltStateMachineV1SPI spi )
		 {
			  BoltChannel boltChannel = BoltTestUtil.newTestBoltChannel();
			  return new BoltStateMachineV1( spi, boltChannel, Clock.systemUTC() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.bolt.runtime.BoltStateMachine newMachineWithTransaction() throws org.neo4j.bolt.security.auth.AuthenticationException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 public static BoltStateMachine NewMachineWithTransaction()
		 {
			  BoltStateMachine machine = NewMachine();
			  Init( machine );
			  RunBegin( machine );
			  return machine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.bolt.runtime.BoltStateMachine newMachineWithTransactionSPI(org.neo4j.bolt.runtime.TransactionStateMachineSPI transactionSPI) throws org.neo4j.bolt.security.auth.AuthenticationException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 public static BoltStateMachine NewMachineWithTransactionSPI( TransactionStateMachineSPI transactionSPI )
		 {
			  BoltStateMachineSPI spi = mock( typeof( BoltStateMachineSPI ), RETURNS_MOCKS );
			  when( spi.TransactionSpi() ).thenReturn(transactionSPI);

			  BoltChannel boltChannel = BoltTestUtil.newTestBoltChannel();
			  BoltStateMachine machine = new BoltStateMachineV1( spi, boltChannel, Clock.systemUTC() );
			  Init( machine );
			  return machine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.bolt.runtime.BoltStateMachine init(org.neo4j.bolt.runtime.BoltStateMachine machine) throws org.neo4j.bolt.security.auth.AuthenticationException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 public static BoltStateMachine Init( BoltStateMachine machine )
		 {
			  return Init( machine, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.bolt.runtime.BoltStateMachine init(org.neo4j.bolt.runtime.BoltStateMachine machine, String owner) throws org.neo4j.bolt.security.auth.AuthenticationException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 private static BoltStateMachine Init( BoltStateMachine machine, string owner )
		 {
			  machine.Process( new InitMessage( USER_AGENT, string.ReferenceEquals( owner, null ) ? emptyMap() : singletonMap(Org.Neo4j.Kernel.api.security.AuthToken_Fields.PRINCIPAL, owner) ), nullResponseHandler() );
			  return machine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runBegin(org.neo4j.bolt.runtime.BoltStateMachine machine) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private static void RunBegin( BoltStateMachine machine )
		 {
			  machine.Process( new RunMessage( "BEGIN", EmptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  assertThat( machine, hasTransaction() );
		 }

	}

}