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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;

	/// <summary>
	/// <seealso cref="MemoryStatsVisitor"/> that can gather stats from multiple sources and give a total.
	/// </summary>
	public class GatheringMemoryStatsVisitor : MemoryStatsVisitor
	{
		 private long _heapUsage;
		 private long _offHeapUsage;

		 public override void HeapUsage( long bytes )
		 {
			  _heapUsage += bytes;
		 }

		 public override void OffHeapUsage( long bytes )
		 {
			  _offHeapUsage += bytes;
		 }

		 public virtual long HeapUsage
		 {
			 get
			 {
				  return _heapUsage;
			 }
		 }

		 public virtual long OffHeapUsage
		 {
			 get
			 {
				  return _offHeapUsage;
			 }
		 }

		 public virtual long TotalUsage
		 {
			 get
			 {
				  return _heapUsage + _offHeapUsage;
			 }
		 }

		 public override string ToString()
		 {
			  return "Memory usage[heap:" + bytes( _heapUsage ) + ", off-heap:" + bytes( _offHeapUsage ) + "]";
		 }

		 public static long TotalMemoryUsageOf( params MemoryStatsVisitor_Visitable[] memoryUsers )
		 {
			  GatheringMemoryStatsVisitor memoryVisitor = new GatheringMemoryStatsVisitor();
			  foreach ( MemoryStatsVisitor_Visitable memoryUser in memoryUsers )
			  {
					memoryUser.AcceptMemoryStatsVisitor( memoryVisitor );
			  }
			  return memoryVisitor.TotalUsage;
		 }

		 public static long HighestMemoryUsageOf( params MemoryStatsVisitor_Visitable[] memoryUsers )
		 {
			  long max = 0;
			  foreach ( MemoryStatsVisitor_Visitable visitable in memoryUsers )
			  {
					max = max( max, TotalMemoryUsageOf( visitable ) );
			  }
			  return max;
		 }
	}

}