﻿using System.Collections.Generic;

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
namespace Neo4Net.Ext.Udc.impl
{

	using HostnamePort = Neo4Net.Helpers.HostnamePort;

	public class UdcTimerTask : TimerTask
	{
		 // ABKTODO: make this thread safe
		 public static readonly IDictionary<string, int> SuccessCounts = new Dictionary<string, int>();
		 public static readonly IDictionary<string, int> FailureCounts = new Dictionary<string, int>();

		 private readonly string _storeId;
		 private readonly Pinger _pinger;

		 public UdcTimerTask( HostnamePort hostAddress, UdcInformationCollector collector )
		 {
			  this._storeId = collector.StoreId;

			  SuccessCounts[_storeId] = 0;
			  FailureCounts[_storeId] = 0;

			  _pinger = new Pinger( hostAddress, collector );
		 }

		 public override void Run()
		 {
			  try
			  {
					_pinger.ping();
					IncrementSuccessCount( _storeId );
			  }
			  catch ( IOException )
			  {
					IncrementFailureCount( _storeId );
			  }
		 }

		 private void IncrementSuccessCount( string storeId )
		 {
			  int? currentCount = SuccessCounts[storeId];
			  currentCount++;
			  SuccessCounts[storeId] = currentCount.Value;
		 }

		 private void IncrementFailureCount( string storeId )
		 {
			  int? currentCount = FailureCounts[storeId];
			  currentCount++;
			  FailureCounts[storeId] = currentCount.Value;
		 }
	}

}