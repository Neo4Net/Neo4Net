using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using Org.Neo4j.Kernel.extension;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.ExternallyManagedPageCache.graphDatabaseFactoryWithPageCache;

	public class CopiedStoreRecovery : LifecycleAdapter
	{
		 private readonly Config _config;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions;
		 private readonly IEnumerable<KernelExtensionFactory<object>> _kernelExtensions;
		 private readonly PageCache _pageCache;

		 private bool _shutdown;

		 public CopiedStoreRecovery<T1>( Config config, IEnumerable<T1> kernelExtensions, PageCache pageCache )
		 {
			  this._config = config;
			  this._kernelExtensions = kernelExtensions;
			  this._pageCache = pageCache;
		 }

		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  _shutdown = true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void recoverCopiedStore(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws DatabaseShutdownException
		 public virtual void RecoverCopiedStore( DatabaseLayout databaseLayout )
		 {
			 lock ( this )
			 {
				  if ( _shutdown )
				  {
						throw new DatabaseShutdownException( "Abort store-copied store recovery due to database shutdown" );
				  }
      
				  try
				  {
						GraphDatabaseService graphDatabaseService = NewTempDatabase( databaseLayout.DatabaseDirectory() );
						graphDatabaseService.Shutdown();
						// as soon as recovery will be extracted we will not gonna need this
						File lockFile = databaseLayout.StoreLayout.storeLockFile();
						if ( lockFile.exists() )
						{
							 FileUtils.deleteFile( lockFile );
						}
				  }
				  catch ( Exception e )
				  {
						Exception peeled = Exceptions.peel( e, t => !( t is UpgradeNotAllowedByConfigurationException ) );
						if ( peeled != null )
						{
							 throw new Exception( FailedToStartMessage(), e );
						}
						else
						{
							 throw e;
						}
				  }
			 }
		 }

		 private string FailedToStartMessage()
		 {
			  string recordFormat = _config.get( GraphDatabaseSettings.record_format );

			  return string.Format( "Failed to start database with copied store. This may be because the core servers and " + "read replicas have a different record format. On this machine: `{0}={1}`. Check the equivalent" + " value on the core server.", GraphDatabaseSettings.record_format.name(), recordFormat );
		 }

		 private GraphDatabaseService NewTempDatabase( File tempStore )
		 {
			  return graphDatabaseFactoryWithPageCache( _pageCache ).setKernelExtensions( _kernelExtensions ).setUserLogProvider( NullLogProvider.Instance ).newEmbeddedDatabaseBuilder( tempStore ).setConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).setConfig( GraphDatabaseSettings.pagecache_warmup_enabled, Settings.FALSE ).setConfig( GraphDatabaseSettings.keep_logical_logs, Settings.FALSE ).setConfig( GraphDatabaseSettings.allow_upgrade, _config.get( GraphDatabaseSettings.allow_upgrade ).ToString() ).setConfig(GraphDatabaseSettings.record_format, _config.get(GraphDatabaseSettings.record_format)).newGraphDatabase();
		 }
	}

}