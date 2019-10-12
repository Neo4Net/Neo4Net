using System;

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
namespace Neo4Net.Kernel.Impl.Api.state
{

	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using SchemaRead = Neo4Net.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using CreateConstraintFailureException = Neo4Net.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using OperationContext = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException.OperationContext;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using Statement = Neo4Net.Kernel.api.Statement;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using AlreadyConstrainedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using AlreadyIndexedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyIndexedException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexProxy = Neo4Net.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Locks_Client = Neo4Net.Kernel.impl.locking.Locks_Client;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.exceptions.schema.ConstraintValidationException.Phase.VERIFICATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.exceptions.schema.SchemaKernelException.OperationContext.CONSTRAINT_CREATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.SecurityContext.AUTH_DISABLED;

	public class ConstraintIndexCreator
	{
		 private readonly IndexingService _indexingService;
		 private readonly System.Func<Kernel> _kernelSupplier;
		 private readonly NodePropertyAccessor _nodePropertyAccessor;
		 private readonly Log _log;

		 public ConstraintIndexCreator( System.Func<Kernel> kernelSupplier, IndexingService indexingService, NodePropertyAccessor nodePropertyAccessor, LogProvider logProvider )
		 {
			  this._kernelSupplier = kernelSupplier;
			  this._indexingService = indexingService;
			  this._nodePropertyAccessor = nodePropertyAccessor;
			  this._log = logProvider.GetLog( typeof( ConstraintIndexCreator ) );
		 }

		 /// <summary>
		 /// You MUST hold a label write lock before you call this method.
		 /// However the label write lock is temporarily released while populating the index backing the constraint.
		 /// It goes a little like this:
		 /// <ol>
		 /// <li>Prerequisite: Getting here means that there's an open schema transaction which has acquired the
		 /// LABEL WRITE lock.</li>
		 /// <li>Index schema rule which is backing the constraint is created in a nested mini-transaction
		 /// which doesn't acquire any locking, merely adds tx state and commits so that the index rule is applied
		 /// to the store, which triggers the index population</li>
		 /// <li>Release the LABEL WRITE lock</li>
		 /// <li>Await index population to complete</li>
		 /// <li>Acquire the LABEL WRITE lock (effectively blocking concurrent transactions changing
		 /// data related to this constraint, and it so happens, most other transactions as well) and verify
		 /// the uniqueness of the built index</li>
		 /// <li>Leave this method, knowing that the uniqueness constraint rule will be added to tx state
		 /// and this tx committed, which will create the uniqueness constraint</li>
		 /// </ol>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long createUniquenessConstraintIndex(org.neo4j.kernel.impl.api.KernelTransactionImplementation transaction, org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor, String provider) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException, org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException, org.neo4j.kernel.api.exceptions.schema.AlreadyConstrainedException
		 public virtual long CreateUniquenessConstraintIndex( KernelTransactionImplementation transaction, SchemaDescriptor descriptor, string provider )
		 {
			  UniquenessConstraintDescriptor constraint = ConstraintDescriptorFactory.uniqueForSchema( descriptor );
			  _log.info( "Starting constraint creation: %s.", constraint.OwnedIndexDescriptor() );

			  IndexReference index;
			  SchemaRead schemaRead = transaction.SchemaRead();
			  try
			  {
					index = GetOrCreateUniquenessConstraintIndex( schemaRead, transaction.TokenRead(), descriptor, provider );
			  }
			  catch ( AlreadyConstrainedException e )
			  {
					throw e;
			  }
			  catch ( Exception e ) when ( e is SchemaKernelException || e is IndexNotFoundKernelException )
			  {
					throw new CreateConstraintFailureException( constraint, e );
			  }

			  bool success = false;
			  bool reacquiredLabelLock = false;
			  Locks_Client locks = transaction.StatementLocks().pessimistic();
			  try
			  {
					long indexId = schemaRead.IndexGetCommittedId( index );
					IndexProxy proxy = _indexingService.getIndexProxy( indexId );

					// Release the LABEL WRITE lock during index population.
					// At this point the integrity of the constraint to be created was checked
					// while holding the lock and the index rule backing the soon-to-be-created constraint
					// has been created. Now it's just the population left, which can take a long time
					locks.ReleaseExclusive( descriptor.KeyType(), descriptor.KeyId() );

					AwaitConstraintIndexPopulation( constraint, proxy, transaction );
					_log.info( "Constraint %s populated, starting verification.", constraint.OwnedIndexDescriptor() );

					// Index population was successful, but at this point we don't know if the uniqueness constraint holds.
					// Acquire LABEL WRITE lock and verify the constraints here in this user transaction
					// and if everything checks out then it will be held until after the constraint has been
					// created and activated.
					locks.AcquireExclusive( transaction.LockTracer(), descriptor.KeyType(), descriptor.KeyId() );
					reacquiredLabelLock = true;

					_indexingService.getIndexProxy( indexId ).verifyDeferredConstraints( _nodePropertyAccessor );
					_log.info( "Constraint %s verified.", constraint.OwnedIndexDescriptor() );
					success = true;
					return indexId;
			  }
			  catch ( SchemaKernelException e )
			  {
					throw new System.InvalidOperationException( string.Format( "Index ({0}) that we just created does not exist.", descriptor ), e );
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new TransactionFailureException( string.Format( "Index ({0}) that we just created does not exist.", descriptor ), e );
			  }
			  catch ( IndexEntryConflictException e )
			  {
					throw new UniquePropertyValueValidationException( constraint, VERIFICATION, e );
			  }
			  catch ( Exception e ) when ( e is InterruptedException || e is IOException )
			  {
					throw new CreateConstraintFailureException( constraint, e );
			  }
			  finally
			  {
					if ( !success )
					{
						 if ( !reacquiredLabelLock )
						 {
							  locks.AcquireExclusive( transaction.LockTracer(), descriptor.KeyType(), descriptor.KeyId() );
						 }

						 if ( IndexStillExists( schemaRead, descriptor, index ) )
						 {
							  DropUniquenessConstraintIndex( ( IndexDescriptor ) index );
						 }
					}
			  }
		 }

