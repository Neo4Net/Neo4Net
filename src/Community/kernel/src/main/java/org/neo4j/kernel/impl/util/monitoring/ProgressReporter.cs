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
namespace Neo4Net.Kernel.impl.util.monitoring
{
	/// <summary>
	/// Progress indicator to track progress of long running processes.
	/// Reporter should be configured with maximum number of progress steps, by starting it.
	/// Each small step of long running task then responsible to call <seealso cref="progress(long)"/> to inform about ongoing
	/// progress.
	/// In the end <seealso cref="completed()"/> should be invoked to signal about execution completion.
	/// </summary>
	public interface ProgressReporter
	{
		 /// <param name="max"> max progress, which <seealso cref="progress(long)"/> moves towards. </param>
		 void Start( long max );

		 /// <summary>
		 /// Percentage completeness for the current section.
		 /// </summary>
		 /// <param name="add"> progress to add towards a maximum. </param>
		 void Progress( long add );

		 /// <summary>
		 /// Called if this section was completed successfully.
		 /// </summary>
		 void Completed();
	}

}