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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Format = Neo4Net.Helpers.Format;
	using DetailLevel = Neo4Net.@unsafe.Impl.Batchimport.stats.DetailLevel;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// <seealso cref="Stat"/> that provides a simple Mb/s stat, mostly used for getting an insight into I/O throughput.
	/// </summary>
	public class IoThroughputStat : Stat
	{
		 private readonly long _startTime;
		 private readonly long _endTime;
		 private readonly long _position;

		 public IoThroughputStat( long startTime, long endTime, long position )
		 {
			  this._startTime = startTime;
			  this._endTime = endTime;
			  this._position = position;
		 }

		 public override DetailLevel DetailLevel()
		 {
			  return DetailLevel.IMPORTANT;
		 }

		 public override long AsLong()
		 {
			  long endTime = this._endTime != 0 ? this._endTime : currentTimeMillis();
			  long totalTime = endTime - _startTime;
			  int seconds = ( int )( totalTime / 1000 );
			  return seconds > 0 ? _position / seconds : -1;
		 }

		 public override string ToString()
		 {
			  long stat = AsLong();
			  return stat == -1 ? "??" : Format.bytes( stat ) + "/s";
		 }
	}

}