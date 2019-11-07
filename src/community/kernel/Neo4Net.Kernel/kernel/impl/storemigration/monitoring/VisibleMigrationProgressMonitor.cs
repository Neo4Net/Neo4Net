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
namespace Neo4Net.Kernel.impl.storemigration.monitoring
{

	using LogProgressReporter = Neo4Net.Kernel.impl.util.monitoring.LogProgressReporter;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.duration;

	public class VisibleMigrationProgressMonitor : MigrationProgressMonitor
	{
		 internal const string MESSAGE_STARTED = "Starting upgrade of database";
		 internal const string MESSAGE_COMPLETED = "Successfully finished upgrade of database";
		 private static readonly string _messageCompletedWithDuration = MESSAGE_COMPLETED + ", took %s";

		 private readonly Log _log;
		 private readonly Clock _clock;
		 private int _numStages;
		 private int _currentStage;
		 private long _startTime;

		 public VisibleMigrationProgressMonitor( Log log ) : this( log, Clock.systemUTC() )
		 {
		 }

		 internal VisibleMigrationProgressMonitor( Log log, Clock clock )
		 {
			  this._log = log;
			  this._clock = clock;
		 }

		 public override void Started( int numStages )
		 {
			  this._numStages = numStages;
			  _log.info( MESSAGE_STARTED );
			  _startTime = _clock.millis();
		 }

		 public override ProgressReporter StartSection( string name )
		 {
			  _log.info( format( "Migrating %s (%d/%d):", name, ++_currentStage, _numStages ) );
			  return new LogProgressReporter( _log );
		 }

		 public override void Completed()
		 {
			  long time = _clock.millis() - _startTime;
			  _log.info( _messageCompletedWithDuration, duration( time ) );
		 }
	}

}