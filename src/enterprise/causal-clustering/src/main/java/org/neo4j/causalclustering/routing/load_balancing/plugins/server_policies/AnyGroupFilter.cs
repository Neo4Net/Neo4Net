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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{

	using Neo4Net.causalclustering.routing.load_balancing.filters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;

	/// <summary>
	/// Only returns servers matching any of the supplied groups.
	/// </summary>
	public class AnyGroupFilter : Filter<ServerInfo>
	{
		 private readonly System.Predicate<ServerInfo> _matchesAnyGroup;
		 private readonly ISet<string> _groups;

		 internal AnyGroupFilter( params string[] groups ) : this( asSet( groups ) )
		 {
		 }

		 internal AnyGroupFilter( ISet<string> groups )
		 {
			  this._matchesAnyGroup = serverInfo =>
			  {
				foreach ( string group in serverInfo.groups() )
				{
					 if ( groups.Contains( group ) )
					 {
						  return true;
					 }
				}
				return false;
			  };
			  this._groups = groups;
		 }

		 public override ISet<ServerInfo> Apply( ISet<ServerInfo> data )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return data.Where( _matchesAnyGroup ).collect( Collectors.toSet() );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  AnyGroupFilter that = ( AnyGroupFilter ) o;
			  return Objects.Equals( _groups, that._groups );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _groups );
		 }

		 public override string ToString()
		 {
			  return "AnyGroupFilter{" +
						"groups=" + _groups +
						'}';
		 }
	}

}