﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.impl.enterprise
{
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using NodeLabelIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeLabelIndexCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using NodePropertyExistenceException = Org.Neo4j.Kernel.Api.Exceptions.schema.NodePropertyExistenceException;
	using RelationshipPropertyExistenceException = Org.Neo4j.Kernel.Api.Exceptions.schema.RelationshipPropertyExistenceException;
	using NodeKeyConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.exceptions.schema.ConstraintValidationException.Phase.VERIFICATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.PropertyExistenceEnforcer.getOrCreatePropertyExistenceEnforcerFrom;

	public class EnterpriseConstraintSemantics : StandardConstraintSemantics
	{
		 public EnterpriseConstraintSemantics() : base("enterpriseConstraints", 2)
		 {
		 }

		 protected internal override ConstraintDescriptor ReadNonStandardConstraint( ConstraintRule rule, string errorMessage )
		 {
			  if ( !rule.ConstraintDescriptor.enforcesPropertyExistence() )
			  {
					throw new System.InvalidOperationException( "Unsupported constraint type: " + rule );
			  }
			  return rule.ConstraintDescriptor;
		 }

		 public override ConstraintRule CreateNodeKeyConstraintRule( long ruleId, NodeKeyConstraintDescriptor descriptor, long indexId )
		 {
			  return ConstraintRule.constraintRule( ruleId, descriptor, indexId );
		 }

		 public override ConstraintRule CreateExistenceConstraint( long ruleId, ConstraintDescriptor descriptor )
		 {
			  return ConstraintRule.constraintRule( ruleId, descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodePropertyExistenceConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override void ValidateNodePropertyExistenceConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  while ( allNodes.Next() )
			  {
					allNodes.Node( nodeCursor );
					while ( nodeCursor.Next() )
					{
						 foreach ( int propertyKey in descriptor.PropertyIds )
						 {
							  nodeCursor.Properties( propertyCursor );
							  if ( !HasProperty( propertyCursor, propertyKey ) )
							  {
									throw CreateConstraintFailure( new NodePropertyExistenceException( descriptor, VERIFICATION, nodeCursor.NodeReference() ) );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateNodeKeyConstraint(org.neo4j.internal.kernel.api.NodeLabelIndexCursor allNodes, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override void ValidateNodeKeyConstraint( NodeLabelIndexCursor allNodes, NodeCursor nodeCursor, PropertyCursor propertyCursor, LabelSchemaDescriptor descriptor )
		 {
			  ValidateNodePropertyExistenceConstraint( allNodes, nodeCursor, propertyCursor, descriptor );
		 }

		 private bool HasProperty( PropertyCursor propertyCursor, int property )
		 {
			  while ( propertyCursor.Next() )
			  {
					if ( propertyCursor.PropertyKey() == property )
					{
						 return true;
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateRelationshipPropertyExistenceConstraint(org.neo4j.internal.kernel.api.RelationshipScanCursor relationshipCursor, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, org.neo4j.internal.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 public override void ValidateRelationshipPropertyExistenceConstraint( RelationshipScanCursor relationshipCursor, PropertyCursor propertyCursor, RelationTypeSchemaDescriptor descriptor )
		 {
			  while ( relationshipCursor.Next() )
			  {
					relationshipCursor.Properties( propertyCursor );

					foreach ( int propertyKey in descriptor.PropertyIds )
					{
						 if ( relationshipCursor.Type() == descriptor.RelTypeId && !HasProperty(propertyCursor, propertyKey) )
						 {
							  throw CreateConstraintFailure( new RelationshipPropertyExistenceException( descriptor, VERIFICATION, relationshipCursor.RelationshipReference() ) );
						 }
					}
			  }
		 }

		 private CreateConstraintFailureException CreateConstraintFailure( ConstraintValidationException it )
		 {
			  return new CreateConstraintFailureException( it.Constraint(), it );
		 }

		 public override TxStateVisitor DecorateTxStateVisitor( StorageReader storageReader, Read read, CursorFactory cursorFactory, ReadableTransactionState txState, TxStateVisitor visitor )
		 {
			  if ( !txState.HasDataChanges() )
			  {
					// If there are no data changes, there is no need to enforce constraints. Since there is no need to
					// enforce constraints, there is no need to build up the state required to be able to enforce constraints.
					// In fact, it might even be counter productive to build up that state, since if there are no data changes
					// there would be schema changes instead, and in that case we would throw away the schema-dependant state
					// we just built when the schema changing transaction commits.
					return visitor;
			  }
			  return getOrCreatePropertyExistenceEnforcerFrom( storageReader ).decorate( visitor, read, cursorFactory );
		 }
	}

}