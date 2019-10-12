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
namespace Db
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using StoreUpgrader = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DatabaseStartupTest
	internal class DatabaseStartupTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startTheDatabaseWithWrongVersionShouldFailWithUpgradeNotAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StartTheDatabaseWithWrongVersionShouldFailWithUpgradeNotAllowed()
		 {
			  // given
			  // create a store
			  File databaseDir = _testDirectory.databaseDir();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(databaseDir);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  Db.shutdown();

			  // mess up the version in the metadatastore
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), ThreadPoolJobScheduler scheduler = new ThreadPoolJobScheduler(), PageCache pageCache = createPageCache(fileSystem, scheduler) )
			  {
					MetaDataStore.setRecord( pageCache, _testDirectory.databaseLayout().metadataStore(), MetaDataStore.Position.STORE_VERSION, MetaDataStore.versionStringToLong("bad") );
			  }

			  Exception exception = assertThrows( typeof( Exception ), () => (new TestGraphDatabaseFactory()).newEmbeddedDatabase(databaseDir) );
			  assertTrue( exception.InnerException is LifecycleException );
			  assertTrue( exception.InnerException.InnerException is System.ArgumentException );
			  assertEquals( "Unknown store version 'bad'", exception.InnerException.InnerException.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startTheDatabaseWithWrongVersionShouldFailAlsoWhenUpgradeIsAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StartTheDatabaseWithWrongVersionShouldFailAlsoWhenUpgradeIsAllowed()
		 {
			  // given
			  // create a store
			  File databaseDirectory = _testDirectory.databaseDir();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(databaseDirectory);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  Db.shutdown();

			  // mess up the version in the metadatastore
			  string badStoreVersion = "bad";
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), ThreadPoolJobScheduler scheduler = new ThreadPoolJobScheduler(), PageCache pageCache = createPageCache(fileSystem, scheduler) )
			  {
					MetaDataStore.setRecord( pageCache, _testDirectory.databaseLayout().metadataStore(), MetaDataStore.Position.STORE_VERSION, MetaDataStore.versionStringToLong(badStoreVersion) );
			  }

			  Exception exception = assertThrows( typeof( Exception ), () => (new TestGraphDatabaseFactory()).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase() );
			  assertTrue( exception.InnerException is LifecycleException );
			  assertTrue( exception.InnerException.InnerException is StoreUpgrader.UnexpectedUpgradingStoreVersionException );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startTestDatabaseOnProvidedNonAbsoluteFile()
		 internal virtual void StartTestDatabaseOnProvidedNonAbsoluteFile()
		 {
			  File directory = new File( "notAbsoluteDirectory" );
			  ( new TestGraphDatabaseFactory() ).newImpermanentDatabase(directory).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startCommunityDatabaseOnProvidedNonAbsoluteFile()
		 internal virtual void StartCommunityDatabaseOnProvidedNonAbsoluteFile()
		 {
			  File directory = new File( "notAbsoluteDirectory" );
			  EphemeralCommunityFacadeFactory factory = new EphemeralCommunityFacadeFactory();
			  GraphDatabaseFactory databaseFactory = new EphemeralGraphDatabaseFactory( factory );
			  GraphDatabaseService service = databaseFactory.NewEmbeddedDatabase( directory );
			  service.Shutdown();
		 }

		 private class EphemeralCommunityFacadeFactory : GraphDatabaseFacadeFactory
		 {
			  internal EphemeralCommunityFacadeFactory() : base(DatabaseInfo.COMMUNITY, CommunityEditionModule::new)
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  }

			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					return new PlatformModuleAnonymousInnerClass( this, storeDir, config, DatabaseInfo, dependencies );
			  }

			  private class PlatformModuleAnonymousInnerClass : PlatformModule
			  {
				  private readonly EphemeralCommunityFacadeFactory _outerInstance;

				  public PlatformModuleAnonymousInnerClass( EphemeralCommunityFacadeFactory outerInstance, File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override FileSystemAbstraction createFileSystemAbstraction()
				  {
						return new EphemeralFileSystemAbstraction();
				  }
			  }
		 }

		 private class EphemeralGraphDatabaseFactory : GraphDatabaseFactory
		 {
			  internal readonly EphemeralCommunityFacadeFactory Factory;

			  internal EphemeralGraphDatabaseFactory( EphemeralCommunityFacadeFactory factory )
			  {
					this.Factory = factory;
			  }

			  protected internal override GraphDatabaseFacadeFactory GraphDatabaseFacadeFactory
			  {
				  get
				  {
						return Factory;
				  }
			  }
		 }
	}

}