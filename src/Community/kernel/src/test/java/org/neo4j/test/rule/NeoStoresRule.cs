using System;
using System.Diagnostics;

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
namespace Neo4Net.Test.rule
{

	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.PageCacheTracer.NULL;

	/// <summary>
	/// Rule for opening a <seealso cref="NeoStores"/>.
	/// </summary>
	public class NeoStoresRule : ExternalResource
	{
		 private readonly Type _testClass;
		 private NeoStores _neoStores;

		 // Custom components which are managed by this rule if user doesn't supply them
		 private EphemeralFileSystemAbstraction _ruleFs;
		 private PageCache _rulePageCache;
		 private JobScheduler _jobScheduler;

		 private readonly StoreType[] _stores;

		 public NeoStoresRule( Type testClass, params StoreType[] stores )
		 {
			  this._testClass = testClass;
			  this._stores = stores;
		 }

		 public virtual Builder Builder()
		 {
			  return new Builder( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.NeoStores open(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.kernel.impl.store.format.RecordFormats format, System.Func<org.neo4j.io.fs.FileSystemAbstraction,org.neo4j.kernel.impl.store.id.IdGeneratorFactory> idGeneratorFactory, String... config) throws java.io.IOException
		 private NeoStores Open( FileSystemAbstraction fs, PageCache pageCache, RecordFormats format, System.Func<FileSystemAbstraction, IdGeneratorFactory> idGeneratorFactory, params string[] config )
		 {
			  Debug.Assert( _neoStores == null, "Already opened" );
			  TestDirectory testDirectory = TestDirectory.TestDirectoryConflict( fs );
			  testDirectory.PrepareDirectory( _testClass, null );
			  Config configuration = ConfigOf( config );
			  StoreFactory storeFactory = new StoreFactory( testDirectory.DatabaseLayout(), configuration, idGeneratorFactory(fs), pageCache, fs, format, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  return _neoStores = _stores.Length == 0 ? storeFactory.OpenAllNeoStores( true ) : storeFactory.OpenNeoStores( true, _stores );
		 }

		 private static Config ConfigOf( params string[] config )
		 {
			  return Config.defaults( stringMap( config ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void after(boolean successful) throws Throwable
		 protected internal override void After( bool successful )
		 {
			  IOUtils.closeAll( _neoStores, _rulePageCache, _jobScheduler );
			  _neoStores = null;
			  if ( _ruleFs != null )
			  {
					_ruleFs.Dispose();
			  }
		 }

		 public class Builder
		 {
			 private readonly NeoStoresRule _outerInstance;

			 public Builder( NeoStoresRule outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal FileSystemAbstraction Fs;
			  internal string[] Config;
			  internal RecordFormats Format;
			  internal PageCache PageCache;
			  internal System.Func<FileSystemAbstraction, IdGeneratorFactory> IdGeneratorFactory;

			  public virtual Builder With( FileSystemAbstraction fs )
			  {
					this.Fs = fs;
					return this;
			  }

			  public virtual Builder With( params string[] config )
			  {
					this.Config = config;
					return this;
			  }

			  public virtual Builder With( RecordFormats format )
			  {
					this.Format = format;
					return this;
			  }

			  public virtual Builder With( PageCache pageCache )
			  {
					this.PageCache = pageCache;
					return this;
			  }

			  public virtual Builder With( System.Func<FileSystemAbstraction, IdGeneratorFactory> idGeneratorFactory )
			  {
					this.IdGeneratorFactory = idGeneratorFactory;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.store.NeoStores build() throws java.io.IOException
			  public virtual NeoStores Build()
			  {
					if ( Fs == null )
					{
						 Fs = outerInstance.ruleFs();
					}
					if ( Config == null )
					{
						 Config = new string[0];
					}
					Config dbConfig = ConfigOf( Config );
					if ( PageCache == null )
					{
						 outerInstance.jobScheduler = new ThreadPoolJobScheduler();
						 PageCache = outerInstance.rulePageCache( dbConfig, Fs, outerInstance.jobScheduler );
					}
					if ( Format == null )
					{
						 Format = RecordFormatSelector.selectForConfig( dbConfig, NullLogProvider.Instance );
					}
					if ( IdGeneratorFactory == null )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 IdGeneratorFactory = DefaultIdGeneratorFactory::new;
					}
					return outerInstance.open( Fs, PageCache, Format, IdGeneratorFactory, Config );
			  }
		 }

		 private PageCache RulePageCache( Config dbConfig, FileSystemAbstraction fs, JobScheduler scheduler )
		 {
			  return _rulePageCache = GetOrCreatePageCache( dbConfig, fs, scheduler );
		 }

		 private EphemeralFileSystemAbstraction RuleFs()
		 {
			  return _ruleFs = new EphemeralFileSystemAbstraction();
		 }

		 private static PageCache GetOrCreatePageCache( Config config, FileSystemAbstraction fs, JobScheduler jobScheduler )
		 {
			  Log log = NullLog.Instance;
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fs, config, NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, log, EmptyVersionContextSupplier.EMPTY, jobScheduler );
			  return pageCacheFactory.OrCreatePageCache;
		 }
	}

}