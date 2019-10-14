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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Neo4Net.Functions;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using SlaveLockManager = Neo4Net.Kernel.ha.@lock.SlaveLockManager;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ReadOnlyLocks = Neo4Net.Kernel.impl.locking.ReadOnlyLocks;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class LockManagerSwitcher : AbstractComponentSwitcher<Locks>
	{
		 private readonly DelegateInvocationHandler<Master> _master;
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly Factory<Locks> _locksFactory;
		 private readonly LogProvider _logProvider;
		 private readonly Config _config;

		 public LockManagerSwitcher( DelegateInvocationHandler<Locks> @delegate, DelegateInvocationHandler<Master> master, RequestContextFactory requestContextFactory, AvailabilityGuard availabilityGuard, Factory<Locks> locksFactory, LogProvider logProvider, Config config ) : base( @delegate )
		 {
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
			  this._availabilityGuard = availabilityGuard;
			  this._locksFactory = locksFactory;
			  this._logProvider = logProvider;
			  this._config = config;
		 }

		 protected internal override Locks MasterImpl
		 {
			 get
			 {
				  return _locksFactory.newInstance();
			 }
		 }

		 protected internal override Locks SlaveImpl
		 {
			 get
			 {
				  return new SlaveLockManager( _locksFactory.newInstance(), _requestContextFactory, _master.cement(), _availabilityGuard, _logProvider, _config );
			 }
		 }

		 protected internal override Locks PendingImpl
		 {
			 get
			 {
				  return new ReadOnlyLocks();
			 }
		 }

		 protected internal override void ShutdownOldDelegate( Locks oldLocks )
		 {
			  if ( oldLocks != null )
			  {
					oldLocks.Close();
			  }
		 }
	}

}