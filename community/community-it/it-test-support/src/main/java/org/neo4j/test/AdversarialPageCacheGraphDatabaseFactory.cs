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
namespace Org.Neo4j.Test
{

	using Adversary = Org.Neo4j.Adversaries.Adversary;
	using AdversarialPageCache = Org.Neo4j.Adversaries.pagecache.AdversarialPageCache;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class AdversarialPageCacheGraphDatabaseFactory
	{
		 private AdversarialPageCacheGraphDatabaseFactory()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static GraphDatabaseFactory Create( FileSystemAbstraction fs, Adversary adversary )
		 {
			  return new TestGraphDatabaseFactoryAnonymousInnerClass( fs, adversary );
		 }

		 private class TestGraphDatabaseFactoryAnonymousInnerClass : TestGraphDatabaseFactory
		 {
			 private FileSystemAbstraction _fs;
			 private Adversary _adversary;

			 public TestGraphDatabaseFactoryAnonymousInnerClass( FileSystemAbstraction fs, Adversary adversary )
			 {
				 this._fs = fs;
				 this._adversary = adversary;
			 }

			 protected internal override GraphDatabaseService newEmbeddedDatabase( File dir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			 {
				  return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, config, dependencies )
				  .newFacade( dir, config, dependencies );
			 }

			 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			 {
				 private readonly TestGraphDatabaseFactoryAnonymousInnerClass _outerInstance;

				 private Config _config;
				 private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				 public GraphDatabaseFacadeFactoryAnonymousInnerClass( TestGraphDatabaseFactoryAnonymousInnerClass outerInstance, DatabaseInfo community, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
				 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					 this.outerInstance = outerInstance;
					 this._config = config;
					 this._dependencies = dependencies;
				 }


				 protected internal override PlatformModule createPlatform( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
				 {
					  File absoluteStoreDir = storeDir.AbsoluteFile;
					  File databasesRoot = absoluteStoreDir.ParentFile;
					  config.augment( GraphDatabaseSettings.active_database, absoluteStoreDir.Name );
					  config.augment( GraphDatabaseSettings.databases_root_path, databasesRoot.AbsolutePath );
					  return new PlatformModuleAnonymousInnerClass( this, databasesRoot, config, databaseInfo, dependencies );
				 }

				 private class PlatformModuleAnonymousInnerClass : PlatformModule
				 {
					 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

					 private new Config _config;

					 public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File databasesRoot, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( databasesRoot, config, databaseInfo, dependencies )
					 {
						 this.outerInstance = outerInstance;
						 this._config = config;
					 }

					 protected internal override FileSystemAbstraction createFileSystemAbstraction()
					 {
						  return _outerInstance.outerInstance.fs;
					 }

					 protected internal override PageCache createPageCache( FileSystemAbstraction fileSystem, Config config, LogService logging, Tracers tracers, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler )
					 {
						  PageCache pageCache = base.createPageCache( fileSystem, config, logging, tracers, versionContextSupplier, jobScheduler );
						  return new AdversarialPageCache( pageCache, _outerInstance.outerInstance.adversary );
					 }
				 }
			 }
		 }
	}

}