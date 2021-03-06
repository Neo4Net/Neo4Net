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
namespace Org.Neo4j.Kernel.ha.com.master
{

	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

	public class DefaultSlaveFactory : SlaveFactory
	{
		 private readonly LogProvider _logProvider;
		 private readonly Monitors _monitors;
		 private readonly int _chunkSize;
		 private StoreId _storeId;
		 private readonly System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> _entryReader;

		 public DefaultSlaveFactory( LogProvider logProvider, Monitors monitors, int chunkSize, System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> logEntryReader )
		 {
			  this._logProvider = logProvider;
			  this._monitors = monitors;
			  this._chunkSize = chunkSize;
			  this._entryReader = logEntryReader;
		 }

		 public override Slave NewSlave( LifeSupport life, ClusterMember clusterMember, string originHostNameOrIp, int originPort )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return life.Add( new SlaveClient( clusterMember.InstanceId, clusterMember.HAUri.Host, clusterMember.HAUri.Port, originHostNameOrIp, _logProvider, _storeId, 2, _chunkSize, _monitors.newMonitor( typeof( ByteCounterMonitor ), typeof( SlaveClient ).FullName ), _monitors.newMonitor( typeof( RequestMonitor ), typeof( SlaveClient ).FullName ), _entryReader.get() ) );
		 }

		 public virtual StoreId StoreId
		 {
			 set
			 {
				  this._storeId = value;
			 }
		 }
	}

}