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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointThreshold_Fields.DEFAULT_CHECKING_FREQUENCY_MILLIS;

	public class CheckPointThresholdTest : CheckPointThresholdTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCreateThresholdThatTriggersAfterTransactionCount()
		 public virtual void MustCreateThresholdThatTriggersAfterTransactionCount()
		 {
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 1 ); // Initialise at transaction id offset by 1.

			  // False because we're not yet at threshold.
			  assertFalse( threshold.IsCheckPointingNeeded( IntervalTx - 1, NotTriggered ) );
			  // Still false because the counter is offset by one, since we initialised with 1.
			  assertFalse( threshold.IsCheckPointingNeeded( IntervalTx, NotTriggered ) );
			  // True because new we're at intervalTx + initial offset.
			  assertTrue( threshold.IsCheckPointingNeeded( IntervalTx + 1, Triggered ) );
			  VerifyTriggered( "count" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCreateThresholdThatTriggersAfterTime()
		 public virtual void MustCreateThresholdThatTriggersAfterTime()
		 {
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 1 );
			  // Skip the initial wait period.
			  Clock.forward( IntervalTime.toMillis(), MILLISECONDS );
			  // The clock will trigger at a random point within the interval in the future.

			  // False because we haven't moved the clock, or the transaction count.
			  assertFalse( threshold.IsCheckPointingNeeded( 2, NotTriggered ) );
			  // True because we now moved forward by an interval.
			  Clock.forward( IntervalTime.toMillis(), MILLISECONDS );
			  assertTrue( threshold.IsCheckPointingNeeded( 4, Triggered ) );
			  VerifyTriggered( "time" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerBeforeTimeWithTooFewCommittedTransactions()
		 public virtual void MustNotTriggerBeforeTimeWithTooFewCommittedTransactions()
		 {
			  WithIntervalTime( "100ms" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  Clock.forward( 50, MILLISECONDS );
			  assertFalse( threshold.IsCheckPointingNeeded( 42, NotTriggered ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustTriggerWhenTimeThresholdIsReachedAndThereAreCommittedTransactions()
		 public virtual void MustTriggerWhenTimeThresholdIsReachedAndThereAreCommittedTransactions()
		 {
			  WithIntervalTime( "100ms" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  Clock.forward( 199, MILLISECONDS );

			  assertTrue( threshold.IsCheckPointingNeeded( 42, Triggered ) );
			  VerifyTriggered( "time" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerWhenTimeThresholdIsReachedAndThereAreNoCommittedTransactions()
		 public virtual void MustNotTriggerWhenTimeThresholdIsReachedAndThereAreNoCommittedTransactions()
		 {
			  WithIntervalTime( "100ms" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 42 );

			  Clock.forward( 199, MILLISECONDS );

			  assertFalse( threshold.IsCheckPointingNeeded( 42, NotTriggered ) );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerPastTimeThresholdSinceLastCheckpointWithNoNewTransactions()
		 public virtual void MustNotTriggerPastTimeThresholdSinceLastCheckpointWithNoNewTransactions()
		 {
			  WithIntervalTime( "100ms" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  Clock.forward( 199, MILLISECONDS );
			  threshold.CheckPointHappened( 42 );
			  Clock.forward( 100, MILLISECONDS );

			  assertFalse( threshold.IsCheckPointingNeeded( 42, NotTriggered ) );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustTriggerPastTimeThresholdSinceLastCheckpointWithNewTransactions()
		 public virtual void MustTriggerPastTimeThresholdSinceLastCheckpointWithNewTransactions()
		 {
			  WithIntervalTime( "100ms" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  Clock.forward( 199, MILLISECONDS );
			  threshold.CheckPointHappened( 42 );
			  Clock.forward( 100, MILLISECONDS );

			  assertTrue( threshold.IsCheckPointingNeeded( 43, Triggered ) );
			  VerifyTriggered( "time" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerOnTransactionCountWhenThereAreNoNewTransactions()
		 public virtual void MustNotTriggerOnTransactionCountWhenThereAreNoNewTransactions()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  assertFalse( threshold.IsCheckPointingNeeded( 2, NotTriggered ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerOnTransactionCountWhenCountIsBellowThreshold()
		 public virtual void MustNotTriggerOnTransactionCountWhenCountIsBellowThreshold()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  assertFalse( threshold.IsCheckPointingNeeded( 3, NotTriggered ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustTriggerOnTransactionCountWhenCountIsAtThreshold()
		 public virtual void MustTriggerOnTransactionCountWhenCountIsAtThreshold()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  assertTrue( threshold.IsCheckPointingNeeded( 4, Triggered ) );
			  VerifyTriggered( "count" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerOnTransactionCountAtThresholdIfCheckPointAlreadyHappened()
		 public virtual void MustNotTriggerOnTransactionCountAtThresholdIfCheckPointAlreadyHappened()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  threshold.CheckPointHappened( 4 );
			  assertFalse( threshold.IsCheckPointingNeeded( 4, NotTriggered ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotTriggerWhenTransactionCountIsWithinThresholdSinceLastTrigger()
		 public virtual void MustNotTriggerWhenTransactionCountIsWithinThresholdSinceLastTrigger()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  threshold.CheckPointHappened( 4 );
			  assertFalse( threshold.IsCheckPointingNeeded( 5, NotTriggered ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustTriggerOnTransactionCountWhenCountIsAtThresholdSinceLastCheckPoint()
		 public virtual void MustTriggerOnTransactionCountWhenCountIsAtThresholdSinceLastCheckPoint()
		 {
			  WithIntervalTx( 2 );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  threshold.CheckPointHappened( 4 );
			  assertTrue( threshold.IsCheckPointingNeeded( 6, Triggered ) );
			  VerifyTriggered( "count" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") @Test public void timeBasedThresholdMustSuggestSchedulingFrequency()
		 public virtual void TimeBasedThresholdMustSuggestSchedulingFrequency()
		 {
			  // By default, the transaction count based threshold wants a higher check frequency than the time based
			  // default threshold.
			  assertThat( CreateThreshold().checkFrequencyMillis(), @is(DEFAULT_CHECKING_FREQUENCY_MILLIS) );

			  WithIntervalTime( "100ms" );
			  assertThat( CreateThreshold().checkFrequencyMillis(), @is(100L) );
		 }
	}

}