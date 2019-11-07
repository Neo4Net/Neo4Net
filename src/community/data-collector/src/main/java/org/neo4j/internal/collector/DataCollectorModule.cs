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
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Preconditions = Neo4Net.Utils.Preconditions;
	using Neo4Net.Values;

	public class DataCollectorModule
	{
		 private DataCollectorModule()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static IDisposable setupDataCollector(Neo4Net.kernel.impl.proc.Procedures procedures, Neo4Net.scheduler.JobScheduler jobScheduler, Neo4Net.Kernel.Api.Internal.Kernel kernel, Neo4Net.kernel.monitoring.Monitors monitors, Neo4Net.values.ValueMapper_JavaMapper valueMapper, Neo4Net.kernel.configuration.Config config) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public static IDisposable SetupDataCollector( Procedures procedures, IJobScheduler jobScheduler, Kernel kernel, Monitors monitors, Neo4Net.Values.ValueMapper_JavaMapper valueMapper, Config config )
		 {
			  Preconditions.checkState( kernel != null, "Kernel was null" );
			  DataCollector dataCollector = new DataCollector( kernel, jobScheduler, monitors, valueMapper, config );
			  procedures.RegisterComponent( typeof( DataCollector ), ctx => dataCollector, false );
			  procedures.RegisterProcedure( typeof( DataCollectorProcedures ) );
			  return dataCollector;
		 }
	}

}