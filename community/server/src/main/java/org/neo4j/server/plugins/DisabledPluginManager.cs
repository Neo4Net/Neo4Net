﻿using System;
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
namespace Org.Neo4j.Server.plugins
{

	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ExtensionPointRepresentation = Org.Neo4j.Server.rest.repr.ExtensionPointRepresentation;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public class DisabledPluginManager : PluginManager
	{
		 public static readonly PluginManager Instance = new DisabledPluginManager();

		 private DisabledPluginManager()
		 {
		 }

		 [Obsolete]
		 public override Representation Invoke<T>( GraphDatabaseAPI graphDb, string name, Type type, string method, T context, ParameterList @params )
		 {
				 type = typeof( T );
			  throw new System.NotSupportedException();
		 }

		 [Obsolete]
		 public override ExtensionPointRepresentation Describe( string name, Type type, string method )
		 {
			  throw new System.NotSupportedException();
		 }

		 [Obsolete]
		 public override IList<ExtensionPointRepresentation> DescribeAll( string extensionName )
		 {
			  return Collections.emptyList();
		 }

		 [Obsolete]
		 public override ISet<string> ExtensionNames()
		 {
			  return Collections.emptySet();
		 }

		 [Obsolete]
		 public override IDictionary<string, IList<string>> GetExensionsFor( Type type )
		 {
			  return Collections.emptyMap();
		 }
	}

}