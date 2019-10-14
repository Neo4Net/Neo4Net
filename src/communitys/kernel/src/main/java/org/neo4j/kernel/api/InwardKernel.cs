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
namespace Neo4Net.Kernel.api
{
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;

	/// <summary>
	/// The main API through which access to the Neo4j kernel is made, both read
	/// and write operations are supported as well as creating transactions.
	/// 
	/// Changes to the graph (i.e. write operations) are performed via a
	/// <seealso cref="beginTransaction(Transaction.Type, LoginContext)"/>  transaction context} where changes done
	/// inside the transaction are visible in read operations for <seealso cref="Statement statements"/>
	/// executed within that transaction context.
	/// </summary>
	public interface InwardKernel : Kernel
	{
		 /// <summary>
		 /// Creates and returns a new <seealso cref="KernelTransaction"/> capable of modifying the
		 /// underlying graph with custom timeout in milliseconds.
		 /// </summary>
		 /// <param name="type"> the type of the new transaction: implicit (internally created) or explicit (created by the user) </param>
		 /// <param name="loginContext"> transaction login context </param>
		 /// <param name="timeout"> transaction timeout in milliseconds </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KernelTransaction beginTransaction(KernelTransaction.Type type, org.neo4j.internal.kernel.api.security.LoginContext loginContext, long timeout) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 KernelTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout );

		 /// <summary>
		 /// Registers a <seealso cref="TransactionHook"/> that will receive notifications about committing transactions
		 /// and the changes they commit. </summary>
		 /// <param name="hook"> <seealso cref="TransactionHook"/> for receiving notifications about transactions to commit. </param>
		 void RegisterTransactionHook( TransactionHook hook );

		 /// <summary>
		 /// Register a procedure that should be available from this kernel. This is not a transactional method, the procedure is not
		 /// durably stored, and is not propagated in a cluster.
		 /// </summary>
		 /// <param name="procedure"> procedure to register </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void registerProcedure(org.neo4j.kernel.api.proc.CallableProcedure procedure) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 void RegisterProcedure( CallableProcedure procedure );

		 /// <summary>
		 /// Register a function that should be available from this kernel. This is not a transactional method, the function is not
		 /// durably stored, and is not propagated in a cluster.
		 /// </summary>
		 /// <param name="function"> function to register </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void registerUserFunction(org.neo4j.kernel.api.proc.CallableUserFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 void RegisterUserFunction( CallableUserFunction function );

		 /// <summary>
		 /// Register an aggregation function that should be available from this kernel. This is not a transactional method, the function is not
		 /// durably stored, and is not propagated in a cluster.
		 /// </summary>
		 /// <param name="function"> function to register </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void registerUserAggregationFunction(org.neo4j.kernel.api.proc.CallableUserAggregationFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 void RegisterUserAggregationFunction( CallableUserAggregationFunction function );
	}

}