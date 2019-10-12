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
namespace Org.Neo4j.Kernel.monitoring.tracing
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using DefaultPageCacheTracer = Org.Neo4j.Io.pagecache.tracing.DefaultPageCacheTracer;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using DefaultTransactionTracer = Org.Neo4j.Kernel.Impl.Api.DefaultTransactionTracer;
	using DefaultCheckPointerTracer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.DefaultCheckPointerTracer;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using Clocks = Org.Neo4j.Time.Clocks;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class TracersTest
	{
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly JobScheduler _jobScheduler = mock( typeof( JobScheduler ) );
		 private readonly SystemNanoClock _clock = Clocks.nanoClock();
		 private readonly Monitors _monitors = new Monitors();

		 private Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _log = _logProvider.getLog( this.GetType() );
			  System.setProperty( "org.neo4j.helpers.Service.printServiceLoaderStackTraces", "true" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceNullImplementationsWhenRequested()
		 public virtual void MustProduceNullImplementationsWhenRequested()
		 {
			  Tracers tracers = CreateTracers( "null" );
			  assertThat( tracers.PageCacheTracer, @is( PageCacheTracer.NULL ) );
			  assertThat( tracers.PageCursorTracerSupplier, @is( Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null ) );
			  assertThat( tracers.TransactionTracer, @is( Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer_Fields.Null ) );
			  AssertNoWarning();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceNullImplementationsWhenRequestedIgnoringCase()
		 public virtual void MustProduceNullImplementationsWhenRequestedIgnoringCase()
		 {
			  Tracers tracers = CreateTracers( "NuLl" );
			  assertThat( tracers.PageCacheTracer, @is( PageCacheTracer.NULL ) );
			  assertThat( tracers.PageCursorTracerSupplier, @is( Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null ) );
			  assertThat( tracers.TransactionTracer, @is( Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer_Fields.Null ) );
			  AssertNoWarning();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceDefaultImplementationForNullConfiguration()
		 public virtual void MustProduceDefaultImplementationForNullConfiguration()
		 {
			  Tracers tracers = CreateTracers( null );
			  AssertDefaultImplementation( tracers );
			  AssertNoWarning();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceDefaultImplementationWhenRequested()
		 public virtual void MustProduceDefaultImplementationWhenRequested()
		 {
			  Tracers tracers = CreateTracers( "default" );
			  AssertDefaultImplementation( tracers );
			  AssertNoWarning();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceDefaultImplementationWhenRequestedIgnoringCase()
		 public virtual void MustProduceDefaultImplementationWhenRequestedIgnoringCase()
		 {
			  Tracers tracers = CreateTracers( "DeFaUlT" );
			  AssertDefaultImplementation( tracers );
			  AssertNoWarning();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustProduceDefaultImplementationWhenRequestingUnknownImplementation()
		 public virtual void MustProduceDefaultImplementationWhenRequestingUnknownImplementation()
		 {
			  Tracers tracers = CreateTracers( "there's nothing like this" );
			  AssertDefaultImplementation( tracers );
			  AssertWarning( "there's nothing like this" );
		 }

		 private Tracers CreateTracers( string s )
		 {
			  return new Tracers( s, _log, _monitors, _jobScheduler, _clock );
		 }

		 private void AssertDefaultImplementation( Tracers tracers )
		 {
			  assertThat( tracers.PageCacheTracer, instanceOf( typeof( DefaultPageCacheTracer ) ) );
			  assertThat( tracers.TransactionTracer, instanceOf( typeof( DefaultTransactionTracer ) ) );
			  assertThat( tracers.CheckPointTracer, instanceOf( typeof( DefaultCheckPointerTracer ) ) );
			  assertThat( tracers.PageCursorTracerSupplier, instanceOf( typeof( DefaultPageCursorTracerSupplier ) ) );
		 }

		 private void AssertNoWarning()
		 {
			  _logProvider.assertNoLoggingOccurred();
		 }

		 private void AssertWarning( string implementationName )
		 {
			  _logProvider.assertExactly( AssertableLogProvider.inLog( this.GetType() ).warn("Using default tracer implementations instead of '%s'", implementationName) );
		 }
	}

}