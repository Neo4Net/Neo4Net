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
namespace Neo4Net.Storageengine.Api.txstate
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using UnmodifiableMap = org.eclipse.collections.impl.UnmodifiableMap;


	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Neo4Net.Storageengine.Api;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

	/// <summary>
	/// This interface contains the methods for reading transaction state from the transaction state.
	/// The implementation of these methods should be free of any side effects (such as initialising lazy state).
	/// </summary>
	public interface ReadableTransactionState
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(TxStateVisitor visitor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.ConstraintValidationException, org.Neo4Net.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 void Accept( TxStateVisitor visitor );

		 bool HasChanges();

		 // IEntity RELATED

		 /// <summary>
		 /// Returns all nodes that, in this tx, have had the labels changed. </summary>
		 /// <param name="label"> </param>
		 LongDiffSets NodesWithLabelChanged( long label );

		 /// <summary>
		 /// Returns nodes that have been added and removed in this tx.
		 /// </summary>
		 LongDiffSets AddedAndRemovedNodes();

		 /// <summary>
		 /// Returns rels that have been added and removed in this tx.
		 /// </summary>
		 LongDiffSets AddedAndRemovedRelationships();

		 /// <summary>
		 /// Nodes that have had labels, relationships, or properties modified in this tx.
		 /// </summary>
		 IEnumerable<NodeState> ModifiedNodes();

		 /// <summary>
		 /// Rels that have properties modified in this tx.
		 /// </summary>
		 IEnumerable<RelationshipState> ModifiedRelationships();

		 bool RelationshipIsAddedInThisTx( long relationshipId );

		 bool RelationshipIsDeletedInThisTx( long relationshipId );

		 LongDiffSets NodeStateLabelDiffSets( long nodeId );

		 bool NodeIsAddedInThisTx( long nodeId );

		 bool NodeIsDeletedInThisTx( long nodeId );

		 /// <returns> {@code true} if the relationship was visited in this state, i.e. if it was created
		 /// by this current transaction, otherwise {@code false} where the relationship might need to be
		 /// visited from the store. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EX extends Exception> boolean relationshipVisit(long relId, org.Neo4Net.storageengine.api.RelationshipVisitor<EX> visitor) throws EX;
		 bool relationshipVisit<EX>( long relId, RelationshipVisitor<EX> visitor );

		 // SCHEMA RELATED

		 DiffSets<IndexDescriptor> IndexDiffSetsByLabel( int labelId );

		 DiffSets<IndexDescriptor> IndexDiffSetsByRelationshipType( int relationshipType );

		 DiffSets<IndexDescriptor> IndexDiffSetsBySchema( SchemaDescriptor schema );

		 DiffSets<IndexDescriptor> IndexChanges();

		 IEnumerable<IndexDescriptor> ConstraintIndexesCreatedInTx();

		 DiffSets<ConstraintDescriptor> ConstraintsChanges();

		 DiffSets<ConstraintDescriptor> ConstraintsChangesForLabel( int labelId );

		 DiffSets<ConstraintDescriptor> ConstraintsChangesForSchema( SchemaDescriptor descriptor );

		 DiffSets<ConstraintDescriptor> ConstraintsChangesForRelationshipType( int relTypeId );

		 long? IndexCreatedForConstraint( ConstraintDescriptor constraint );

		 // INDEX UPDATES

		 /// <summary>
		 /// A readonly view of all index updates for the provided schema. Returns {@code null}, if the index
		 /// updates for this schema have not been initialized.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable UnmodifiableMap<org.Neo4Net.values.storable.ValueTuple, ? extends LongDiffSets> getIndexUpdates(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor schema);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 UnmodifiableMap<ValueTuple, ? extends LongDiffSets> GetIndexUpdates( SchemaDescriptor schema );

		 /// <summary>
		 /// A readonly view of all index updates for the provided schema, in sorted order. The returned
		 /// Map is unmodifiable. Returns {@code null}, if the index updates for this schema have not been initialized.
		 /// 
		 /// Ensure sorted index updates for a given index. This is needed for range query support and
		 /// ay involve converting the existing hash map first.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable NavigableMap<org.Neo4Net.values.storable.ValueTuple, ? extends LongDiffSets> getSortedIndexUpdates(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 NavigableMap<ValueTuple, ? extends LongDiffSets> GetSortedIndexUpdates( SchemaDescriptor descriptor );

		 // OTHER

		 NodeState GetNodeState( long id );

		 RelationshipState GetRelationshipState( long id );

		 GraphState GraphState { get; }

		 MutableLongSet AugmentLabels( MutableLongSet labels, NodeState nodeState );

		 /// <summary>
		 /// The way tokens are created is that the first time a token is needed it gets created in its own little
		 /// token mini-transaction, separate from the surrounding transaction that creates or modifies data that need it.
		 /// From the kernel POV it's interesting to know whether or not any tokens have been created in this tx state,
		 /// because then we know it's a mini-transaction like this and won't have to let transaction event handlers
		 /// know about it, for example.
		 /// 
		 /// The same applies to schema changes, such as creating and dropping indexes and constraints.
		 /// </summary>
		 bool HasDataChanges();

	}

}