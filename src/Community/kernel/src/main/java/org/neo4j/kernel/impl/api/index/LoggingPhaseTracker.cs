using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using TimeUtil = Neo4Net.Helpers.TimeUtil;
	using Log = Neo4Net.Logging.Log;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

	public class LoggingPhaseTracker : PhaseTracker
	{
		 private const string MESSAGE_PREFIX = "TIME/PHASE ";
		 internal static readonly int PeriodInterval = FeatureToggles.getInteger( typeof( LoggingPhaseTracker ), "period_interval", 600 );

		 private readonly long _periodInterval;
		 private readonly Log _log;
		 private readonly Clock _clock;

		 private Dictionary<PhaseTracker_Phase, Logger> _times = new Dictionary<PhaseTracker_Phase, Logger>( typeof( PhaseTracker_Phase ) );
		 private PhaseTracker_Phase _currentPhase;
		 private long _timeEnterPhase;
		 private bool _stopped;
		 private long _lastPeriodReport = -1;

		 internal LoggingPhaseTracker( Log log ) : this( PeriodInterval, log, Clock.systemUTC() )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting LoggingPhaseTracker(long periodIntervalInSeconds, org.neo4j.logging.Log log, java.time.Clock clock)
		 internal LoggingPhaseTracker( long periodIntervalInSeconds, Log log, Clock clock )
		 {
			  this._periodInterval = TimeUnit.SECONDS.toMillis( periodIntervalInSeconds );
			  this._log = log;
			  this._clock = clock;
			  foreach ( PhaseTracker_Phase phase in Enum.GetValues( typeof( PhaseTracker_Phase ) ) )
			  {
					_times[phase] = new Logger( this, phase );
			  }
		 }

		 public override void EnterPhase( PhaseTracker_Phase phase )
		 {
			  if ( _stopped )
			  {
					throw new System.InvalidOperationException( "Trying to report a new phase after phase tracker has been stopped." );
			  }
			  if ( phase != _currentPhase )
			  {
					long now = LogCurrentTime();
					_currentPhase = phase;
					_timeEnterPhase = now;

					if ( _lastPeriodReport == -1 )
					{
						 _lastPeriodReport = now;
					}

					long millisSinceLastPeriodReport = now - _lastPeriodReport;
					if ( millisSinceLastPeriodReport >= _periodInterval )
					{
						 // Report period
						 PeriodReport( millisSinceLastPeriodReport );
						 _lastPeriodReport = now;
					}
			  }
		 }

		 public override void Stop()
		 {
			  _stopped = true;
			  LogCurrentTime();
			  _currentPhase = null;
			  FinalReport();
		 }

		 internal virtual Dictionary<PhaseTracker_Phase, Logger> Times()
		 {
			  return _times;
		 }

		 private void FinalReport()
		 {
			  _log.info( MESSAGE_PREFIX + MainReportString( "Final" ) );
		 }

		 private void PeriodReport( long millisSinceLastPerioReport )
		 {
			  string periodReportString = periodReportString( millisSinceLastPerioReport );
			  string mainReportString = mainReportString( "Total" );
			  _log.debug( MESSAGE_PREFIX + mainReportString + ", " + periodReportString );
		 }

		 private string MainReportString( string title )
		 {
			  StringJoiner joiner = new StringJoiner( ", ", title + ": ", "" );
			  _times.Values.forEach(logger =>
			  {
				ReportToJoiner( joiner, logger );
			  });
			  return joiner.ToString();
		 }

		 private string PeriodReportString( long millisSinceLastPeriodReport )
		 {
			  long secondsSinceLastPeriodReport = TimeUnit.MILLISECONDS.toSeconds( millisSinceLastPeriodReport );
			  StringJoiner joiner = new StringJoiner( ", ", "Last " + secondsSinceLastPeriodReport + " sec: ", "" );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _times.Values.Select( Logger::period ).ForEach(period =>
			  {
						  ReportToJoiner( joiner, period );
						  period.reset();

			  });
			  return joiner.ToString();
		 }

		 private void ReportToJoiner( StringJoiner joiner, Counter counter )
		 {
			  if ( counter.NbrOfReports > 0 )
			  {
					joiner.add( counter.ToString() );
			  }
		 }

		 private long LogCurrentTime()
		 {
			  long now = _clock.millis();
			  if ( _currentPhase != null )
			  {
					Logger logger = _times[_currentPhase];
					long timeMillis = now - _timeEnterPhase;
					logger.Log( timeMillis );
			  }
			  return now;
		 }

		 public class Logger : Counter
		 {
			 private readonly LoggingPhaseTracker _outerInstance;

			  internal readonly Counter PeriodCounter;

			  internal Logger( LoggingPhaseTracker outerInstance, PhaseTracker_Phase phase ) : base( outerInstance, phase )
			  {
				  this._outerInstance = outerInstance;
					PeriodCounter = new Counter( outerInstance, phase );
					PeriodCounter.reset();
			  }

			  internal virtual void Log( long timeMillis )
			  {
					base.Log( timeMillis );
					PeriodCounter.log( timeMillis );
			  }

			  internal virtual Counter Period()
			  {
					return PeriodCounter;
			  }
		 }

		 public class Counter
		 {
			 private readonly LoggingPhaseTracker _outerInstance;

			  internal readonly PhaseTracker_Phase Phase;
			  internal long TotalTime;
			  internal long NbrOfReports;
			  internal long MaxTime;
			  internal long MinTime;

			  internal Counter( LoggingPhaseTracker outerInstance, PhaseTracker_Phase phase )
			  {
				  this._outerInstance = outerInstance;
					this.Phase = phase;
			  }

			  internal virtual void Log( long timeMillis )
			  {
					TotalTime += timeMillis;
					NbrOfReports++;
					MaxTime = Math.Max( MaxTime, timeMillis );
					MinTime = Math.Min( MinTime, timeMillis );
			  }

			  internal virtual void Reset()
			  {
					TotalTime = 0;
					NbrOfReports = 0;
					MaxTime = long.MinValue;
					MinTime = long.MaxValue;
			  }

			  public override string ToString()
			  {
					StringJoiner joiner = new StringJoiner( ", ", Phase.ToString() + "[", "]" );
					if ( NbrOfReports == 0 )
					{
						 AddToString( "nbrOfReports", NbrOfReports, joiner, false );
					}
					else if ( NbrOfReports == 1 )
					{
						 AddToString( "totalTime", TotalTime, joiner, true );
					}
					else
					{
						 long avgTime = TotalTime / NbrOfReports;
						 AddToString( "totalTime", TotalTime, joiner, true );
						 AddToString( "avgTime", avgTime, joiner, true );
						 AddToString( "minTime", MinTime, joiner, true );
						 AddToString( "maxTime", MaxTime, joiner, true );
						 AddToString( "nbrOfReports", NbrOfReports, joiner, false );
					}
					return joiner.ToString();
			  }

			  internal virtual void AddToString( string name, long measurement, StringJoiner joiner, bool isTime )
			  {
					string measurementString;
					if ( isTime )
					{
						 long timeInNanos = TimeUnit.MILLISECONDS.toNanos( measurement );
						 measurementString = TimeUtil.nanosToString( timeInNanos );
					}
					else
					{
						 measurementString = Convert.ToString( measurement );
					}
					joiner.add( string.Format( "{0}={1}", name, measurementString ) );
			  }
		 }
	}

}