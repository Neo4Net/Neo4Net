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
namespace Neo4Net.Graphdb.facade.spi
{

	using Neo4Net.Function;
	using KernelEventHandler = Neo4Net.Graphdb.@event.KernelEventHandler;
	using Neo4Net.Graphdb.@event;
	using DataSourceModule = Neo4Net.Graphdb.factory.module.DataSourceModule;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using CoreAPIAvailabilityGuard = Neo4Net.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public class ProcedureGDBFacadeSPI : GraphDatabaseFacade.SPI
	{
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly DataSourceModule _sourceModule;
		 private readonly DependencyResolver _resolver;
		 private readonly CoreAPIAvailabilityGuard _availability;
		 private readonly ThrowingFunction<URL, URL, URLAccessValidationError> _urlValidator;
		 private readonly SecurityContext _securityContext;
		 private readonly ThreadToStatementContextBridge _threadToTransactionBridge;

		 public ProcedureGDBFacadeSPI( DataSourceModule sourceModule, DependencyResolver resolver, CoreAPIAvailabilityGuard availability, ThrowingFunction<URL, URL, URLAccessValidationError> urlValidator, SecurityContext securityContext, ThreadToStatementContextBridge threadToTransactionBridge )
		 {
			  this._databaseLayout = sourceModule.NeoStoreDataSource.DatabaseLayout;
			  this._sourceModule = sourceModule;
			  this._resolver = resolver;
			  this._availability = availability;
			  this._urlValidator = urlValidator;
			  this._securityContext = securityContext;
			  this._threadToTransactionBridge = threadToTransactionBridge;
		 }

		 public override bool DatabaseIsAvailable( long timeout )
		 {
			  return _availability.isAvailable( timeout );
		 }

		 public override DependencyResolver Resolver()
		 {
			  return _resolver;
		 }

		 public override StoreId StoreId()
		 {
			  return _sourceModule.storeId.get();
		 }

		 public override DatabaseLayout DatabaseLayout()
		 {
			  return _databaseLayout;
		 }

		 public override string Name()
		 {
			  return "ProcedureGraphDatabaseService";
		 }

		 public override Result ExecuteQuery( string query, MapValue parameters, TransactionalContext tc )
		 {
			  try
			  {
					_availability.assertDatabaseAvailable();
					return _sourceModule.neoStoreDataSource.ExecutionEngine.executeQuery( query, parameters, tc );
			  }
			  catch ( QueryExecutionKernelException e )
			  {
					throw e.AsUserException();
			  }
		 }

		 public override AutoIndexing AutoIndexing()
		 {
			  return _sourceModule.neoStoreDataSource.AutoIndexing;
		 }

		 public override void RegisterKernelEventHandler( KernelEventHandler handler )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void UnregisterKernelEventHandler( KernelEventHandler handler )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void RegisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void UnregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validateURLAccess(java.net.URL url) throws org.neo4j.graphdb.security.URLAccessValidationError
		 public override URL ValidateURLAccess( URL url )
		 {
			  return _urlValidator.apply( url );
		 }

		 public override GraphDatabaseQueryService QueryService()
		 {
			  return _resolver.resolveDependency( typeof( GraphDatabaseQueryService ) );
		 }

		 public override Kernel Kernel()
		 {
			  return _resolver.resolveDependency( typeof( Kernel ) );
		 }

		 public override void Shutdown()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override KernelTransaction BeginTransaction( KernelTransaction.Type type, LoginContext ignored, long timeout )
		 {
			  try
			  {
					_availability.assertDatabaseAvailable();
					KernelTransaction kernelTx = _sourceModule.kernelAPI.get().beginTransaction(type, this._securityContext, timeout);
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