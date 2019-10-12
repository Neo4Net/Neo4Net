using System.Collections.Generic;

namespace Org.Neo4j.Server.enterprise
{
	/*
	 * Copyright (c) 2002-2018 "Neo Technology,"
	 * Network Engine for Objects in Lund AB [http://neotechnology.com]
	 *
	 * This file is part of Neo4j.
	 *
	 * Neo4j is free software: you can redistribute it and/or modify
	 * it under the terms of the GNU Affero General Public License as
	 * published by the Free Software Foundation, either version 3 of the
	 * License, or (at your option) any later version.
	 *
	 * This program is distributed in the hope that it will be useful,
	 * but WITHOUT ANY WARRANTY; without even the implied warranty of
	 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	 * GNU Affero General Public License for more details.
	 *
	 * You should have received a copy of the GNU Affero General Public License
	 * along with this program. If not, see <http://www.gnu.org/licenses/>.
	 */

	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using OnlineBackupSettings = Org.Neo4j.backup.OnlineBackupSettings;
	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_level;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.store;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerTestUtils.getRelativePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	//import org.neo4j.kernel.GraphDatabaseDependencies;
	//import org.neo4j.server.BaseBootstrapperTestIT;
	//import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;
	//import static org.neo4j.server.configuration.ServerSettings.
	//import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;

	public class EnterpriseBootstrapperTestIT : BaseBootstrapperIT
	{
		private bool InstanceFieldsInitialized = false;

		public EnterpriseBootstrapperTestIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _folder ).around( _cleanupRule );
		}

		 private readonly new TemporaryFolder _folder = new TemporaryFolder();
		 private readonly CleanupRule _cleanupRule = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(folder).around(cleanupRule);
		 public RuleChain RuleChain;

		 protected internal override ServerBootstrapper NewBootstrapper()
		 {
			  return new EnterpriseBootstrapper();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStartInSingleMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStartInSingleMode()
		 {
			  // When
			  int resultCode = ServerBootstrapper.start( Bootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "-c", ConfigOption( EnterpriseEditionSettings.mode, "SINGLE" ), "-c", ConfigOption( GraphDatabaseSettings.data_directory, getRelativePath( _folder.Root, GraphDatabaseSettings.data_directory ) ), "-c", ConfigOption( logs_directory, TempDir.Root.AbsolutePath ), "-c", ConfigOption( certificates_directory, getRelativePath( _folder.Root, certificates_directory ) ), "-c", ( new BoltConnector( "BOLT" ) ).listen_address.name() + "=localhost:0", "-c", "dbms.connector.https.listen_address=localhost:0", "-c", "dbms.connector.1.type=HTTP", "-c", "dbms.connector.1.listen_address=localhost:0", "-c", "dbms.connector.1.encryption=NONE", "-c", "dbms.connector.1.enabled=true" );

			  // Then
			  assertEquals( ServerBootstrapper.OK, resultCode );
			  assertEventually( "Server was not started", Bootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
		 }

		 // @Test
		 // TODO: Update this for causal clustering testing.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldBeAbleToStartInHAMode() throws Exception
		 public virtual void ShouldBeAbleToStartInHAMode()
		 {
			  // When
			  int clusterPort = PortAuthority.allocatePort();
			  int resultCode = ServerBootstrapper.start( Bootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "-c", ConfigOption( EnterpriseEditionSettings.mode, "CORE" ), "-c", ConfigOption( ClusterSettings.server_id, "1" ), "-c", ConfigOption( ClusterSettings.initial_hosts, "127.0.0.1:" + clusterPort ), "-c", ConfigOption( ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort ), "-c", ConfigOption( GraphDatabaseSettings.data_directory, getRelativePath( _folder.Root, GraphDatabaseSettings.data_directory ) ), "-c", ConfigOption( logs_directory, TempDir.Root.AbsolutePath ), "-c", ConfigOption( certificates_directory, getRelativePath( _folder.Root, certificates_directory ) ), "-c", ( new BoltConnector( "BOLT" ) ).listen_address.name() + "=localhost:0", "-c", "dbms.connector.https.listen_address=localhost:0", "-c", "dbms.connector.1.type=HTTP", "-c", "dbms.connector.1.encryption=NONE", "-c", "dbms.connector.1.listen_address=localhost:0", "-c", "dbms.connector.1.enabled=true", "-c", "causal_clustering.initial_discovery_members=localhost:5000" );

			  // Then
			  assertEquals( ServerBootstrapper.OK, resultCode );
			  assertEventually( "Server was not started", Bootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void debugLoggingDisabledByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DebugLoggingDisabledByDefault()
		 {
			  // When
			  File configFile = TempDir.newFile( Config.DEFAULT_CONFIG_FILE_NAME );

			  IDictionary<string, string> properties = stringMap();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ServerTestUtils.DefaultRelativeProperties );
			  properties["dbms.connector.https.listen_address"] = "localhost:0";
			  properties["dbms.connector.1.type"] = "HTTP";
			  properties["dbms.connector.1.encryption"] = "NONE";
			  properties["dbms.connector.1.listen_address"] = "localhost:0";
			  properties["dbms.connector.1.enabled"] = "true";
			  properties[( new BoltConnector( "BOLT" ) ).listen_address.name()] = "localhost:0";
			  properties[OnlineBackupSettings.online_backup_server.name()] = "127.0.0.1:0";
			  store( properties, configFile );

			  // When
			  UncoveredEnterpriseBootstrapper uncoveredEnterpriseBootstrapper = new UncoveredEnterpriseBootstrapper( this );
			  _cleanupRule.add( uncoveredEnterpriseBootstrapper );
			  ServerBootstrapper.start( uncoveredEnterpriseBootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "--config-dir", configFile.ParentFile.AbsolutePath );

			  // Then
			  assertEventually( "Server was started", uncoveredEnterpriseBootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
			  LogProvider userLogProvider = uncoveredEnterpriseBootstrapper.UserLogProvider;
			  assertFalse( "Debug logging is disabled by default", userLogProvider.getLog( this.GetType() ).DebugEnabled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void debugLoggingEnabledBySetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DebugLoggingEnabledBySetting()
		 {
			  // When
			  File configFile = TempDir.newFile( Config.DEFAULT_CONFIG_FILE_NAME );

			  IDictionary<string, string> properties = stringMap( store_internal_log_level.name(), "DEBUG" );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ServerTestUtils.DefaultRelativeProperties );
			  properties["dbms.connector.https.listen_address"] = "localhost:0";
			  properties["dbms.connector.1.type"] = "HTTP";
			  properties["dbms.connector.1.encryption"] = "NONE";
			  properties["dbms.connector.1.listen_address"] = "localhost:0";
			  properties["dbms.connector.1.enabled"] = "true";
			  properties[( new BoltConnector( "BOLT" ) ).listen_address.name()] = "localhost:0";
			  properties[OnlineBackupSettings.online_backup_server.name()] = "127.0.0.1:0";
			  store( properties, configFile );

			  // When
			  UncoveredEnterpriseBootstrapper uncoveredEnterpriseBootstrapper = new UncoveredEnterpriseBootstrapper( this );
			  _cleanupRule.add( uncoveredEnterpriseBootstrapper );
			  ServerBootstrapper.start( uncoveredEnterpriseBootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "--config-dir", configFile.ParentFile.AbsolutePath );

			  // Then
			  assertEventually( "Server was started", uncoveredEnterpriseBootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
			  LogProvider userLogProvider = uncoveredEnterpriseBootstrapper.UserLogProvider;
			  assertTrue( "Debug logging enabled by setting value.", userLogProvider.getLog( this.GetType() ).DebugEnabled );
		 }

		 private class UncoveredEnterpriseBootstrapper : EnterpriseBootstrapper
		 {
			 private readonly EnterpriseBootstrapperTestIT _outerInstance;

			 public UncoveredEnterpriseBootstrapper( EnterpriseBootstrapperTestIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal LogProvider UserLogProviderConflict;

			  protected internal override NeoServer CreateNeoServer( GraphFactory graphFactory, Config config, GraphDatabaseDependencies dependencies )
			  {
					this.UserLogProviderConflict = dependencies.UserLogProvider();

					return base.CreateNeoServer( graphFactory, config, dependencies );
			  }

			  internal virtual LogProvider UserLogProvider
			  {
				  get
				  {
						return UserLogProviderConflict;
				  }
			  }
		 }
	}

}