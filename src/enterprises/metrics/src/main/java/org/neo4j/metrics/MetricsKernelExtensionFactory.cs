/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.metrics
{
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using Neo4jMetricsBuilder = Neo4Net.metrics.source.Neo4jMetricsBuilder;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class MetricsKernelExtensionFactory : KernelExtensionFactory<MetricsKernelExtensionFactory.Dependencies>
	{
		 public interface Dependencies : Neo4jMetricsBuilder.Dependencies
		 {
			  Config Configuration();

			  LogService LogService();

			  FileSystemAbstraction FileSystemAbstraction();

			  JobScheduler Scheduler();

			  ConnectorPortRegister PortRegister();
		 }

		 public MetricsKernelExtensionFactory() : base("metrics")
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  return new MetricsExtension( context, dependencies );
		 }
	}

}