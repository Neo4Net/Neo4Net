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
	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.Api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;

	public class StandardConstraintSemantics : ConstraintSemantics
	{
		 public const string ERROR_MESSAGE_EXISTS = "Property existence constraint requires Neo4Net Enterprise Edition";
		 public const string ERROR_MESSAGE_NODE_KEY = "Node Key constraint requires Neo4Net Enterprise Edition";

		 public StandardConstraintSemantics() : this("standardConstraints", 1)
		 {
		 }

		 protected internal StandardConstraintSemantics( string key, int priority ) : base( key, priority )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodeKeyConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override void ValidateNodeKeyConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  throw NodeKeyConstraintsNotAllowed( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodePropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override void ValidateNodePropertyExistenceConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  throw PropertyExistenceConstraintsNotAllowed( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateRelationshipPropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.RelationshipScanCursor relationshipCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override void ValidateRelationshipPropertyExistenceConstraint( RelationshipScanCursor relationshipCursor, PropertyCursor propertyCursor, RelationTypeSchemaDescriptor descriptor )
		 {
			  throw PropertyExistenceConstraintsNotAllowed( descriptor );
		 }

		 public override ConstraintDescriptor ReadConstraint( ConstraintRule rule )
		 {
			  ConstraintDescriptor desc = rule.ConstraintDescriptor;
			  switch ( desc.Type() )
			  {
			  case EXISTS:
					return ReadNonStandardConstraint( rule, ERROR_MESSAGE_EXISTS );
			  case UNIQUE_EXISTS:
					return ReadNonStandardConstraint( rule, ERROR_MESSAGE_NODE_KEY );
			  default:
					return desc;
			  }
		 }

		 protected internal virtual ConstraintDescriptor ReadNonStandardConstraint( ConstraintRule rule, string errorMessage )
		 {
			  // When opening a store in Community Edition that contains a Property Existence Constraint
			  throw new System.InvalidOperationException( errorMessage );
		 }

		 private CreateConstraintFailureException PropertyExistenceConstraintsNotAllowed( SchemaDescriptor descriptor )
		 {
			  // When creating a Property Existence Constraint in Community Edition
			  return new CreateConstraintFailureException( ConstraintDescriptorFactory.existsForSchema( descriptor ), ERROR_MESSAGE_EXISTS );
		 }

		 private CreateConstraintFailureException NodeKeyConstraintsNotAllowed( SchemaDescriptor descriptor )
		 {
			  // When creating a Node Key Constraint in Community Edition
			  return new CreateConstraintFailureException( ConstraintDescriptorFactory.existsForSchema( descriptor ), ERROR_MESSAGE_NODE_KEY );
		 }

		 public override ConstraintRule CreateUniquenessConstraintRule( long ruleId, UniquenessConstraintDescriptor descriptor, long indexId )
		 {
			  return ConstraintRule.constraintRule( ruleId, descriptor, indexId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.store.record.ConstraintRule createNodeKeyConstraintRule(long ruleId, Neo4Net.kernel.api.schema.constraints.NodeKeyConstraintDescriptor descriptor, long indexId) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override ConstraintRule CreateNodeKeyConstraintRule( long ruleId, NodeKeyConstraintDescriptor descriptor, long indexId )
		 {
			  throw NodeKeyConstraintsNotAllowed( descriptor.Schema() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.store.record.ConstraintRule createExistenceConstraint(long ruleId, Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override ConstraintRule CreateExistenceConstraint( long ruleId, ConstraintDescriptor descriptor )
		 {
			  throw PropertyExistenceConstraintsNotAllowed( descriptor.Schema() );
		 }

		 public override TxStateVisitor DecorateTxStateVisitor( StorageReader storageReader, Read read, CursorFactory cursorFactory, ReadableTransactionState state, TxStateVisitor visitor )
		 {
			  return visitor;
		 }
	}

}