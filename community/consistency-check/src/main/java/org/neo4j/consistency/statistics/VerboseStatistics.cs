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
namespace Org.Neo4j.Consistency.statistics
{
	using Format = Org.Neo4j.Helpers.Format;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	public class VerboseStatistics : Statistics
	{
		 private readonly AccessStatistics _accessStatistics;
		 private readonly Counts _counts;
		 private readonly Log _logger;
		 private long _startTime;

		 public VerboseStatistics( AccessStatistics accessStatistics, Counts counts, Log logger )
		 {
			  this._accessStatistics = accessStatistics;
			  this._counts = counts;
			  this._logger = logger;
		 }

		 public override void Print( string name )
		 {
			  string accessStr = _accessStatistics.AccessStatSummary;
			  _logger.info( format( "=== %s ===", name ) );
			  _logger.info( format( "I/Os%n%s", accessStr ) );
			  _logger.info( _counts.ToString() );
			  _logger.info( MemoryStats() );
			  _logger.info( "Done in  " + Format.duration( currentTimeMillis() - _startTime ) );
		 }

		 public override void Reset()
		 {
			  _accessStatistics.reset();
			  _counts.reset();
			  _startTime = currentTimeMillis();
		 }

		 private static string MemoryStats()
		 {
			  Runtime runtime = Runtime.Runtime;
			  return format( "Memory[used:%s, free:%s, total:%s, max:%s]", Format.bytes( runtime.totalMemory() - runtime.freeMemory() ), Format.bytes(runtime.freeMemory()), Format.bytes(runtime.totalMemory()), Format.bytes(runtime.maxMemory()) );
		 }

		 public virtual Counts Counts
		 {
			 get
			 {
				  return _counts;
			 }
		 }
	}

}