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
namespace Neo4Net.Storageengine.Api
{

	using DiagnosticsManager = Neo4Net.@internal.Diagnostics.DiagnosticsManager;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ConstraintValidationException = Neo4Net.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;

	/// <summary>
	/// A StorageEngine provides the functionality to durably store data, and read it back.
	/// </summary>
	public interface StorageEngine
	{
		 /// <summary>
		 /// Creates a new <seealso cref="StorageReader"/> for reading committed data from the underlying storage.
		 /// The returned instance is intended to be used by one transaction at a time, although can and should be reused
		 /// for multiple transactions.
		 /// </summary>
		 /// <returns> an interface for accessing data previously
		 /// <seealso cref="apply(CommandsToApply, TransactionApplicationMode) applied"/> to this storage. </returns>
		 StorageReader NewReader();

		 /// <returns> a new <seealso cref="CommandCreationContext"/> meant to be kept for multiple calls to
		 /// <seealso cref="createCommands(System.Collections.ICollection, ReadableTransactionState, StorageReader, ResourceLocker, long, TxStateVisitor.Decorator)"/>.
		 /// Must be <seealso cref="CommandCreationContext.close() closed"/> after used, before being discarded. </returns>
		 CommandCreationContext AllocateCommandCreationContext();

		 /// <summary>
		 /// Generates a list of <seealso cref="StorageCommand commands"/> representing the changes in the given transaction state
		 /// ({@code state}.
		 /// The returned commands can be used to form <seealso cref="CommandsToApply"/> batches, which can be applied to this
		 /// storage using <seealso cref="apply(CommandsToApply, TransactionApplicationMode)"/>.
		 /// The reason this is separated like this is that the generated commands can be used for other things
		 /// than applying to storage, f.ex replicating to another storage engine. </summary>
		 /// <param name="target"> <seealso cref="System.Collections.ICollection"/> to put <seealso cref="StorageCommand commands"/> into. </param>
		 /// <param name="state"> <seealso cref="ReadableTransactionState"/> representing logical store changes to generate commands for. </param>
		 /// <param name="storageReader"> <seealso cref="StorageReader"/> to use for reading store state during creation of commands. </param>
		 /// <param name="locks"> <seealso cref="ResourceLocker"/> can grab additional locks.
		 /// This locks client still have the potential to acquire more locks at this point.
		 /// TODO we should try to get rid of this locking mechanism during creation of commands
		 /// The reason it's needed is that some relationship changes in the record storage engine
		 /// needs to lock prev/next relationships and these changes happens when creating commands
		 /// The EntityLocker interface is a subset of Locks.Client interface, just to fit in while it's here. </param>
		 /// <param name="lastTransactionIdWhenStarted"> transaction id which was seen as last committed when this
		 /// transaction started, i.e. before any changes were made and before any data was read.
		 /// TODO Transitional (Collection), might be <seealso cref="Stream"/> or whatever. </param>
		 /// <param name="additionalTxStateVisitor"> any additional tx state visitor decoration.
		 /// </param>
		 /// <exception cref="TransactionFailureException"> if command generation fails or some prerequisite of some command
		 /// didn't validate, for example if trying to delete a node that still has relationships. </exception>
		 /// <exception cref="CreateConstraintFailureException"> if this transaction was set to create a constraint and that failed. </exception>
		 /// <exception cref="ConstraintValidationException"> if this transaction was set to create a constraint
		 /// and some data violates that constraint. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createCommands(java.util.Collection<StorageCommand> target, org.neo4j.storageengine.api.txstate.ReadableTransactionState state, StorageReader storageReader, org.neo4j.storageengine.api.lock.ResourceLocker locks, long lastTransactionIdWhenStarted, org.neo4j.storageengine.api.txstate.TxStateVisitor_Decorator additionalTxStateVisitor) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException, org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 void CreateCommands( ICollection<StorageCommand> target, ReadableTransactionState state, StorageReader storageReader, ResourceLocker locks, long lastTransactionIdWhenStarted, Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Decorator additionalTxStateVisitor );

		 /// <summary>
		 /// Apply a batch of groups of commands to this storage.
		 /// </summary>
		 /// <param name="batch"> batch of groups of commands to apply to storage. </param>
		 /// <param name="mode"> <seealso cref="TransactionApplicationMode"/> when applying. </param>
		 /// <exception cref="Exception"> if an error occurs during application. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void apply(CommandsToApply batch, TransactionApplicationMode mode) throws Exception;
		 void Apply( CommandsToApply batch, TransactionApplicationMode mode );

		 /// <returns> a <seealso cref="CommandReaderFactory"/> capable of returning <seealso cref="CommandReader commands readers"/>
		 /// for specific log entry versions. </returns>
		 CommandReaderFactory CommandReaderFactory();

		 /// <summary>
		 /// Flushes and forces all changes down to underlying storage. This is a blocking call and when it returns
		 /// all changes applied to this storage engine will be durable. </summary>
		 /// <param name="limiter"> The <seealso cref="IOLimiter"/> used to moderate the rate of IO caused by the flush process. </param>
		 void FlushAndForce( IOLimiter limiter );

		 /// <summary>
		 /// Registers diagnostics about the storage onto <seealso cref="DiagnosticsManager"/>.
		 /// </summary>
		 /// <param name="diagnosticsManager"> <seealso cref="DiagnosticsManager"/> to register diagnostics at. </param>
		 void RegisterDiagnostics( DiagnosticsManager diagnosticsManager );

		 /// <summary>
		 /// Force close all opened resources. This may be called during startup if there's a failure
		 /// during recovery or similar.
		 /// </summary>
		 void ForceClose();

		 /// <summary>
		 /// Startup process have reached the conclusion that recovery is required. Make the necessary
		 /// preparations to be ready for recovering transactions.
		 /// </summary>
		 void PrepareForRecoveryRequired();

		 /// <returns> a <seealso cref="System.Collections.ICollection"/> of <seealso cref="StoreFileMetadata"/> containing metadata about all store files managed by
		 /// this <seealso cref="StorageEngine"/>. </returns>
		 ICollection<StoreFileMetadata> ListStorageFiles();

		 /// <returns> the <seealso cref="StoreId"/> of the underlying store. </returns>
		 StoreId StoreId { get; }

		 /// <summary>
		 /// The life cycle that is used for initialising the token holders, and filling the schema cache.
		 /// </summary>
		 Lifecycle SchemaAndTokensLifecycle();

		 // ====================================================================
		 // All these methods below are temporary while in the process of
		 // creating this API, take little notice to them, as they will go away
		 // ====================================================================

		 [Obsolete]
		 void LoadSchemaCache();

		 [Obsolete]
		 void ClearBufferedIds();
	}

}