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
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using NodeLabelIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeLabelIndexCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;

	public class StandardConstraintSemantics : ConstraintSemantics
	{
		 public const string ERROR_MESSAGE_EXISTS = "Property existence constraint requires Neo4j Enterprise Edition";
		 public const string ERROR_MESSAGE_NODE_KEY = "Node Key constraint requires Neo4j Enterprise Edition";

		 public StandardConstraintSemantics() : this("standardConstraints", 1)
		 {
		 }

		 protected internal StandardConstraintSemantics( string key, int priority ) : base( key, priority )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodeKeyConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override void ValidateNodeKeyConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  throw NodeKeyConstraintsNotAllowed( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodePropertyExistenceConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override void ValidateNodePropertyExistenceConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  throw PropertyExistenceConstraintsNotAllowed( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateRelationshipPropertyExistenceConstraint(org.neo4j.internal.kernel.api.RelationshipScanCursor relationshipCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
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
//ORIGINAL LINE: public org.neo4j.kernel.impl.store.record.ConstraintRule createNodeKeyConstraintRule(long ruleId, org.neo4j.kernel.api.schema.constraints.NodeKeyConstraintDescriptor descriptor, long indexId) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override ConstraintRule CreateNodeKeyConstraintRule( long ruleId, NodeKeyConstraintDescriptor descriptor, long indexId )
		 {
			  throw NodeKeyConstraintsNotAllowed( descriptor.Schema() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.store.record.ConstraintRule createExistenceConstraint(long ruleId, org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
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