using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.runtime
{

	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	public interface TransactionStateMachineSPI
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void awaitUpToDate(long oldestAcceptableTxId) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 void AwaitUpToDate( long oldestAcceptableTxId );

		 long NewestEncounteredTxId();

		 KernelTransaction BeginTransaction( LoginContext loginContext, Duration txTimeout, IDictionary<string, object> txMetaData );

		 void BindTransactionToCurrentThread( KernelTransaction tx );

		 void UnbindTransactionFromCurrentThread();

		 bool IsPeriodicCommit( string query );

		 BoltResultHandle ExecuteQuery( LoginContext loginContext, string statement, MapValue @params, Duration txTimeout, IDictionary<string, object> txMetaData );
	}

}