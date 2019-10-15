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
namespace Neo4Net.Kernel.ha
{
	using InstanceId = Neo4Net.cluster.InstanceId;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HighAvailabilityMemberStateMachine = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using InvalidEpochExceptionHandler = Neo4Net.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Helper factory that provide more convenient way of construction and dependency management for update pulling
	/// related components
	/// </summary>
	public class PullerFactory
	{
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly Master _master;
		 private readonly LastUpdateTime _lastUpdateTime;
		 private readonly LogProvider _logging;
		 private readonly InstanceId _serverId;
		 private readonly InvalidEpochExceptionHandler _invalidEpochHandler;
		 private readonly long _pullInterval;
		 private readonly IJobScheduler _jobScheduler;
		 private readonly DependencyResolver _dependencyResolver;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly HighAvailabilityMemberStateMachine _memberStateMachine;
		 private readonly Monitors _monitors;
		 private readonly string _activeDatabaseName;

		 public PullerFactory( RequestContextFactory requestContextFactory, Master master, LastUpdateTime lastUpdateTime, LogProvider logging, InstanceId serverId, InvalidEpochExceptionHandler invalidEpochHandler, long pullInterval, IJobScheduler jobScheduler, DependencyResolver dependencyResolver, AvailabilityGuard availabilityGuard, HighAvailabilityMemberStateMachine memberStateMachine, Monitors monitors, Config config )
		 {

			  this._requestContextFactory = requestContextFactory;
			  this._master = master;
			  this._lastUpdateTime = lastUpdateTime;
			  this._logging = logging;
			  this._serverId = serverId;
			  this._invalidEpochHandler = invalidEpochHandler;
			  this._pullInterval = pullInterval;
			  this._jobScheduler = jobScheduler;
			  this._dependencyResolver = dependencyResolver;
			  this._availabilityGuard = availabilityGuard;
			  this._memberStateMachine = memberStateMachine;
			  this._monitors = monitors;
			  this._activeDatabaseName = config.Get( GraphDatabaseSettings.active_database );
		 }

		 public virtual SlaveUpdatePuller CreateSlaveUpdatePuller()
		 {
			  return new SlaveUpdatePuller( _requestContextFactory, _master, _lastUpdateTime, _logging, _serverId, _availabilityGuard, _invalidEpochHandler, _jobScheduler, _monitors.newMonitor( typeof( SlaveUpdatePuller.Monitor ) ) );
		 }

		 public virtual UpdatePullingTransactionObligationFulfiller CreateObligationFulfiller( UpdatePuller updatePuller )
		 {
			  return new UpdatePullingTransactionObligationFulfiller(updatePuller, _memberStateMachine, _serverId, () =>
			  {
				GraphDatabaseFacade databaseFacade = this._dependencyResolver.resolveDependency( typeof( DatabaseManager ) ).getDatabaseFacade( _activeDatabaseName ).get();
				DependencyResolver databaseResolver = databaseFacade.DependencyResolver;
				return databaseResolver.resolveDependency( typeof( TransactionIdStore ) );
			  });
		 }

		 public virtual UpdatePullerScheduler CreateUpdatePullerScheduler( UpdatePuller updatePuller )
		 {
			  return new UpdatePullerScheduler( _jobScheduler, _logging, updatePuller, _pullInterval );
		 }
	}

}