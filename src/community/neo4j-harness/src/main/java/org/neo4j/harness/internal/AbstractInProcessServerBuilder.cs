using System;
using System.Collections.Generic;

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
namespace Neo4Net.Harness.Internal
{
	using DigestUtils = org.apache.commons.codec.digest.DigestUtils;
	using FileUtils = org.apache.commons.io.FileUtils;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseDependencies = Neo4Net.GraphDb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using AbstractNeoServer = Neo4Net.Server.AbstractNeoServer;
	using DisabledNeoServer = Neo4Net.Server.DisabledNeoServer;
	using NeoServer = Neo4Net.Server.NeoServer;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ThirdPartyJaxRsPackage = Neo4Net.Server.configuration.ThirdPartyJaxRsPackage;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;

	using static Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory.Dependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.data_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.db_timezone;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.append;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.file.Files.createOrOpenAsOutputStream;

	public abstract class AbstractInProcessServerBuilder : TestServerBuilder
	{
		 private File _serverFolder;
		 private readonly Extensions _extensions = new Extensions();
		 private readonly HarnessRegisteredProcs _procedures = new HarnessRegisteredProcs();
		 private readonly Fixtures _fixtures = new Fixtures();

		 /// <summary>
		 /// Config options for both database and server.
		 /// </summary>
		 private readonly IDictionary<string, string> _config = new Dictionary<string, string>();

		 public AbstractInProcessServerBuilder( File workingDir )
		 {
			  File dataDir = ( new File( workingDir, RandomFolderName() ) ).AbsoluteFile;
			  Init( dataDir );
		 }

		 public AbstractInProcessServerBuilder( File workingDir, string dataSubDir )
		 {
			  File dataDir = ( new File( workingDir, dataSubDir ) ).AbsoluteFile;
			  Init( dataDir );
		 }

		 private void Init( File workingDir )
		 {
			  Directory = workingDir;
			  withConfig( auth_enabled, "false" );
			  withConfig( pagecache_memory, "8m" );

			  BoltConnector bolt0 = new BoltConnector( "bolt" );
			  HttpConnector http1 = new HttpConnector( "http", HttpConnector.Encryption.NONE );
			  HttpConnector http2 = new HttpConnector( "https", HttpConnector.Encryption.TLS );

			  WithConfig( http1.Type, "HTTP" );
			  withConfig( http1.Encryption, HttpConnector.Encryption.NONE.name() );
			  WithConfig( http1.Enabled, "true" );
			  WithConfig( http1.Address, "localhost:0" );

			  WithConfig( http2.Type, "HTTP" );
			  withConfig( http2.Encryption, HttpConnector.Encryption.TLS.name() );
			  WithConfig( http2.Enabled, "false" );
			  WithConfig( http2.Address, "localhost:0" );

			  WithConfig( bolt0.Type, "BOLT" );
			  WithConfig( bolt0.Enabled, "true" );
			  WithConfig( bolt0.Address, "localhost:0" );
		 }

