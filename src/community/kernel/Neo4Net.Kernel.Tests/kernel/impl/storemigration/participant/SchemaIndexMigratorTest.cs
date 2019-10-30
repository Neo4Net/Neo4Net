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
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SchemaIndexMigratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaIndexMigratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_migrator = new SchemaIndexMigrator( _fs, _indexProvider );
		}

		 private readonly FileSystemAbstraction _fs = mock( typeof( FileSystemAbstraction ) );
		 private readonly ProgressReporter _progressReporter = mock( typeof( ProgressReporter ) );
		 private readonly IndexProvider _indexProvider = mock( typeof( IndexProvider ) );
		 private readonly DatabaseLayout _databaseLayout = DatabaseLayout.of( new File( "store" ), GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
		 private readonly DatabaseLayout _migrationLayout = DatabaseLayout.of( new File( "migrationDir" ), GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

		 private SchemaIndexMigrator _migrator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void schemaAndLabelIndexesRemovedAfterSuccessfulMigration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SchemaAndLabelIndexesRemovedAfterSuccessfulMigration()
		 {
			  IndexDirectoryStructure directoryStructure = mock( typeof( IndexDirectoryStructure ) );
			  File indexProviderRootDirectory = _databaseLayout.file( "just-some-directory" );
			  when( directoryStructure.RootDirectory() ).thenReturn(indexProviderRootDirectory);
			  when( _indexProvider.directoryStructure() ).thenReturn(directoryStructure);
			  when( _indexProvider.ProviderDescriptor ).thenReturn( new IndexProviderDescriptor( "key", "version" ) );

			  _migrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );

			  _migrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );

			  verify( _fs ).deleteRecursively( indexProviderRootDirectory );
		 }
	}

}