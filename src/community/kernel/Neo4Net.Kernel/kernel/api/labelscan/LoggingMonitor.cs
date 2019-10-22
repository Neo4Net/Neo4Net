using System;

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
namespace Neo4Net.Kernel.api.labelscan
{
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;

	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;

	/// <summary>
	/// Logs about important events about <seealso cref="LabelScanStore"/> <seealso cref="Monitor"/>.
	/// </summary>
	public class LoggingMonitor : Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
	{
		 private readonly Log _log;

		 public LoggingMonitor( Log log )
		 {
			  this._log = log;
		 }

		 public override void NoIndex()
		 {
			  _log.info( "No label index found, this might just be first use. Preparing to rebuild." );
		 }

		 public override void NotValidIndex()
		 {
			  _log.warn( "Label index could not be read. Preparing to rebuild." );
		 }

		 public override void Rebuilding()
		 {
			  _log.info( "Rebuilding label index, this may take a while" );
		 }

		 public override void Rebuilt( long roughNodeCount )
		 {
			  _log.info( "Label index rebuilt (roughly " + roughNodeCount + " nodes)" );
		 }

		 public override void RecoveryCleanupRegistered()
		 {
			  _log.info( "Label index cleanup job registered" );
		 }

		 public override void RecoveryCleanupStarted()
		 {
			  _log.info( "Label index cleanup job started" );
		 }

		 public override void RecoveryCleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
		 {
			  StringJoiner joiner = new StringJoiner( ", ", "Label index cleanup job finished: ", "" );
			  joiner.add( "Number of pages visited: " + numberOfPagesVisited );
			  joiner.add( "Number of cleaned crashed pointers: " + numberOfCleanedCrashPointers );
			  joiner.add( "Time spent: " + duration( durationMillis ) );
			  _log.info( joiner.ToString() );
		 }

		 public override void RecoveryCleanupClosed()
		 {
			  _log.info( "Label index cleanup job closed" );
		 }

		 public override void RecoveryCleanupFailed( Exception throwable )
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("Label index cleanup job failed.%nCaused by: %s", org.apache.commons.lang3.exception.ExceptionUtils.getStackTrace(throwable)));
			  _log.info( string.Format( "Label index cleanup job failed.%nCaused by: %s", ExceptionUtils.getStackTrace( throwable ) ) );
		 }
	}

}