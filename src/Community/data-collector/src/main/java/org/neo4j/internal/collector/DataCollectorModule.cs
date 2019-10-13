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
namespace Neo4Net.@internal.Collector
{
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Preconditions = Neo4Net.Utils.Preconditions;
	using Neo4Net.Values;

	public class DataCollectorModule
	{
		 private DataCollectorModule()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static AutoCloseable setupDataCollector(org.neo4j.kernel.impl.proc.Procedures procedures, org.neo4j.scheduler.JobScheduler jobScheduler, org.neo4j.internal.kernel.api.Kernel kernel, org.neo4j.kernel.monitoring.Monitors monitors, org.neo4j.values.ValueMapper_JavaMapper valueMapper, org.neo4j.kernel.configuration.Config config) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public static AutoCloseable SetupDataCollector( Procedures procedures, JobScheduler jobScheduler, Kernel kernel, Monitors monitors, Neo4Net.Values.ValueMapper_JavaMapper valueMapper, Config config )
		 {
			  Preconditions.checkState( kernel != null, "Kernel was null" );
			  DataCollector dataCollector = new DataCollector( kernel, jobScheduler, monitors, valueMapper, config );
			  procedures.RegisterComponent( typeof( DataCollector ), ctx => dataCollector, false );
			  procedures.RegisterProcedure( typeof( DataCollectorProcedures ) );
			  return dataCollector;
		 }
	}

}