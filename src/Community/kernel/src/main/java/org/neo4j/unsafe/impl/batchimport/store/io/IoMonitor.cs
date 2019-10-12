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
namespace Neo4Net.@unsafe.Impl.Batchimport.store.io
{
	using Key = Neo4Net.@unsafe.Impl.Batchimport.stats.Key;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// <seealso cref="IoTracer"/> exposed as a <seealso cref="StatsProvider"/>.
	/// 
	/// Assumes that I/O is busy all the time.
	/// </summary>
	public class IoMonitor : StatsProvider
	{
		 private volatile long _startTime;
		 private volatile long _endTime;
		 private readonly IoTracer _tracer;
		 private long _resetPoint;

		 public IoMonitor( IoTracer tracer )
		 {
			  this._tracer = tracer;
			  Reset();
		 }

		 public virtual void Reset()
		 {
			  _startTime = currentTimeMillis();
			  _endTime = 0;
			  _resetPoint = _tracer.countBytesWritten();
		 }

		 public virtual void Stop()
		 {
			  _endTime = currentTimeMillis();
		 }

		 private long TotalBytesWritten()
		 {
			  return _tracer.countBytesWritten() - _resetPoint;
		 }

		 public override Stat Stat( Key key )
		 {
			  if ( key == Keys.io_throughput )
			  {
					return new IoThroughputStat( _startTime, _endTime, TotalBytesWritten() );
			  }
			  return null;
		 }

		 public override Key[] Keys()
		 {
			  return new Key[] { Keys.io_throughput };
		 }
	}

}