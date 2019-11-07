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
namespace Neo4Net.Kernel.impl.query
{

	using Service = Neo4Net.Helpers.Service;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.asList;

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