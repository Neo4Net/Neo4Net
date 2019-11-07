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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using CapabilityType = Neo4Net.Kernel.impl.store.format.CapabilityType;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;

	/// <summary>
	/// Migrates schema and label indexes between different Neo4Net versions.
	/// Participates in store upgrade as one of the migration participants.
	/// <para>
	/// Since index format can be completely incompatible between version should be executed before <seealso cref="StoreMigrator"/>
	/// </para>
	/// </summary>
	public class SchemaIndexMigrator : AbstractStoreMigrationParticipant
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private bool _deleteObsoleteIndexes;
		 private File _schemaIndexDirectory;
		 private readonly IndexProvider _indexProvider;

		 public SchemaIndexMigrator( FileSystemAbstraction fileSystem, IndexProvider indexProvider ) : base( "Indexes" )
		 {
			  this._fileSystem = fileSystem;
			  this._indexProvider = indexProvider;
		 }

		 public override void Migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressReporter, string versionToMigrateFrom, string versionToMigrateTo )
		 {
			  RecordFormats from = RecordFormatSelector.selectForVersion( versionToMigrateFrom );
			  RecordFormats to = RecordFormatSelector.selectForVersion( versionToMigrateTo );
			  if ( !from.HasCompatibleCapabilities( to, CapabilityType.INDEX ) )
			  {
					_schemaIndexDirectory = _indexProvider.directoryStructure().rootDirectory();
					if ( _schemaIndexDirectory != null )
					{
						 _deleteObsoleteIndexes = true;
					}
					// else this schema index provider doesn't have any persistent storage to delete.
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(Neo4Net.io.layout.DatabaseLayout migrationLayout, Neo4Net.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToMigrateTo) throws java.io.IOException
		 public override void MoveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToMigrateTo )
		 {
			  if ( _deleteObsoleteIndexes )
			  {
					DeleteIndexes( _schemaIndexDirectory );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteIndexes(java.io.File indexRootDirectory) throws java.io.IOException
		 private void DeleteIndexes( File indexRootDirectory )
		 {
			  _fileSystem.deleteRecursively( indexRootDirectory );
		 }
	}

}