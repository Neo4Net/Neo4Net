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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using TransactionPropagator = Neo4Net.Kernel.ha.transaction.TransactionPropagator;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;

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