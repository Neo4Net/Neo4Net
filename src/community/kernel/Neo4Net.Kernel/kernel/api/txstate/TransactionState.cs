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
namespace Neo4Net.Kernel.api.txstate
{
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

	/// <summary>
	/// Kernel transaction state, please see <seealso cref="org.neo4j.kernel.impl.api.state.TxState"/> for implementation details.
	/// 
	/// This interface defines the mutating methods for the transaction state, methods for reading are defined in
	/// <seealso cref="ReadableTransactionState"/>. These mutating methods follow the rule that they all contain the word "Do" in the name.
	/// This naming convention helps deciding where to set <seealso cref="hasChanges()"/> in the
	/// <seealso cref="org.neo4j.kernel.impl.api.state.TxState main implementation class"/>.
	/// </summary>
	public interface TransactionState : ReadableTransactionState
	{
		 // ENTITY RELATED

		 void RelationshipDoCreate( long id, int relationshipTypeId, long startNodeId, long endNodeId );

		 void NodeDoCreate( long id );

		 void RelationshipDoDelete( long relationshipId, int type, long startNode, long endNode );

		 void RelationshipDoDeleteAddedInThisTx( long relationshipId );

		 void NodeDoDelete( long nodeId );

		 void NodeDoAddProperty( long nodeId, int newPropertyKeyId, Value value );

		 void NodeDoChangeProperty( long nodeId, int propertyKeyId, Value newValue );

		 void RelationshipDoReplaceProperty( long relationshipId, int propertyKeyId, Value replacedValue, Value newValue );

		 void GraphDoReplaceProperty( int propertyKeyId, Value replacedValue, Value newValue );

		 void NodeDoRemoveProperty( long nodeId, int propertyKeyId );

		 void RelationshipDoRemoveProperty( long relationshipId, int propertyKeyId );

		 void GraphDoRemoveProperty( int propertyKeyId );

		 void NodeDoAddLabel( long labelId, long nodeId );

		 void NodeDoRemoveLabel( long labelId, long nodeId );

		 // TOKEN RELATED

		 void LabelDoCreateForName( string labelName, long id );

		 void PropertyKeyDoCreateForName( string propertyKeyName, int id );

		 void RelationshipTypeDoCreateForName( string relationshipTypeName, int id );

		 // SCHEMA RELATED

		 void IndexDoAdd( IndexDescriptor descriptor );

		 void IndexDoDrop( IndexDescriptor descriptor );

		 bool IndexDoUnRemove( IndexDescriptor constraint );

		 void ConstraintDoAdd( ConstraintDescriptor constraint );

		 void ConstraintDoAdd( IndexBackedConstraintDescriptor constraint, long indexId );

		 void ConstraintDoDrop( ConstraintDescriptor constraint );

		 bool ConstraintDoUnRemove( ConstraintDescriptor constraint );

		 void IndexDoUpdateEntry( SchemaDescriptor descriptor, long nodeId, ValueTuple before, ValueTuple after );

	}

}