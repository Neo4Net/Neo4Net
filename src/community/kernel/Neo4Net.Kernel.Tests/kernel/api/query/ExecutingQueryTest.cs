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
namespace Neo4Net.Kernel.api.query
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using MathUtil = Neo4Net.Helpers.MathUtil;
	using PageCursorCounters = Neo4Net.Io.pagecache.tracing.cursor.PageCursorCounters;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using LockWaitEvent = Neo4Net.Storageengine.Api.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Neo4Net.Storageengine.Api.@lock;
	using FakeCpuClock = Neo4Net.Test.FakeCpuClock;
	using FakeHeapAllocation = Neo4Net.Test.FakeHeapAllocation;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ExecutingQueryTest
	{
		private bool InstanceFieldsInitialized = false;

		public ExecutingQueryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_query = CreateExecutingquery( 1, "hello world", _page, _clock, CpuClock, HeapAllocation );
			_subQuery = CreateExecutingquery( 2, "goodbye world", _page, _clock, CpuClock, HeapAllocation );
		}

		 private readonly FakeClock _clock = Clocks.fakeClock( ZonedDateTime.parse( "2016-12-03T15:10:00+01:00" ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.FakeCpuClock cpuClock = new org.neo4j.test.FakeCpuClock().add(randomLong(0x1_0000_0000L));
		 public readonly FakeCpuClock CpuClock = new FakeCpuClock().add(RandomLong(0x1_0000_0000L));
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.FakeHeapAllocation heapAllocation = new org.neo4j.test.FakeHeapAllocation().add(randomLong(0x1_0000_0000L));
		 public readonly FakeHeapAllocation HeapAllocation = new FakeHeapAllocation().add(RandomLong(0x1_0000_0000L));
		 private readonly PageCursorCountersStub _page = new PageCursorCountersStub();
		 private long _lockCount;
		 private ExecutingQuery _query;
		 private ExecutingQuery _subQuery;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportElapsedTime()
		 public virtual void ShouldReportElapsedTime()
		 {
			  // when
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  long elapsedTime = _query.snapshot().elapsedTimeMicros();

			  // then
			  assertEquals( 10_000, elapsedTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTransitionBetweenStates()
		 public virtual void ShouldTransitionBetweenStates()
		 {
			  // initial
			  assertEquals( "planning", _query.snapshot().status() );

			  // when
			  _query.compilationCompleted( new CompilerInfo( "the-planner", "the-runtime", emptyList() ), null );

			  // then
			  assertEquals( "running", _query.snapshot().status() );

			  // when
			  using ( LockWaitEvent @event = Lock( "NODE", 17 ) )
			  {
					// then
					assertEquals( "waiting", _query.snapshot().status() );
			  }
			  // then
			  assertEquals( "running", _query.snapshot().status() );

			  // when
			  _query.waitsForQuery( _subQuery );

			  // then
			  assertEquals( "waiting", _query.snapshot().status() );

			  // when
			  _query.waitsForQuery( null );

			  // then
			  assertEquals( "running", _query.snapshot().status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPlanningTime()
		 public virtual void ShouldReportPlanningTime()
		 {
			  // when
			  _clock.forward( 124, TimeUnit.MICROSECONDS );

			  // then
			  QuerySnapshot snapshot = _query.snapshot();
			  assertEquals( snapshot.CompilationTimeMicros(), snapshot.ElapsedTimeMicros() );

			  // when
			  _clock.forward( 16, TimeUnit.MICROSECONDS );
			  _query.compilationCompleted( new CompilerInfo( "the-planner", "the-runtime", emptyList() ), null );
			  _clock.forward( 200, TimeUnit.MICROSECONDS );

			  // then
			  snapshot = _query.snapshot();
			  assertEquals( 140, snapshot.CompilationTimeMicros() );
			  assertEquals( 340, snapshot.ElapsedTimeMicros() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportWaitTime()
		 public virtual void ShouldReportWaitTime()
		 {
			  // given
			  _query.compilationCompleted( new CompilerInfo( "the-planner", "the-runtime", emptyList() ), null );

			  // then
			  assertEquals( "running", _query.snapshot().status() );

			  // when
			  _clock.forward( 10, TimeUnit.SECONDS );
			  using ( LockWaitEvent @event = Lock( "NODE", 17 ) )
			  {
					_clock.forward( 5, TimeUnit.SECONDS );

					// then
					QuerySnapshot snapshot = _query.snapshot();
					assertEquals( "waiting", snapshot.Status() );
					assertThat( snapshot.ResourceInformation(), CoreMatchers.allOf<IDictionary<string, object>>(hasEntry("waitTimeMillis", 5_000L), hasEntry("resourceType", "NODE"), hasEntry(equalTo("resourceIds"), LongArray(17))) );
					assertEquals( 5_000_000, snapshot.WaitTimeMicros() );
			  }
			  {
					QuerySnapshot snapshot = _query.snapshot();
					assertEquals( "running", snapshot.Status() );
					assertEquals( 5_000_000, snapshot.WaitTimeMicros() );
			  }

			  // when
			  _clock.forward( 2, TimeUnit.SECONDS );
			  using ( LockWaitEvent @event = Lock( "RELATIONSHIP", 612 ) )
			  {
					_clock.forward( 1, TimeUnit.SECONDS );

					// then
					QuerySnapshot snapshot = _query.snapshot();
					assertEquals( "waiting", snapshot.Status() );
					assertThat( snapshot.ResourceInformation(), CoreMatchers.allOf<IDictionary<string, object>>(hasEntry("waitTimeMillis", 1_000L), hasEntry("resourceType", "RELATIONSHIP"), hasEntry(equalTo("resourceIds"), LongArray(612))) );
					assertEquals( 6_000_000, snapshot.WaitTimeMicros() );
			  }
			  {
					QuerySnapshot snapshot = _query.snapshot();
					assertEquals( "running", snapshot.Status() );
					assertEquals( 6_000_000, snapshot.WaitTimeMicros() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportQueryWaitTime()
		 public virtual void ShouldReportQueryWaitTime()
		 {
			  // given
			  _query.compilationCompleted( new CompilerInfo( "the-planner", "the-runtime", emptyList() ), null );

			  // when
			  _query.waitsForQuery( _subQuery );
			  _clock.forward( 5, TimeUnit.SECONDS );

			  // then
			  QuerySnapshot snapshot = _query.snapshot();
			  assertEquals( 5_000_000L, snapshot.WaitTimeMicros() );
			  assertEquals( "waiting", snapshot.Status() );
			  assertThat( snapshot.ResourceInformation(), CoreMatchers.allOf<IDictionary<string, object>>(hasEntry("waitTimeMillis", 5_000L), hasEntry("queryId", "query-2")) );

			  // when
			  _clock.forward( 1, TimeUnit.SECONDS );
			  _query.waitsForQuery( null );
			  _clock.forward( 2, TimeUnit.SECONDS );

			  // then
			  snapshot = _query.snapshot();
			  assertEquals( 6_000_000L, snapshot.WaitTimeMicros() );
			  assertEquals( "running", snapshot.Status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCpuTime()
		 public virtual void ShouldReportCpuTime()
		 {
			  // given
			  CpuClock.add( 60, TimeUnit.MICROSECONDS );

			  // when
			  long cpuTime = _query.snapshot().cpuTimeMicros();

			  // then
			  assertEquals( 60, cpuTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportCpuTimeIfUnavailable()
		 public virtual void ShouldNotReportCpuTimeIfUnavailable()
		 {
			  // given
			  ExecutingQuery query = new ExecutingQuery( 17, ClientConnectionInfo.EMBEDDED_CONNECTION, "neo4j", "hello world", EMPTY_MAP, Collections.emptyMap(), () => _lockCount, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, _clock, FakeCpuClock.NOT_AVAILABLE, HeapAllocation.NOT_AVAILABLE );

			  // when
			  QuerySnapshot snapshot = query.Snapshot();

			  // then
			  assertNull( snapshot.CpuTimeMicros() );
			  assertNull( snapshot.IdleTimeMicros() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportHeapAllocation()
		 public virtual void ShouldReportHeapAllocation()
		 {
			  // given
			  HeapAllocation.add( 4096 );

			  // when
			  long allocatedBytes = _query.snapshot().allocatedBytes();

			  // then
			  assertEquals( 4096, allocatedBytes );

			  // when
			  HeapAllocation.add( 4096 );
			  allocatedBytes = _query.snapshot().allocatedBytes();

			  // then
			  assertEquals( 8192, allocatedBytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportHeapAllocationIfUnavailable()
		 public virtual void ShouldNotReportHeapAllocationIfUnavailable()
		 {
			  // given
			  ExecutingQuery query = new ExecutingQuery( 17, ClientConnectionInfo.EMBEDDED_CONNECTION, "neo4j", "hello world", EMPTY_MAP, Collections.emptyMap(), () => _lockCount, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, _clock, FakeCpuClock.NOT_AVAILABLE, HeapAllocation.NOT_AVAILABLE );

			  // when
			  QuerySnapshot snapshot = query.Snapshot();

			  // then
			  assertNull( snapshot.AllocatedBytes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportLockCount()
		 public virtual void ShouldReportLockCount()
		 {
			  // given
			  _lockCount = 11;

			  // then
			  assertEquals( 11, _query.snapshot().activeLockCount() );

			  // given
			  _lockCount = 2;

			  // then
			  assertEquals( 2, _query.snapshot().activeLockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPageHitsAndFaults()
		 public virtual void ShouldReportPageHitsAndFaults()
		 {
			  // given
			  _page.hits( 7 );
			  _page.faults( 3 );

			  // when
			  QuerySnapshot snapshot = _query.snapshot();

			  // then
			  assertEquals( 7, snapshot.PageHits() );
			  assertEquals( 3, snapshot.PageFaults() );

			  // when
			  _page.hits( 2 );
			  _page.faults( 5 );
			  snapshot = _query.snapshot();

			  // then
			  assertEquals( 9, snapshot.PageHits() );
			  assertEquals( 8, snapshot.PageFaults() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void includeQueryExecutorThreadName()
		 public virtual void IncludeQueryExecutorThreadName()
		 {
			  string queryDescription = _query.ToString();
			  assertTrue( queryDescription.Contains( "threadExecutingTheQueryName=" + Thread.CurrentThread.Name ) );
		 }

		 private LockWaitEvent Lock( string resourceType, long resourceId )
		 {
			  return _query.lockTracer().waitForLock(false, resourceType(resourceType), resourceId);
		 }

		 internal static ResourceType ResourceType( string name )
		 {
			  return new ResourceTypeAnonymousInnerClass( name );
		 }

		 private class ResourceTypeAnonymousInnerClass : ResourceType
		 {
			 private string _name;

			 public ResourceTypeAnonymousInnerClass( string name )
			 {
				 this._name = name;
			 }

			 public override string ToString()
			 {
				  return name();
			 }

			 public int typeId()
			 {
				  throw new System.NotSupportedException( "not used" );
			 }

			 public WaitStrategy waitStrategy()
			 {
				  throw new System.NotSupportedException( "not used" );
			 }

			 public string name()
			 {
				  return _name;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static org.hamcrest.Matcher<Object> longArray(long... expected)
		 private static Matcher<object> LongArray( params long[] expected )
		 {
			  return ( Matcher ) new TypeSafeMatcherAnonymousInnerClass( expected );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<long[]>
		 {
			 private long[] _expected;

			 public TypeSafeMatcherAnonymousInnerClass( long[] expected )
			 {
				 this._expected = expected;
			 }

			 protected internal override bool matchesSafely( long[] item )
			 {
				  return Arrays.Equals( _expected, item );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( _expected );
			 }
		 }

		 private static long RandomLong( long bound )
		 {
			  return ThreadLocalRandom.current().nextLong(bound);
		 }

		 private ExecutingQuery CreateExecutingquery( int queryId, string helloWorld, PageCursorCountersStub page, FakeClock clock, FakeCpuClock cpuClock, FakeHeapAllocation heapAllocation )
		 {
			  return new ExecutingQuery( queryId, ClientConnectionInfo.EMBEDDED_CONNECTION, "neo4j", helloWorld, EMPTY_MAP, Collections.emptyMap(), () => _lockCount, page, Thread.CurrentThread.Id, Thread.CurrentThread.Name, clock, cpuClock, heapAllocation );
		 }

		 private class PageCursorCountersStub : PageCursorCounters
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FaultsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long PinsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long UnpinsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long HitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long BytesReadConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long EvictionsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long EvictionExceptionsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long BytesWrittenConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FlushesConflict;

			  public override long Faults()
			  {
					return FaultsConflict;
			  }

			  public virtual void Faults( long increment )
			  {
					FaultsConflict += increment;
			  }

			  public override long Pins()
			  {
					return PinsConflict;
			  }

			  public virtual void Pins( long increment )
			  {
					PinsConflict += increment;
			  }

			  public override long Unpins()
			  {
					return UnpinsConflict;
			  }

			  public virtual void Unpins( long increment )
			  {
					UnpinsConflict += increment;
			  }

			  public override long Hits()
			  {
					return HitsConflict;
			  }

			  public virtual void Hits( long increment )
			  {
					HitsConflict += increment;
			  }

			  public override long BytesRead()
			  {
					return BytesReadConflict;
			  }

			  public virtual void BytesRead( long increment )
			  {
					BytesReadConflict += increment;
			  }

			  public override long Evictions()
			  {
					return EvictionsConflict;
			  }

			  public virtual void Evictions( long increment )
			  {
					EvictionsConflict += increment;
			  }

			  public override long EvictionExceptions()
			  {
					return EvictionExceptionsConflict;
			  }

			  public virtual void EvictionExceptions( long increment )
			  {
					EvictionExceptionsConflict += increment;
			  }

			  public override long BytesWritten()
			  {
					return BytesWrittenConflict;
			  }

			  public virtual void BytesWritten( long increment )
			  {
					BytesWrittenConflict += increment;
			  }

			  public override long Flushes()
			  {
					return FlushesConflict;
			  }

			  public virtual void Flushes( long increment )
			  {
					FlushesConflict += increment;
			  }

			  public override double HitRatio()
			  {
					return MathUtil.portion( Hits(), Faults() );
			  }
		 }
	}

}