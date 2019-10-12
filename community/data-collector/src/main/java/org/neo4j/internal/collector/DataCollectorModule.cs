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
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Preconditions = Org.Neo4j.Util.Preconditions;
	using Org.Neo4j.Values;

	public class DataCollectorModule
	{
		 private DataCollectorModule()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static AutoCloseable setupDataCollector(org.neo4j.kernel.impl.proc.Procedures procedures, org.neo4j.scheduler.JobScheduler jobScheduler, org.neo4j.internal.kernel.api.Kernel kernel, org.neo4j.kernel.monitoring.Monitors monitors, org.neo4j.values.ValueMapper_JavaMapper valueMapper, org.neo4j.kernel.configuration.Config config) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public static AutoCloseable SetupDataCollector( Procedures procedures, JobScheduler jobScheduler, Kernel kernel, Monitors monitors, Org.Neo4j.Values.ValueMapper_JavaMapper valueMapper, Config config )
		 {
			  Preconditions.checkState( kernel != null, "Kernel was null" );
			  DataCollector dataCollector = new DataCollector( kernel, jobScheduler, monitors, valueMapper, config );
			  procedures.RegisterComponent( typeof( DataCollector ), ctx => dataCollector, false );
			  procedures.RegisterProcedure( typeof( DataCollectorProcedures ) );
			  return dataCollector;
		 }
	}

}