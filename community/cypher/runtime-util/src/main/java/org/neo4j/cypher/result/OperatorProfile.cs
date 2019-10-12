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
namespace Org.Neo4j.Cypher.result
{
	using MathUtil = Org.Neo4j.Helpers.MathUtil;

	/// <summary>
	/// Profile for a operator during a query execution.
	/// </summary>
	public interface OperatorProfile
	{
		 /// <summary>
		 /// Time spent executing this operator.
		 /// </summary>
		 long Time();

		 /// <summary>
		 /// Database hits caused while executing this operator. This is an approximate measure
		 /// of how many nodes, records and properties that have been read.
		 /// </summary>
		 long DbHits();

		 /// <summary>
		 /// Number of rows produced by this operator.
		 /// </summary>
		 long Rows();

		 /// <summary>
		 /// Page cache hits while executing this operator.
		 /// </summary>
		 long PageCacheHits();

		 /// <summary>
		 /// Page cache misses while executing this operator.
		 /// </summary>
		 long PageCacheMisses();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default double pageCacheHitRatio()
	//	 {
	//		  return (pageCacheHits() == NO_DATA || pageCacheMisses() == NO_DATA) ? NO_DATA : MathUtil.portion(pageCacheHits(), pageCacheMisses());
	//	 }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 OperatorProfile NONE = new OperatorProfile()
	//	 {
	//		  @@Override public long time()
	//		  {
	//				return -1;
	//		  }
	//
	//		  @@Override public long dbHits()
	//		  {
	//				return -1;
	//		  }
	//
	//		  @@Override public long rows()
	//		  {
	//				return -1;
	//		  }
	//
	//		  @@Override public long pageCacheHits()
	//		  {
	//				return -1;
	//		  }
	//
	//		  @@Override public long pageCacheMisses()
	//		  {
	//				return -1;
	//		  }
	//	 };
	}

	public static class OperatorProfile_Fields
	{
		 public const long NO_DATA = -1L;
	}

}