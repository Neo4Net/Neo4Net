using System;

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
namespace Org.Neo4j.Kernel.impl.util.monitoring
{
	using Log = Org.Neo4j.Logging.Log;

	/// <summary>
	/// Progress reporter that reports its progress into provided log.
	/// Progress measured in percents (from 0 till 100) where just started reporter is at 0 percents and
	/// completed is at 100. Progress reporter each 10 percents.
	/// </summary>
	public class LogProgressReporter : ProgressReporter
	{
		 private const int STRIDE = 10;
		 private const int HUNDRED = 100;

		 private readonly Log _log;

		 private long _current;
		 private int _currentPercent;
		 private long _max;

		 public LogProgressReporter( Log log )
		 {
			  this._log = log;
		 }

		 public override void Progress( long add )
		 {
			  _current += add;
			  int percent = _max == 0 ? HUNDRED : Math.Min( HUNDRED, ( int )( ( _current * HUNDRED ) / _max ) );
			  EnsurePercentReported( percent );
		 }

		 private void EnsurePercentReported( int percent )
		 {
			  while ( _currentPercent < percent )
			  {
					ReportPercent( ++_currentPercent );
			  }
		 }

		 private void ReportPercent( int percent )
		 {
			  if ( percent % STRIDE == 0 )
			  {
					_log.info( format( "  %d%% completed", percent ) );
			  }
		 }

		 public override void Start( long max )
		 {
			  this._max = max;
		 }

		 public override void Completed()
		 {
			  EnsurePercentReported( HUNDRED );
		 }
	}

}