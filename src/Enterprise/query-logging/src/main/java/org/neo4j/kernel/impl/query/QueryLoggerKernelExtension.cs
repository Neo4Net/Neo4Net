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
namespace Neo4Net.Kernel.impl.query
{
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class QueryLoggerKernelExtension extends org.neo4j.kernel.extension.KernelExtensionFactory<QueryLoggerKernelExtension.Dependencies>
	public class QueryLoggerKernelExtension : KernelExtensionFactory<QueryLoggerKernelExtension.Dependencies>
	{
		 public interface Dependencies
		 {
			  FileSystemAbstraction FileSystem();

			  Config Config();

			  Monitors Monitoring();

			  LogService Logger();

			  JobScheduler JobScheduler();
		 }

		 public QueryLoggerKernelExtension() : base(ExtensionType.DATABASE, "query-logging")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.neo4j.kernel.lifecycle.Lifecycle newInstance(@SuppressWarnings("unused") org.neo4j.kernel.impl.spi.KernelContext context, final Dependencies dependencies)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  FileSystemAbstraction fileSystem = dependencies.FileSystem();
			  Config config = dependencies.Config();
			  Monitors monitoring = dependencies.Monitoring();
			  LogService logService = dependencies.Logger();
			  JobScheduler jobScheduler = dependencies.JobScheduler();

			  return new LifecycleAdapterAnonymousInnerClass( this, fileSystem, config, monitoring, logService, jobScheduler );
		 }

		 private class LifecycleAdapterAnonymousInnerClass : LifecycleAdapter
		 {
			 private readonly QueryLoggerKernelExtension _outerInstance;

			 private FileSystemAbstraction _fileSystem;
			 private Config _config;
			 private Monitors _monitoring;
			 private LogService _logService;
			 private JobScheduler _jobScheduler;

			 public LifecycleAdapterAnonymousInnerClass( QueryLoggerKernelExtension outerInstance, FileSystemAbstraction fileSystem, Config config, Monitors monitoring, LogService logService, JobScheduler jobScheduler )
			 {
				 this.outerInstance = outerInstance;
				 this._fileSystem = fileSystem;
				 this._config = config;
				 this._monitoring = monitoring;
				 this._logService = logService;
				 this._jobScheduler = jobScheduler;
			 }

			 internal DynamicLoggingQueryExecutionMonitor logger;

			 public override void init()
			 {
				  Log debugLog = _logService.getInternalLog( typeof( DynamicLoggingQueryExecutionMonitor ) );
				  this.logger = new DynamicLoggingQueryExecutionMonitor( _config, _fileSystem, _jobScheduler, debugLog );
				  this.logger.init();
				  _monitoring.addMonitorListener( this.logger );
			 }

			 public override void shutdown()
			 {
				  logger.shutdown();
			 }
		 }
	}

}