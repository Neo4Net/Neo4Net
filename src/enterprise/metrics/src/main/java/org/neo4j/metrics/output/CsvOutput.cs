using System;
using System.Collections.Generic;

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
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Timer = com.codahale.metrics.Timer;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvEnabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvInterval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvPath;

	public class CsvOutput : Lifecycle, EventReporter
	{
		 private readonly Config _config;
		 private readonly MetricRegistry _registry;
		 private readonly Log _logger;
		 private readonly KernelContext _kernelContext;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly JobScheduler _scheduler;
		 private RotatableCsvReporter _csvReporter;
		 private File _outputPath;

		 internal CsvOutput( Config config, MetricRegistry registry, Log logger, KernelContext kernelContext, FileSystemAbstraction fileSystem, JobScheduler scheduler )
		 {
			  this._config = config;
			  this._registry = registry;
			  this._logger = logger;
			  this._kernelContext = kernelContext;
			  this._fileSystem = fileSystem;
			  this._scheduler = scheduler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws java.io.IOException
		 public override void Init()
		 {
			  // Setup CSV reporting
			  File configuredPath = _config.get( csvPath );
			  if ( configuredPath == null )
			  {
					throw new System.ArgumentException( csvPath.name() + " configuration is required since " + csvEnabled.name() + " is enabled" );
			  }
			  long? rotationThreshold = _config.get( MetricsSettings.csvRotationThreshold );
			  int? maxArchives = _config.get( MetricsSettings.csvMaxArchives );
			  _outputPath = AbsoluteFileOrRelativeTo( _kernelContext.directory(), configuredPath );
			  _csvReporter = RotatableCsvReporter.ForRegistry( _registry ).convertRatesTo( TimeUnit.SECONDS ).convertDurationsTo( TimeUnit.MILLISECONDS ).formatFor( Locale.US ).outputStreamSupplierFactory( GetFileRotatingFileOutputStreamSupplier( rotationThreshold, maxArchives ) ).build( EnsureDirectoryExists( _outputPath ) );
		 }

		 public override void Start()
		 {
			  _csvReporter.start( _config.get( csvInterval ).toMillis(), TimeUnit.MILLISECONDS );
			  _logger.info( "Sending metrics to CSV file at " + _outputPath );
		 }

		 public override void Stop()
		 {
			  _csvReporter.stop();
		 }

		 public override void Shutdown()
		 {
			  _csvReporter = null;
		 }

		 public override void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers )
		 {
			  _csvReporter.report( gauges, counters, histograms, meters, timers );
		 }

		 private System.Func<File, RotatingFileOutputStreamSupplier.RotationListener, RotatingFileOutputStreamSupplier> GetFileRotatingFileOutputStreamSupplier( long? rotationThreshold, int? maxArchives )
		 {
			  return ( file, listener ) =>
			  {
				try
				{
					 return new RotatingFileOutputStreamSupplier( _fileSystem, file, rotationThreshold.Value, 0, maxArchives.Value, _scheduler.executor( Group.LOG_ROTATION ), listener );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File ensureDirectoryExists(java.io.File dir) throws java.io.IOException
		 private File EnsureDirectoryExists( File dir )
		 {
			  if ( !_fileSystem.fileExists( dir ) )
			  {
					_fileSystem.mkdirs( dir );
			  }
			  if ( !_fileSystem.isDirectory( dir ) )
			  {
					throw new System.InvalidOperationException( "The given path for CSV files points to a file, but a directory is required: " + dir.AbsolutePath );
			  }
			  return dir;
		 }

		 /// <summary>
		 /// Looks at configured file {@code absoluteOrRelativeFile} and just returns it if absolute, otherwise
		 /// returns a <seealso cref="File"/> with {@code baseDirectoryIfRelative} as parent.
		 /// </summary>
		 /// <param name="baseDirectoryIfRelative"> base directory to use as parent if {@code absoluteOrRelativeFile}
		 /// is relative, otherwise unused. </param>
		 /// <param name="absoluteOrRelativeFile"> file to return as absolute or relative to {@code baseDirectoryIfRelative}. </param>
		 private static File AbsoluteFileOrRelativeTo( File baseDirectoryIfRelative, File absoluteOrRelativeFile )
		 {
			  return absoluteOrRelativeFile.Absolute ? absoluteOrRelativeFile : new File( baseDirectoryIfRelative, absoluteOrRelativeFile.Path );
		 }
	}

}