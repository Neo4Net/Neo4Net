/*
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
namespace Org.Neo4j.Kernel.ha.cluster.modeswitch
{
	using Org.Neo4j.Function;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.ha;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using SlaveLockManager = Org.Neo4j.Kernel.ha.@lock.SlaveLockManager;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using ReadOnlyLocks = Org.Neo4j.Kernel.impl.locking.ReadOnlyLocks;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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