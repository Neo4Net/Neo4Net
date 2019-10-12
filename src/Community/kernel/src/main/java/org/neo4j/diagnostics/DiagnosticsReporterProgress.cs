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
namespace Neo4Net.Diagnostics
{
	/// <summary>
	/// Interface for handling feedback to the user. Implementations of this should be responsible of presenting the progress
	/// to the user. Some specialised implementations can choose to omit any of the information provided here.
	/// </summary>
	public interface DiagnosticsReporterProgress
	{
		 /// <summary>
		 /// Calling this will notify the user that the percentage has changed.
		 /// </summary>
		 /// <param name="percent"> to display to the user. </param>
		 void PercentChanged( int percent );

		 /// <summary>
		 /// Adds an additional information string to the output. Useful if the task has multiple steeps and the current step
		 /// should be displayed.
		 /// </summary>
		 /// <param name="info"> string to present to the user. </param>
		 void Info( string info );

		 /// <summary>
		 /// Called if an internal error occurs with an optional exception.
		 /// </summary>
		 /// <param name="msg"> message to display to the user. </param>
		 /// <param name="throwable"> optional exception, used to include a stacktrace if applicable. </param>
		 void Error( string msg, Exception throwable );

		 /// <summary>
		 /// @apiNote Called by dispatching class. Should not be called from diagnostics sources.
		 /// </summary>
		 long TotalSteps { set; }

		 /// <summary>
		 /// @apiNote Called by dispatching class. Should not be called from diagnostics sources.
		 /// </summary>
		 void Started( long currentStepIndex, string target );

		 /// <summary>
		 /// @apiNote Called by dispatching class. Should not be called from diagnostics sources.
		 /// </summary>
		 void Finished();
	}

}