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
namespace Org.Neo4j.Kernel.impl.constraints
{
	using Service = Org.Neo4j.Helpers.Service;
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using NodeLabelIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeLabelIndexCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;

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