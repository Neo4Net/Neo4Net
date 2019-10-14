using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.util.collection
{
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TimedRepositoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public TimedRepositoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_provider = _valueGenerator.getAndIncrement;
			_consumer = _reapedValues.add;
			_repo = new TimedRepository<long, long>( _provider, _consumer, _timeout, _clock );
		}

		 private readonly AtomicLong _valueGenerator = new AtomicLong();
		 private readonly IList<long> _reapedValues = new List<long>();

		 private Factory<long> _provider;
		 private System.Action<long> _consumer;

		 private readonly long _timeout = 100;
		 private readonly FakeClock _clock = Clocks.fakeClock();
		 private TimedRepository<long, long> _repo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldManageLifecycleWithNoTimeouts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldManageLifecycleWithNoTimeouts()
		 {
			  // When
			  _repo.begin( 1L );
			  long acquired = _repo.acquire( 1L );
			  _repo.release( 1L );
			  _repo.end( 1L );

			  // Then
			  assertThat( acquired, equalTo( 0L ) );
			  assertThat( _reapedValues, equalTo( asList( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowOthersAccessWhenAcquired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowOthersAccessWhenAcquired()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.acquire( 1L );

			  // When
			  try
			  {
					_repo.acquire( 1L );
					fail( "Should not have been allowed access." );
			  }
			  catch ( ConcurrentAccessException e )
			  {
					// Then
					assertThat( e.Message, equalTo( "Cannot access '1', because another client is currently using it." ) );
			  }

			  // But when
			  _repo.release( 1L );

			  // Then
			  assertThat( _repo.acquire( 1L ), equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowAccessAfterEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowAccessAfterEnd()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.end( 1L );

			  // When
			  try
			  {
					_repo.acquire( 1L );
					fail( "Should not have been able to acquire." );
			  }
			  catch ( NoSuchEntryException e )
			  {
					assertThat( e.Message, equalTo( "Cannot access '1', no such entry exists." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSilentlyAllowMultipleEndings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSilentlyAllowMultipleEndings()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.end( 1L );

			  // When
			  _repo.end( 1L );

			  // Then no exception should've been thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEndImmediatelyIfEntryIsUsed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotEndImmediatelyIfEntryIsUsed()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.acquire( 1L );

			  // When
			  _repo.end( 1L );

			  // Then
			  assertTrue( _reapedValues.Count == 0 );

			  // But when
			  _repo.release( 1L );

			  // Then
			  assertThat( _reapedValues, equalTo( asList( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowBeginningWithDuplicateKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowBeginningWithDuplicateKey()
		 {
			  // Given
			  _repo.begin( 1L );

			  // When
			  try
			  {
					_repo.begin( 1L );
					fail( "Should not have been able to begin." );
			  }
			  catch ( ConcurrentAccessException e )
			  {
					assertThat( e.Message, containsString( "Cannot begin '1', because Entry" ) );
					assertThat( e.Message, containsString( " with that key already exists." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeOutUnusedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeOutUnusedEntries()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.acquire( 1L );
			  _repo.release( 1L );

			  // When
			  _repo.run();

			  // Then nothing should've happened, because the entry should not yet get timed out
			  assertThat( _repo.acquire( 1L ), equalTo( 0L ) );
			  _repo.release( 1L );

			  // But When
			  _clock.forward( _timeout + 1, MILLISECONDS );
			  _repo.run();

			  // Then
			  assertThat( _reapedValues, equalTo( asList( 0L ) ) );

			  try
			  {
					_repo.acquire( 1L );
					fail( "Should not have been possible to acquire." );
			  }
			  catch ( NoSuchEntryException e )
			  {
					assertThat( e.Message, equalTo( "Cannot access '1', no such entry exists." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usingDuplicateKeysShouldDisposeOfPreemptiveAllocatedValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UsingDuplicateKeysShouldDisposeOfPreemptiveAllocatedValue()
		 {
			  // Given
			  _repo.begin( 1L );

			  // When
			  try
			  {
					_repo.begin( 1L );
					fail( "Should not have been able to begin." );
			  }
			  catch ( ConcurrentAccessException e )
			  {

					// Then
					assertThat( e.Message, containsString( "Cannot begin '1', because Entry" ) );
					assertThat( e.Message, containsString( " with that key already exists." ) );
			  }
			  assertThat( _reapedValues, equalTo( asList( 1L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowBeginWithSameKeyAfterSessionRelease() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowBeginWithSameKeyAfterSessionRelease()
		 {
			  // Given
			  _repo.begin( 1L );
			  _repo.acquire( 1L );

			  // when
			  _repo.release( 1L );
			  _repo.end( 1L );

			  //then
			  _repo.begin( 1L );
			  assertThat( _reapedValues, equalTo( asList( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unusedEntriesSafelyAcquiredOnCleanup() throws ConcurrentAccessException, NoSuchEntryException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnusedEntriesSafelyAcquiredOnCleanup()
		 {
			  CountDownReaper countDownReaper = new CountDownReaper();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TimedRepository<Object,long> timedRepository = new TimedRepository<>(provider, countDownReaper, 1, clock);
			  TimedRepository<object, long> timedRepository = new TimedRepository<object, long>( _provider, countDownReaper, 1, _clock );
			  ExecutorService singleThreadExecutor = Executors.newSingleThreadExecutor();
			  NonStoppableCleaner cleaner = new NonStoppableCleaner( timedRepository );
			  try
			  {
					singleThreadExecutor.submit( cleaner );

					long entryKey = 1L;
					long iterations = 100000L;
					while ( entryKey++ < iterations )
					{
						 timedRepository.Begin( entryKey );
						 timedRepository.Acquire( entryKey );
						 _clock.forward( 10, TimeUnit.MILLISECONDS );
						 timedRepository.Release( entryKey );
						 timedRepository.End( entryKey );

						 countDownReaper.Await( "Reaper should consume entry from cleaner thread or from our 'end' call. " + "If it was not consumed it mean cleaner and worker thread where not able to" + " figure out who removes entry, and block will ends up in the repo forever.", 10, SECONDS );
						 countDownReaper.Reset();
					}
			  }
			  finally
			  {
					cleaner.Stop();
					singleThreadExecutor.shutdownNow();
			  }
		 }

		 private class NonStoppableCleaner : ThreadStart
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool StopConflict;
			  internal readonly TimedRepository<object, long> TimedRepository;

			  internal NonStoppableCleaner( TimedRepository<object, long> timedRepository )
			  {
					this.TimedRepository = timedRepository;
			  }

			  public override void Run()
			  {
					while ( !StopConflict )
					{
						 TimedRepository.run();
					}
			  }

			  public virtual void Stop()
			  {
					StopConflict = true;
			  }
		 }

		 private class CountDownReaper : System.Action<long>
		 {
			  internal volatile System.Threading.CountdownEvent ReaperLatch;

			  internal CountDownReaper()
			  {
					Reset();
			  }

			  public virtual void Reset()
			  {
					ReaperLatch = new System.Threading.CountdownEvent( 1 );
			  }

			  public override void Accept( long? aLong )
			  {
					ReaperLatch.Signal();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void await(String message, long timeout, java.util.concurrent.TimeUnit timeUnit) throws InterruptedException
			  public virtual void Await( string message, long timeout, TimeUnit timeUnit )
			  {
					if ( !ReaperLatch.await( timeout, timeUnit ) )
					{
						 throw new System.InvalidOperationException( message );
					}
			  }

		 }
	}

}