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
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Internal.Kernel.Api.NodeLabelIndexCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using CreateConstraintFailureException = Neo4Net.Internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;

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