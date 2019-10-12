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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ConstraintViolationTransactionFailureException = Org.Neo4j.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using IndexPopulationFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using UniquePropertyValueValidationException = Org.Neo4j.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using TransactionRecordState = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

	/// <summary>
	/// Validates data integrity during the prepare phase of <seealso cref="TransactionRecordState"/>.
	/// </summary>
	public class IntegrityValidator
	{
		 private readonly NeoStores _neoStores;
		 private readonly IndexingService _indexes;

		 public IntegrityValidator( NeoStores neoStores, IndexingService indexes )
		 {
			  this._neoStores = neoStores;
			  this._indexes = indexes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodeRecord(org.neo4j.kernel.impl.store.record.NodeRecord record) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void ValidateNodeRecord( NodeRecord record )
		 {
			  if ( !record.InUse() && record.NextRel != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					throw new ConstraintViolationTransactionFailureException( "Cannot delete node<" + record.Id + ">, because it still has relationships. " + "To delete this node, you must first delete its relationships." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateTransactionStartKnowledge(long lastCommittedTxWhenTransactionStarted) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void ValidateTransactionStartKnowledge( long lastCommittedTxWhenTransactionStarted )
		 {
			  long latestConstraintIntroducingTx = _neoStores.MetaDataStore.LatestConstraintIntroducingTx;
			  if ( lastCommittedTxWhenTransactionStarted < latestConstraintIntroducingTx )
			  {
					// Constraints have changed since the transaction begun

					// This should be a relatively uncommon case, window for this happening is a few milliseconds when an admin
					// explicitly creates a constraint, after the index has been populated. We can improve this later on by
					// replicating the constraint validation logic down here, or rethinking where we validate constraints.
					// For now, we just kill these transactions.
					throw new TransactionFailureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.ConstraintsChanged, "Database constraints have changed (txId=%d) after this transaction (txId=%d) started, " + "which is not yet supported. Please retry your transaction to ensure all " + "constraints are executed.", latestConstraintIntroducingTx, lastCommittedTxWhenTransactionStarted );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateSchemaRule(org.neo4j.storageengine.api.schema.SchemaRule schemaRule) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void ValidateSchemaRule( SchemaRule schemaRule )
		 {
			  if ( schemaRule is ConstraintRule )
			  {
					ConstraintRule constraintRule = ( ConstraintRule ) schemaRule;
					if ( constraintRule.ConstraintDescriptor.enforcesUniqueness() )
					{
						 try
						 {
							  _indexes.validateIndex( constraintRule.OwnedIndex );
						 }
						 catch ( UniquePropertyValueValidationException e )
						 {
							  throw new TransactionFailureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionValidationFailed, e, "Index validation failed" );
						 }
						 catch ( Exception e ) when ( e is IndexNotFoundKernelException || e is IndexPopulationFailedKernelException )
						 {
							  // We don't expect this to occur, and if they do, it is because we are in a very bad state - out of
							  // disk or index corruption, or similar. This will kill the database such that it can be shut down
							  // and have recovery performed. It's the safest bet to avoid loosing data.
							  throw new TransactionFailureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionValidationFailed, e, "Index population failure" );
						 }
					}
			  }
		 }
	}

}