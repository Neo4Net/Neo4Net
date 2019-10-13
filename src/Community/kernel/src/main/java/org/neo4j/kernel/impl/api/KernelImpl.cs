﻿/*
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
namespace Neo4Net.Kernel.Impl.Api
{
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using InwardKernel = Neo4Net.Kernel.api.InwardKernel;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Neo4Net.Kernel.api;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.transaction_timeout;

	/// <summary>
	/// This is the Neo4j Kernel, an implementation of the Kernel API which is an internal component used by Cypher and the
	/// Core API (the API under org.neo4j.graphdb).
	/// 
	/// <h1>Structure</h1>
	/// 
	/// The Kernel lets you start transactions. The transactions allow you to create "statements", which, in turn, operate
	/// against the database. Statements and transactions are separate concepts due to isolation requirements. A single
	/// cypher query will normally use one statement, and there can be multiple statements executed in one transaction.
	/// 
	/// Please refer to the <seealso cref="KernelTransaction"/> javadoc for details.
	/// 
	/// </summary>
	public class KernelImpl : LifecycleAdapter, InwardKernel
	{
		 private readonly KernelTransactions _transactions;
		 private readonly TransactionHooks _hooks;
		 private readonly DatabaseHealth _health;
		 private readonly TransactionMonitor _transactionMonitor;
		 private readonly Procedures _procedures;
		 private readonly Config _config;
		 private volatile bool _isRunning;

		 public KernelImpl( KernelTransactions transactionFactory, TransactionHooks hooks, DatabaseHealth health, TransactionMonitor transactionMonitor, Procedures procedures, Config config )
		 {
			  this._transactions = transactionFactory;
			  this._hooks = hooks;
			  this._health = health;
			  this._transactionMonitor = transactionMonitor;
			  this._procedures = procedures;
			  this._config = config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.KernelTransaction beginTransaction(org.neo4j.internal.kernel.api.Transaction_Type type, org.neo4j.internal.kernel.api.security.LoginContext loginContext) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override KernelTransaction BeginTransaction( Neo4Net.@internal.Kernel.Api.Transaction_Type type, LoginContext loginContext )
		 {
			  if ( !_isRunning )
			  {
					throw new System.InvalidOperationException( "Kernel is not running, so it is not possible to use it" );
			  }
			  return BeginTransaction( type, loginContext, _config.get( transaction_timeout ).toMillis() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.KernelTransaction beginTransaction(org.neo4j.internal.kernel.api.Transaction_Type type, org.neo4j.internal.kernel.api.security.LoginContext loginContext, long timeout) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override KernelTransaction BeginTransaction( Neo4Net.@internal.Kernel.Api.Transaction_Type type, LoginContext loginContext, long timeout )
		 {
			  _health.assertHealthy( typeof( TransactionFailureException ) );
			  KernelTransaction transaction = _transactions.newInstance( type, loginContext, timeout );
			  _transactionMonitor.transactionStarted();
			  return transaction;
		 }

		 public override void RegisterTransactionHook( TransactionHook hook )
		 {
			  _hooks.register( hook );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerProcedure(org.neo4j.kernel.api.proc.CallableProcedure procedure) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override void RegisterProcedure( CallableProcedure procedure )
		 {
			  _procedures.register( procedure );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerUserFunction(org.neo4j.kernel.api.proc.CallableUserFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override void RegisterUserFunction( CallableUserFunction function )
		 {
			  _procedures.register( function );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerUserAggregationFunction(org.neo4j.kernel.api.proc.CallableUserAggregationFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override void RegisterUserAggregationFunction( CallableUserAggregationFunction function )
		 {
			  _procedures.register( function );
		 }

		 public override void Start()
		 {
			  _isRunning = true;
		 }

		 public override void Stop()
		 {
			  if ( !_isRunning )
			  {
					throw new System.InvalidOperationException( "kernel is not running, so it is not possible to stop it" );
			  }
			  _isRunning = false;
		 }
	}

}