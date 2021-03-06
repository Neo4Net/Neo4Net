﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.metrics
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using Org.Neo4j.Kernel.extension;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using Neo4jMetricsBuilder = Org.Neo4j.metrics.source.Neo4jMetricsBuilder;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

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