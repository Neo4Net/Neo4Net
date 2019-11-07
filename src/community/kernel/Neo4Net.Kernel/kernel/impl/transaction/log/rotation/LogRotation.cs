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

	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;

	/// <summary>
	/// Used to check if a log rotation is needed, and also to execute a log rotation.
	/// 
	/// The implementation also makes sure that stores are forced to disk.
	/// 
	/// </summary>
	public interface LogRotation
	{

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LogRotation NO_ROTATION = new LogRotation()
	//	 {
	//		  @@Override public boolean rotateLogIfNeeded(LogAppendEvent logAppendEvent)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void rotateLogFile()
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// Rotates the undelying log if it is required. Returns true if rotation happened, false otherwise </summary>
		 /// <param name="logAppendEvent"> A trace event for the current log append operation. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean rotateLogIfNeeded(Neo4Net.kernel.impl.transaction.tracing.LogAppendEvent logAppendEvent) throws java.io.IOException;
		 bool RotateLogIfNeeded( LogAppendEvent logAppendEvent );

		 /// <summary>
		 /// Force a log rotation.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void rotateLogFile() throws java.io.IOException;
		 void RotateLogFile();
	}

	 public interface LogRotation_Monitor
	 {
		  void StartedRotating( long currentVersion );

		  void FinishedRotating( long currentVersion );
	 }

}