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
namespace Neo4Net.Server.plugins
{

	using Configuration = org.apache.commons.configuration.Configuration;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;

	/// <summary>
	///  Interface to be implemented and exposed via the Java ServiceLocator mechanism that allows
	///  plugins to provide their own initialization.<br>
	///  The implementations of this interface have to be listed in a file
	///  META-INF/services/org.neo4j.server.plugins.PluginLifecycle
	///  that contains the fully qualified class names of the individual plugin. This file
	///  has to be supplied with the plugin jar to the Neo4j server.<br>
	///  The plugin might return a collection of <seealso cref="Injectable"/>s that can later be used with
	///  {@literal @Context} injections. </summary>
	///  @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public interface PluginLifecycle
	{
		 /// <summary>
		 /// Called at initialization time, before the plugin resources are actually loaded. </summary>
		 /// <param name="graphDatabaseService"> of the Neo4j service, use it to integrate it with custom configuration mechanisms </param>
		 /// <param name="config"> server configuration </param>
		 /// <returns> A list of <seealso cref="Injectable"/>s that will be available to resource dependency injection later </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Collection<Injectable<?>> start(org.neo4j.graphdb.GraphDatabaseService graphDatabaseService, org.apache.commons.configuration.Configuration config);
		 [Obsolete]
		 ICollection<Injectable<object>> Start( GraphDatabaseService graphDatabaseService, Configuration config );

		 /// <summary>
		 /// called to shutdown individual external resources or configurations
		 /// </summary>
		 [Obsolete]
		 void Stop();
	}

}