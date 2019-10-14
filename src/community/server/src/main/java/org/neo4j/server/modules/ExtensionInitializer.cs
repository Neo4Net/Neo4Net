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
namespace Neo4Net.Server.modules
{

	using Service = Neo4Net.Helpers.Service;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfigAdapter = Neo4Net.Server.plugins.ConfigAdapter;
	using Neo4Net.Server.plugins;
	using PluginLifecycle = Neo4Net.Server.plugins.PluginLifecycle;
	using SPIPluginLifecycle = Neo4Net.Server.plugins.SPIPluginLifecycle;

	/// <summary>
	/// Allows unmanaged extensions to provide their own initialization
	/// </summary>
	public class ExtensionInitializer
	{
		 private readonly IEnumerable<PluginLifecycle> _lifecycles;
		 private readonly NeoServer _neoServer;

		 public ExtensionInitializer( NeoServer neoServer )
		 {
			  this._neoServer = neoServer;
			  _lifecycles = Service.load( typeof( PluginLifecycle ) );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Collection<org.neo4j.server.plugins.Injectable<?>> initializePackages(Iterable<String> packageNames)
		 public virtual ICollection<Injectable<object>> InitializePackages( IEnumerable<string> packageNames )
		 {
			  GraphDatabaseAPI graphDatabaseService = _neoServer.Database.Graph;
			  Config configuration = _neoServer.Config;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.server.plugins.Injectable<?>> injectables = new java.util.HashSet<>();
			  ICollection<Injectable<object>> injectables = new HashSet<Injectable<object>>();
			  foreach ( PluginLifecycle lifecycle in _lifecycles )
			  {
					if ( HasPackage( lifecycle, packageNames ) )
					{
						 if ( lifecycle is SPIPluginLifecycle )
						 {
							  SPIPluginLifecycle lifeCycleSpi = ( SPIPluginLifecycle ) lifecycle;
							  injectables.addAll( lifeCycleSpi.Start( _neoServer ) );
						 }
						 else
						 {
							  injectables.addAll( lifecycle.Start( graphDatabaseService, new ConfigAdapter( configuration ) ) );
						 }
					}
			  }
			  return injectables;
		 }

		 private bool HasPackage( PluginLifecycle pluginLifecycle, IEnumerable<string> packageNames )
		 {
			  string lifecyclePackageName = pluginLifecycle.GetType().Assembly.GetName().Name;
			  foreach ( string packageName in packageNames )
			  {
					if ( lifecyclePackageName.StartsWith( packageName, StringComparison.Ordinal ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual void Stop()
		 {
			  foreach ( PluginLifecycle pluginLifecycle in _lifecycles )
			  {
					pluginLifecycle.Stop();
			  }
		 }
	}

}