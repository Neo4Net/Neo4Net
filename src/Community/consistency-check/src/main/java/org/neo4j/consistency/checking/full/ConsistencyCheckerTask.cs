using System.Threading;

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
namespace Neo4Net.Consistency.checking.full
{
	using Statistics = Neo4Net.Consistency.statistics.Statistics;

	public abstract class ConsistencyCheckerTask : ThreadStart
	{
		 protected internal readonly string Name;
		 protected internal readonly Statistics Statistics;
		 protected internal readonly int NumberOfThreads;

		 protected internal ConsistencyCheckerTask( string name, Statistics statistics, int threads )
		 {
			  this.Name = name;
			  this.Statistics = statistics;
			  this.NumberOfThreads = threads;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + Name + "]";
		 }
	}

}