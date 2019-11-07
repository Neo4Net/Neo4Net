/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise
{
	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using NodePropertyExistenceException = Neo4Net.Kernel.Api.Exceptions.schema.NodePropertyExistenceException;
	using RelationshipPropertyExistenceException = Neo4Net.Kernel.Api.Exceptions.schema.RelationshipPropertyExistenceException;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException.Phase.VERIFICATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.enterprise.PropertyExistenceEnforcer.getOrCreatePropertyExistenceEnforcerFrom;

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
//ORIGINAL LINE: public void validateNodePropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
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
//ORIGINAL LINE: public void validateNodeKeyConstraint(Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor allNodes, Neo4Net.Kernel.Api.Internal.NodeCursor nodeCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
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
//ORIGINAL LINE: public void validateRelationshipPropertyExistenceConstraint(Neo4Net.Kernel.Api.Internal.RelationshipScanCursor relationshipCursor, Neo4Net.Kernel.Api.Internal.PropertyCursor propertyCursor, Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor descriptor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
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