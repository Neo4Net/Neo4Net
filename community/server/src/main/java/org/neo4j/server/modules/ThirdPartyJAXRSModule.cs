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
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using ThirdPartyJaxRsPackage = Org.Neo4j.Server.configuration.ThirdPartyJaxRsPackage;
	using Org.Neo4j.Server.plugins;
	using WebServer = Org.Neo4j.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class ThirdPartyJAXRSModule : ServerModule
	{
		 private readonly Config _config;
		 private readonly WebServer _webServer;

		 private readonly ExtensionInitializer _extensionInitializer;
		 private IList<ThirdPartyJaxRsPackage> _packages;
		 private readonly Log _log;

		 public ThirdPartyJAXRSModule( WebServer webServer, Config config, LogProvider logProvider, NeoServer neoServer )
		 {
			  this._webServer = webServer;
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
			  _extensionInitializer = new ExtensionInitializer( neoServer );
		 }

		 public override void Start()
		 {
			  this._packages = _config.get( ServerSettings.third_party_packages );
			  foreach ( ThirdPartyJaxRsPackage tpp in _packages )
			  {
					IList<string> packageNames = PackagesFor( tpp );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.server.plugins.Injectable<?>> injectables = extensionInitializer.initializePackages(packageNames);
					ICollection<Injectable<object>> injectables = _extensionInitializer.initializePackages( packageNames );
					_webServer.addJAXRSPackages( packageNames, tpp.MountPoint, injectables );
					_log.info( "Mounted unmanaged extension [%s] at [%s]", tpp.PackageName, tpp.MountPoint );
			  }
		 }

		 private IList<string> PackagesFor( ThirdPartyJaxRsPackage tpp )
		 {
			  return new IList<string> { tpp.PackageName };
		 }

		 public override void Stop()
		 {
			  if ( _packages == null )
			  {
					return;
			  }

			  foreach ( ThirdPartyJaxRsPackage tpp in _packages )
			  {
					_webServer.removeJAXRSPackages( PackagesFor( tpp ), tpp.MountPoint );
			  }

			  _extensionInitializer.stop();
		 }
	}

}