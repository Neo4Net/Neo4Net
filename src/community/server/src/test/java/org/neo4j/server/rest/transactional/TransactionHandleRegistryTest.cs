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
namespace Neo4Net.Server.rest.transactional
{
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using InvalidConcurrentTransactionAccess = Neo4Net.Server.rest.transactional.error.InvalidConcurrentTransactionAccess;
	using InvalidTransactionId = Neo4Net.Server.rest.transactional.error.InvalidTransactionId;
	using TransactionLifecycleException = Neo4Net.Server.rest.transactional.error.TransactionLifecycleException;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class TransactionHandleRegistryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTransactionId()
		 public virtual void ShouldGenerateTransactionId()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  TransactionHandleRegistry registry = new TransactionHandleRegistry( Clocks.fakeClock(), 0, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  // when
			  long id1 = registry.Begin( handle );
			  long id2 = registry.Begin( handle );

			  // then
			  assertNotEquals( id1, id2 );
			  logProvider.AssertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreSuspendedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreSuspendedTransaction()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  TransactionHandleRegistry registry = new TransactionHandleRegistry( Clocks.fakeClock(), 0, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  long id = registry.Begin( handle );

			  // When
			  registry.Release( id, handle );
			  TransactionHandle acquiredHandle = registry.Acquire( id );

			  // Then
			  assertSame( handle, acquiredHandle );
			  logProvider.AssertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquiringATransactionThatHasAlreadyBeenAcquiredShouldThrowInvalidConcurrentTransactionAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquiringATransactionThatHasAlreadyBeenAcquiredShouldThrowInvalidConcurrentTransactionAccess()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  TransactionHandleRegistry registry = new TransactionHandleRegistry( Clocks.fakeClock(), 0, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  long id = registry.Begin( handle );
			  registry.Release( id, handle );
			  registry.Acquire( id );

			  // When
			  try
			  {
					registry.Acquire( id );
					fail( "Should have thrown exception" );
			  }
			  catch ( InvalidConcurrentTransactionAccess )
			  {
					// expected
			  }

			  // then
			  logProvider.AssertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquiringANonExistentTransactionShouldThrowErrorInvalidTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquiringANonExistentTransactionShouldThrowErrorInvalidTransactionId()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  TransactionHandleRegistry registry = new TransactionHandleRegistry( Clocks.fakeClock(), 0, logProvider );

			  long madeUpTransactionId = 1337;

			  // When
			  try
			  {
					registry.Acquire( madeUpTransactionId );
					fail( "Should have thrown exception" );
			  }
			  catch ( InvalidTransactionId )
			  {
					// expected
			  }

			  // then
			  logProvider.AssertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionsShouldBeEvictedWhenUnusedLongerThanTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionsShouldBeEvictedWhenUnusedLongerThanTimeout()
		 {
			  // Given
			  FakeClock clock = Clocks.fakeClock();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  TransactionHandleRegistry registry = new TransactionHandleRegistry( clock, 0, logProvider );
			  TransactionHandle oldTx = mock( typeof( TransactionHandle ) );
			  TransactionHandle newTx = mock( typeof( TransactionHandle ) );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  long txId1 = registry.Begin( handle );
			  long txId2 = registry.Begin( handle );

			  // And given one transaction was stored one minute ago, and another was stored just now
			  registry.Release( txId1, oldTx );
			  clock.Forward( 1, TimeUnit.MINUTES );
			  registry.Release( txId2, newTx );

			  // When
			  registry.RollbackSuspendedTransactionsIdleSince( clock.Millis() - 1000 );

			  // Then
			  assertThat( registry.Acquire( txId2 ), equalTo( newTx ) );

			  // And then the other should have been evicted
			  try
			  {
					registry.Acquire( txId1 );
					fail( "Should have thrown exception" );
			  }
			  catch ( InvalidTransactionId )
			  {
					// ok
			  }

			  logProvider.AssertExactly( inLog( typeof( TransactionHandleRegistry ) ).info( "Transaction with id 1 has been automatically rolled " + "back due to transaction timeout." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void expiryTimeShouldBeSetToCurrentTimePlusTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExpiryTimeShouldBeSetToCurrentTimePlusTimeout()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FakeClock clock = Clocks.fakeClock();
			  int timeoutLength = 123;

			  TransactionHandleRegistry registry = new TransactionHandleRegistry( clock, timeoutLength, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  long id = registry.Begin( handle );

			  // When
			  long timesOutAt = registry.Release( id, handle );

			  // Then
			  assertThat( timesOutAt, equalTo( clock.Millis() + timeoutLength ) );

			  // And when
			  clock.Forward( 1337, TimeUnit.MILLISECONDS );
			  registry.Acquire( id );
			  timesOutAt = registry.Release( id, handle );

			  // Then
			  assertThat( timesOutAt, equalTo( clock.Millis() + timeoutLength ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideInterruptHandlerForActiveTransaction() throws org.neo4j.server.rest.transactional.error.TransactionLifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideInterruptHandlerForActiveTransaction()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FakeClock clock = Clocks.fakeClock();
			  int timeoutLength = 123;

			  TransactionHandleRegistry registry = new TransactionHandleRegistry( clock, timeoutLength, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  // Active Tx in Registry
			  long id = registry.Begin( handle );

			  // When
			  registry.Terminate( id );

			  // Then
			  verify( handle, times( 1 ) ).terminate();
			  verifyNoMoreInteractions( handle );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideInterruptHandlerForSuspendedTransaction() throws org.neo4j.server.rest.transactional.error.TransactionLifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideInterruptHandlerForSuspendedTransaction()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FakeClock clock = Clocks.fakeClock();
			  int timeoutLength = 123;

			  TransactionHandleRegistry registry = new TransactionHandleRegistry( clock, timeoutLength, logProvider );
			  TransactionHandle handle = mock( typeof( TransactionHandle ) );

			  // Suspended Tx in Registry
			  long id = registry.Begin( handle );
			  registry.Release( id, handle );

			  // When
			  registry.Terminate( id );

			  // Then
			  verify( handle, times( 1 ) ).terminate();
			  verifyNoMoreInteractions( handle );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.transactional.error.InvalidTransactionId.class) public void gettingInterruptHandlerForUnknownIdShouldThrowErrorInvalidTransactionId() throws org.neo4j.server.rest.transactional.error.TransactionLifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GettingInterruptHandlerForUnknownIdShouldThrowErrorInvalidTransactionId()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  FakeClock clock = Clocks.fakeClock();
			  int timeoutLength = 123;

			  TransactionHandleRegistry registry = new TransactionHandleRegistry( clock, timeoutLength, logProvider );

			  // When
			  registry.Terminate( 456 );
		 }
	}

}