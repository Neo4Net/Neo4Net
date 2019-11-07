using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.query
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PageCursorCounters = Neo4Net.Io.pagecache.tracing.cursor.PageCursorCounters;
	using CompilerInfo = Neo4Net.Kernel.Api.query.CompilerInfo;
	using ExecutingQuery = Neo4Net.Kernel.Api.query.ExecutingQuery;
	using Config = Neo4Net.Kernel.configuration.Config;
	using BoltConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.BoltConnectionInfo;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using FakeCpuClock = Neo4Net.Test.FakeCpuClock;
	using FakeHeapAllocation = Neo4Net.Test.FakeHeapAllocation;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;

	public class ConfiguredQueryLoggerTest
	{
		 private static readonly SocketAddress DUMMY_ADDRESS = new SocketAddressAnonymousInnerClass();

		 private class SocketAddressAnonymousInnerClass : SocketAddress
		 {
		 }
		 private static readonly ClientConnectionInfo _session_1 = new BoltConnectionInfo( "bolt-1", "{session one}", "client", DUMMY_ADDRESS, DUMMY_ADDRESS );
		 private static readonly ClientConnectionInfo _session_2 = new BoltConnectionInfo( "bolt-2", "{session two}", "client", DUMMY_ADDRESS, DUMMY_ADDRESS );
		 private static readonly ClientConnectionInfo _session_3 = new BoltConnectionInfo( "bolt-3", "{session three}", "client", DUMMY_ADDRESS, DUMMY_ADDRESS );
		 private const string QUERY_1 = "MATCH (n) RETURN n";
		 private const string QUERY_2 = "MATCH (a)--(b) RETURN b.name";
		 private const string QUERY_3 = "MATCH (c)-[:FOO]->(d) RETURN d.size";
		 private const string QUERY_4 = "MATCH (n) WHERE n.age IN {ages} RETURN n";
		 private readonly FakeClock _clock = Clocks.fakeClock();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.FakeCpuClock cpuClock = new Neo4Net.test.FakeCpuClock();
		 public readonly FakeCpuClock CpuClock = new FakeCpuClock();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.FakeHeapAllocation heapAllocation = new Neo4Net.test.FakeHeapAllocation();
		 public readonly FakeHeapAllocation HeapAllocation = new FakeHeapAllocation();
		 private long _pageHits;
		 private long _pageFaults;
		 private long _thresholdInMillis = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQuerySlowerThanThreshold()
		 public virtual void ShouldLogQuerySlowerThanThreshold()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );

			  // when
			  _clock.forward( 11, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  // then
			  string expectedSessionString = SessionConnectionDetails( _session_1, "TestUser" );
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {}", 11L, expectedSessionString, QUERY_1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectThreshold()
		 public virtual void ShouldRespectThreshold()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );

			  // when
			  _clock.forward( 9, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  // then
			  logProvider.AssertNoLoggingOccurred();

			  // and when
			  ExecutingQuery query2 = query( _session_2, "TestUser2", QUERY_2 );
			  _thresholdInMillis = 5;
			  queryLogger = queryLogger( logProvider ); // Rebuild queryLogger, like the DynamicQueryLogger would.
			  _clock.forward( 9, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query2 );

			  // then
			  string expectedSessionString = SessionConnectionDetails( _session_2, "TestUser2" );
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {}", 9L, expectedSessionString, QUERY_2)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepTrackOfDifferentSessions()
		 public virtual void ShouldKeepTrackOfDifferentSessions()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutingQuery query1 = Query( _session_1, "TestUser1", QUERY_1 );
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  ExecutingQuery query2 = Query( _session_2, "TestUser2", QUERY_2 );
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  ExecutingQuery query3 = Query( _session_3, "TestUser3", QUERY_3 );

			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );

			  // when
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  _clock.forward( 7, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query3 );
			  _clock.forward( 7, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query2 );
			  _clock.forward( 7, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query1 );

			  // then
			  string expectedSession1String = SessionConnectionDetails( _session_1, "TestUser1" );
			  string expectedSession2String = SessionConnectionDetails( _session_2, "TestUser2" );
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {}", 17L, expectedSession2String, QUERY_2)), inLog(this.GetType()).info(format("%d ms: %s - %s - {}", 25L, expectedSession1String, QUERY_1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQueryOnFailureEvenIfFasterThanThreshold()
		 public virtual void ShouldLogQueryOnFailureEvenIfFasterThanThreshold()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );

			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );
			  Exception failure = new Exception();

			  // when
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  queryLogger.Failure( query, failure );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).error(@is("1 ms: " + SessionConnectionDetails(_session_1, "TestUser") + " - MATCH (n) RETURN n - {}"), sameInstance(failure)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQueryParameters()
		 public virtual void ShouldLogQueryParameters()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["ages"] = Arrays.asList( 41, 42, 43 );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_4, @params, emptyMap() );
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_parameter_logging_enabled, "true" ) );

			  // when
			  _clock.forward( 11, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  // then
			  string expectedSessionString = SessionConnectionDetails( _session_1, "TestUser" );
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - %s - {}", 11L, expectedSessionString, QUERY_4, "{ages: " + "[41, 42, 43]}")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQueryParametersOnFailure()
		 public virtual void ShouldLogQueryParametersOnFailure()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["ages"] = Arrays.asList( 41, 42, 43 );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_4, @params, emptyMap() );
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_parameter_logging_enabled, "true" ) );
			  Exception failure = new Exception();

			  // when
			  _clock.forward( 1, TimeUnit.MILLISECONDS );
			  queryLogger.Failure( query, failure );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).error(@is("1 ms: " + SessionConnectionDetails(_session_1, "TestUser") + " - MATCH (n) WHERE n.age IN {ages} RETURN n - {ages: [41, 42, 43]} - {}"), sameInstance(failure)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUserName()
		 public virtual void ShouldLogUserName()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );

			  // when
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  ExecutingQuery anotherQuery = query( _session_1, "AnotherUser", QUERY_1 );
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  queryLogger.Success( anotherQuery );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {}", 10L, SessionConnectionDetails(_session_1, "TestUser"), QUERY_1)), inLog(this.GetType()).info(format("%d ms: %s - %s - {}", 10L, SessionConnectionDetails(_session_1, "AnotherUser"), QUERY_1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogMetaData()
		 public virtual void ShouldLogMetaData()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider );

			  // when
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1, emptyMap(), map("User", "UltiMate") );
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  ExecutingQuery anotherQuery = query( _session_1, "AnotherUser", QUERY_1, emptyMap(), map("Place", "Town") );
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  Exception error = new Exception();
			  queryLogger.Failure( anotherQuery, error );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {User: 'UltiMate'}", 10L, SessionConnectionDetails(_session_1, "TestUser"), QUERY_1)), inLog(this.GetType()).error(equalTo(format("%d ms: %s - %s - {Place: 'Town'}", 10L, SessionConnectionDetails(_session_1, "AnotherUser"), QUERY_1)), sameInstance(error)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPassword()
		 public virtual void ShouldNotLogPassword()
		 {
			  string inputQuery = "CALL dbms.security.changePassword('abc123')";
			  string outputQuery = "CALL dbms.security.changePassword(******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordNull()
		 public virtual void ShouldNotLogPasswordNull()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword(null, 'password')";
			  string outputQuery = "CALL dbms.security.changeUserPassword(null, ******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordWhenMalformedArgument()
		 public virtual void ShouldNotLogPasswordWhenMalformedArgument()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword('user, 'password')";
			  string outputQuery = "CALL dbms.security.changeUserPassword('user, ******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordExplain()
		 public virtual void ShouldNotLogPasswordExplain()
		 {
			  string inputQuery = "EXPLAIN CALL dbms.security.changePassword('abc123')";
			  string outputQuery = "EXPLAIN CALL dbms.security.changePassword(******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogChangeUserPassword()
		 public virtual void ShouldNotLogChangeUserPassword()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword('abc123')";
			  string outputQuery = "CALL dbms.security.changeUserPassword(******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordEvenIfPasswordIsSilly()
		 public virtual void ShouldNotLogPasswordEvenIfPasswordIsSilly()
		 {
			  string inputQuery = "CALL dbms.security.changePassword('.changePassword(\\'si\"lly\\')')";
			  string outputQuery = "CALL dbms.security.changePassword(******)";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordEvenIfYouDoTwoThingsAtTheSameTime()
		 public virtual void ShouldNotLogPasswordEvenIfYouDoTwoThingsAtTheSameTime()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword('Neo4Net','.changePassword(silly)') " +
						 "CALL dbms.security.changeUserPassword('smith','other$silly') RETURN 1";
			  string outputQuery = "CALL dbms.security.changeUserPassword('Neo4Net',******) " +
						 "CALL dbms.security.changeUserPassword('smith',******) RETURN 1";

			  RunAndCheck( inputQuery, outputQuery, emptyMap(), "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordEvenIfYouDoTwoThingsAtTheSameTimeWithSeveralParms()
		 public virtual void ShouldNotLogPasswordEvenIfYouDoTwoThingsAtTheSameTimeWithSeveralParms()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword('Neo4Net',$first) " +
						 "CALL dbms.security.changeUserPassword('smith',$second) RETURN 1";
			  string outputQuery = "CALL dbms.security.changeUserPassword('Neo4Net',$first) " +
						 "CALL dbms.security.changeUserPassword('smith',$second) RETURN 1";

			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["first"] = ".changePassword(silly)";
			  @params["second"] = ".other$silly";

			  RunAndCheck( inputQuery, outputQuery, @params, "first: ******, second: ******" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordInParams()
		 public virtual void ShouldNotLogPasswordInParams()
		 {
			  string inputQuery = "CALL dbms.changePassword($password)";
			  string outputQuery = "CALL dbms.changePassword($password)";

			  RunAndCheck( inputQuery, outputQuery, Collections.singletonMap( "password", ".changePassword(silly)" ), "password: ******" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordInDeprecatedParams()
		 public virtual void ShouldNotLogPasswordInDeprecatedParams()
		 {
			  string inputQuery = "CALL dbms.changePassword({password})";
			  string outputQuery = "CALL dbms.changePassword({password})";

			  RunAndCheck( inputQuery, outputQuery, Collections.singletonMap( "password", "abc123" ), "password: ******" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPasswordDifferentWhitespace()
		 public virtual void ShouldNotLogPasswordDifferentWhitespace()
		 {
			  string inputQuery = "CALL dbms.security.changeUserPassword(%s'abc123'%s)";
			  string outputQuery = "CALL dbms.security.changeUserPassword(%s******%s)";

			  RunAndCheck( format( inputQuery, "'user',", "" ), format( outputQuery, "'user',", "" ), emptyMap(), "" );
			  RunAndCheck( format( inputQuery, "'user', ", "" ), format( outputQuery, "'user', ", "" ), emptyMap(), "" );
			  RunAndCheck( format( inputQuery, "'user' ,", " " ), format( outputQuery, "'user' ,", " " ), emptyMap(), "" );
			  RunAndCheck( format( inputQuery, "'user',  ", "  " ), format( outputQuery, "'user',  ", "  " ), emptyMap(), "" );
		 }

		 private void RunAndCheck( string inputQuery, string outputQuery, IDictionary<string, object> @params, string paramsString )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_parameter_logging_enabled, "true" ) );

			  // when
			  ExecutingQuery query = query( _session_1, "neo", inputQuery, @params, emptyMap() );
			  _clock.forward( 10, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - {%s} - {}", 10L, SessionConnectionDetails(_session_1, "neo"), outputQuery, paramsString)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLogDetailedTime()
		 public virtual void ShouldBeAbleToLogDetailedTime()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_detailed_time_logging_enabled, "true" ) );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );

			  // when
			  _clock.forward( 17, TimeUnit.MILLISECONDS );
			  CpuClock.add( 12, TimeUnit.MILLISECONDS );
			  queryLogger.Success( query );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(containsString("17 ms: (planning: 17, cpu: 12, waiting: 0) - ")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLogAllocatedBytes()
		 public virtual void ShouldBeAbleToLogAllocatedBytes()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_allocation_logging_enabled, "true" ) );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );

			  // when
			  _clock.forward( 17, TimeUnit.MILLISECONDS );
			  HeapAllocation.add( 4096 );
			  queryLogger.Success( query );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(containsString("ms: 4096 B - ")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLogPageHitsAndPageFaults()
		 public virtual void ShouldBeAbleToLogPageHitsAndPageFaults()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConfiguredQueryLogger queryLogger = queryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_page_detail_logging_enabled, "true" ) );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_1 );

			  // when
			  _clock.forward( 12, TimeUnit.MILLISECONDS );
			  _pageHits = 17;
			  _pageFaults = 12;
			  queryLogger.Success( query );

			  // then
			  logProvider.AssertExactly( inLog( this.GetType() ).info(containsString(" 17 page hits, 12 page faults - ")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogRuntime()
		 public virtual void ShouldLogRuntime()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["ages"] = Arrays.asList( 41, 42, 43 );
			  ExecutingQuery query = query( _session_1, "TestUser", QUERY_4, @params, emptyMap() );
			  Config config = Config.defaults();
			  config.Augment( GraphDatabaseSettings.log_queries_parameter_logging_enabled, "true" );
			  config.Augment( GraphDatabaseSettings.log_queries_runtime_logging_enabled, "true" );
			  QueryLogger queryLogger = queryLogger( logProvider, config );

			  // when
			  _clock.forward( 11, TimeUnit.MILLISECONDS );
			  query.CompilationCompleted( new CompilerInfo( "magic", "quantum", Collections.emptyList() ), null );
			  queryLogger.Success( query );

			  // then
			  string expectedSessionString = SessionConnectionDetails( _session_1, "TestUser" );
			  logProvider.AssertExactly( inLog( this.GetType() ).info(format("%d ms: %s - %s - %s - {}", 11L, expectedSessionString, QUERY_4, "{ages: [41, 42, 43]} - runtime=quantum")) );
		 }

		 private ConfiguredQueryLogger QueryLogger( LogProvider logProvider )
		 {
			  return QueryLogger( logProvider, Config.defaults( GraphDatabaseSettings.log_queries_parameter_logging_enabled, "false" ) );
		 }

		 private ConfiguredQueryLogger QueryLogger( LogProvider logProvider, Config config )
		 {
			  config.Augment( GraphDatabaseSettings.log_queries_threshold, _thresholdInMillis + "ms" );
			  return new ConfiguredQueryLogger( logProvider.getLog( this.GetType() ), config );
		 }

		 private ExecutingQuery Query( ClientConnectionInfo sessionInfo, string username, string queryText )
		 {
			  return Query( sessionInfo, username, queryText, emptyMap(), emptyMap() );
		 }

		 private string SessionConnectionDetails( ClientConnectionInfo sessionInfo, string username )
		 {
			  return sessionInfo.WithUsername( username ).asConnectionDetails();
		 }

		 private int _queryId;

		 private ExecutingQuery Query( ClientConnectionInfo sessionInfo, string username, string queryText, IDictionary<string, object> @params, IDictionary<string, object> metaData )
		 {
			  Thread thread = Thread.CurrentThread;
			  return new ExecutingQuery(_queryId++, sessionInfo.WithUsername(username), username, queryText, ValueUtils.asMapValue(@params), metaData, () => 0, new PageCursorCountersAnonymousInnerClass(this)
						, thread.Id, thread.Name, _clock, CpuClock, HeapAllocation);
		 }

		 private class PageCursorCountersAnonymousInnerClass : PageCursorCounters
		 {
			 private readonly ConfiguredQueryLoggerTest _outerInstance;

			 public PageCursorCountersAnonymousInnerClass( ConfiguredQueryLoggerTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public long faults()
			 {
				  return _outerInstance.pageFaults;
			 }

			 public long hits()
			 {
				  return _outerInstance.pageHits;
			 }

			 public long pins()
			 {
				  return 0;
			 }

			 public long unpins()
			 {
				  return 0;
			 }

			 public long bytesRead()
			 {
				  return 0;
			 }

			 public long evictions()
			 {
				  return 0;
			 }

			 public long evictionExceptions()
			 {
				  return 0;
			 }

			 public long bytesWritten()
			 {
				  return 0;
			 }

			 public long flushes()
			 {
				  return 0;
			 }

			 public double hitRatio()
			 {
				  return 0d;
			 }
		 }
	}

}