		 private bool IndexStillExists( SchemaRead schemaRead, SchemaDescriptor descriptor, IndexReference index )
		 {
			  IndexReference existingIndex = schemaRead.Index( descriptor );
			  return existingIndex != IndexReference.NO_INDEX && existingIndex.Equals( index );
		 }

		 /// <summary>
		 /// You MUST hold a schema write lock before you call this method.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dropUniquenessConstraintIndex(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void DropUniquenessConstraintIndex( IndexDescriptor descriptor )
		 {
			  using ( Transaction transaction = _kernelSupplier.get().beginTransaction(@implicit, AUTH_DISABLED), Statement ignore = ((KernelTransaction)transaction).acquireStatement() )
			  {
					( ( KernelTransactionImplementation ) transaction ).txState().indexDoDrop(descriptor);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitConstraintIndexPopulation(org.neo4j.kernel.api.schema.constraints.UniquenessConstraintDescriptor constraint, org.neo4j.kernel.impl.api.index.IndexProxy proxy, org.neo4j.kernel.impl.api.KernelTransactionImplementation transaction) throws InterruptedException, org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException
		 private void AwaitConstraintIndexPopulation( UniquenessConstraintDescriptor constraint, IndexProxy proxy, KernelTransactionImplementation transaction )
		 {
			  try
			  {
					bool stillGoing;
					do
					{
						 stillGoing = proxy.AwaitStoreScanCompleted( 1, TimeUnit.SECONDS );
						 if ( transaction.Terminated )
						 {
							  throw new TransactionTerminatedException( transaction.ReasonIfTerminated.get() );
						 }
					} while ( stillGoing );
			  }
			  catch ( IndexPopulationFailedKernelException e )
			  {
					Exception cause = e.InnerException;
					if ( cause is IndexEntryConflictException )
					{
						 throw new UniquePropertyValueValidationException( constraint, VERIFICATION, ( IndexEntryConflictException ) cause );
					}
					else
					{
						 throw new UniquePropertyValueValidationException( constraint, VERIFICATION, e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference getOrCreateUniquenessConstraintIndex(org.neo4j.internal.kernel.api.SchemaRead schemaRead, org.neo4j.internal.kernel.api.TokenRead tokenRead, org.neo4j.internal.kernel.api.schema.SchemaDescriptor schema, String provider) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private IndexReference GetOrCreateUniquenessConstraintIndex( SchemaRead schemaRead, TokenRead tokenRead, SchemaDescriptor schema, string provider )
		 {
			  IndexReference descriptor = schemaRead.Index( schema );
			  if ( descriptor != IndexReference.NO_INDEX )
			  {
					if ( descriptor.Unique )
					{
						 // OK so we found a matching constraint index. We check whether or not it has an owner
						 // because this may have been a left-over constraint index from a previously failed
						 // constraint creation, due to crash or similar, hence the missing owner.
						 if ( schemaRead.IndexGetOwningUniquenessConstraintId( descriptor ) == null )
						 {
							  return descriptor;
						 }
						 throw new AlreadyConstrainedException( ConstraintDescriptorFactory.uniqueForSchema( schema ), SchemaKernelException.OperationContext.CONSTRAINT_CREATION, new SilentTokenNameLookup( tokenRead ) );
					}
					// There's already an index for this schema descriptor, which isn't of the type we're after.
					throw new AlreadyIndexedException( schema, CONSTRAINT_CREATION );
			  }
			  IndexDescriptor indexDescriptor = CreateConstraintIndex( schema, provider );
			  IndexProxy indexProxy = _indexingService.getIndexProxy( indexDescriptor.Schema() );
			  return indexProxy.Descriptor;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexDescriptor createConstraintIndex(final org.neo4j.internal.kernel.api.schema.SchemaDescriptor schema, String provider)
		 public virtual IndexDescriptor CreateConstraintIndex( SchemaDescriptor schema, string provider )
		 {
			  try
			  {
					  using ( Transaction transaction = _kernelSupplier.get().beginTransaction(@implicit, AUTH_DISABLED) )
					  {
						IndexDescriptor index = ( ( KernelTransaction ) transaction ).indexUniqueCreate( schema, provider );
						transaction.Success();
						return index;
					  }
			  }
			  catch ( Exception e ) when ( e is TransactionFailureException || e is SchemaKernelException )
			  {
					throw new Exception( e );
			  }
		 }
	}

}