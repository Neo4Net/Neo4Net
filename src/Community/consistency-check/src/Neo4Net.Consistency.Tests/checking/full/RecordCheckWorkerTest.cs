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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;


	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class RecordCheckWorkerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDoProcessingInitializationInOrder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDoProcessingInitializationInOrder()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.Race race = new Neo4Net.test.Race();
			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger coordination = new java.util.concurrent.atomic.AtomicInteger(-1);
			  AtomicInteger coordination = new AtomicInteger( -1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger expected = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger expected = new AtomicInteger();
			  const int threads = 30;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") final RecordCheckWorker<int>[] workers = new RecordCheckWorker[threads];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  RecordCheckWorker<int>[] workers = new RecordCheckWorker[threads];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordProcessor<int> processor = new RecordProcessor_Adapter<int>()
			  RecordProcessor<int> processor = new RecordProcessor_AdapterAnonymousInnerClass( this, expected );
			  for ( int id = 0; id < threads; id++ )
			  {
					ArrayBlockingQueue<int> queue = new ArrayBlockingQueue<int>( 10 );
					race.AddContestant( workers[id] = new RecordCheckWorker<int>( id, coordination, queue, processor ) );
			  }
			  race.AddContestant(() =>
			  {
				try
				{
					 long end = currentTimeMillis() + SECONDS.toMillis(100);
					 while ( currentTimeMillis() < end && expected.get() < threads )
					 {
						  parkNanos( MILLISECONDS.toNanos( 10 ) );
					 }
					 assertEquals( threads, expected.get() );
				}
				finally
				{
					 foreach ( RecordCheckWorker<int> worker in workers )
					 {
						  worker.Done();
					 }
				}
			  });

			  // WHEN
			  race.Go();
		 }

		 private class RecordProcessor_AdapterAnonymousInnerClass : RecordProcessor_Adapter<int>
		 {
			 private readonly RecordCheckWorkerTest _outerInstance;

			 private AtomicInteger _expected;

			 public RecordProcessor_AdapterAnonymousInnerClass( RecordCheckWorkerTest outerInstance, AtomicInteger expected )
			 {
				 this.outerInstance = outerInstance;
				 this._expected = expected;
			 }

			 public override void process( int? record )
			 {
				  // We're testing init() here, not really process()
			 }

			 public override void init( int id )
			 {
				  assertEquals( id, _expected.getAndAdd( 1 ) );
			 }
		 }
	}

}