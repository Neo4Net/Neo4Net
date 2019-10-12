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
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ExplicitIndexMigrationException = Neo4Net.Upgrade.Lucene.ExplicitIndexMigrationException;
	using LuceneExplicitIndexUpgrader = Neo4Net.Upgrade.Lucene.LuceneExplicitIndexUpgrader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ExplicitIndexMigratorTest
	{
		 private readonly FileSystemAbstraction _fs = mock( typeof( FileSystemAbstraction ) );
		 private readonly LogProvider _logProvider = mock( typeof( LogProvider ) );
		 private readonly ProgressReporter _progressMonitor = mock( typeof( ProgressReporter ) );
		 private readonly DatabaseLayout _storeLayout = DatabaseLayout.of( new File( GraphDatabaseSettings.DEFAULT_DATABASE_NAME ) );
		 private readonly DatabaseLayout _migrationLayout = DatabaseLayout.of( new File( StoreUpgrader.MIGRATION_DIRECTORY ) );
		 private readonly File _originalIndexStore = mock( typeof( File ) );
		 private readonly File _migratedIndexStore = new File( "." );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _originalIndexStore.ParentFile ).thenReturn( _storeLayout.databaseDirectory() );
			  when( _fs.isDirectory( _originalIndexStore ) ).thenReturn( true );
			  when( _fs.listFiles( _originalIndexStore ) ).thenReturn( new File[]{ mock( typeof( File ) ) } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipEmptyIndexStorageMigration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SkipEmptyIndexStorageMigration()
		 {
			  when( _fs.listFiles( _originalIndexStore ) ).thenReturn( null );

			  ExplicitIndexProvider indexProviders = ExplicitIndexProvider;
			  ExplicitIndexMigrator indexMigrator = new TestExplicitIndexMigrator( this, _fs, indexProviders, _logProvider, true );

			  indexMigrator.Migrate( _storeLayout, _migrationLayout, _progressMonitor, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );

			  verify( _fs, never() ).deleteRecursively(_originalIndexStore);
			  verify( _fs, never() ).moveToDirectory(_migratedIndexStore, _storeLayout.databaseDirectory());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transferOriginalDataToMigrationDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransferOriginalDataToMigrationDirectory()
		 {
			  ExplicitIndexProvider indexProviders = ExplicitIndexProvider;
			  ExplicitIndexMigrator indexMigrator = new TestExplicitIndexMigrator( this, _fs, indexProviders, _logProvider, true );

			  indexMigrator.Migrate( _storeLayout, _migrationLayout, _progressMonitor, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );

			  verify( _fs ).copyRecursively( _originalIndexStore, _migratedIndexStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transferMigratedIndexesToStoreDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransferMigratedIndexesToStoreDirectory()
		 {
			  ExplicitIndexProvider indexProviders = ExplicitIndexProvider;
			  ExplicitIndexMigrator indexMigrator = new TestExplicitIndexMigrator( this, _fs, indexProviders, _logProvider, true );

			  indexMigrator.Migrate( _storeLayout, _migrationLayout, _progressMonitor, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );
			  reset( _fs );

			  indexMigrator.MoveMigratedFiles( _migrationLayout, _storeLayout, "any", "any" );

			  verify( _fs ).deleteRecursively( _originalIndexStore );
			  verify( _fs ).moveToDirectory( _migratedIndexStore, _storeLayout.databaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logErrorWithIndexNameOnIndexMigrationException()
		 public virtual void LogErrorWithIndexNameOnIndexMigrationException()
		 {
			  Log log = mock( typeof( Log ) );
			  when( _logProvider.getLog( typeof( TestExplicitIndexMigrator ) ) ).thenReturn( log );

			  ExplicitIndexProvider indexProviders = ExplicitIndexProvider;
			  try
			  {
					ExplicitIndexMigrator indexMigrator = new TestExplicitIndexMigrator( this, _fs, indexProviders, _logProvider, false );
					indexMigrator.Migrate( _storeLayout, _migrationLayout, _progressMonitor, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );

					fail( "Index migration should fail" );
			  }
			  catch ( IOException )
			  {
					// ignored
			  }

			  verify( log ).error( eq( "Migration of explicit indexes failed. Index: testIndex can't be migrated." ), any( typeof( Exception ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cleanupMigrationDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanupMigrationDirectory()
		 {
			  when( _fs.fileExists( _migratedIndexStore ) ).thenReturn( true );

			  ExplicitIndexProvider indexProviders = ExplicitIndexProvider;
			  ExplicitIndexMigrator indexMigrator = new TestExplicitIndexMigrator( this, _fs, indexProviders, _logProvider, true );
			  indexMigrator.Migrate( _storeLayout, _migrationLayout, _progressMonitor, StandardV2_3.STORE_VERSION, StandardV3_0.STORE_VERSION );
			  indexMigrator.Cleanup( _migrationLayout );

			  verify( _fs ).deleteRecursively( _migratedIndexStore );
		 }

		 private ExplicitIndexProvider ExplicitIndexProvider
		 {
			 get
			 {
				  IndexImplementation indexImplementation = mock( typeof( IndexImplementation ) );
   
				  when( indexImplementation.GetIndexImplementationDirectory( _storeLayout ) ).thenReturn( _originalIndexStore );
				  when( indexImplementation.GetIndexImplementationDirectory( _migrationLayout ) ).thenReturn( _migratedIndexStore );
   
				  ExplicitIndexProvider explicitIndexProvider = mock( typeof( ExplicitIndexProvider ) );
				  when( explicitIndexProvider.GetProviderByName( "lucene" ) ).thenReturn( indexImplementation );
				  return explicitIndexProvider;
			 }
		 }

		 private class TestExplicitIndexMigrator : ExplicitIndexMigrator
		 {
			 private readonly ExplicitIndexMigratorTest _outerInstance;


			  internal readonly bool SuccessfulMigration;

			  internal TestExplicitIndexMigrator( ExplicitIndexMigratorTest outerInstance, FileSystemAbstraction fileSystem, ExplicitIndexProvider explicitIndexProvider, LogProvider logProvider, bool successfulMigration ) : base( fileSystem, explicitIndexProvider, logProvider )
			  {
				  this._outerInstance = outerInstance;
					this.SuccessfulMigration = successfulMigration;
			  }

			  internal override LuceneExplicitIndexUpgrader CreateLuceneExplicitIndexUpgrader( Path indexRootPath, ProgressReporter progressReporter )
			  {
					return new HumbleExplicitIndexUpgrader( _outerInstance, indexRootPath, SuccessfulMigration );
			  }
		 }

		 private class HumbleExplicitIndexUpgrader : LuceneExplicitIndexUpgrader
		 {
			 private readonly ExplicitIndexMigratorTest _outerInstance;

			  internal readonly bool SuccessfulMigration;

			  internal HumbleExplicitIndexUpgrader( ExplicitIndexMigratorTest outerInstance, Path indexRootPath, bool successfulMigration ) : base( indexRootPath, NO_MONITOR )
			  {
				  this._outerInstance = outerInstance;
					this.SuccessfulMigration = successfulMigration;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void upgradeIndexes() throws org.neo4j.upgrade.lucene.ExplicitIndexMigrationException
			  public override void UpgradeIndexes()
			  {
					if ( !SuccessfulMigration )
					{
						 throw new ExplicitIndexMigrationException( "testIndex", "Index migration failed", null );
					}
			  }
		 }
	}

}