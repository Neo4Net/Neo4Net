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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.testing.BoltMatchers.hasTransaction;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.testing.NullResponseHandler.nullResponseHandler;

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
//ORIGINAL LINE: public static org.Neo4Net.bolt.runtime.BoltStateMachine newMachineWithTransaction() throws org.Neo4Net.bolt.security.auth.AuthenticationException, org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 public static BoltStateMachine NewMachineWithTransaction()
		 {
			  BoltStateMachine machine = NewMachine();
			  Init( machine );
			  RunBegin( machine );
			  return machine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.Neo4Net.bolt.runtime.BoltStateMachine newMachineWithTransactionSPI(org.Neo4Net.bolt.runtime.TransactionStateMachineSPI transactionSPI) throws org.Neo4Net.bolt.security.auth.AuthenticationException, org.Neo4Net.bolt.runtime.BoltConnectionFatality
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
//ORIGINAL LINE: public static org.Neo4Net.bolt.runtime.BoltStateMachine init(org.Neo4Net.bolt.runtime.BoltStateMachine machine) throws org.Neo4Net.bolt.security.auth.AuthenticationException, org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 public static BoltStateMachine Init( BoltStateMachine machine )
		 {
			  return Init( machine, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.Neo4Net.bolt.runtime.BoltStateMachine init(org.Neo4Net.bolt.runtime.BoltStateMachine machine, String owner) throws org.Neo4Net.bolt.security.auth.AuthenticationException, org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 private static BoltStateMachine Init( BoltStateMachine machine, string owner )
		 {
			  machine.Process( new InitMessage( USER_AGENT, string.ReferenceEquals( owner, null ) ? emptyMap() : singletonMap(Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, owner) ), nullResponseHandler() );
			  return machine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runBegin(org.Neo4Net.bolt.runtime.BoltStateMachine machine) throws org.Neo4Net.bolt.runtime.BoltConnectionFatality
		 private static void RunBegin( BoltStateMachine machine )
		 {
			  machine.Process( new RunMessage( "BEGIN", EmptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  assertThat( machine, hasTransaction() );
		 }

	}

}