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
namespace Org.Neo4j.Kernel.impl.cache
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using VmPauseMonitor = Org.Neo4j.Kernel.monitoring.VmPauseMonitor;
	using Log = Org.Neo4j.Logging.Log;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class VmPauseMonitorComponent : LifecycleAdapter
	{
		 private readonly Config _config;
		 private readonly Log _log;
		 private readonly JobScheduler _jobScheduler;
		 private volatile VmPauseMonitor _vmPauseMonitor;

		 public VmPauseMonitorComponent( Config config, Log log, JobScheduler jobScheduler )
		 {
			  this._config = config;
			  this._log = log;
			  this._jobScheduler = jobScheduler;
		 }

		 public override void Start()
		 {
			  _vmPauseMonitor = new VmPauseMonitor( _config.get( GraphDatabaseSettings.vm_pause_monitor_measurement_duration ), _config.get( GraphDatabaseSettings.vm_pause_monitor_stall_alert_threshold ), _log, _jobScheduler, vmPauseInfo => _log.warn( "Detected VM stop-the-world pause: %s", vmPauseInfo ) );
			  _vmPauseMonitor.start();
		 }

		 public override void Stop()
		 {
			  _vmPauseMonitor.stop();
			  _vmPauseMonitor = null;
		 }

	}

}