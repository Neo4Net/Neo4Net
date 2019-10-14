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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;

	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class HighestTransactionIdTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHardSetHighest()
		 public virtual void ShouldHardSetHighest()
		 {
			  // GIVEN
			  HighestTransactionId highest = new HighestTransactionId( 10, 10, 10 );

			  // WHEN
			  highest.Set( 8, 1299128, 42 );

			  // THEN
			  assertEquals( new TransactionId( 8, 1299128, 42 ), highest.Get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyKeepTheHighestOffered()
		 public virtual void ShouldOnlyKeepTheHighestOffered()
		 {
			  // GIVEN
			  HighestTransactionId highest = new HighestTransactionId( -1, -1, -1 );

			  // WHEN/THEN
			  AssertAccepted( highest, 2 );
			  AssertAccepted( highest, 5 );
			  AssertRejected( highest, 3 );
			  AssertRejected( highest, 4 );
			  AssertAccepted( highest, 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepHighestDuringConcurrentOfferings() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepHighestDuringConcurrentOfferings()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighestTransactionId highest = new HighestTransactionId(-1, -1, -1);
			  HighestTransactionId highest = new HighestTransactionId( -1, -1, -1 );
			  Race race = new Race();
			  int updaters = max( 2, Runtime.availableProcessors() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger accepted = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger accepted = new AtomicInteger();
			  for ( int i = 0; i < updaters; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long id = i + 1;
					long id = i + 1;
					race.AddContestant(() =>
					{
					 if ( highest.Offer( id, id, id ) )
					 {
						  accepted.incrementAndGet();
					 }
					});
			  }

			  // WHEN
			  race.Go();

			  // THEN
			  assertTrue( accepted.get() > 0 );
			  assertEquals( updaters, highest.Get().transactionId() );
		 }

		 private void AssertAccepted( HighestTransactionId highest, long txId )
		 {
			  TransactionId current = highest.Get();
			  assertTrue( highest.Offer( txId, -1, -1 ) );
			  assertTrue( txId > current.TransactionIdConflict() );
		 }

		 private void AssertRejected( HighestTransactionId highest, long txId )
		 {
			  TransactionId current = highest.Get();
			  assertFalse( highest.Offer( txId, -1, -1 ) );
			  assertEquals( current, highest.Get() );
		 }
	}

}