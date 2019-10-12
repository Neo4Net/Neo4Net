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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using StoreVersion = Neo4Net.Kernel.impl.store.format.StoreVersion;
	using SilentProgressReporter = Neo4Net.Kernel.impl.util.monitoring.SilentProgressReporter;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CountsMigratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public CountsMigratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( _fs );
			RuleChain = RuleChain.outerRule( _fs ).around( _directory );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _directory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(directory);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccidentallyDeleteStoreFilesIfNoMigrationWasRequired() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAccidentallyDeleteStoreFilesIfNoMigrationWasRequired()
		 {
			  // given
			  CountsMigrator migrator = new CountsMigrator( _fs, null, Config.defaults() );
			  DatabaseLayout sourceLayout = _directory.databaseLayout();
			  File countsStoreFileA = sourceLayout.CountStoreA();
			  File countsStoreFileB = sourceLayout.CountStoreB();
			  _fs.create( countsStoreFileA );
			  _fs.create( countsStoreFileB );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( "migration" );
			  string versionToMigrateFrom = StoreVersion.STANDARD_V3_2.versionString();
			  string versionToMigrateTo = StoreVersion.STANDARD_V3_4.versionString();
			  migrator.Migrate( sourceLayout, migrationLayout, SilentProgressReporter.INSTANCE, versionToMigrateFrom, versionToMigrateTo );
			  assertEquals( "Invalid test assumption: There should not have been migration for those versions", 0, migrationLayout.ListDatabaseFiles( ( dir, name ) => true ).Length );

			  // when
			  migrator.MoveMigratedFiles( migrationLayout, sourceLayout, versionToMigrateFrom, versionToMigrateTo );

			  // then
			  assertTrue( _fs.fileExists( countsStoreFileA ) );
			  assertTrue( _fs.fileExists( countsStoreFileB ) );
		 }
	}

}