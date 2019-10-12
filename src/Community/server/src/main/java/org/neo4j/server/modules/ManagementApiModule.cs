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

	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using JmxService = Neo4Net.Server.rest.management.JmxService;
	using RootService = Neo4Net.Server.rest.management.RootService;
	using VersionAndEditionService = Neo4Net.Server.rest.management.VersionAndEditionService;
	using WebServer = Neo4Net.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class ManagementApiModule : ServerModule
	{
		 private readonly Config _config;
		 private readonly WebServer _webServer;

		 public ManagementApiModule( WebServer webServer, Config config )
		 {
			  this._webServer = webServer;
			  this._config = config;
		 }

		 public override void Start()
		 {
			  string serverMountPoint = ManagementApiUri().ToString();
			  _webServer.addJAXRSClasses( ClassNames, serverMountPoint, null );
		 }

		 private IList<string> ClassNames
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return new IList<string> { typeof( JmxService ).FullName, typeof( RootService ).FullName, typeof( VersionAndEditionService ).FullName };
			 }
		 }

		 public override void Stop()
		 {
			  _webServer.removeJAXRSClasses( ClassNames, ManagementApiUri().ToString() );
		 }

		 private URI ManagementApiUri()
		 {
			  return _config.get( ServerSettings.management_api_path );
		 }
	}

}