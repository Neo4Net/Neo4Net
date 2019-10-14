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
namespace Neo4Net.Kernel.api
{

	using NotInTransactionException = Neo4Net.Graphdb.NotInTransactionException;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using SchemaKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;

	/// <summary>
	/// Extends the outwards-facing <seealso cref="org.neo4j.internal.kernel.api.Transaction"/> with additional functionality
	/// that is used inside the kernel (and in some other places, ahum). Please do not rely on this class unless you
	/// have to.
	/// </summary>
	public interface KernelTransaction : Transaction, AssertOpen
	{

		 /// <summary>
		 /// Acquires a new <seealso cref="Statement"/> for this transaction which allows for reading and writing data from and
		 /// to the underlying database. After the group of reads and writes have been performed the statement
		 /// must be <seealso cref="Statement.close() released"/>.
		 /// </summary>
		 /// <returns> a <seealso cref="Statement"/> with access to underlying database. </returns>
		 Statement AcquireStatement();

		 /// <summary>
		 /// Create unique index which will be used to support uniqueness constraint.
		 /// </summary>
		 /// <param name="schema"> schema to create unique index for. </param>
		 /// <param name="provider"> index provider identifier </param>
		 /// <returns> IndexDescriptor for the index to be created. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.IndexDescriptor indexUniqueCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor schema, String provider) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 IndexDescriptor IndexUniqueCreate( SchemaDescriptor schema, string provider );

		 /// <returns> the security context this transaction is currently executing in. </returns>
		 /// <exception cref="NotInTransactionException"> if the transaction is closed. </exception>
		 SecurityContext SecurityContext();

		 /// <returns> the subject executing this transaction, or <seealso cref="AuthSubject.ANONYMOUS"/> if the transaction is closed. </returns>
		 AuthSubject SubjectOrAnonymous();

		 /// <returns> The timestamp of the last transaction that was committed to the store when this transaction started. </returns>
		 long LastTransactionTimestampWhenStarted();

		 /// <returns> The id of the last transaction that was committed to the store when this transaction started. </returns>
		 long LastTransactionIdWhenStarted();

		 /// <returns> start time of this transaction, i.e. basically <seealso cref="System.currentTimeMillis()"/> when user called
		 /// <seealso cref="Kernel.beginTransaction(Type, LoginContext)"/>. </returns>
		 long StartTime();

		 /// <returns> start time of this transaction, i.e. basically <seealso cref="System.nanoTime()"/> when user called
		 /// <seealso cref="org.neo4j.internal.kernel.api.Session.beginTransaction(Type)"/>. </returns>
		 long StartTimeNanos();

		 /// <summary>
		 /// Timeout for transaction in milliseconds. </summary>
		 /// <returns> transaction timeout in milliseconds. </returns>
		 long Timeout();

		 /// <summary>
		 /// Register a <seealso cref="CloseListener"/> to be invoked after commit, but before transaction events "after" hooks
		 /// are invoked. </summary>
		 /// <param name="listener"> <seealso cref="CloseListener"/> to get these notifications. </param>
		 void RegisterCloseListener( KernelTransaction_CloseListener listener );

		 /// <summary>
		 /// Kernel transaction type
		 /// 
		 /// Implicit if created internally in the database
		 /// Explicit if created by the end user
		 /// </summary>
		 /// <returns> the transaction type: implicit or explicit </returns>
		 Type TransactionType();

		 /// <summary>
		 /// Return transaction id that assigned during transaction commit process. </summary>
		 /// <seealso cref= org.neo4j.kernel.impl.api.TransactionCommitProcess </seealso>
		 /// <returns> transaction id. </returns>
		 /// <exception cref="IllegalStateException"> if transaction id is not assigned yet </exception>
		 long TransactionId { get; }

		 /// <summary>
		 /// Return transaction commit time (in millis) that assigned during transaction commit process. </summary>
		 /// <seealso cref= org.neo4j.kernel.impl.api.TransactionCommitProcess </seealso>
		 /// <returns> transaction commit time </returns>
		 /// <exception cref="IllegalStateException"> if commit time is not assigned yet </exception>
		 long CommitTime { get; }

		 /// <summary>
		 /// Temporarily override this transaction's SecurityContext. The override should be reverted using
		 /// the returned <seealso cref="Revertable"/>.
		 /// </summary>
		 /// <param name="context"> the temporary SecurityContext. </param>
		 /// <returns> <seealso cref="Revertable"/> which reverts to the original SecurityContext. </returns>
		 KernelTransaction_Revertable OverrideWith( SecurityContext context );

		 /// <summary>
		 /// Clocks associated with this transaction.
		 /// </summary>
		 ClockContext Clocks();

		 /// <summary>
		 /// USE WITH CAUTION:
		 /// The internal node cursor instance used to serve kernel API calls. If some kernel API call
		 /// is made while this cursor is used, it might get corrupted and return wrong results.
		 /// </summary>
		 NodeCursor AmbientNodeCursor();

		 /// <summary>
		 /// USE WITH CAUTION:
		 /// The internal relationship scan cursor instance used to serve kernel API calls. If some kernel
		 /// API call is made while this cursor is used, it might get corrupted and return wrong results.
		 /// </summary>
		 RelationshipScanCursor AmbientRelationshipCursor();

		 /// <summary>
		 /// USE WITH CAUTION:
		 /// The internal property cursor instance used to serve kernel API calls. If some kernel
		 /// API call is made while this cursor is used, it might get corrupted and return wrong results.
		 /// </summary>
		 PropertyCursor AmbientPropertyCursor();

		 /// <summary>
		 /// Attaches a map of data to this transaction.
		 /// The data will be printed when listing queries and inserted in to the query log. </summary>
		 /// <param name="metaData"> The data to add. </param>
		 IDictionary<string, object> MetaData { set;get; }


		 /// <returns> whether or not this transaction is a schema transaction. Type of transaction is decided
		 /// on first write operation, be it data or schema operation. </returns>
		 bool SchemaTransaction { get; }
	}

	 public interface KernelTransaction_CloseListener
	 {
		  /// <param name="txId"> On success, the actual id of the current transaction if writes have been performed, 0 otherwise.
		  /// On rollback, always -1. </param>
		  void Notify( long txId );
	 }

	 public interface KernelTransaction_Revertable : AutoCloseable
	 {
		  void Close();
	 }

}