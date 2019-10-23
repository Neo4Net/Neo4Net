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
namespace Neo4Net.@unsafe.Impl.Batchimport.stats
{
	/// <summary>
	/// Common <seealso cref="Stat statistic"/> keys.
	/// </summary>
	public sealed class Keys : Key
	{
		 public static readonly Keys ReceivedBatches = new Keys( "ReceivedBatches", InnerEnum.ReceivedBatches, ">", "Number of batches received from upstream" );
		 public static readonly Keys DoneBatches = new Keys( "DoneBatches", InnerEnum.DoneBatches, "!", "Number of batches processed and done, and sent off downstream" );
		 public static readonly Keys TotalProcessingTime = new Keys( "TotalProcessingTime", InnerEnum.TotalProcessingTime, "=", "Total processing time for all done batches" );
		 public static readonly Keys UpstreamIdleTime = new Keys( "UpstreamIdleTime", InnerEnum.UpstreamIdleTime, "^", "Time spent waiting for batch from upstream" );
		 public static readonly Keys DownstreamIdleTime = new Keys( "DownstreamIdleTime", InnerEnum.DownstreamIdleTime, "v", "Time spent waiting for downstream to catch up" );
		 public static readonly Keys AvgProcessingTime = new Keys( "AvgProcessingTime", InnerEnum.AvgProcessingTime, "avg", "Average processing time per done batch" );
		 public static readonly Keys IoThroughput = new Keys( "IoThroughput", InnerEnum.IoThroughput, null, "I/O throughput per second" );
		 public static readonly Keys MemoryUsage = new Keys( "MemoryUsage", InnerEnum.MemoryUsage, null, "Memory usage" );
		 public static readonly Keys Progress = new Keys( "Progress", InnerEnum.Progress, null, "Progress" ); // overrides progress calculation using done_batches, if this stat exists

		 private static readonly IList<Keys> valueList = new List<Keys>();

		 static Keys()
		 {
			 valueList.Add( ReceivedBatches );
			 valueList.Add( DoneBatches );
			 valueList.Add( TotalProcessingTime );
			 valueList.Add( UpstreamIdleTime );
			 valueList.Add( DownstreamIdleTime );
			 valueList.Add( AvgProcessingTime );
			 valueList.Add( IoThroughput );
			 valueList.Add( MemoryUsage );
			 valueList.Add( Progress );
		 }

		 public enum InnerEnum
		 {
			 ReceivedBatches,
			 DoneBatches,
			 TotalProcessingTime,
			 UpstreamIdleTime,
			 DownstreamIdleTime,
			 AvgProcessingTime,
			 IoThroughput,
			 MemoryUsage,
			 Progress
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal Keys( string name, InnerEnum innerEnum, string shortName, string description )
		 {
			  this._shortName = shortName;
			  this._description = description;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string ShortName()
		 {
			  return _shortName;
		 }

		 public string Description()
		 {
			  return _description;
		 }

		public static IList<Keys> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static Keys ValueOf( string name )
		{
			foreach ( Keys enumInstance in Keys.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}