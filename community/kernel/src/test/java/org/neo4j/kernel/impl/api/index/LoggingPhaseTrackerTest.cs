using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;


	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.LoggingPhaseTracker.PERIOD_INTERVAL;

	public class LoggingPhaseTrackerTest
	{
		 private FakeClock _clock = new FakeClock();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSingleTime()
		 public virtual void ShouldLogSingleTime()
		 {
			  LoggingPhaseTracker phaseTracker = PhaseTracker;

			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.Stop();

			  Dictionary<PhaseTracker_Phase, LoggingPhaseTracker.Logger> times = phaseTracker.Times();
			  foreach ( PhaseTracker_Phase phase in times.Keys )
			  {
					LoggingPhaseTracker.Logger logger = times[phase];
					if ( phase == PhaseTracker_Phase.Scan )
					{
						 assertTrue( logger.TotalTime >= 100 );
						 assertTrue( logger.TotalTime < 500 );
					}
					else
					{
						 assertEquals( 0, logger.TotalTime );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogMultipleTimes()
		 public virtual void ShouldLogMultipleTimes()
		 {
			  LoggingPhaseTracker phaseTracker = PhaseTracker;

			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );
			  Sleep( 100 );
			  phaseTracker.Stop();

			  Dictionary<PhaseTracker_Phase, LoggingPhaseTracker.Logger> times = phaseTracker.Times();
			  foreach ( PhaseTracker_Phase phase in times.Keys )
			  {
					LoggingPhaseTracker.Logger logger = times[phase];
					if ( phase == PhaseTracker_Phase.Scan || phase == PhaseTracker_Phase.Write )
					{
						 assertTrue( logger.TotalTime >= 100 );
						 assertTrue( logger.TotalTime < 500 );
					}
					else
					{
						 assertEquals( 0, logger.TotalTime );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccumulateTimes()
		 public virtual void ShouldAccumulateTimes()
		 {
			  LoggingPhaseTracker phaseTracker = PhaseTracker;

			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );
			  LoggingPhaseTracker.Logger scanLogger = phaseTracker.Times()[PhaseTracker_Phase.Scan];
			  long firstCount = scanLogger.TotalTime;
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.Stop();

			  assertTrue( scanLogger.TotalTime > firstCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwIfEnterAfterStop()
		 public virtual void ThrowIfEnterAfterStop()
		 {
			  PhaseTracker phaseTracker = PhaseTracker;
			  phaseTracker.Stop();
			  try
			  {
					phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, containsString( "Trying to report a new phase after phase tracker has been stopped." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportMain()
		 public virtual void MustReportMain()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  Log log = logProvider.GetLog( typeof( IndexPopulationJob ) );
			  PhaseTracker phaseTracker = GetPhaseTracker( log );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Merge );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Build );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.ApplyExternal );
			  Sleep( 100 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Flip );
			  Sleep( 100 );

			  // when
			  phaseTracker.Stop();

			  // then
			  AssertableLogProvider.LogMatcher logMatcher = AssertableLogProvider.inLog( typeof( IndexPopulationJob ) ).info( "TIME/PHASE Final: " + "SCAN[totalTime=200ms, avgTime=100ms, minTime=0ns, maxTime=100ms, nbrOfReports=2], " + "WRITE[totalTime=200ms, avgTime=100ms, minTime=0ns, maxTime=100ms, nbrOfReports=2], " + "MERGE[totalTime=100ms], BUILD[totalTime=100ms], APPLY_EXTERNAL[totalTime=100ms], FLIP[totalTime=100ms]" );
			  logProvider.AssertAtLeastOnce( logMatcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportPeriod()
		 public virtual void MustReportPeriod()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  Log log = logProvider.GetLog( typeof( IndexPopulationJob ) );
			  PhaseTracker phaseTracker = GetPhaseTracker( 1, log );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );

			  // when
			  Sleep( 1000 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );

			  // then
			  AssertableLogProvider.LogMatcher firstEntry = AssertableLogProvider.inLog( typeof( IndexPopulationJob ) ).debug( "TIME/PHASE Total: SCAN[totalTime=1s], Last 1 sec: SCAN[totalTime=1s]" );
			  logProvider.AssertExactly( firstEntry );

			  // when
			  Sleep( 1000 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Scan );

			  // then
			  AssertableLogProvider.LogMatcher secondEntry = AssertableLogProvider.inLog( typeof( IndexPopulationJob ) ).debug( "TIME/PHASE Total: SCAN[totalTime=1s], WRITE[totalTime=1s], Last 1 sec: WRITE[totalTime=1s]" );
			  logProvider.AssertExactly( firstEntry, secondEntry );

			  // when
			  Sleep( 1000 );
			  phaseTracker.EnterPhase( PhaseTracker_Phase.Write );

			  // then
			  AssertableLogProvider.LogMatcher thirdEntry = AssertableLogProvider.inLog( typeof( IndexPopulationJob ) ).debug( "TIME/PHASE Total: " + "SCAN[totalTime=2s, avgTime=1s, minTime=0ns, maxTime=1s, nbrOfReports=2], " + "WRITE[totalTime=1s], " + "Last 1 sec: SCAN[totalTime=1s]" );
			  logProvider.AssertExactly( firstEntry, secondEntry, thirdEntry );
		 }

		 private LoggingPhaseTracker PhaseTracker
		 {
			 get
			 {
				  return GetPhaseTracker( NullLog.Instance );
			 }
		 }

		 private LoggingPhaseTracker getPhaseTracker( Log log )
		 {
			  return GetPhaseTracker( PERIOD_INTERVAL, log );
		 }

		 private LoggingPhaseTracker getPhaseTracker( int periodIntervalInSeconds, Log log )
		 {
			  return new LoggingPhaseTracker( periodIntervalInSeconds, log, _clock );
		 }

		 private void Sleep( int i )
		 {
			  _clock.forward( i, TimeUnit.MILLISECONDS );
		 }
	}

}