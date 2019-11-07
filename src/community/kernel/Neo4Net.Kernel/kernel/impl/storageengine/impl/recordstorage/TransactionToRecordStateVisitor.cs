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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;


	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using DuplicateSchemaRuleException = Neo4Net.Kernel.Api.Exceptions.schema.DuplicateSchemaRuleException;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.IndexBackedConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using StorageProperty = Neo4Net.Kernel.Api.StorageEngine.StorageProperty;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;

	internal class TransactionToRecordStateVisitor : Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Adapter
	{
		 private bool _clearSchemaState;
		 private readonly TransactionRecordState _recordState;
		 private readonly SchemaState _schemaState;
		 private readonly SchemaStorage _schemaStorage;
		 private readonly ConstraintSemantics _constraintSemantics;

		 internal TransactionToRecordStateVisitor( TransactionRecordState recordState, SchemaState schemaState, SchemaStorage schemaStorage, ConstraintSemantics constraintSemantics )
		 {
			  this._recordState = recordState;
			  this._schemaState = schemaState;
			  this._schemaStorage = schemaStorage;
			  this._constraintSemantics = constraintSemantics;
		 }

		 public override void Close()
		 {
			  try
			  {
					if ( _clearSchemaState )
					{
						 _schemaState.clear();
					}
			  }
			  finally
			  {
					_clearSchemaState = false;
			  }
		 }

		 public override void VisitCreatedNode( long id )
		 {
			  _recordState.nodeCreate( id );
		 }

		 public override void VisitDeletedNode( long id )
		 {
			  _recordState.nodeDelete( id );
		 }

		 public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
		 {
			  // record the state changes to be made to the store
			  _recordState.relCreate( id, type, startNode, endNode );
		 }

		 public override void VisitDeletedRelationship( long id )
		 {
			  // record the state changes to be made to the store
			  _recordState.relDelete( id );
		 }

		 public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		 {
			  removed.each( propId => _recordState.nodeRemoveProperty( id, propId ) );
			  while ( changed.MoveNext() )
			  {
					StorageProperty prop = changed.Current;
					_recordState.nodeChangeProperty( id, prop.PropertyKeyId(), prop.Value() );
			  }
			  while ( added.MoveNext() )
			  {
					StorageProperty prop = added.Current;
					_recordState.nodeAddProperty( id, prop.PropertyKeyId(), prop.Value() );
			  }
		 }

		 public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		 {
			  removed.each( relId => _recordState.relRemoveProperty( id, relId ) );
			  while ( changed.MoveNext() )
			  {
					StorageProperty prop = changed.Current;
					_recordState.relChangeProperty( id, prop.PropertyKeyId(), prop.Value() );
			  }
			  while ( added.MoveNext() )
			  {
					StorageProperty prop = added.Current;
					_recordState.relAddProperty( id, prop.PropertyKeyId(), prop.Value() );
			  }
		 }

		 public override void VisitGraphPropertyChanges( IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		 {
			  removed.each( _recordState.graphRemoveProperty );
			  while ( changed.MoveNext() )
			  {
					StorageProperty prop = changed.Current;
					_recordState.graphChangeProperty( prop.PropertyKeyId(), prop.Value() );
			  }
			  while ( added.MoveNext() )
			  {
					StorageProperty prop = added.Current;
					_recordState.graphAddProperty( prop.PropertyKeyId(), prop.Value() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void visitNodeLabelChanges(long id, final org.eclipse.collections.api.set.primitive.LongSet added, final org.eclipse.collections.api.set.primitive.LongSet removed)
		 public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
		 {
			  // record the state changes to be made to the store
			  removed.each( label => _recordState.removeLabelFromNode( label, id ) );
			  added.each( label => _recordState.addLabelToNode( label, id ) );
		 }

		 public override void VisitAddedIndex( IndexDescriptor index )
		 {
			  StoreIndexDescriptor rule = index.WithId( _schemaStorage.newRuleId() );
			  _recordState.createSchemaRule( rule );
		 }

		 public override void VisitRemovedIndex( IndexDescriptor index )
		 {
			  StoreIndexDescriptor rule;
			  Optional<string> name = index.UserSuppliedName;
			  if ( name.Present )
			  {
					string indexName = name.get();
					rule = _schemaStorage.indexGetForName( indexName );
			  }
			  else
			  {
					rule = _schemaStorage.indexGetForSchema( index, true );
					if ( rule == null )
					{
						 // Loosen the filtering a bit. The reason we do this during drop is this scenario where a uniqueness constraint creation
						 // crashed or similar, where the UNIQUE index exists, but not its constraint and so the only way to drop it
						 // (if you don't want to go the route of first creating a constraint and then drop that, where the index would be dropped along with it),
						 // is to do "DROP INDEX ON :Label(name) which has the type as GENERAL and would miss it.
						 rule = _schemaStorage.indexGetForSchema( index, false );
					}
			  }
			  if ( rule != null )
			  {
					_recordState.dropSchemaRule( rule );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitAddedConstraint(Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor constraint) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 public override void VisitAddedConstraint( ConstraintDescriptor constraint )
		 {
			  _clearSchemaState = true;
			  long constraintId = _schemaStorage.newRuleId();

			  switch ( constraint.Type() )
			  {
			  case UNIQUE:
					VisitAddedUniquenessConstraint( ( UniquenessConstraintDescriptor ) constraint, constraintId );
					break;

			  case UNIQUE_EXISTS:
					VisitAddedNodeKeyConstraint( ( NodeKeyConstraintDescriptor ) constraint, constraintId );
					break;

			  case EXISTS:
					_recordState.createSchemaRule( _constraintSemantics.createExistenceConstraint( _schemaStorage.newRuleId(), constraint ) );
					break;

			  default:
					throw new System.InvalidOperationException( constraint.Type().ToString() );
			  }
		 }

		 private void VisitAddedUniquenessConstraint( UniquenessConstraintDescriptor uniqueConstraint, long constraintId )
		 {
			  StoreIndexDescriptor indexRule = _schemaStorage.indexGetForSchema( uniqueConstraint.OwnedIndexDescriptor() );
			  _recordState.createSchemaRule( _constraintSemantics.createUniquenessConstraintRule( constraintId, uniqueConstraint, indexRule.Id ) );
			  _recordState.setConstraintIndexOwner( indexRule, constraintId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void visitAddedNodeKeyConstraint(Neo4Net.kernel.api.schema.constraints.NodeKeyConstraintDescriptor uniqueConstraint, long constraintId) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
		 private void VisitAddedNodeKeyConstraint( NodeKeyConstraintDescriptor uniqueConstraint, long constraintId )
		 {
			  StoreIndexDescriptor indexRule = _schemaStorage.indexGetForSchema( uniqueConstraint.OwnedIndexDescriptor() );
			  _recordState.createSchemaRule( _constraintSemantics.createNodeKeyConstraintRule( constraintId, uniqueConstraint, indexRule.Id ) );
			  _recordState.setConstraintIndexOwner( indexRule, constraintId );
		 }

		 public override void VisitRemovedConstraint( ConstraintDescriptor constraint )
		 {
			  _clearSchemaState = true;
			  try
			  {
					_recordState.dropSchemaRule( _schemaStorage.constraintsGetSingle( constraint ) );
			  }
			  catch ( SchemaRuleNotFoundException )
			  {
					throw new System.InvalidOperationException( "Constraint to be removed should exist, since its existence should have been validated earlier " + "and the schema should have been locked." );
			  }
			  catch ( DuplicateSchemaRuleException )
			  {
					throw new System.InvalidOperationException( "Multiple constraints found for specified label and property." );
			  }
			  if ( constraint.EnforcesUniqueness() )
			  {
					// Remove the index for the constraint as well
					VisitRemovedIndex( ( ( IndexBackedConstraintDescriptor )constraint ).ownedIndexDescriptor() );
			  }
		 }

		 public override void VisitCreatedLabelToken( long id, string name )
		 {
			  _recordState.createLabelToken( name, id );
		 }

		 public override void VisitCreatedPropertyKeyToken( long id, string name )
		 {
			  _recordState.createPropertyKeyToken( name, id );
		 }

		 public override void VisitCreatedRelationshipTypeToken( long id, string name )
		 {
			  _recordState.createRelationshipTypeToken( name, id );
		 }

	}

}