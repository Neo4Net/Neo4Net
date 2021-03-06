﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com.storecopy
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using Org.Neo4j.Kernel.extension;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	/// <summary>
	/// A PageCache implementation that delegates to another page cache, whose life cycle is managed elsewhere.
	/// 
	/// This page cache implementation DOES NOT delegate close() method calls, so it can be used to safely share a page
	/// cache with a component that might try to close the page cache it gets.
	/// </summary>
	public class ExternallyManagedPageCache : PageCache
	{
		 private readonly PageCache @delegate;

		 private ExternallyManagedPageCache( PageCache @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override void Close()
		 {
			  // Don't close the delegate, because we are not in charge of its life cycle.
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
		 public override PagedFile Map( File file, int pageSize, params OpenOption[] openOptions )
		 {
			  return @delegate.Map( file, pageSize, openOptions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.neo4j.io.pagecache.PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException
		 public override Optional<PagedFile> GetExistingMapping( File file )
		 {
			  return @delegate.GetExistingMapping( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.neo4j.io.pagecache.PagedFile> listExistingMappings() throws java.io.IOException
		 public override IList<PagedFile> ListExistingMappings()
		 {
			  return @delegate.ListExistingMappings();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  @delegate.FlushAndForce();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  @delegate.FlushAndForce( limiter );
		 }

		 public override int PageSize()
		 {
			  return @delegate.PageSize();
		 }

		 public override long MaxCachedPages()
		 {
			  return @delegate.MaxCachedPages();
		 }

		 public override void ReportEvents()
		 {
			  @delegate.ReportEvents();
		 }

		 /// <summary>
		 /// Create a GraphDatabaseFactory that will build EmbeddedGraphDatabase instances that all use the given page cache.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static GraphDatabaseFactoryWithPageCacheFactory graphDatabaseFactoryWithPageCache(final org.neo4j.io.pagecache.PageCache delegatePageCache)
		 public static GraphDatabaseFactoryWithPageCacheFactory GraphDatabaseFactoryWithPageCache( PageCache delegatePageCache )
		 {
			  return new GraphDatabaseFactoryWithPageCacheFactory( delegatePageCache );
		 }

		 public class GraphDatabaseFactoryWithPageCacheFactory : GraphDatabaseFactory
		 {
			  internal readonly PageCache DelegatePageCache;

			  internal GraphDatabaseFactoryWithPageCacheFactory( PageCache delegatePageCache )
			  {
					this.DelegatePageCache = delegatePageCache;
			  }

			  protected internal override GraphDatabaseService NewDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			  {
					File absoluteStoreDir = storeDir.AbsoluteFile;
					File databasesRoot = absoluteStoreDir.ParentFile;
					config.Augment( GraphDatabaseSettings.ephemeral, Settings.FALSE );
					config.augment( GraphDatabaseSettings.active_database, absoluteStoreDir.Name );
					config.augment( GraphDatabaseSettings.databases_root_path, databasesRoot.AbsolutePath );
					return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.ENTERPRISE, storeDir, config, dependencies )
					.newFacade( databasesRoot, config, dependencies );
			  }

			  private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			  {
				  private readonly GraphDatabaseFactoryWithPageCacheFactory _outerInstance;

				  private File _storeDir;
				  private Config _config;
				  private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				  public GraphDatabaseFacadeFactoryAnonymousInnerClass( GraphDatabaseFactoryWithPageCacheFactory outerInstance, DatabaseInfo enterprise, File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( enterprise, EnterpriseEditionModule::new )
				  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._config = config;
					  this._dependencies = dependencies;
				  }

				  protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				  {
						return new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
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
							return new ExternallyManagedPageCache( _outerInstance.outerInstance.delegatePageCache );
					  }
				  }
			  }

			  public virtual GraphDatabaseFactoryWithPageCacheFactory setKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
			  {
					CurrentState.KernelExtensions = newKernelExtensions;
					return this;
			  }
		 }
	}

}