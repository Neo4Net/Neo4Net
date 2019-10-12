using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.routing.load_balancing.procedure
{

	using SocketAddress = Org.Neo4j.Helpers.SocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.Role.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.Role.ROUTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.Role.WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.procedure.RoutingResultFormatHelper.parseEndpoints;

	/// <summary>
	/// The result format of GetServersV1 and GetServersV2 procedures.
	/// </summary>
	public class ResultFormatV1
	{
		 private const string ROLE_KEY = "role";
		 private const string ADDRESSES_KEY = "addresses";

		 private ResultFormatV1()
		 {
		 }

		 internal static object[] Build( Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  object[] routers = result.RouteEndpoints().Select(Endpoint::address).Select(SocketAddress::toString).ToArray();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  object[] readers = result.ReadEndpoints().Select(Endpoint::address).Select(SocketAddress::toString).ToArray();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  object[] writers = result.WriteEndpoints().Select(Endpoint::address).Select(SocketAddress::toString).ToArray();

			  IList<IDictionary<string, object>> servers = new List<IDictionary<string, object>>();

			  if ( writers.Length > 0 )
			  {
					IDictionary<string, object> map = new SortedDictionary<string, object>();

					map[ROLE_KEY] = WRITE.name();
					map[ADDRESSES_KEY] = writers;

					servers.Add( map );
			  }

			  if ( readers.Length > 0 )
			  {
					IDictionary<string, object> map = new SortedDictionary<string, object>();

					map[ROLE_KEY] = READ.name();
					map[ADDRESSES_KEY] = readers;

					servers.Add( map );
			  }

			  if ( routers.Length > 0 )
			  {
					IDictionary<string, object> map = new SortedDictionary<string, object>();

					map[ROLE_KEY] = ROUTE.name();
					map[ADDRESSES_KEY] = routers;

					servers.Add( map );
			  }

			  long timeToLiveSeconds = MILLISECONDS.toSeconds( result.TtlMillis() );
			  return new object[]{ timeToLiveSeconds, servers };
		 }

		 public static LoadBalancingResult Parse( object[] record )
		 {
			  long timeToLiveSeconds = ( long ) record[0];
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String,Object>> endpointData = (java.util.List<java.util.Map<String,Object>>) record[1];
			  IList<IDictionary<string, object>> endpointData = ( IList<IDictionary<string, object>> ) record[1];

			  IDictionary<Role, IList<Endpoint>> endpoints = ParseRows( endpointData );

			  return new LoadBalancingResult( endpoints[ROUTE], endpoints[WRITE], endpoints[READ], timeToLiveSeconds * 1000 );
		 }

		 public static LoadBalancingResult Parse( IDictionary<string, object> record )
		 {
			  return Parse( new object[]{ record[ParameterNames.Ttl.parameterName()], record[ParameterNames.Servers.parameterName()] } );
		 }

		 private static IDictionary<Role, IList<Endpoint>> ParseRows( IList<IDictionary<string, object>> result )
		 {
			  IDictionary<Role, IList<Endpoint>> endpoints = new Dictionary<Role, IList<Endpoint>>();
			  foreach ( IDictionary<string, object> single in result )
			  {
					Role role = Enum.Parse( typeof( Role ), ( string ) single["role"] );
					IList<Endpoint> addresses = parseEndpoints( ( object[] ) single["addresses"], role );
					endpoints[role] = addresses;
			  }

			  java.util.Enum.GetValues( typeof( Role ) ).ForEach( r => endpoints.putIfAbsent( r, Collections.emptyList() ) );

			  return endpoints;
		 }

	}

}