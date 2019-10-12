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
namespace Neo4Net.Graphdb
{

	using MathUtil = Neo4Net.Helpers.MathUtil;

	/// <summary>
	/// Instances describe single execution steps in a Cypher query execution plan
	/// 
	/// Execution plans form a tree of execution steps.  Each step is described by a <seealso cref="ExecutionPlanDescription"/> object.
	/// </summary>
	public interface ExecutionPlanDescription
	{
		 /// <summary>
		 /// Retrieves the name of this execution step.
		 /// </summary>
		 /// <returns> descriptive name for this kind of execution step </returns>
		 string Name { get; }

		 /// <summary>
		 /// Retrieves the children of this execution step.
		 /// </summary>
		 /// <returns> list of previous (child) execution step descriptions </returns>
		 IList<ExecutionPlanDescription> Children { get; }

		 /// <summary>
		 /// Retrieve argument map for the associated execution step
		 /// 
		 /// Valid arguments are all Java primitive values, Strings, Arrays of those, and Maps from Strings to
		 /// valid arguments.  Results are guaranteed to be trees (i.e. there are no cyclic dependencies among values)
		 /// </summary>
		 /// <returns> a map containing arguments that describe this execution step in more detail </returns>
		 IDictionary<string, object> Arguments { get; }

		 /// <returns> the set of identifiers used in this execution step </returns>
		 ISet<string> Identifiers { get; }

		 /// <summary>
		 /// Signifies that the query was profiled, and that statistics from the profiling can
		 /// <seealso cref="getProfilerStatistics() be retrieved"/>.
		 /// 
		 /// The <a href="https://neo4j.com/docs/developer-manual/current/cypher/execution-plans/">{@code PROFILE}</a> directive in Cypher
		 /// ensures the presence of profiler statistics in the plan description.
		 /// </summary>
		 /// <returns> true, if <seealso cref="ProfilerStatistics"/> are available for this execution step </returns>
		 bool HasProfilerStatistics();

		 /// <summary>
		 /// Retrieve the statistics collected from profiling this query.
		 /// 
		 /// If the query was not profiled, this method will throw <seealso cref="java.util.NoSuchElementException"/>.
		 /// </summary>
		 /// <returns> profiler statistics for this execution step iff available </returns>
		 /// <exception cref="java.util.NoSuchElementException"> iff profiler statistics are not available </exception>
		 ExecutionPlanDescription_ProfilerStatistics ProfilerStatistics { get; }

		 /// <summary>
		 /// Instances describe statistics from the profiler of a particular step in the execution plan.
		 /// </summary>
	}

	 public interface ExecutionPlanDescription_ProfilerStatistics
	 {
		  /// <returns> number of rows processed by the associated execution step </returns>
		  long Rows { get; }

		  /// <returns> number of database hits (potential disk accesses) caused by executing the associated execution step </returns>
		  long DbHits { get; }

		  /// <returns> number of page cache hits caused by executing the associated execution step </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default long getPageCacheHits()
	//	  {
	//			return 0;
	//	  }

		  /// <returns> number of page cache misses caused by executing the associated execution step </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default long getPageCacheMisses()
	//	  {
	//			return 0;
	//	  }

		  /// <returns> the ratio of page cache hits to total number of lookups or <seealso cref="Double.NaN"/> if no data is available </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default double getPageCacheHitRatio()
	//	  {
	//			return MathUtil.portion(getPageCacheHits(), getPageCacheMisses());
	//	  }
	 }

}