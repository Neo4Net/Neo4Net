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
namespace Org.Neo4j.Kernel.ha
{
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HighAvailabilityMemberStateMachine = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using InvalidEpochExceptionHandler = Org.Neo4j.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

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
		 private readonly JobScheduler _jobScheduler;
		 private readonly DependencyResolver _dependencyResolver;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly HighAvailabilityMemberStateMachine _memberStateMachine;
		 private readonly Monitors _monitors;
		 private readonly string _activeDatabaseName;

		 public PullerFactory( RequestContextFactory requestContextFactory, Master master, LastUpdateTime lastUpdateTime, LogProvider logging, InstanceId serverId, InvalidEpochExceptionHandler invalidEpochHandler, long pullInterval, JobScheduler jobScheduler, DependencyResolver dependencyResolver, AvailabilityGuard availabilityGuard, HighAvailabilityMemberStateMachine memberStateMachine, Monitors monitors, Config config )
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