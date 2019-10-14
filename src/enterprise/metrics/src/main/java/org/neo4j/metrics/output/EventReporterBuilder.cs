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
namespace Neo4Net.metrics.output
{
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Log = Neo4Net.Logging.Log;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.graphiteEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.graphiteInterval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.graphiteServer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.prometheusEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.prometheusEndpoint;

	public class EventReporterBuilder
	{
		 private readonly Config _config;
		 private readonly MetricRegistry _registry;
		 private readonly Log _logger;
		 private readonly KernelContext _kernelContext;
		 private readonly LifeSupport _life;
		 private readonly ConnectorPortRegister _portRegister;
		 private FileSystemAbstraction _fileSystem;
		 private JobScheduler _scheduler;

		 public EventReporterBuilder( Config config, MetricRegistry registry, Log logger, KernelContext kernelContext, LifeSupport life, FileSystemAbstraction fileSystem, JobScheduler scheduler, ConnectorPortRegister portRegister )
		 {
			  this._config = config;
			  this._registry = registry;
			  this._logger = logger;
			  this._kernelContext = kernelContext;
			  this._life = life;
			  this._fileSystem = fileSystem;
			  this._scheduler = scheduler;
			  this._portRegister = portRegister;
		 }

		 public virtual CompositeEventReporter Build()
		 {
			  CompositeEventReporter reporter = new CompositeEventReporter();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String prefix = createMetricsPrefix(config);
			  string prefix = CreateMetricsPrefix( _config );
			  if ( _config.get( csvEnabled ) )
			  {
					CsvOutput csvOutput = new CsvOutput( _config, _registry, _logger, _kernelContext, _fileSystem, _scheduler );
					reporter.Add( csvOutput );
					_life.add( csvOutput );
			  }

			  if ( _config.get( graphiteEnabled ) )
			  {
					HostnamePort server = _config.get( graphiteServer );
					long period = _config.get( graphiteInterval ).toMillis();
					GraphiteOutput graphiteOutput = new GraphiteOutput( server, period, _registry, _logger, prefix );
					reporter.Add( graphiteOutput );
					_life.add( graphiteOutput );
			  }

			  if ( _config.get( prometheusEnabled ) )
			  {
					HostnamePort server = _config.get( prometheusEndpoint );
					PrometheusOutput prometheusOutput = new PrometheusOutput( server, _registry, _logger, _portRegister );
					reporter.Add( prometheusOutput );
					_life.add( prometheusOutput );
			  }

			  return reporter;
		 }

		 private string CreateMetricsPrefix( Config config )
		 {
			  string prefix = config.Get( MetricsSettings.metricsPrefix );

			  if ( prefix.Equals( MetricsSettings.metricsPrefix.DefaultValue ) )
			  {
					// If default name and in HA, try to figure out a nicer name
					if ( config.IsConfigured( ClusterSettings.server_id ) )
					{
						 prefix += "." + config.Get( ClusterSettings.cluster_name );
						 prefix += "." + config.Get( ClusterSettings.server_id );
					}
			  }
			  return prefix;
		 }
	}

}