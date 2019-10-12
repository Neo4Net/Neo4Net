using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Server.helpers
{

	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HttpConnector = Org.Neo4j.Kernel.configuration.HttpConnector;
	using Encryption = Org.Neo4j.Kernel.configuration.HttpConnector.Encryption;
	using LegacySslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using CommunityGraphFactory = Org.Neo4j.Server.database.CommunityGraphFactory;
	using Database = Org.Neo4j.Server.database.Database;
	using InMemoryGraphFactory = Org.Neo4j.Server.database.InMemoryGraphFactory;
	using PreFlightTasks = Org.Neo4j.Server.preflight.PreFlightTasks;
	using DatabaseActions = Org.Neo4j.Server.rest.web.DatabaseActions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerTestUtils.asOneLine;

	public class CommunityServerBuilder
	{
		 private static readonly ListenSocketAddress _anyAddress = new ListenSocketAddress( "localhost", 0 );

		 protected internal readonly LogProvider LogProvider;
		 private ListenSocketAddress _address = new ListenSocketAddress( "localhost", HttpConnector.Encryption.NONE.defaultPort );
		 private ListenSocketAddress _httpsAddress = new ListenSocketAddress( "localhost", HttpConnector.Encryption.TLS.defaultPort );
		 private string _maxThreads;
		 private string _dataDir;
		 private string _managementUri = "/db/manage/";
		 private string _restUri = "/db/data/";
		 private PreFlightTasks _preflightTasks;
		 private readonly Dictionary<string, string> _thirdPartyPackages = new Dictionary<string, string>();
		 private readonly Properties _arbitraryProperties = new Properties();

		 static CommunityServerBuilder()
		 {
			  System.setProperty( "sun.net.http.allowRestrictedHeaders", "true" );
		 }

		 private string[] _autoIndexedNodeKeys;
		 private readonly string[] _autoIndexedRelationshipKeys = null;
		 private string[] _securityRuleClassNames;
		 private bool _persistent;
		 private bool _httpEnabled = true;
		 private bool _httpsEnabled;

		 public static CommunityServerBuilder Server( LogProvider logProvider )
		 {
			  return new CommunityServerBuilder( logProvider );
		 }

		 public static CommunityServerBuilder Server()
		 {
			  return new CommunityServerBuilder( NullLogProvider.Instance );
		 }

		 public static CommunityServerBuilder ServerOnRandomPorts()
		 {
			  return Server().onRandomPorts();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.CommunityNeoServer build() throws java.io.IOException
		 public virtual CommunityNeoServer Build()
		 {
			  if ( string.ReferenceEquals( _dataDir, null ) && _persistent )
			  {
					throw new System.InvalidOperationException( "Must specify path" );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File configFile = buildBefore();
			  File configFile = BuildBefore();

			  Log log = LogProvider.getLog( this.GetType() );
			  Config config = Config.fromFile( configFile ).withServerDefaults().build();
			  config.Logger = log;
			  return Build( configFile, config, GraphDatabaseDependencies.newDependencies().userLogProvider(LogProvider).monitors(new Monitors()) );
		 }

		 protected internal virtual CommunityNeoServer Build( File configFile, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return new TestCommunityNeoServer( this, config, configFile, dependencies );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File createConfigFiles() throws java.io.IOException
		 public virtual File CreateConfigFiles()
		 {
			  File temporaryConfigFile = ServerTestUtils.createTempConfigFile();
			  File temporaryFolder = ServerTestUtils.createTempDir();

			  ServerTestUtils.writeConfigToFile( CreateConfiguration( temporaryFolder ), temporaryConfigFile );

			  return temporaryConfigFile;
		 }

		 public virtual IDictionary<string, string> CreateConfiguration( File temporaryFolder )
		 {
			  IDictionary<string, string> properties = stringMap( ServerSettings.management_api_path.name(), _managementUri, ServerSettings.rest_api_path.name(), _restUri );

			  ServerTestUtils.addDefaultRelativeProperties( properties, temporaryFolder );

			  if ( !string.ReferenceEquals( _dataDir, null ) )
			  {
					properties[GraphDatabaseSettings.data_directory.name()] = _dataDir;
			  }

			  if ( !string.ReferenceEquals( _maxThreads, null ) )
			  {
					properties[ServerSettings.webserver_max_threads.name()] = _maxThreads;
			  }

			  if ( _thirdPartyPackages.Keys.Count > 0 )
			  {
					properties[ServerSettings.third_party_packages.name()] = asOneLine(_thirdPartyPackages);
			  }

			  if ( _autoIndexedNodeKeys != null && _autoIndexedNodeKeys.Length > 0 )
			  {
					properties["dbms.auto_index.nodes.enabled"] = "true";
					string propertyKeys = org.apache.commons.lang.StringUtils.join( _autoIndexedNodeKeys, "," );
					properties["dbms.auto_index.nodes.keys"] = propertyKeys;
			  }

			  if ( _autoIndexedRelationshipKeys != null && _autoIndexedRelationshipKeys.Length > 0 )
			  {
					properties["dbms.auto_index.relationships.enabled"] = "true";
					string propertyKeys = org.apache.commons.lang.StringUtils.join( _autoIndexedRelationshipKeys, "," );
					properties["dbms.auto_index.relationships.keys"] = propertyKeys;
			  }

			  if ( _securityRuleClassNames != null && _securityRuleClassNames.Length > 0 )
			  {
					string propertyKeys = org.apache.commons.lang.StringUtils.join( _securityRuleClassNames, "," );
					properties[ServerSettings.security_rules.name()] = propertyKeys;
			  }

			  HttpConnector httpConnector = new HttpConnector( "http", HttpConnector.Encryption.NONE );
			  HttpConnector httpsConnector = new HttpConnector( "https", HttpConnector.Encryption.TLS );

			  properties[httpConnector.Type.name()] = "HTTP";
			  properties[httpConnector.Enabled.name()] = _httpEnabled.ToString();
			  properties[httpConnector.Address.name()] = _address.ToString();
			  properties[httpConnector.Encryption.name()] = "NONE";

			  properties[httpsConnector.Type.name()] = "HTTP";
			  properties[httpsConnector.Enabled.name()] = _httpsEnabled.ToString();
			  properties[httpsConnector.Address.name()] = _httpsAddress.ToString();
			  properties[httpsConnector.Encryption.name()] = "TLS";

			  properties[GraphDatabaseSettings.auth_enabled.name()] = "false";
			  properties[LegacySslPolicyConfig.certificates_directory.name()] = (new File(temporaryFolder, "certificates")).AbsolutePath;
			  properties[GraphDatabaseSettings.logs_directory.name()] = (new File(temporaryFolder, "logs")).AbsolutePath;
			  properties[GraphDatabaseSettings.logical_logs_location.name()] = (new File(temporaryFolder, "transaction-logs")).AbsolutePath;
			  properties[GraphDatabaseSettings.pagecache_memory.name()] = "8m";
			  properties[GraphDatabaseSettings.shutdown_transaction_end_timeout.name()] = "0s";

			  foreach ( object key in _arbitraryProperties.Keys )
			  {
					properties[key.ToString()] = _arbitraryProperties.get(key).ToString();
			  }
			  return properties;
		 }

		 protected internal CommunityServerBuilder( LogProvider logProvider )
		 {
			  this.LogProvider = logProvider;
		 }

		 public virtual CommunityServerBuilder Persistent()
		 {
			  this._persistent = true;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithMaxJettyThreads( int maxThreads )
		 {
			  this._maxThreads = maxThreads.ToString();
			  return this;
		 }

		 public virtual CommunityServerBuilder UsingDataDir( string dataDir )
		 {
			  this._dataDir = dataDir;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithRelativeManagementApiUriPath( string uri )
		 {
			  try
			  {
					URI theUri = new URI( uri );
					if ( theUri.Absolute )
					{
						 this._managementUri = theUri.Path;
					}
					else
					{
						 this._managementUri = theUri.ToString();
					}
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
			  return this;
		 }

		 public virtual CommunityServerBuilder WithRelativeRestApiUriPath( string uri )
		 {
			  try
			  {
					URI theUri = new URI( uri );
					if ( theUri.Absolute )
					{
						 this._restUri = theUri.Path;
					}
					else
					{
						 this._restUri = theUri.ToString();
					}
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
			  return this;
		 }

		 public virtual CommunityServerBuilder WithDefaultDatabaseTuning()
		 {
			  return this;
		 }

		 public virtual CommunityServerBuilder WithThirdPartyJaxRsPackage( string packageName, string mountPoint )
		 {
			  _thirdPartyPackages[packageName] = mountPoint;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithAutoIndexingEnabledForNodes( params string[] keys )
		 {
			  _autoIndexedNodeKeys = keys;
			  return this;
		 }

		 public virtual CommunityServerBuilder OnRandomPorts()
		 {
			  this.OnHttpsAddress( _anyAddress );
			  this.OnAddress( _anyAddress );
			  return this;
		 }

		 public virtual CommunityServerBuilder OnAddress( ListenSocketAddress address )
		 {
			  this._address = address;
			  return this;
		 }

		 public virtual CommunityServerBuilder OnHttpsAddress( ListenSocketAddress address )
		 {
			  this._httpsAddress = address;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithSecurityRules( params string[] securityRuleClassNames )
		 {
			  this._securityRuleClassNames = securityRuleClassNames;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithHttpsEnabled()
		 {
			  _httpsEnabled = true;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithHttpDisabled()
		 {
			  _httpEnabled = false;
			  return this;
		 }

		 public virtual CommunityServerBuilder WithProperty( string key, string value )
		 {
			  _arbitraryProperties.put( key, value );
			  return this;
		 }

		 protected internal virtual DatabaseActions CreateDatabaseActionsObject( Database database )
		 {
			  return new DatabaseActions( database.Graph );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File buildBefore() throws java.io.IOException
		 private File BuildBefore()
		 {
			  File configFile = CreateConfigFiles();

			  if ( _preflightTasks == null )
			  {
					_preflightTasks = new PreFlightTasksAnonymousInnerClass( this, NullLogProvider.Instance );
			  }
			  return configFile;
		 }

		 private class PreFlightTasksAnonymousInnerClass : PreFlightTasks
		 {
			 private readonly CommunityServerBuilder _outerInstance;

			 public PreFlightTasksAnonymousInnerClass( CommunityServerBuilder outerInstance, NullLogProvider getInstance ) : base( getInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool run()
			 {
				  return true;
			 }
		 }

		 private class TestCommunityNeoServer : CommunityNeoServer
		 {
			 private readonly CommunityServerBuilder _outerInstance;

			  internal readonly File ConfigFile;

			  internal TestCommunityNeoServer( CommunityServerBuilder outerInstance, Config config, File configFile, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( config, outerInstance.persistent ? new CommunityGraphFactory() : new InMemoryGraphFactory(), dependencies )
			  {
				  this._outerInstance = outerInstance;
					this.ConfigFile = configFile;
			  }

			  protected internal override DatabaseActions CreateDatabaseActions()
			  {
					return outerInstance.CreateDatabaseActionsObject( DatabaseConflict );
			  }

			  public override void Stop()
			  {
					base.Stop();
					if ( ConfigFile != null )
					{
						 ConfigFile.delete();
					}
			  }
		 }
	}

}