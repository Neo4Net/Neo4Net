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
namespace Neo4Net.Server.plugins
{

	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using ExtensionPointRepresentation = Neo4Net.Server.rest.repr.ExtensionPointRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public interface PluginInvocator
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T> Neo4Net.server.rest.repr.Representation invoke(Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, String name, Class<T> type, String method, T context, ParameterList params) throws PluginLookupException, Neo4Net.server.rest.repr.BadInputException, PluginInvocationFailureException, BadPluginInvocationException;
		 [Obsolete]
		 Representation invoke<T>( GraphDatabaseAPI graphDb, string name, Type type, string method, T context, ParameterList @params );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExtensionPointRepresentation describe(String name, Class type, String method) throws PluginLookupException;
		 [Obsolete]
		 ExtensionPointRepresentation Describe( string name, Type type, string method );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: List<Neo4Net.server.rest.repr.ExtensionPointRepresentation> describeAll(String extensionName) throws PluginLookupException;
		 [Obsolete]
		 IList<ExtensionPointRepresentation> DescribeAll( string extensionName );

		 [Obsolete]
		 ISet<string> ExtensionNames();
	}

}