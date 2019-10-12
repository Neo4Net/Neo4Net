using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com.storecopy
{
	using Test = org.junit.Test;


	using Org.Neo4j.com;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using Org.Neo4j.Helpers.Collection;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using SimpleTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.SimpleTransactionIdStore;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionCursor = Org.Neo4j.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;

	public class ResponsePackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveFixedTargetTransactionIdEvenIfLastTransactionIdIsMoving() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveFixedTargetTransactionIdEvenIfLastTransactionIdIsMoving()
		 {
			  // GIVEN
			  LogicalTransactionStore transactionStore = mock( typeof( LogicalTransactionStore ) );
			  long lastAppliedTransactionId = 5L;
			  TransactionCursor endlessCursor = new EndlessCursor( this, lastAppliedTransactionId + 1 );
			  when( transactionStore.getTransactions( anyLong() ) ).thenReturn(endlessCursor);
			  const long targetTransactionId = 8L;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.TransactionIdStore transactionIdStore = new org.neo4j.kernel.impl.transaction.SimpleTransactionIdStore(targetTransactionId, 0, BASE_TX_COMMIT_TIMESTAMP, 0, 0);
			  TransactionIdStore transactionIdStore = new SimpleTransactionIdStore( targetTransactionId, 0, BASE_TX_COMMIT_TIMESTAMP, 0, 0 );
			  ResponsePacker packer = new ResponsePacker( transactionStore, transactionIdStore, Suppliers.singleton( StoreIdTestFactory.newStoreIdForCurrentVersion() ) );

			  // WHEN
			  Response<object> response = packer.PackTransactionStreamResponse( RequestContextStartingAt( 5L ), null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong nextExpectedVisit = new java.util.concurrent.atomic.AtomicLong(lastAppliedTransactionId);
			  AtomicLong nextExpectedVisit = new AtomicLong( lastAppliedTransactionId );
			  response.Accept( new HandlerAnonymousInnerClass( this, targetTransactionId, transactionIdStore, nextExpectedVisit ) );
		 }

		 private class HandlerAnonymousInnerClass : Response.Handler
		 {
			 private readonly ResponsePackerTest _outerInstance;

			 private long _targetTransactionId;
			 private TransactionIdStore _transactionIdStore;
			 private AtomicLong _nextExpectedVisit;

			 public HandlerAnonymousInnerClass( ResponsePackerTest outerInstance, long targetTransactionId, TransactionIdStore transactionIdStore, AtomicLong nextExpectedVisit )
			 {
				 this.outerInstance = outerInstance;
				 this._targetTransactionId = targetTransactionId;
				 this._transactionIdStore = transactionIdStore;
				 this._nextExpectedVisit = nextExpectedVisit;
			 }

			 public void obligation( long txId )
			 {
				  fail( "Should not be called" );
			 }

			 public Visitor<CommittedTransactionRepresentation, Exception> transactions()
			 {
				  return element =>
				  {
					// THEN
					long txId = element.CommitEntry.TxId;
					assertThat( txId, lessThanOrEqualTo( _targetTransactionId ) );
					assertEquals( _nextExpectedVisit.incrementAndGet(), txId );

					// Move the target transaction id forward one step, effectively always keeping it out of reach
					_transactionIdStore.setLastCommittedAndClosedTransactionId( _transactionIdStore.LastCommittedTransactionId + 1, 0, BASE_TX_COMMIT_TIMESTAMP, 3, 4 );
					return true;
				  };
			 }
		 }

		 private RequestContext RequestContextStartingAt( long txId )
		 {
			  return new RequestContext( 0, 0, 0, txId, 0 );
		 }

		 public class EndlessCursor : TransactionCursor
		 {
			 private readonly ResponsePackerTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LogPosition PositionConflict = new LogPosition( 0, 0 );
			  internal long TxId;
			  internal CommittedTransactionRepresentation Transaction;

			  public EndlessCursor( ResponsePackerTest outerInstance, long txId )
			  {
				  this._outerInstance = outerInstance;
					this.TxId = txId;
			  }

			  public override void Close()
			  {
			  }

			  public override CommittedTransactionRepresentation Get()
			  {
					return Transaction;
			  }

			  public override bool Next()
			  {
					Transaction = new CommittedTransactionRepresentation( null, null, new LogEntryCommit( TxId++, 0 ) );
					return true;
			  }

			  public override LogPosition Position()
			  {
					return PositionConflict;
			  }
		 }
	}

}