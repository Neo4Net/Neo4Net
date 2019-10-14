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
namespace Neo4Net.Kernel.impl.transaction
{
	using TransactionCounters = Neo4Net.Kernel.impl.transaction.stats.TransactionCounters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	internal class TransactionCountersChecker
	{
		 private readonly long _numberOfActiveReadTransactions;
		 private readonly long _numberOfActiveWriteTransactions;
		 private readonly long _numberOfActiveTransactions;
		 private readonly long _numberOfCommittedReadTransactions;
		 private readonly long _numberOfCommittedWriteTransactions;
		 private readonly long _numberOfCommittedTransactions;
		 private readonly long _numberOfRolledBackReadTransactions;
		 private readonly long _numberOfRolledBackWriteTransactions;
		 private readonly long _numberOfRolledBackTransactions;
		 private readonly long _numberOfStartedTransactions;
		 private readonly long _numberOfTerminatedReadTransactions;
		 private readonly long _numberOfTerminatedWriteTransactions;
		 private readonly long _numberOfTerminatedTransactions;
		 private readonly long _peakConcurrentNumberOfTransactions;

		 internal TransactionCountersChecker( TransactionCounters pre )
		 {
			  // Active
			  _numberOfActiveReadTransactions = pre.NumberOfActiveReadTransactions;
			  _numberOfActiveWriteTransactions = pre.NumberOfActiveWriteTransactions;
			  _numberOfActiveTransactions = pre.NumberOfActiveTransactions;

			  assertEquals( _numberOfActiveTransactions, _numberOfActiveReadTransactions + _numberOfActiveWriteTransactions );

			  // Committed
			  _numberOfCommittedReadTransactions = pre.NumberOfCommittedReadTransactions;
			  _numberOfCommittedWriteTransactions = pre.NumberOfCommittedWriteTransactions;
			  _numberOfCommittedTransactions = pre.NumberOfCommittedTransactions;

			  assertEquals( _numberOfCommittedTransactions, _numberOfCommittedReadTransactions + _numberOfCommittedWriteTransactions );

			  // RolledBack
			  _numberOfRolledBackReadTransactions = pre.NumberOfRolledBackReadTransactions;
			  _numberOfRolledBackWriteTransactions = pre.NumberOfRolledBackWriteTransactions;
			  _numberOfRolledBackTransactions = pre.NumberOfRolledBackTransactions;

			  assertEquals( _numberOfRolledBackTransactions, _numberOfRolledBackReadTransactions + _numberOfRolledBackWriteTransactions );

			  // Terminated
			  _numberOfTerminatedReadTransactions = pre.NumberOfTerminatedReadTransactions;
			  _numberOfTerminatedWriteTransactions = pre.NumberOfTerminatedWriteTransactions;
			  _numberOfTerminatedTransactions = pre.NumberOfTerminatedTransactions;

			  assertEquals( _numberOfTerminatedTransactions, _numberOfTerminatedReadTransactions + _numberOfTerminatedWriteTransactions );

			  // started
			  _numberOfStartedTransactions = pre.NumberOfStartedTransactions;

			  // peak
			  _peakConcurrentNumberOfTransactions = pre.PeakConcurrentNumberOfTransactions;
		 }

		 public virtual void VerifyCommitted( bool isWriteTx, TransactionCounters post )
		 {
			  VerifyActiveAndStarted( post );
			  VerifyCommittedIncreasedBy( post, 1, isWriteTx );
			  VerifyRolledBackIncreasedBy( post, 0, isWriteTx );
			  VerifyTerminatedIncreasedBy( post, 0, isWriteTx );
		 }

		 public virtual void VerifyRolledBacked( bool isWriteTx, TransactionCounters post )
		 {
			  VerifyActiveAndStarted( post );
			  VerifyCommittedIncreasedBy( post, 0, isWriteTx );
			  VerifyRolledBackIncreasedBy( post, 1, isWriteTx );
			  VerifyTerminatedIncreasedBy( post, 0, isWriteTx );
		 }

		 public virtual void VerifyTerminated( bool isWriteTx, TransactionCounters post )
		 {
			  VerifyActiveAndStarted( post );
			  VerifyCommittedIncreasedBy( post, 0, isWriteTx );
			  VerifyRolledBackIncreasedBy( post, 1, isWriteTx );
			  VerifyTerminatedIncreasedBy( post, 1, isWriteTx );
		 }

		 private void VerifyCommittedIncreasedBy( TransactionCounters post, int diff, bool isWriteTx )
		 {
			  if ( isWriteTx )
			  {
					assertEquals( _numberOfCommittedReadTransactions, post.NumberOfCommittedReadTransactions );
					assertEquals( _numberOfCommittedWriteTransactions + diff, post.NumberOfCommittedWriteTransactions );
			  }
			  else
			  {
					assertEquals( _numberOfCommittedReadTransactions + diff, post.NumberOfCommittedReadTransactions );
					assertEquals( _numberOfCommittedWriteTransactions, post.NumberOfCommittedWriteTransactions );
			  }

			  assertEquals( _numberOfCommittedTransactions + diff, post.NumberOfCommittedTransactions );
		 }

		 private void VerifyRolledBackIncreasedBy( TransactionCounters post, int diff, bool isWriteTx )
		 {
			  if ( isWriteTx )
			  {
					assertEquals( _numberOfRolledBackReadTransactions, post.NumberOfRolledBackReadTransactions );
					assertEquals( _numberOfRolledBackWriteTransactions + diff, post.NumberOfRolledBackWriteTransactions );
			  }
			  else
			  {
					assertEquals( _numberOfRolledBackReadTransactions + diff, post.NumberOfRolledBackReadTransactions );
					assertEquals( _numberOfRolledBackWriteTransactions, post.NumberOfRolledBackWriteTransactions );
			  }

			  assertEquals( _numberOfRolledBackTransactions + diff, post.NumberOfRolledBackTransactions );
		 }

		 private void VerifyTerminatedIncreasedBy( TransactionCounters post, int diff, bool isWriteTx )
		 {
			  if ( isWriteTx )
			  {
					assertEquals( _numberOfTerminatedReadTransactions, post.NumberOfTerminatedReadTransactions );
					assertEquals( _numberOfTerminatedWriteTransactions + diff, post.NumberOfTerminatedWriteTransactions );
			  }
			  else
			  {
					assertEquals( _numberOfTerminatedReadTransactions + diff, post.NumberOfTerminatedReadTransactions );
					assertEquals( _numberOfTerminatedWriteTransactions, post.NumberOfTerminatedWriteTransactions );
			  }

			  assertEquals( _numberOfTerminatedTransactions + diff, post.NumberOfTerminatedTransactions );
		 }

		 private void VerifyActiveAndStarted( TransactionCounters post )
		 {
			  assertEquals( _numberOfActiveReadTransactions, post.NumberOfActiveReadTransactions );
			  assertEquals( _numberOfActiveWriteTransactions, post.NumberOfActiveWriteTransactions );
			  assertEquals( _numberOfActiveTransactions, post.NumberOfActiveTransactions );

			  assertEquals( _numberOfStartedTransactions + 1, post.NumberOfStartedTransactions );
			  assertEquals( Math.Max( 1, _peakConcurrentNumberOfTransactions ), post.PeakConcurrentNumberOfTransactions );
		 }
	}

}