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
namespace Neo4Net.Kernel.impl.constraints
{
	using Service = Neo4Net.Helpers.Service;
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Internal.Kernel.Api.NodeLabelIndexCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using CreateConstraintFailureException = Neo4Net.Internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;

	/// <summary>
	/// Implements semantics of constraint creation and enforcement.
	/// </summary>
	public abstract class ConstraintSemantics : Service
	{
		 private readonly int _priority;

		 protected internal ConstraintSemantics( string key, int priority ) : base( key )
		 {
			  this._priority = priority;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void validateNodeKeyConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 public abstract void ValidateNodeKeyConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void validateNodePropertyExistenceConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 public abstract void ValidateNodePropertyExistenceConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void validateRelationshipPropertyExistenceConstraint(org.neo4j.internal.kernel.api.RelationshipScanCursor relationshipCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 public abstract void ValidateRelationshipPropertyExistenceConstraint( RelationshipScanCursor relationshipCursor, PropertyCursor propertyCursor, RelationTypeSchemaDescriptor descriptor );

		 public abstract ConstraintDescriptor ReadConstraint( ConstraintRule rule );

		 public abstract ConstraintRule CreateUniquenessConstraintRule( long ruleId, UniquenessConstraintDescriptor descriptor, long indexId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.kernel.impl.store.record.ConstraintRule createNodeKeyConstraintRule(long ruleId, org.neo4j.kernel.api.schema.constraints.NodeKeyConstraintDescriptor descriptor, long indexId) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 public abstract ConstraintRule CreateNodeKeyConstraintRule( long ruleId, NodeKeyConstraintDescriptor descriptor, long indexId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.kernel.impl.store.record.ConstraintRule createExistenceConstraint(long ruleId, org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 public abstract ConstraintRule CreateExistenceConstraint( long ruleId, ConstraintDescriptor descriptor );

		 public abstract TxStateVisitor DecorateTxStateVisitor( StorageReader storageReader, Read read, CursorFactory cursorFactory, ReadableTransactionState state, TxStateVisitor visitor );

		 public virtual int Priority
		 {
			 get
			 {
				  return _priority;
			 }
		 }
	}

}