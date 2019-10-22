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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.GraphDb;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexConfigProvider = Neo4Net.Kernel.Api.Index.IndexConfigProvider;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Controls access to <seealso cref="IndexPopulator"/>, <seealso cref="IndexAccessor"/> during different stages
	/// of the lifecycle of an index. It's designed to be decorated with multiple stacked instances.
	/// 
	/// The contract of <seealso cref="IndexProxy"/> is
	/// 
	/// <ul>
	///     <li>The index may not be created twice</li>
	///     <li>The context may not be closed twice</li>
	///     <li>Close or drop both close the context</li>
	///     <li>The index may not be dropped before it has been created</li>
	///     <li>newUpdater and force may only be called after the index has been created and before it is closed</li>
	///     <li>It is an error to not close an updater before doing any other call on an index</li>
	///     <li>It is an error to close or drop the index while there are still ongoing calls to update and force</li>
	/// </ul>
	/// </summary>
	/// <seealso cref= ContractCheckingIndexProxy </seealso>
	public interface IndexProxy : IndexConfigProvider
	{
		 void Start();

		 IndexUpdater NewUpdater( IndexUpdateMode mode );

		 /// <summary>
		 /// Drop index.
		 /// Must close the context as well.
		 /// </summary>
		 void Drop();

		 /// <summary>
		 /// Close this index context.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException;
		 void Close();

		 CapableIndexDescriptor Descriptor { get; }

		 InternalIndexState State { get; }

		 /// <returns> failure message. Expect a call to it if <seealso cref="getState()"/> returns <seealso cref="InternalIndexState.FAILED"/>. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexPopulationFailure getPopulationFailure() throws IllegalStateException;
		 IndexPopulationFailure PopulationFailure { get; }

		 PopulationProgress IndexPopulationProgress { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void force(org.Neo4Net.io.pagecache.IOLimiter ioLimiter) throws java.io.IOException;
		 void Force( IOLimiter ioLimiter );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void refresh() throws java.io.IOException;
		 void Refresh();

		 /// <exception cref="IndexNotFoundKernelException"> if the index isn't online yet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.storageengine.api.schema.IndexReader newReader() throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReader NewReader();

		 /// <param name="time"> time to wait maximum. A value of 0 means indefinite wait. </param>
		 /// <param name="unit"> unit of time to wait. </param>
		 /// <returns> {@code true} if the call waited, {@code false} if the condition was already reached. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean awaitStoreScanCompleted(long time, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException;
		 bool AwaitStoreScanCompleted( long time, TimeUnit unit );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void activate() throws org.Neo4Net.kernel.api.exceptions.index.IndexActivationFailedKernelException;
		 void Activate();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void validate() throws org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException, org.Neo4Net.kernel.api.exceptions.schema.UniquePropertyValueValidationException;
		 void Validate();

		 /// <summary>
		 /// Validates a <seealso cref="Value"/> so that it's OK to later apply to the index. This method is designed to be
		 /// called before committing a transaction as to prevent exception during applying that transaction.
		 /// </summary>
		 /// <param name="tuple"> <seealso cref="Value value tuple"/> to validate. </param>
		 void ValidateBeforeCommit( Value[] tuple );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.graphdb.ResourceIterator<java.io.File> snapshotFiles() throws java.io.IOException;
		 ResourceIterator<File> SnapshotFiles();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void verifyDeferredConstraints(org.Neo4Net.storageengine.api.NodePropertyAccessor accessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
	//	 {
	//		  throw new IllegalStateException(this.toString());
	//	 }
	}

}