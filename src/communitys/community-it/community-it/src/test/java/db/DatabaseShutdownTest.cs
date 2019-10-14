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
namespace Db
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DelegatingPageCache = Neo4Net.Io.pagecache.DelegatingPageCache;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using LifecycleStatus = Neo4Net.Kernel.Lifecycle.LifecycleStatus;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class DatabaseShutdownTest
	internal class DatabaseShutdownTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldShutdownCorrectlyWhenCheckPointingOnShutdownFails()
		 internal virtual void ShouldShutdownCorrectlyWhenCheckPointingOnShutdownFails()
		 {
			  TestGraphDatabaseFactoryWithFailingPageCacheFlush factory = new TestGraphDatabaseFactoryWithFailingPageCacheFlush();
			  assertThrows( typeof( LifecycleException ), () => factory.NewEmbeddedDatabase(_testDirectory.storeDir()).shutdown() );
			  assertEquals( LifecycleStatus.SHUTDOWN, factory.NeoStoreDataSourceStatus );
		 }

		 private class TestGraphDatabaseFactoryWithFailingPageCacheFlush : TestGraphDatabaseFactory
		 {
			  internal LifeSupport Life;

			  protected internal override GraphDatabaseService NewEmbeddedDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			  {
					return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, storeDir, config, dependencies )
					.newFacade( storeDir, config, dependencies );
			  }

			  private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			  {
				  private readonly TestGraphDatabaseFactoryWithFailingPageCacheFlush _outerInstance;

				  private File _storeDir;
				  private Config _config;
				  private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				  public GraphDatabaseFacadeFactoryAnonymousInnerClass( TestGraphDatabaseFactoryWithFailingPageCacheFlush outerInstance, DatabaseInfo community, File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
				  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._config = config;
					  this._dependencies = dependencies;
				  }


				  protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				  {
						PlatformModule platformModule = new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
						_outerInstance.life = platformModule.Life;
						return platformModule;
				  }

				  private class PlatformModuleAnonymousInnerClass : PlatformModule
				  {
					  private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

					  private new Config _config;

					  public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
					  {
						  this.outerInstance = outerInstance;
						  this._config = config;
					  }

					  protected internal override PageCache createPageCache( FileSystemAbstraction fileSystem, Config config, LogService logging, Tracers tracers, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler )
					  {
							PageCache pageCache = base.createPageCache( fileSystem, config, logging, tracers, versionContextSupplier, jobScheduler );
							return new DelegatingPageCacheAnonymousInnerClass( this, pageCache );
					  }

					  private class DelegatingPageCacheAnonymousInnerClass : DelegatingPageCache
					  {
						  private readonly PlatformModuleAnonymousInnerClass _outerInstance;

						  public DelegatingPageCacheAnonymousInnerClass( PlatformModuleAnonymousInnerClass outerInstance, PageCache pageCache ) : base( pageCache )
						  {
							  this.outerInstance = outerInstance;
						  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter ioLimiter) throws java.io.IOException
						  public override void flushAndForce( IOLimiter ioLimiter )
						  {
								// this is simulating a failing check pointing on shutdown
								throw new IOException( "Boom!" );
						  }
					  }
				  }
			  }

			  internal virtual LifecycleStatus NeoStoreDataSourceStatus
			  {
				  get
				  {
						return Life.Status;
				  }
			  }
		 }
	}

}