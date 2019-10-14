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
namespace Neo4Net.Kernel.ha.@lock
{
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class SlaveLockManager : Locks
	{
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly Locks _local;
		 private readonly Master _master;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly LogProvider _logProvider;

		 public SlaveLockManager( Locks localLocks, RequestContextFactory requestContextFactory, Master master, AvailabilityGuard availabilityGuard, LogProvider logProvider, Config config )
		 {
			  this._requestContextFactory = requestContextFactory;
			  this._availabilityGuard = availabilityGuard;
			  this._local = localLocks;
			  this._master = master;
			  this._logProvider = logProvider;
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client NewClient()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client client = _local.newClient();
			  return new SlaveLocksClient( _master, client, _local, _requestContextFactory, _availabilityGuard, _logProvider );
		 }

		 public override void Accept( Neo4Net.Kernel.impl.locking.Locks_Visitor visitor )
		 {
			  _local.accept( visitor );
		 }

		 public override void Close()
		 {
			  _local.close();
		 }
	}

}