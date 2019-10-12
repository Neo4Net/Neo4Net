using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Test = org.junit.Test;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Race = Org.Neo4j.Test.Race;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.LOCAL_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.LOCAL_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.ZONED_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.ZONED_TIME;

	public class TemporalIndexCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateOverCreatedParts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIterateOverCreatedParts()
		 {
			  StringFactory factory = new StringFactory();
			  TemporalIndexCache<string> cache = new TemporalIndexCache<string>( factory );

			  assertEquals( Iterables.count( cache ), 0 );

			  cache.Select( LOCAL_DATE_TIME );
			  cache.Select( ZONED_TIME );

			  assertThat( cache, containsInAnyOrder( "LocalDateTime", "ZonedTime" ) );

			  cache.Select( DATE );
			  cache.Select( LOCAL_TIME );
			  cache.Select( LOCAL_DATE_TIME );
			  cache.Select( ZONED_TIME );
			  cache.Select( ZONED_DATE_TIME );
			  cache.Select( DURATION );

			  assertThat( cache, containsInAnyOrder( "Date", "LocalDateTime", "ZonedDateTime", "LocalTime", "ZonedTime", "Duration" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") @Test public void stressCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressCache()
		 {
			  StringFactory factory = new StringFactory();
			  TemporalIndexCache<string> cache = new TemporalIndexCache<string>( factory );

			  ExecutorService pool = Executors.newFixedThreadPool( 20 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?>[] futures = new java.util.concurrent.Future[100];
			  Future<object>[] futures = new Future[100];
			  AtomicBoolean shouldContinue = new AtomicBoolean( true );

			  try
			  {
					for ( int i = 0; i < futures.Length; i++ )
					{
						 futures[i] = pool.submit( new CacheStresser( cache, shouldContinue ) );
					}

					Thread.Sleep( 5_000 );

					shouldContinue.set( false );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
					foreach ( Future<object> future in futures )
					{
						 future.get( 10, TimeUnit.SECONDS );
					}
			  }
			  finally
			  {
					pool.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressInstantiationWithClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressInstantiationWithClose()
		 {
			  // given
			  StringFactory factory = new StringFactory();
			  TemporalIndexCache<string> cache = new TemporalIndexCache<string>( factory );
			  Race race = ( new Race() ).withRandomStartDelays();
			  MutableInt instantiatedAtClose = new MutableInt();
			  race.AddContestant(() =>
			  {
				try
				{
					 cache.UncheckedSelect( _valueGroups[0] );
					 cache.UncheckedSelect( _valueGroups[1] );
				}
				catch ( System.InvalidOperationException )
				{
					 // This exception is OK since it may have been closed
				}
			  }, 1);
			  race.AddContestant(() =>
			  {
				cache.CloseInstantiateCloseLock();
				instantiatedAtClose.Value = count( cache );
			  }, 1);

			  // when
			  race.Go();

			  // then
			  try
			  {
					cache.UncheckedSelect( _valueGroups[2] );
					fail( "No instantiation after closed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// good
			  }
			  assertEquals( instantiatedAtClose.intValue(), count(cache) );
		 }

		 private static readonly ValueGroup[] _valueGroups = new ValueGroup[] { ZONED_DATE_TIME, LOCAL_DATE_TIME, DATE, ZONED_TIME, LOCAL_TIME, DURATION };

		 internal class CacheStresser : Thread
		 {
			  internal readonly TemporalIndexCache<string> Cache;
			  internal readonly AtomicBoolean ShouldContinue;
			  internal readonly Random R = new Random();

			  internal CacheStresser( TemporalIndexCache<string> cache, AtomicBoolean shouldContinue )
			  {
					this.Cache = cache;
					this.ShouldContinue = shouldContinue;
			  }

			  public override void Run()
			  {
					while ( ShouldContinue.get() )
					{
						 Stress();
					}
			  }

			  internal virtual void Stress()
			  {
					// select
					Cache.select( _valueGroups[R.Next( _valueGroups.Length )] );

					// iterate
					foreach ( string s in Cache )
					{
						 if ( string.ReferenceEquals( s, null ) )
						 {
							  throw new System.InvalidOperationException( "iterated over null" );
						 }
					}
			  }
		 }

		 private class StringFactory : TemporalIndexCache.Factory<string>
		 {
			  internal AtomicInteger DateCounter = new AtomicInteger( 0 );
			  internal AtomicInteger LocalDateTimeCounter = new AtomicInteger( 0 );
			  internal AtomicInteger ZonedDateTimeCounter = new AtomicInteger( 0 );
			  internal AtomicInteger LocalTimeCounter = new AtomicInteger( 0 );
			  internal AtomicInteger ZonedTimeCounter = new AtomicInteger( 0 );
			  internal AtomicInteger DurationCounter = new AtomicInteger( 0 );

			  public override string NewDate()
			  {
					UpdateCounterAndAssertSingleUpdate( DateCounter );
					return "Date";
			  }

			  public override string NewLocalDateTime()
			  {
					UpdateCounterAndAssertSingleUpdate( LocalDateTimeCounter );
					return "LocalDateTime";
			  }

			  public override string NewZonedDateTime()
			  {
					UpdateCounterAndAssertSingleUpdate( ZonedDateTimeCounter );
					return "ZonedDateTime";
			  }

			  public override string NewLocalTime()
			  {
					UpdateCounterAndAssertSingleUpdate( LocalTimeCounter );
					return "LocalTime";
			  }

			  public override string NewZonedTime()
			  {
					UpdateCounterAndAssertSingleUpdate( ZonedTimeCounter );
					return "ZonedTime";
			  }

			  public override string NewDuration()
			  {
					UpdateCounterAndAssertSingleUpdate( DurationCounter );
					return "Duration";
			  }

			  internal virtual void UpdateCounterAndAssertSingleUpdate( AtomicInteger counter )
			  {
					int count = counter.incrementAndGet();
					if ( count > 1 )
					{
						 throw new System.InvalidOperationException( "called new on same factory method multiple times" );
					}
			  }
		 }
	}

}