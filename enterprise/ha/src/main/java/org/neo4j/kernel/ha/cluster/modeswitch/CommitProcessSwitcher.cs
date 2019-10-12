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
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.ha;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using TransactionPropagator = Org.Neo4j.Kernel.ha.transaction.TransactionPropagator;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using IntegrityValidator = Org.Neo4j.Kernel.impl.transaction.state.IntegrityValidator;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

	public class CommitProcessSwitcher : AbstractComponentSwitcher<TransactionCommitProcess>
	{
		 private readonly TransactionPropagator _txPropagator;
		 private readonly Master _master;
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly DependencyResolver _dependencyResolver;
		 private readonly MasterTransactionCommitProcess.Monitor _monitor;
		 private readonly string _activeDatabaseName;

		 public CommitProcessSwitcher( TransactionPropagator txPropagator, Master master, DelegateInvocationHandler<TransactionCommitProcess> @delegate, RequestContextFactory requestContextFactory, Monitors monitors, DependencyResolver dependencyResolver, Config config ) : base( @delegate )
		 {
			  this._txPropagator = txPropagator;
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
			  this._dependencyResolver = dependencyResolver;
			  this._monitor = monitors.NewMonitor( typeof( MasterTransactionCommitProcess.Monitor ) );
			  this._activeDatabaseName = config.Get( GraphDatabaseSettings.active_database );
		 }

		 protected internal override TransactionCommitProcess SlaveImpl
		 {
			 get
			 {
				  return new SlaveTransactionCommitProcess( _master, _requestContextFactory );
			 }
		 }

		 protected internal override TransactionCommitProcess MasterImpl
		 {
			 get
			 {
				  GraphDatabaseFacade databaseFacade = this._dependencyResolver.resolveDependency( typeof( DatabaseManager ) ).getDatabaseFacade( _activeDatabaseName ).get();
				  DependencyResolver databaseResolver = databaseFacade.DependencyResolver;
				  TransactionCommitProcess commitProcess = new TransactionRepresentationCommitProcess( databaseResolver.ResolveDependency( typeof( TransactionAppender ) ), databaseResolver.ResolveDependency( typeof( StorageEngine ) ) );
   
				  IntegrityValidator validator = databaseResolver.ResolveDependency( typeof( IntegrityValidator ) );
				  return new MasterTransactionCommitProcess( commitProcess, _txPropagator, validator, _monitor );
			 }
		 }
	}

}