using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.multi_cluster.procedure
{


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.procedure.RoutingResultFormatHelper.parseEndpoints;

	/// <summary>
	/// The result format of <seealso cref="GetRoutersForDatabaseProcedure"/> and
	/// <seealso cref="GetRoutersForAllDatabasesProcedure"/> procedures.
	/// </summary>
	public class MultiClusterRoutingResultFormat
	{

		 private const string DB_NAME_KEY = "database";
		 private const string ADDRESSES_KEY = "addresses";

		 private MultiClusterRoutingResultFormat()
		 {
		 }

		 internal static object[] Build( MultiClusterRoutingResult result )
		 {
			  System.Func<IList<Endpoint>, object[]> stringifyAddresses = es => es.Select( e => e.address().ToString() ).ToArray();

			  IList<IDictionary<string, object>> response = result.Routers().SetOfKeyValuePairs().Select(entry =>
			  {
				string dbName = entry.Key;
				object[] addresses = stringifyAddresses( entry.Value );

				IDictionary<string, object> responseRow = new SortedDictionary<string, object>();

				responseRow.put( DB_NAME_KEY, dbName );
				responseRow.put( ADDRESSES_KEY, addresses );

				return responseRow;
			  }).ToList();

			  long ttlSeconds = MILLISECONDS.toSeconds( result.TtlMillis() );
			  return new object[]{ ttlSeconds, response };
		 }

		 public static MultiClusterRoutingResult Parse( IDictionary<string, object> record )
		 {
			  return Parse( new object[]{ record[ParameterNames.Ttl.parameterName()], record[ParameterNames.Routers.parameterName()] } );
		 }

		 public static MultiClusterRoutingResult Parse( object[] record )
		 {
			  long ttlSeconds = ( long ) record[0];
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String,Object>> rows = (java.util.List<java.util.Map<String,Object>>) record[1];
			  IList<IDictionary<string, object>> rows = ( IList<IDictionary<string, object>> ) record[1];
			  IDictionary<string, IList<Endpoint>> routers = ParseRouters( rows );

			  return new MultiClusterRoutingResult( routers, ttlSeconds * 1000 );
		 }

		 private static IDictionary<string, IList<Endpoint>> ParseRouters( IList<IDictionary<string, object>> responseRows )
		 {
			  System.Func<IDictionary<string, object>, string> dbNameFromRow = row => ( string ) row.get( DB_NAME_KEY );
			  System.Func<IDictionary<string, object>, IList<Endpoint>> endpointsFromRow = row => parseEndpoints( ( object[] ) row.get( ADDRESSES_KEY ), Role.ROUTE );
			  return responseRows.ToDictionary( dbNameFromRow, endpointsFromRow );
		 }
	}

}