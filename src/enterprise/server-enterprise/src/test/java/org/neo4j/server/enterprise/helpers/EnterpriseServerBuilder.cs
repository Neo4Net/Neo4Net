using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.enterprise.helpers
{

	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using MetricsSettings = Neo4Net.metrics.MetricsSettings;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using DatabaseActions = Neo4Net.Server.rest.web.DatabaseActions;

	public class EnterpriseServerBuilder : CommunityServerBuilder
	{
		 protected internal EnterpriseServerBuilder( LogProvider logProvider ) : base( logProvider )
		 {
		 }

		 public static EnterpriseServerBuilder Server()
		 {
			  return Server( NullLogProvider.Instance );
		 }

		 public static EnterpriseServerBuilder ServerOnRandomPorts()
		 {
			  EnterpriseServerBuilder server = server();
			  server.OnRandomPorts();
			  server.WithProperty( ( new BoltConnector( "bolt" ) ).listen_address.name(), "localhost:0" );
			  server.WithProperty( OnlineBackupSettings.online_backup_server.name(), "127.0.0.1:0" );
			  return server;
		 }

		 public static EnterpriseServerBuilder Server( LogProvider logProvider )
		 {
			  return new EnterpriseServerBuilder( logProvider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.enterprise.OpenEnterpriseNeoServer build() throws java.io.IOException
		 public override OpenEnterpriseNeoServer Build()
		 {
			  return ( OpenEnterpriseNeoServer ) base.Build();
		 }

		 public override EnterpriseServerBuilder UsingDataDir( string dataDir )
		 {
			  base.UsingDataDir( dataDir );
			  return this;
		 }

		 protected internal override CommunityNeoServer Build( File configFile, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return new TestEnterpriseNeoServer( this, config, configFile, GraphDatabaseDependencies.newDependencies( dependencies ).userLogProvider( LogProvider ) );
		 }

		 private class TestEnterpriseNeoServer : OpenEnterpriseNeoServer
		 {
			 private readonly EnterpriseServerBuilder _outerInstance;

			  internal readonly File ConfigFile;

			  internal TestEnterpriseNeoServer( EnterpriseServerBuilder outerInstance, Config config, File configFile, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( config, dependencies )
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

		 public override IDictionary<string, string> CreateConfiguration( File temporaryFolder )
		 {
			  IDictionary<string, string> configuration = base.CreateConfiguration( temporaryFolder );

			  configuration[OnlineBackupSettings.online_backup_server.name()] = "127.0.0.1:0";
			  if ( !configuration.ContainsKey( MetricsSettings.csvPath.name() ) ) configuration.Add(MetricsSettings.csvPath.name(), (new File(temporaryFolder, "metrics")).AbsolutePath);

			  return configuration;
		 }
	}

}