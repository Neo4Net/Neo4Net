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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using MathUtil = Neo4Net.Helpers.MathUtil;

	/// <summary>
	/// Profiler statistics for a single execution step of a Cypher query execution plan
	/// </summary>
	public interface ProfilerStatistics
	{
		 /// <returns> number of rows processed by the associated execution step </returns>
		 long Rows { get; }

		 /// <returns> number of database hits (potential disk accesses) caused by executing the associated execution step </returns>
		 long DbHits { get; }

		 /// <returns> number of page cache hits caused by executing the associated execution step </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getPageCacheHits()
	//	 {
	//		  return 0;
	//	 }

		 /// <returns> number of page cache misses caused by executing the associated execution step </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getPageCacheMisses()
	//	 {
	//		  return 0;
	//	 }

		 /// <returns> the ratio of page cache hits to total number of lookups or <seealso cref="Double.NaN"/> if no data is available </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default double getPageCacheHitRatio()
	//	 {
	//		  return MathUtil.portion(getPageCacheHits(), getPageCacheMisses());
	//	 }
	}

}