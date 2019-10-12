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
namespace Org.Neo4j.@internal.Collector
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Org.Neo4j.Values;

	public class DataCollector : AutoCloseable
	{
		 internal readonly Kernel Kernel;
		 internal readonly JobScheduler JobScheduler;
		 internal readonly Org.Neo4j.Values.ValueMapper_JavaMapper ValueMapper;
		 internal readonly QueryCollector QueryCollector;

		 internal DataCollector( Kernel kernel, JobScheduler jobScheduler, Monitors monitors, Org.Neo4j.Values.ValueMapper_JavaMapper valueMapper, Config config )
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