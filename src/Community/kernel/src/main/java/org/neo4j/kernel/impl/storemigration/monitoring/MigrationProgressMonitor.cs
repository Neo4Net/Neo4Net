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
namespace Neo4Net.Kernel.impl.storemigration.monitoring
{
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;

	public interface MigrationProgressMonitor
	{
		 /// <summary>
		 /// Signals that the migration process has started. </summary>
		 /// <param name="numStages"> The number of migration stages is the migration process that we are monitoring. </param>
		 void Started( int numStages );

		 /// <summary>
		 /// Signals that migration goes into section with given {@code name}.
		 /// </summary>
		 /// <param name="name"> descriptive name of the section to migration. </param>
		 /// <returns> <seealso cref="ProgressReporter"/> which should be notified about progress in the given section. </returns>
		 ProgressReporter StartSection( string name );

		 /// <summary>
		 /// The migration process has completed successfully.
		 /// </summary>
		 void Completed();

	}

}