		 public override TestServerBuilder CopyFrom( File originalStoreDir )
		 {
			  try
			  {
					FileUtils.copyDirectory( originalStoreDir, _serverFolder );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  return this;
		 }

		 public override ServerControls NewServer()
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
					  {
						File userLogFile = new File( _serverFolder, "Neo4Net.log" );
						File internalLogFile = new File( _serverFolder, "debug.log" );
      
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream logOutputStream;
						Stream logOutputStream;
						try
						{
							 logOutputStream = createOrOpenAsOutputStream( fileSystem, userLogFile, true );
						}
						catch ( IOException e )
						{
							 throw new Exception( "Unable to create log file", e );
						}
      
						_config[ServerSettings.third_party_packages.name()] = ToStringForThirdPartyPackageProperty(_extensions.toList());
						_config[GraphDatabaseSettings.store_internal_log_path.name()] = internalLogFile.AbsolutePath;
      
						LogProvider userLogProvider = FormattedLogProvider.withZoneId( LogZoneIdFrom( _config ) ).toOutputStream( logOutputStream );
						GraphDatabaseDependencies dependencies = GraphDatabaseDependencies.newDependencies().userLogProvider(userLogProvider);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<Neo4Net.kernel.extension.KernelExtensionFactory<?>> kernelExtensions = append(new Neo4NetHarnessExtensions(procedures), dependencies.kernelExtensions());
						IEnumerable<KernelExtensionFactory<object>> kernelExtensions = append( new Neo4NetHarnessExtensions( _procedures ), dependencies.KernelExtensions() );
						dependencies = dependencies.KernelExtensions( kernelExtensions );
      
						Config dbConfig = Config.defaults( _config );
						GraphFactory graphFactory = CreateGraphFactory( dbConfig );
						bool httpAndHttpsDisabled = dbConfig.EnabledHttpConnectors().Count == 0;
      
						NeoServer server = httpAndHttpsDisabled ? new DisabledNeoServer( graphFactory, dependencies, dbConfig ) : CreateNeoServer( graphFactory, dbConfig, dependencies );
      
						InProcessServerControls controls = new InProcessServerControls( _serverFolder, userLogFile, internalLogFile, server, logOutputStream );
						controls.Start();
      
						try
						{
							 _fixtures.applyTo( controls );
						}
						catch ( Exception e )
						{
							 controls.Close();
							 throw e;
						}
						return controls;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 protected internal abstract GraphFactory CreateGraphFactory( Config config );

		 protected internal abstract AbstractNeoServer CreateNeoServer( GraphFactory graphFactory, Config config, Dependencies dependencies );

		 public override TestServerBuilder WithConfig<T1>( Setting<T1> key, string value )
		 {
			  return WithConfig( key.Name(), value );
		 }

		 public override TestServerBuilder WithConfig( string key, string value )
		 {
			  _config[key] = value;
			  return this;
		 }

		 public override TestServerBuilder WithExtension( string mountPath, Type extension )
		 {
			  return withExtension( mountPath, extension.Assembly.GetName().Name );
		 }

		 public override TestServerBuilder WithExtension( string mountPath, string packageName )
		 {
			  _extensions.add( mountPath, packageName );
			  return this;
		 }

		 public override TestServerBuilder WithFixture( File cypherFileOrDirectory )
		 {
			  _fixtures.add( cypherFileOrDirectory );
			  return this;
		 }

		 public override TestServerBuilder WithFixture( string fixtureStatement )
		 {
			  _fixtures.add( fixtureStatement );
			  return this;
		 }

		 public override TestServerBuilder WithFixture( System.Func<GraphDatabaseService, Void> fixtureFunction )
		 {
			  _fixtures.add( fixtureFunction );
			  return this;
		 }

		 public override TestServerBuilder WithProcedure( Type procedureClass )
		 {
			  _procedures.addProcedure( procedureClass );
			  return this;
		 }

		 public override TestServerBuilder WithFunction( Type functionClass )
		 {
			  _procedures.addFunction( functionClass );
			  return this;
		 }

		 public override TestServerBuilder WithAggregationFunction( Type functionClass )
		 {
			  _procedures.addAggregationFunction( functionClass );
			  return this;
		 }

		 private TestServerBuilder setDirectory( File dir )
		 {
			  this._serverFolder = dir;
			  _config[data_directory.name()] = _serverFolder.AbsolutePath;
			  return this;
		 }

		 private string RandomFolderName()
		 {
			  return DigestUtils.md5Hex( Convert.ToString( ThreadLocalRandom.current().nextLong() ) );
		 }

		 private static string ToStringForThirdPartyPackageProperty( IList<ThirdPartyJaxRsPackage> extensions )
		 {
			  string propertyString = "";
			  int packageCount = extensions.Count;

			  if ( packageCount == 0 )
			  {
					return propertyString;
			  }
			  else
			  {
					ThirdPartyJaxRsPackage jaxRsPackage;
					for ( int i = 0; i < packageCount - 1; i++ )
					{
						 jaxRsPackage = extensions[i];
						 propertyString += jaxRsPackage.PackageName + "=" + jaxRsPackage.MountPoint + Settings.SEPARATOR;
					}
					jaxRsPackage = extensions[packageCount - 1];
					propertyString += jaxRsPackage.PackageName + "=" + jaxRsPackage.MountPoint;
					return propertyString;
			  }
		 }

		 private static ZoneId LogZoneIdFrom( IDictionary<string, string> config )
		 {
			  string dbTimeZone = config.getOrDefault( db_timezone.name(), db_timezone.DefaultValue );
			  return LogTimeZone.ValueOf( dbTimeZone ).ZoneId;
		 }

		 /// <summary>
		 /// A kernel extension used to ensure we load user-registered procedures
		 /// after other kernel extensions have initialized, since kernel extensions
		 /// can add custom injectables that procedures need.
		 /// </summary>
		 private class Neo4NetHarnessExtensions : KernelExtensionFactory<Neo4NetHarnessExtensions.Dependencies>
		 {
			  internal interface Dependencies
			  {
					Procedures Procedures();
			  }

			  internal HarnessRegisteredProcs UserProcs;

			  internal Neo4NetHarnessExtensions( HarnessRegisteredProcs userProcs ) : base( ExtensionType.DATABASE, "harness" )
			  {
					this.UserProcs = userProcs;
			  }

			  public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
			  {
					return new LifecycleAdapterAnonymousInnerClass( this, dependencies );
			  }

			  private class LifecycleAdapterAnonymousInnerClass : LifecycleAdapter
			  {
				  private readonly Neo4NetHarnessExtensions _outerInstance;

				  private Neo4Net.Harness.Internal.AbstractInProcessServerBuilder.Neo4NetHarnessExtensions.Dependencies _dependencies;

				  public LifecycleAdapterAnonymousInnerClass( Neo4NetHarnessExtensions outerInstance, Neo4Net.Harness.Internal.AbstractInProcessServerBuilder.Neo4NetHarnessExtensions.Dependencies dependencies )
				  {
					  this.outerInstance = outerInstance;
					  this._dependencies = dependencies;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
				  public override void start()
				  {
						_outerInstance.userProcs.applyTo( _dependencies.procedures() );
				  }
			  }

		 }
	}

}