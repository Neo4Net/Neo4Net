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
namespace Org.Neo4j.Server.modules
{

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using ConsoleService = Org.Neo4j.Server.rest.management.console.ConsoleService;
	using WebServer = Org.Neo4j.Server.web.WebServer;

	public class ConsoleModule : ServerModule
	{
		 private readonly WebServer _webServer;
		 private Config _config;

		 public ConsoleModule( WebServer webServer, Config config )
		 {
			  this._webServer = webServer;
			  this._config = config;
		 }

		 public override void Start()
		 {
			  if ( _config.get( ServerSettings.console_module_enabled ) )
			  {
					string serverMountPoint = ManagementApiUri().ToString();
					_webServer.addJAXRSClasses( ClassNames, serverMountPoint, null );
			  }
		 }

		 private IList<string> ClassNames
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return Collections.singletonList( typeof( ConsoleService ).FullName );
			 }
		 }

		 private URI ManagementApiUri()
		 {
			  return _config.get( ServerSettings.management_api_path );
		 }

		 public override void Stop()
		 {
		 }
	}

}