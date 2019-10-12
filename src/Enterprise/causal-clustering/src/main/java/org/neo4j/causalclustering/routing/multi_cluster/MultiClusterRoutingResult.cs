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
namespace Neo4Net.causalclustering.routing.multi_cluster
{


	/// <summary>
	/// Simple struct containing the the result of multi-cluster routing procedure execution.
	/// </summary>
	public class MultiClusterRoutingResult : RoutingResult
	{
		 private readonly IDictionary<string, IList<Endpoint>> _routers;
		 private readonly long _timeToLiveMillis;

		 public MultiClusterRoutingResult( IDictionary<string, IList<Endpoint>> routers, long timeToLiveMillis )
		 {
			  this._routers = routers;
			  this._timeToLiveMillis = timeToLiveMillis;
		 }

		 public virtual IDictionary<string, IList<Endpoint>> Routers()
		 {
			  return _routers;
		 }

		 public override long TtlMillis()
		 {
			  return _timeToLiveMillis;
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
			  MultiClusterRoutingResult that = ( MultiClusterRoutingResult ) o;
			  return _timeToLiveMillis == that._timeToLiveMillis && Objects.Equals( _routers, that._routers );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _routers, _timeToLiveMillis );
		 }
	}


}