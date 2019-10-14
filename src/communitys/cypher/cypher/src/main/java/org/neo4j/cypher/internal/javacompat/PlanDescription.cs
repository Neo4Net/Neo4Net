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
namespace Neo4Net.Cypher.@internal.javacompat
{



	/// <summary>
	/// Instances describe single execution steps in a Cypher query execution plan
	/// 
	/// Execution plans form a tree of execution steps.  Each step is described by a PlanDescription object.
	/// </summary>
	public interface PlanDescription
	{
		 /// <returns> descriptive name for this kind of execution step </returns>
		 string Name { get; }

		 /// <summary>
		 /// Retrieve argument map for the associated execution step
		 /// 
		 /// Valid arguments are all Java primitive values, Strings, Arrays of those, and Maps from Strings to
		 /// valid arguments.  Results are guaranteed to be trees (i.e. there are no cyclic dependencies among values)
		 /// </summary>
		 /// <returns> a map containing arguments that describe this execution step in more detail </returns>
		 IDictionary<string, object> Arguments { get; }

		 /// <returns> list of previous (child) execution step descriptions </returns>
		 IList<PlanDescription> Children { get; }

		 /// <returns> true, if ProfilerStatistics are available for this execution step </returns>
		 bool HasProfilerStatistics();

		 /// <returns> profiler statistics for this execution step iff available </returns>
		 /// <exception cref="ProfilerStatisticsNotReadyException"> iff profiler statistics are not available </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProfilerStatistics getProfilerStatistics() throws org.neo4j.cypher.ProfilerStatisticsNotReadyException;
		 ProfilerStatistics ProfilerStatistics { get; }
	}

}