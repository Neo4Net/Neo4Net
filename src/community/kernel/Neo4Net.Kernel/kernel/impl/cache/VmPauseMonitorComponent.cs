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
namespace Neo4Net.Kernel.impl.cache
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using VmPauseMonitor = Neo4Net.Kernel.monitoring.VmPauseMonitor;
	using Log = Neo4Net.Logging.Log;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class VmPauseMonitorComponent : LifecycleAdapter
	{
		 private readonly Config _config;
		 private readonly Log _log;
		 private readonly IJobScheduler _jobScheduler;
		 private volatile VmPauseMonitor _vmPauseMonitor;

		 public VmPauseMonitorComponent( Config config, Log log, IJobScheduler jobScheduler )
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