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
namespace Neo4Net.Server.modules
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ConsoleService = Neo4Net.Server.rest.management.console.ConsoleService;
	using WebServer = Neo4Net.Server.web.WebServer;

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