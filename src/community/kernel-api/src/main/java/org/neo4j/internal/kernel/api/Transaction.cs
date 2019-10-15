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
namespace Neo4Net.Internal.Kernel.Api
{

	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// A transaction with the graph database.
	/// 
	/// Access to the graph is performed via sub-interfaces like <seealso cref="org.neo4j.internal.kernel.api.Read"/>.
	/// Changes made within a transaction are immediately visible to all operations within it, but are only
	/// visible to other transactions after the successful commit of the transaction.
	/// 
	/// Typical usage:
	/// <pre>
	/// try ( Transaction transaction = session.beginTransaction() )
	/// {
	///      ...
	///      transaction.success();
	/// }
	/// catch ( SomeException e )
	/// {
	///      ...
	/// }
	/// </pre>
	/// 
	/// Typical usage of failure if failure isn't controlled with exceptions:
	/// <pre>
	/// try ( Transaction transaction = session.beginTransaction() )
	/// {
	///      ...
	///      if ( ... some condition )
	///      {
	///          transaction.failure();
	///      }
	/// 
	///      transaction.success();
	/// }
	/// </pre>
	/// </summary>
	public interface Transaction : IDisposable
	{

		 /// <summary>
		 /// The store id of a rolled back transaction.
		 /// </summary>

		 /// <summary>
		 /// The store id of a read-only transaction.
		 /// </summary>

		 /// <summary>
		 /// Marks this transaction as successful. When this transaction later gets <seealso cref="close() closed"/>
		 /// its changes, if any, will be committed. If this method hasn't been called or if <seealso cref="failure()"/>
		 /// has been called then any changes in this transaction will be rolled back as part of <seealso cref="close() closing"/>.
		 /// </summary>
		 void Success();

		 /// <summary>
		 /// Marks this transaction as failed. No amount of calls to <seealso cref="success()"/> will clear this flag.
		 /// When <seealso cref="close() closing"/> this transaction any changes will be rolled back.
		 /// </summary>
		 void Failure();

		 /// <returns> The Read operations of the graph. The returned instance targets the active transaction state layer. </returns>
		 Read DataRead();

		 /// <returns> The Write operations of the graph. The returned instance writes to the active transaction state layer. </returns>
		 /// <exception cref="InvalidTransactionTypeKernelException"> when transaction cannot be upgraded to a write transaction. This
		 /// can happen when there have been schema modifications. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Write dataWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException;
		 Write DataWrite();

		 /// <returns> The explicit index read operations of the graph. </returns>
		 ExplicitIndexRead IndexRead();

		 /// <returns> The explicit index write operations of the graph. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexWrite indexWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException;
		 ExplicitIndexWrite IndexWrite();

		 /// <returns> Token read operations </returns>
		 TokenRead TokenRead();

		 /// <returns> Token read operations </returns>
		 TokenWrite TokenWrite();

		 /// <returns> Token read and write operations </returns>
		 Token Token();

		 /// <returns> The schema index read operations of the graph, used for finding indexes. </returns>
		 SchemaRead SchemaRead();

		 /// <returns> The schema index write operations of the graph, used for creating and dropping indexes and constraints. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SchemaWrite schemaWrite() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException;
		 SchemaWrite SchemaWrite();

		 /// <returns> The lock operations of the graph. </returns>
		 Locks Locks();

		 /// <returns> The cursor factory </returns>
		 CursorFactory Cursors();

		 /// <returns> Returns procedure operations </returns>
		 Procedures Procedures();

		 /// <returns> statistics about the execution </returns>
		 ExecutionStatistics ExecutionStatistics();

		 /// <summary>
		 /// Closes this transaction, committing its changes if <seealso cref="success()"/> has been called and neither
		 /// <seealso cref="failure()"/> nor <seealso cref="markForTermination(Status)"/> has been called.
		 /// Otherwise its changes will be rolled back.
		 /// </summary>
		 /// <returns> id of the committed transaction or <seealso cref="ROLLBACK"/> if transaction was rolled back or
		 /// <seealso cref="READ_ONLY"/> if transaction was read-only. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long closeTransaction() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 long CloseTransaction();

		 /// <summary>
		 /// Closes this transaction, committing its changes if <seealso cref="success()"/> has been called and neither
		 /// <seealso cref="failure()"/> nor <seealso cref="markForTermination(Status)"/> has been called.
		 /// Otherwise its changes will be rolled back.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void close() throws org.neo4j.Internal.kernel.api.exceptions.TransactionFailureException
	//	 {
	//		  closeTransaction();
	//	 }

		 /// <returns> {@code true} if the transaction is still open, i.e. if <seealso cref="close()"/> hasn't been called yet. </returns>
		 bool Open { get; }

		 /// <returns> <seealso cref="Status"/> if <seealso cref="markForTermination(Status)"/> has been invoked, otherwise empty optional. </returns>
		 Optional<Status> ReasonIfTerminated { get; }

		 /// <returns> true if transaction was terminated, otherwise false </returns>
		 bool Terminated { get; }

		 /// <summary>
		 /// Marks this transaction for termination, such that it cannot commit successfully and will try to be
		 /// terminated by having other methods throw a specific termination exception, as to sooner reach the assumed
		 /// point where <seealso cref="close()"/> will be invoked.
		 /// </summary>
		 void MarkForTermination( Status reason );
	}

	public static class Transaction_Fields
	{
		 public const long ROLLBACK = -1;
		 public const long READ_ONLY = 0;
	}

	 public enum Transaction_Type
	 {
		  Implicit,
		  Explicit
	 }

}