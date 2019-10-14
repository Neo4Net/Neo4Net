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
namespace Neo4Net.Internal.Collector
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Neo4Net.Values;

	public class DataCollector : AutoCloseable
	{
		 internal readonly Kernel Kernel;
		 internal readonly JobScheduler JobScheduler;
		 internal readonly Neo4Net.Values.ValueMapper_JavaMapper ValueMapper;
		 internal readonly QueryCollector QueryCollector;

		 internal DataCollector( Kernel kernel, JobScheduler jobScheduler, Monitors monitors, Neo4Net.Values.ValueMapper_JavaMapper valueMapper, Config config )
		 {
			  this.Kernel = kernel;
			  this.JobScheduler = jobScheduler;
			  this.ValueMapper = valueMapper;
			  this.QueryCollector = new QueryCollector( jobScheduler, config.Get( GraphDatabaseSettings.data_collector_max_recent_query_count ), config.Get( GraphDatabaseSettings.data_collector_max_query_text_size ) );
			  try
			  {
					this.QueryCollector.collect( Collections.emptyMap() );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					throw new System.InvalidOperationException( "An empty config cannot be invalid", e );
			  }
			  monitors.AddMonitorListener( QueryCollector );
		 }

		 public override void Close()
		 {
			  // intended to eventually be used to stop any ongoing collection
		 }
	}

}