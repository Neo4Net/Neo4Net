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
namespace Neo4Net.Server.rest
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ServerModule = Neo4Net.Server.modules.ServerModule;
	using CausalClusteringService = Neo4Net.Server.rest.causalclustering.CausalClusteringService;
	using WebServer = Neo4Net.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class DatabaseRoleInfoServerModule : ServerModule
	{
		 private readonly WebServer _server;
		 private readonly Config _config;
		 private readonly Log _log;

		 public DatabaseRoleInfoServerModule( WebServer server, Config config, LogProvider logProvider )
		 {
			  this._server = server;
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Start()
		 {
			  URI baseUri = ManagementApiUri();
			  _server.addJAXRSClasses( ClassNames, baseUri.ToString(), null );

			  _log.info( "Mounted REST API at: %s", baseUri.ToString() );
		 }

		 public override void Stop()
		 {
			  URI baseUri = ManagementApiUri();
			  _server.removeJAXRSClasses( ClassNames, baseUri.ToString() );
		 }

		 private IList<string> ClassNames
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return new IList<string> { typeof( MasterInfoService ).FullName, typeof( CoreDatabaseAvailabilityService ).FullName, typeof( ReadReplicaDatabaseAvailabilityService ).FullName, typeof( CausalClusteringService ).FullName };
			 }
		 }

		 private URI ManagementApiUri()
		 {
			  return _config.get( ServerSettings.management_api_path );
		 }
	}

}