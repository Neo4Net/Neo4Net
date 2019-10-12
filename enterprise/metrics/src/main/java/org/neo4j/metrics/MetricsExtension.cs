/*
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
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using CompositeEventReporter = Org.Neo4j.metrics.output.CompositeEventReporter;
	using EventReporterBuilder = Org.Neo4j.metrics.output.EventReporterBuilder;
	using Neo4jMetricsBuilder = Org.Neo4j.metrics.source.Neo4jMetricsBuilder;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class MetricsExtension : Lifecycle
	{
		 private readonly LifeSupport _life = new LifeSupport();
		 private Log _logger;
		 private CompositeEventReporter _reporter;
		 private bool _metricsBuilt;

		 internal MetricsExtension( KernelContext kernelContext, MetricsKernelExtensionFactory.Dependencies dependencies )
		 {
			  LogService logService = dependencies.LogService();
			  Config configuration = dependencies.Configuration();
			  FileSystemAbstraction fileSystem = dependencies.FileSystemAbstraction();
			  JobScheduler scheduler = dependencies.Scheduler();
			  _logger = logService.GetUserLog( this.GetType() );

			  MetricRegistry registry = new MetricRegistry();
			  _reporter = ( new EventReporterBuilder( configuration, registry, _logger, kernelContext, _life, fileSystem, scheduler, dependencies.PortRegister() ) ).build();
			  _metricsBuilt = ( new Neo4jMetricsBuilder( registry, _reporter, configuration, logService, kernelContext, dependencies, _life ) ).build();
		 }

		 public override void Init()
		 {
			  _logger.info( "Initiating metrics..." );
			  if ( _metricsBuilt && _reporter.Empty )
			  {
					_logger.warn( "Several metrics were enabled but no exporting option was configured to report values to. " + "Disabling kernel metrics extension." );
					_life.clear();
			  }

			  if ( !_reporter.Empty && !_metricsBuilt )
			  {
					_logger.warn( "Exporting tool have been configured to report values to but no metrics were enabled. " + "Disabling kernel metrics extension." );
					_life.clear();
			  }

			  _life.init();
		 }

		 public override void Start()
		 {
			  _life.start();
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _life.shutdown();
		 }
	}

}