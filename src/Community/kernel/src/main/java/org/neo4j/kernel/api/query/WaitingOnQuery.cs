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
namespace Neo4Net.Kernel.api.query
{

	internal class WaitingOnQuery : ExecutingQueryStatus
	{
		 private readonly ExecutingQuery _query;
		 private readonly long _startTimeNanos;

		 internal WaitingOnQuery( ExecutingQuery query, long startTimeNanos )
		 {
			  this._query = query;
			  this._startTimeNanos = startTimeNanos;
		 }

		 internal override long WaitTimeNanos( long currentTimeNanos )
		 {
			  return currentTimeNanos - _startTimeNanos;
		 }

		 internal override IDictionary<string, object> ToMap( long currentTimeNanos )
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["queryId"] = "query-" + _query.internalQueryId();
			  map["waitTimeMillis"] = TimeUnit.NANOSECONDS.toMillis( WaitTimeNanos( currentTimeNanos ) );
			  return map;
		 }

		 internal override string Name()
		 {
			  return WAITING_STATE;
		 }
	}

}