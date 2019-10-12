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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{

	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using IdMapper = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Org.Neo4j.@unsafe.Impl.Batchimport.input.Collector;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Performs <seealso cref="IdMapper.prepare(LongFunction, Collector, ProgressListener)"/>
	/// embedded in a <seealso cref="Stage"/> as to take advantage of statistics and monitoring provided by that framework.
	/// </summary>
	public class IdMapperPreparationStage : Stage
	{
		 public const string NAME = "Prepare node index";

		 public IdMapperPreparationStage( Configuration config, IdMapper idMapper, System.Func<long, object> inputIdLookup, Collector collector, StatsProvider memoryUsageStats ) : base( NAME, null, config, 0 )
		 {
			  Add( new IdMapperPreparationStep( Control(), config, idMapper, inputIdLookup, collector, memoryUsageStats ) );
		 }
	}

}