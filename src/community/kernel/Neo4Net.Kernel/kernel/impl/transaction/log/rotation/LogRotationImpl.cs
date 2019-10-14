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
namespace Neo4Net.Kernel.impl.transaction.log.rotation
{

	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogRotateEvent = Neo4Net.Kernel.impl.transaction.tracing.LogRotateEvent;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;

	/// <summary>
	/// Default implementation of the LogRotation interface.
	/// </summary>
	public class LogRotationImpl : LogRotation
	{
		 private readonly LogRotation_Monitor _monitor;
		 private readonly LogFiles _logFiles;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly LogFile _logFile;

		 public LogRotationImpl( LogRotation_Monitor monitor, LogFiles logFiles, DatabaseHealth databaseHealth )
		 {
			  this._monitor = monitor;
			  this._logFiles = logFiles;
			  this._databaseHealth = databaseHealth;
			  this._logFile = logFiles.LogFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean rotateLogIfNeeded(org.neo4j.kernel.impl.transaction.tracing.LogAppendEvent logAppendEvent) throws java.io.IOException
		 public override bool RotateLogIfNeeded( LogAppendEvent logAppendEvent )
		 {
			  /* We synchronize on the writer because we want to have a monitor that another thread
			   * doing force (think batching of writes), such that it can't see a bad state of the writer
			   * even when rotating underlying channels.
			   */
			  if ( _logFile.rotationNeeded() )
			  {
					lock ( _logFile )
					{
						 if ( _logFile.rotationNeeded() )
						 {
							  using ( LogRotateEvent rotateEvent = logAppendEvent.BeginLogRotate() )
							  {
									DoRotate();
							  }
							  return true;
						 }
					}
			  }
			  return false;
		 }

		 /// <summary>
		 /// use for test purpose only </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void rotateLogFile() throws java.io.IOException
		 public override void RotateLogFile()
		 {
			  lock ( _logFile )
			  {
					DoRotate();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doRotate() throws java.io.IOException
		 private void DoRotate()
		 {
			  long currentVersion = _logFiles.HighestLogVersion;
			  /*
			   * In order to rotate the current log file safely we need to assert that the kernel is still
			   * at full health. In case of a panic this rotation will be aborted, which is the safest alternative.
			   */
			  _databaseHealth.assertHealthy( typeof( IOException ) );
			  _monitor.startedRotating( currentVersion );
			  _logFile.rotate();
			  _monitor.finishedRotating( currentVersion );
		 }
	}

}