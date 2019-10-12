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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;

	/// <summary>
	/// Default empty implementation of StoreMigrationParticipant.
	/// Base class for all StoreMigrationParticipant implementations.
	/// </summary>
	/// <seealso cref= org.neo4j.kernel.impl.storemigration.StoreUpgrader </seealso>
	public class AbstractStoreMigrationParticipant : StoreMigrationParticipant
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string NameConflict;

		 public AbstractStoreMigrationParticipant( string name )
		 {
			  this.NameConflict = name;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(org.neo4j.io.layout.DatabaseLayout directoryLayout, org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.kernel.impl.util.monitoring.ProgressReporter progressMonitor, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressMonitor, string versionToMigrateFrom, string versionToMigrateTo )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.io.layout.DatabaseLayout directoryLayout, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToMigrateFrom, string versionToMigrateTo )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup(org.neo4j.io.layout.DatabaseLayout migrationLayout) throws java.io.IOException
		 public override void Cleanup( DatabaseLayout migrationLayout )
		 {
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return NameConflict;
			 }
		 }
	}

}