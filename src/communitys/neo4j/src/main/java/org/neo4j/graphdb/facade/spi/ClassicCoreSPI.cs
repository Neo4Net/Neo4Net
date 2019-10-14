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
namespace Neo4Net.Graphdb.facade.spi
{

	using KernelEventHandler = Neo4Net.Graphdb.@event.KernelEventHandler;
	using Neo4Net.Graphdb.@event;
	using DataSourceModule = Neo4Net.Graphdb.factory.module.DataSourceModule;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using CoreAPIAvailabilityGuard = Neo4Net.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using Logger = Neo4Net.Logging.Logger;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

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
					throw new Neo4Net.Graphdb.TransactionFailureException( e.Message, e );
			  }
		 }
	}

}