﻿/*
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
namespace Neo4Net.Ext.Udc.impl
{

	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using UsageData = Neo4Net.Udc.UsageData;

	/// <summary>
	/// Kernel extension for UDC, the Usage Data Collector. The UDC runs as a background
	/// daemon, waking up once a day to collect basic usage information about a long
	/// running graph database.
	/// <para>
	/// The first update is delayed to avoid needless activity during integration
	/// testing and short-run applications. Subsequent updates are made at regular
	/// intervals. Both times are specified in milliseconds.
	/// </para>
	/// </summary>
	public class UdcKernelExtension : LifecycleAdapter
	{
		 private Timer _timer;
		 private readonly UsageData _usageData;
		 private readonly Config _config;
		 private readonly DataSourceManager _dataSourceManager;

		 internal UdcKernelExtension( Config config, DataSourceManager dataSourceManager, UsageData usageData, Timer timer )
		 {
			  this._config = config;
			  this._dataSourceManager = dataSourceManager;
			  this._usageData = usageData;
			  this._timer = timer;
		 }

		 public override void Start()
		 {
			  if ( !_config.get( UdcSettings.udc_enabled ) )
			  {
					return;
			  }

			  int firstDelay = _config.get( UdcSettings.first_delay );
			  int interval = _config.get( UdcSettings.interval );
			  HostnamePort hostAddress = _config.get( UdcSettings.udc_host );

			  UdcInformationCollector collector = new DefaultUdcInformationCollector( _config, _dataSourceManager, _usageData );
			  UdcTimerTask task = new UdcTimerTask( hostAddress, collector );

			  _timer.scheduleAtFixedRate( task, firstDelay, interval );
		 }

		 public override void Stop()
		 {
			  if ( _timer != null )
			  {
					_timer.cancel();
					_timer = null;
			  }
		 }

	}

}