﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.query
{

	using Service = Org.Neo4j.Helpers.Service;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;

	public abstract class QueryEngineProvider : Service
	{
		 public QueryEngineProvider( string name ) : base( name )
		 {
		 }

		 protected internal abstract QueryExecutionEngine CreateEngine( Dependencies deps, GraphDatabaseAPI graphAPI );

		 protected internal abstract int EnginePriority();

		 public static QueryExecutionEngine Initialize( Dependencies deps, GraphDatabaseAPI graphAPI, IEnumerable<QueryEngineProvider> providers )
		 {
			  IList<QueryEngineProvider> engineProviders = new IList<QueryEngineProvider> { providers };
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  engineProviders.sort( System.Collections.IComparer.comparingInt( QueryEngineProvider::enginePriority ) );
			  QueryEngineProvider provider = Iterables.firstOrNull( engineProviders );

			  if ( provider == null )
			  {
					return NoEngine();
			  }
			  QueryExecutionEngine engine = provider.CreateEngine( deps, graphAPI );
			  return deps.SatisfyDependency( engine );
		 }

		 public static QueryExecutionEngine NoEngine()
		 {
			  return NoQueryEngine.Instance;
		 }
	}

}