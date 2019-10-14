using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.runtime
{

	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using Neo4Net.Functions;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public interface StatementProcessor
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginTransaction(org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 void BeginTransaction( Bookmark bookmark );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginTransaction(org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 void BeginTransaction( Bookmark bookmark, Duration txTimeout, IDictionary<string, object> txMetadata );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StatementMetadata run(String statement, org.neo4j.values.virtual.MapValue params) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 StatementMetadata Run( string statement, MapValue @params );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StatementMetadata run(String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetaData) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 StatementMetadata Run( string statement, MapValue @params, Bookmark bookmark, Duration txTimeout, IDictionary<string, object> txMetaData );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.bolt.v1.runtime.bookmarking.Bookmark streamResult(org.neo4j.function.ThrowingConsumer<BoltResult,Exception> resultConsumer) throws Exception;
		 Bookmark StreamResult( ThrowingConsumer<BoltResult, Exception> resultConsumer );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.bolt.v1.runtime.bookmarking.Bookmark commitTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 Bookmark CommitTransaction();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void rollbackTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 void RollbackTransaction();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void reset() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 void Reset();

		 void MarkCurrentTransactionForTermination();

		 bool HasTransaction();

		 bool HasOpenStatement();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void validateTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 void ValidateTransaction();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 StatementProcessor EMPTY = new StatementProcessor()
	//	 {
	//		  @@Override public void beginTransaction(Bookmark bookmark) throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to run statements");
	//		  }
	//
	//		  @@Override public void beginTransaction(Bookmark bookmark, Duration txTimeout, Map<String,Object> txMetadata) throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to begin a transaction");
	//		  }
	//
	//		  @@Override public StatementMetadata run(String statement, MapValue @params) throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to run statements");
	//		  }
	//
	//		  @@Override public StatementMetadata run(String statement, MapValue @params, Bookmark bookmark, Duration txTimeout, Map<String,Object> txMetaData) throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to run statements");
	//		  }
	//
	//		  @@Override public Bookmark streamResult(ThrowingConsumer<BoltResult,Exception> resultConsumer) throws Exception
	//		  {
	//				throw new UnsupportedOperationException("Unable to stream results");
	//		  }
	//
	//		  @@Override public Bookmark commitTransaction() throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to commit a transaction");
	//		  }
	//
	//		  @@Override public void rollbackTransaction() throws KernelException
	//		  {
	//				throw new UnsupportedOperationException("Unable to rollback a transaction");
	//		  }
	//
	//		  @@Override public void reset() throws TransactionFailureException
	//		  {
	//		  }
	//
	//		  @@Override public void markCurrentTransactionForTermination()
	//		  {
	//		  }
	//
	//		  @@Override public boolean hasTransaction()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean hasOpenStatement()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void validateTransaction() throws KernelException
	//		  {
	//		  }
	//	 };
	}

}