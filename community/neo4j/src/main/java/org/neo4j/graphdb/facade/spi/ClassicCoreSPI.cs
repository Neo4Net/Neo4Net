/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Graphdb.facade.spi
{

	using KernelEventHandler = Org.Neo4j.Graphdb.@event.KernelEventHandler;
	using Org.Neo4j.Graphdb.@event;
	using DataSourceModule = Org.Neo4j.Graphdb.factory.module.DataSourceModule;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using URLAccessValidationError = Org.Neo4j.Graphdb.security.URLAccessValidationError;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using CoreAPIAvailabilityGuard = Org.Neo4j.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using Logger = Org.Neo4j.Logging.Logger;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	/// <summary>
	/// This implements the backend for the "classic" Core API - meaning the surface-layer-of-the-database, thread bound API.
	/// It's a thin veneer to wire the various components the kernel and related utilities expose in a way that
	/// <seealso cref="GraphDatabaseFacade"/> likes. </summary>
	/// <seealso cref= org.neo4j.kernel.impl.factory.GraphDatabaseFacade.SPI </seealso>
	public class ClassicCoreSPI : GraphDatabaseFacade.SPI
	{
		 private readonly PlatformModule _platform;
		 private readonly DataSourceModule _dataSource;
		 private readonly Logger _msgLog;
		 private readonly CoreAPIAvailabilityGuard _availability;
		 private readonly ThreadToStatementContextBridge _threadToTransactionBridge;

		 public ClassicCoreSPI( PlatformModule platform, DataSourceModule dataSource, Logger msgLog, CoreAPIAvailabilityGuard availability, ThreadToStatementContextBridge threadToTransactionBridge )
		 {
			  this._platform = platform;
			  this._dataSource = dataSource;
			  this._msgLog = msgLog;
			  this._availability = availability;
			  this._threadToTransactionBridge = threadToTransactionBridge;
		 }

		 public override bool DatabaseIsAvailable( long timeout )
		 {
			  return _dataSource.neoStoreDataSource.DatabaseAvailabilityGuard.isAvailable( timeout );
		 }

		 public override Result ExecuteQuery( string query, MapValue parameters, TransactionalContext transactionalContext )
		 {
			  try
			  {
					_availability.assertDatabaseAvailable();
					return _dataSource.neoStoreDataSource.ExecutionEngine.executeQuery( query, parameters, transactionalContext );
			  }
			  catch ( QueryExecutionKernelException e )
			  {
					throw e.AsUserException();
			  }
		 }

		 public override AutoIndexing AutoIndexing()
		 {
			  return _dataSource.neoStoreDataSource.AutoIndexing;
		 }

		 public override DependencyResolver Resolver()
		 {
			  return _dataSource.neoStoreDataSource.DependencyResolver;
		 }

		 public override void RegisterKernelEventHandler( KernelEventHandler handler )
		 {
			  _platform.eventHandlers.registerKernelEventHandler( handler );
		 }

		 public override void UnregisterKernelEventHandler( KernelEventHandler handler )
		 {
			  _platform.eventHandlers.unregisterKernelEventHandler( handler );
		 }

		 public override void RegisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  _dataSource.neoStoreDataSource.TransactionEventHandlers.registerTransactionEventHandler( handler );
		 }

		 public override void UnregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  _dataSource.neoStoreDataSource.TransactionEventHandlers.unregisterTransactionEventHandler( handler );
		 }

		 public override StoreId StoreId()
		 {
			  return _dataSource.storeId.get();
		 }

		 public override DatabaseLayout DatabaseLayout()
		 {
			  return _dataSource.neoStoreDataSource.DatabaseLayout;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validateURLAccess(java.net.URL url) throws org.neo4j.graphdb.security.URLAccessValidationError
		 public override URL ValidateURLAccess( URL url )
		 {
			  return _platform.urlAccessRule.validate( _platform.config, url );
		 }

		 public override GraphDatabaseQueryService QueryService()
		 {
			  return Resolver().resolveDependency(typeof(GraphDatabaseQueryService));
		 }

		 public override Kernel Kernel()
		 {
			  return Resolver().resolveDependency(typeof(Kernel));
		 }

		 public override string Name()
		 {
			  return _platform.databaseInfo.ToString();
		 }

		 public override void Shutdown()
		 {
			  try
			  {
					_msgLog.log( "Shutdown started" );
					_dataSource.neoStoreDataSource.DatabaseAvailabilityGuard.shutdown();
					_platform.life.shutdown();
			  }
			  catch ( LifecycleException throwable )
			  {
					_msgLog.log( "Shutdown failed", throwable );
					throw throwable;
			  }
		 }

		 public override KernelTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout )
		 {
			  try
			  {
					_availability.assertDatabaseAvailable();
					KernelTransaction kernelTx = _dataSource.kernelAPI.get().beginTransaction(type, loginContext, timeout);
					kernelTx.RegisterCloseListener( txId => _threadToTransactionBridge.unbindTransactionFromCurrentThread() );
					_threadToTransactionBridge.bindTransactionToCurrentThread( kernelTx );
					return kernelTx;
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Org.Neo4j.Graphdb.TransactionFailureException( e.Message, e );
			  }
		 }
	}

}