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
namespace Neo4Net.Server.modules
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using UserService = Neo4Net.Server.rest.dbms.UserService;
	using DiscoverableURIs = Neo4Net.Server.rest.discovery.DiscoverableURIs;
	using DiscoveryService = Neo4Net.Server.rest.discovery.DiscoveryService;
	using WebServer = Neo4Net.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.plugins.Injectable.injectable;

	/// <summary>
	/// Mounts the DBMS REST API.
	/// </summary>
	public class DBMSModule : ServerModule
	{
		 private const string ROOT_PATH = "/";

		 private readonly WebServer _webServer;
		 private readonly Config _config;
		 private readonly System.Func<DiscoverableURIs> _discoverableURIs;

		 public DBMSModule( WebServer webServer, Config config, System.Func<DiscoverableURIs> discoverableURIs )
		 {
			  this._webServer = webServer;
			  this._config = config;
			  this._discoverableURIs = discoverableURIs;
		 }

		 public override void Start()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _webServer.addJAXRSClasses( singletonList( typeof( DiscoveryService ).FullName ), ROOT_PATH, singletonList( injectable( typeof( DiscoverableURIs ), _discoverableURIs.get() ) ) );
			  _webServer.addJAXRSClasses( ClassNames, ROOT_PATH, null );

		 }

		 private IList<string> ClassNames
		 {
			 get
			 {
				  IList<string> toReturn = new List<string>( 2 );
   
				  if ( _config.get( GraphDatabaseSettings.auth_enabled ) )
				  {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						toReturn.Add( typeof( UserService ).FullName );
				  }
   
				  return toReturn;
			 }
		 }

		 public override void Stop()
		 {
			  _webServer.removeJAXRSClasses( ClassNames, ROOT_PATH );
		 }
	}

}