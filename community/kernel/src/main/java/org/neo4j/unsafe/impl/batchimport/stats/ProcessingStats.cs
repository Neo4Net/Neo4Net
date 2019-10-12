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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.stats
{
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.stats.Stats.longStat;

	/// <summary>
	/// Provides common <seealso cref="Stat statistics"/> about a <seealso cref="Step"/>, stats like number of processed batches,
	/// processing time a.s.o.
	/// </summary>
	public class ProcessingStats : GenericStatsProvider
	{
		 public ProcessingStats( long receivedBatches, long doneBatches, long totalProcessingTime, long average, long upstreamIdleTime, long downstreamIdleTime )
		 {
			  Add( Keys.ReceivedBatches, longStat( receivedBatches ) );
			  Add( Keys.DoneBatches, longStat( doneBatches ) );
			  Add( Keys.TotalProcessingTime, longStat( totalProcessingTime ) );
			  Add( Keys.UpstreamIdleTime, longStat( upstreamIdleTime ) );
			  Add( Keys.DownstreamIdleTime, longStat( downstreamIdleTime ) );
			  Add( Keys.AvgProcessingTime, longStat( average ) );
		 }
	}

}