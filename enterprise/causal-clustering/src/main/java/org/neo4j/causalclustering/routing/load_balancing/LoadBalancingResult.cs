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
namespace Org.Neo4j.causalclustering.routing.load_balancing
{

	/// <summary>
	/// The outcome of applying a load balancing plugin, which will be used by client
	/// software for scheduling work at the endpoints.
	/// </summary>
	public class LoadBalancingResult : LoadBalancingProcessor_Result
	{
		 private readonly IList<Endpoint> _routeEndpoints;
		 private readonly IList<Endpoint> _writeEndpoints;
		 private readonly IList<Endpoint> _readEndpoints;
		 private readonly long _timeToLiveMillis;

		 public LoadBalancingResult( IList<Endpoint> routeEndpoints, IList<Endpoint> writeEndpoints, IList<Endpoint> readEndpoints, long timeToLiveMillis )
		 {
			  this._routeEndpoints = routeEndpoints;
			  this._writeEndpoints = writeEndpoints;
			  this._readEndpoints = readEndpoints;
			  this._timeToLiveMillis = timeToLiveMillis;
		 }

		 public override long TtlMillis()
		 {
			  return _timeToLiveMillis;
		 }

		 public override IList<Endpoint> RouteEndpoints()
		 {
			  return _routeEndpoints;
		 }

		 public override IList<Endpoint> WriteEndpoints()
		 {
			  return _writeEndpoints;
		 }

		 public override IList<Endpoint> ReadEndpoints()
		 {
			  return _readEndpoints;
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
			  LoadBalancingResult that = ( LoadBalancingResult ) o;
			  return _timeToLiveMillis == that._timeToLiveMillis && Objects.Equals( _routeEndpoints, that._routeEndpoints ) && Objects.Equals( _writeEndpoints, that._writeEndpoints ) && Objects.Equals( _readEndpoints, that._readEndpoints );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _routeEndpoints, _writeEndpoints, _readEndpoints, _timeToLiveMillis );
		 }

		 public override string ToString()
		 {
			  return "LoadBalancingResult{" +
						"routeEndpoints=" + _routeEndpoints +
						", writeEndpoints=" + _writeEndpoints +
						", readEndpoints=" + _readEndpoints +
						", timeToLiveMillis=" + _timeToLiveMillis +
						'}';
		 }
	}

}