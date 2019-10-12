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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{

	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;

	internal class FilterBuilder
	{
		 private IList<Filter<ServerInfo>> _current = new List<Filter<ServerInfo>>();
		 private IList<FilterChain<ServerInfo>> _rules = new List<FilterChain<ServerInfo>>();

		 internal static FilterBuilder Filter()
		 {
			  return new FilterBuilder();
		 }

		 internal virtual FilterBuilder Min( int minCount )
		 {
			  _current.Add( new MinimumCountFilter<>( minCount ) );
			  return this;
		 }

		 internal virtual FilterBuilder Groups( params string[] groups )
		 {
			  _current.Add( new AnyGroupFilter( groups ) );
			  return this;
		 }

		 internal virtual FilterBuilder All()
		 {
			  _current.Add( IdentityFilter.@as() );
			  return this;
		 }

		 internal virtual FilterBuilder NewRule()
		 {
			  if ( _current.Count > 0 )
			  {
					_rules.Add( new FilterChain<>( _current ) );
					_current = new List<Filter<ServerInfo>>();
			  }
			  return this;
		 }

		 internal virtual Filter<ServerInfo> Build()
		 {
			  NewRule();
			  return new FirstValidRule<ServerInfo>( _rules );
		 }
	}

}