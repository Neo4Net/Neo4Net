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
namespace Neo4Net.Kernel.api.txtracking
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class TransactionIdTrackerTest
	{
		 private static readonly Duration _defaultDuration = ofSeconds( 10 );

		 private readonly TransactionIdStore _transactionIdStore = mock( typeof( TransactionIdStore ) );
		 private readonly AvailabilityGuard _databaseAvailabilityGuard = mock( typeof( DatabaseAvailabilityGuard ) );

		 private TransactionIdTracker _transactionIdTracker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _databaseAvailabilityGuard.Available ).thenReturn( true );
			  _transactionIdTracker = new TransactionIdTracker( () => _transactionIdStore, _databaseAvailabilityGuard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnImmediatelyForBaseTxIdOrLess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnImmediatelyForBaseTxIdOrLess()
		 {
			  // when
			  _transactionIdTracker.awaitUpToDate( BASE_TX_ID, ofSeconds( 5 ) );

			  // then
			  verify( _transactionIdStore, never() ).awaitClosedTransactionId(anyLong(), anyLong());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitForRequestedVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForRequestedVersion()
		 {
			  // given
			  long version = 5L;

			  // when
			  _transactionIdTracker.awaitUpToDate( version, _defaultDuration );

			  // then
			  verify( _transactionIdStore ).awaitClosedTransactionId( version, _defaultDuration.toMillis() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateTimeoutException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateTimeoutException()
		 {
			  // given
			  long version = 5L;
			  TimeoutException timeoutException = new TimeoutException();
			  doThrow( timeoutException ).when( _transactionIdStore ).awaitClosedTransactionId( anyLong(), anyLong() );

			  try
			  {
					// when
					_transactionIdTracker.awaitUpToDate( version + 1, ofMillis( 50 ) );
					fail( "should have thrown" );
			  }
			  catch ( TransactionFailureException ex )
			  {
					// then
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.InstanceStateChanged, ex.Status() );
					assertEquals( timeoutException, ex.InnerException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWaitIfTheDatabaseIsUnavailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWaitIfTheDatabaseIsUnavailable()
		 {
			  // given
			  when( _databaseAvailabilityGuard.Available ).thenReturn( false );

			  try
			  {
					// when
					_transactionIdTracker.awaitUpToDate( 1000, ofMillis( 60_000 ) );
					fail( "should have thrown" );
			  }
			  catch ( TransactionFailureException ex )
			  {
					// then
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, ex.Status() );
			  }

			  verify( _transactionIdStore, never() ).awaitClosedTransactionId(anyLong(), anyLong());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNewestTransactionId()
		 public virtual void ShouldReturnNewestTransactionId()
		 {
			  when( _transactionIdStore.LastClosedTransactionId ).thenReturn( 42L );
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 4242L );

			  assertEquals( 4242L, _transactionIdTracker.newestEncounteredTxId() );
		 }
	}

}