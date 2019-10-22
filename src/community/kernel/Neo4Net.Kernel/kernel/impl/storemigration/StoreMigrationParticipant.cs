﻿/*
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
namespace Neo4Net.Kernel.impl.storemigration
{

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using AbstractStoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.participant.AbstractStoreMigrationParticipant;
	using UnsatisfiedDependencyException = Neo4Net.Kernel.impl.util.UnsatisfiedDependencyException;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;

	public interface StoreMigrationParticipant
	{
		 /// <summary>
		 /// Default empty implementation of StoreMigrationParticipant
		 /// </summary>

		 /// <summary>
		 /// Performs migration of data this participant is responsible for if necessary.
		 /// 
		 /// Data to migrate sits in {@code sourceDirectory} and must not be modified.
		 /// Migrated data should go into {@code targetStoreDir}, where source and target dirs are
		 /// highest level database store dirs.
		 /// </summary>
		 /// <param name="directoryLayout"> data to migrate. </param>
		 /// <param name="migrationLayout"> place to migrate to. </param>
		 /// <param name="progress"> migration progress monitor </param>
		 /// <param name="versionToMigrateFrom"> the version to migrate from </param>
		 /// <param name="versionToMigrateTo"> the version to migrate to </param>
		 /// <exception cref="IOException"> if there was an error migrating. </exception>
		 /// <exception cref="UnsatisfiedDependencyException"> if one or more dependencies were unsatisfied. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void migrate(org.Neo4Net.io.layout.DatabaseLayout directoryLayout, org.Neo4Net.io.layout.DatabaseLayout migrationLayout, org.Neo4Net.kernel.impl.util.monitoring.ProgressReporter progress, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException;
		 void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progress, string versionToMigrateFrom, string versionToMigrateTo );

		 /// <summary>
		 /// After a successful migration, move all affected files from {@code upgradeDirectory} over to
		 /// the {@code workingDirectory}, effectively activating the migration changes. </summary>
		 /// <param name="migrationLayout"> directory where the
		 /// <seealso cref="migrate(DatabaseLayout, DatabaseLayout, ProgressReporter, string, string) migration"/> put its files. </param>
		 /// <param name="directoryLayout"> directory the store directory of the to move the migrated files to. </param>
		 /// <param name="versionToMigrateFrom"> the version we have migrated from </param>
		 /// <param name="versionToMigrateTo"> the version we want to migrate to </param>
		 /// <exception cref="IOException"> if unable to move one or more files. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void moveMigratedFiles(org.Neo4Net.io.layout.DatabaseLayout migrationLayout, org.Neo4Net.io.layout.DatabaseLayout directoryLayout, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException;
		 void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToMigrateFrom, string versionToMigrateTo );

		 /// <summary>
		 /// Delete any file from {@code migrationLayout} produced during migration. </summary>
		 /// <param name="migrationLayout"> the directory where migrated files end up. </param>
		 /// <exception cref="IOException"> if unable to clean up one or more files. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void cleanup(org.Neo4Net.io.layout.DatabaseLayout migrationLayout) throws java.io.IOException;
		 void Cleanup( DatabaseLayout migrationLayout );

		 /// <returns> descriptive name of this migration participant. </returns>
		 string Name { get; }
	}

	public static class StoreMigrationParticipant_Fields
	{
		 public static readonly StoreMigrationParticipant NotParticipating = new AbstractStoreMigrationParticipant( "Nothing" );
	}

}