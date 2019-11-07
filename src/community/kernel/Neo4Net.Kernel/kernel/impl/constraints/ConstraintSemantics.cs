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
	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;

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
//ORIGINAL LINE: public abstract void validateNodeKeyConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
		 public abstract void ValidateNodeKeyConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void validateNodePropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
		 public abstract void ValidateNodePropertyExistenceConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void validateRelationshipPropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.RelationshipScanCursor relationshipCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
		 public abstract void ValidateRelationshipPropertyExistenceConstraint( RelationshipScanCursor relationshipCursor, PropertyCursor propertyCursor, RelationTypeSchemaDescriptor descriptor );

		 public abstract ConstraintDescriptor ReadConstraint( ConstraintRule rule );

		 public abstract ConstraintRule CreateUniquenessConstraintRule( long ruleId, UniquenessConstraintDescriptor descriptor, long indexId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract Neo4Net.kernel.impl.store.record.ConstraintRule createNodeKeyConstraintRule(long ruleId, Neo4Net.kernel.api.schema.constraints.NodeKeyConstraintDescriptor descriptor, long indexId) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
		 public abstract ConstraintRule CreateNodeKeyConstraintRule( long ruleId, NodeKeyConstraintDescriptor descriptor, long indexId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract Neo4Net.kernel.impl.store.record.ConstraintRule createExistenceConstraint(long ruleId, Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
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