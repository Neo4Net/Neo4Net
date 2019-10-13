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
namespace Neo4Net.Kernel.api.query
{

	internal sealed class SimpleState : ExecutingQueryStatus
	{
		 private static readonly ExecutingQueryStatus _planning = new SimpleState( PLANNING_STATE );
		 private static readonly ExecutingQueryStatus _running = new SimpleState( RUNNING_STATE );
		 private readonly string _name;

		 internal static ExecutingQueryStatus Planning()
		 {
			  return _planning;
		 }

		 internal static ExecutingQueryStatus Running()
		 {
			  return _running;
		 }

		 private SimpleState( string name )
		 {
			  this._name = name;
		 }

		 internal override long WaitTimeNanos( long currentTimeNanos )
		 {
			  return 0;
		 }

		 internal override IDictionary<string, object> ToMap( long currentTimeNanos )
		 {
			  return emptyMap();
		 }

		 internal override string Name()
		 {
			  return _name;
		 }

		 internal override bool Planning
		 {
			 get
			 {
				  return this == _planning;
			 }
		 }
	}

}