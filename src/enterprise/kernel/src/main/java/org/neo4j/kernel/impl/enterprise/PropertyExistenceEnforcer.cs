using System;
using System.Collections.Generic;

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
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using IntSet = org.eclipse.collections.api.set.primitive.IntSet;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using LabelSet = Neo4Net.Kernel.Api.Internal.LabelSet;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaProcessor = Neo4Net.Kernel.Api.Internal.Schema.SchemaProcessor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using NodePropertyExistenceException = Neo4Net.Kernel.Api.Exceptions.schema.NodePropertyExistenceException;
	using RelationshipPropertyExistenceException = Neo4Net.Kernel.Api.Exceptions.schema.RelationshipPropertyExistenceException;
	using StorageProperty = Neo4Net.Kernel.Api.StorageEngine.StorageProperty;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveArrays.union;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException.Phase.VALIDATION;

	internal class PropertyExistenceEnforcer
	{
		 internal static PropertyExistenceEnforcer GetOrCreatePropertyExistenceEnforcerFrom( StorageReader storageReader )
		 {
			  return storageReader.GetOrCreateSchemaDependantState( typeof( PropertyExistenceEnforcer ), _factory );
		 }

		 private readonly IList<LabelSchemaDescriptor> _nodeConstraints;
		 private readonly IList<RelationTypeSchemaDescriptor> _relationshipConstraints;
		 private readonly MutableLongObjectMap<int[]> _mandatoryNodePropertiesByLabel = new LongObjectHashMap<int[]>();
		 private readonly MutableLongObjectMap<int[]> _mandatoryRelationshipPropertiesByType = new LongObjectHashMap<int[]>();

		 private PropertyExistenceEnforcer( IList<LabelSchemaDescriptor> nodes, IList<RelationTypeSchemaDescriptor> rels )
		 {
			  this._nodeConstraints = nodes;
			  this._relationshipConstraints = rels;
			  foreach ( LabelSchemaDescriptor constraint in nodes )
			  {
					Update( _mandatoryNodePropertiesByLabel, constraint.LabelId, CopyAndSortPropertyIds( constraint.PropertyIds ) );
			  }
			  foreach ( RelationTypeSchemaDescriptor constraint in rels )
			  {
					Update( _mandatoryRelationshipPropertiesByType, constraint.RelTypeId, CopyAndSortPropertyIds( constraint.PropertyIds ) );
			  }
		 }

		 private static void Update( MutableLongObjectMap<int[]> map, int key, int[] sortedValues )
		 {
			  int[] current = map.get( key );
			  if ( current != null )
			  {
					sortedValues = union( current, sortedValues );
			  }
			  map.put( key, sortedValues );
		 }

		 private static int[] CopyAndSortPropertyIds( int[] propertyIds )
		 {
			  int[] values = new int[propertyIds.Length];
			  Array.Copy( propertyIds, 0, values, 0, propertyIds.Length );
			  Arrays.sort( values );
			  return values;
		 }

		 internal virtual TxStateVisitor Decorate( TxStateVisitor visitor, Read read, CursorFactory cursorFactory )
		 {
			  return new Decorator( this, visitor, read, cursorFactory );
		 }

		 private static readonly PropertyExistenceEnforcer NO_CONSTRAINTS = new PropertyExistenceEnforcerAnonymousInnerClass( emptyList(), emptyList() );

		 private class PropertyExistenceEnforcerAnonymousInnerClass : PropertyExistenceEnforcer
		 {
			 public PropertyExistenceEnforcerAnonymousInnerClass( UnknownType emptyList, UnknownType emptyList ) : base( emptyList, emptyList )
			 {
			 }

			 internal override TxStateVisitor decorate( TxStateVisitor visitor, Read read, CursorFactory cursorFactory )
			 {
				  return visitor;
			 }
		 }
		 private static readonly System.Func<StorageReader, PropertyExistenceEnforcer> _factory = storageReader =>
		 {
		  IList<LabelSchemaDescriptor> nodes = new List<LabelSchemaDescriptor>();
		  IList<RelationTypeSchemaDescriptor> relationships = new List<RelationTypeSchemaDescriptor>();
		  for ( IEnumerator<ConstraintDescriptor> constraints = storageReader.constraintsGetAll(); constraints.hasNext(); )
		  {
				ConstraintDescriptor constraint = constraints.next();
				if ( constraint.enforcesPropertyExistence() )
				{
					 constraint.schema().processWith(new SchemaProcessorAnonymousInnerClass(this, nodes, relationships));
				}
		  }
		  if ( nodes.Empty && relationships.Empty )
		  {
				return NO_CONSTRAINTS;
		  }
		  return new PropertyExistenceEnforcer( nodes, relationships );
		 };

		 private class Decorator : Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Delegator
		 {
			 private readonly PropertyExistenceEnforcer _outerInstance;

			 private class SchemaProcessorAnonymousInnerClass : SchemaProcessor
			 {
				 private readonly PropertyExistenceEnforcer _outerInstance;

				 private IList<LabelSchemaDescriptor> nodes;
				 private IList<RelationTypeSchemaDescriptor> relationships;

				 public SchemaProcessorAnonymousInnerClass( PropertyExistenceEnforcer outerInstance, IList<LabelSchemaDescriptor> nodes, IList<RelationTypeSchemaDescriptor> relationships )
				 {
					 this.outerInstance = outerInstance;
					 this.nodes = nodes;
					 this.relationships = relationships;
				 }

				 public override void processSpecific( LabelSchemaDescriptor schema )
				 {
					  nodes.add( schema );
				 }

				 public override void processSpecific( RelationTypeSchemaDescriptor schema )
				 {
					  relationships.add( schema );
				 }

				 public override void processSpecific( SchemaDescriptor schema )
				 {
					  throw new System.NotSupportedException( "General SchemaDescriptor cannot support constraints" );
				 }
			 }

			  internal readonly MutableIntSet PropertyKeyIds = new IntHashSet();
			  internal readonly Read Read;
			  internal readonly CursorFactory CursorFactory;
			  internal readonly NodeCursor NodeCursor;
			  internal readonly PropertyCursor PropertyCursor;
			  internal readonly RelationshipScanCursor RelationshipCursor;

			  internal Decorator( PropertyExistenceEnforcer outerInstance, TxStateVisitor next, Read read, CursorFactory cursorFactory ) : base( next )
			  {
				  this._outerInstance = outerInstance;
					this.Read = read;
					this.CursorFactory = cursorFactory;
					this.NodeCursor = cursorFactory.AllocateNodeCursor();
					this.PropertyCursor = cursorFactory.AllocatePropertyCursor();
					this.RelationshipCursor = cursorFactory.AllocateRelationshipScanCursor();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitNodePropertyChanges(long id, java.util.Iterator<org.Neo4Net.Kernel.Api.StorageEngine.StorageProperty> added, java.util.Iterator<org.Neo4Net.Kernel.Api.StorageEngine.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException
			  public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			  {
					ValidateNode( id );
					base.VisitNodePropertyChanges( id, added, changed, removed );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitNodeLabelChanges(long id, org.eclipse.collections.api.set.primitive.LongSet added, org.eclipse.collections.api.set.primitive.LongSet removed) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException
			  public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
			  {
					ValidateNode( id );
					base.VisitNodeLabelChanges( id, added, removed );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitCreatedRelationship(long id, int type, long startNode, long endNode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException
			  public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
			  {
					ValidateRelationship( id );
					base.VisitCreatedRelationship( id, type, startNode, endNode );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitRelPropertyChanges(long id, java.util.Iterator<org.Neo4Net.Kernel.Api.StorageEngine.StorageProperty> added, java.util.Iterator<org.Neo4Net.Kernel.Api.StorageEngine.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException
			  public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			  {
					ValidateRelationship( id );
					base.VisitRelPropertyChanges( id, added, changed, removed );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNode(long nodeId) throws org.Neo4Net.kernel.api.exceptions.schema.NodePropertyExistenceException
			  internal virtual void ValidateNode( long nodeId )
			  {
					if ( outerInstance.mandatoryNodePropertiesByLabel.Empty )
					{
						 return;
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.LabelSet labelIds;
					LabelSet labelIds;
					Read.singleNode( nodeId, NodeCursor );
					if ( NodeCursor.next() )
					{
						 labelIds = NodeCursor.labels();
						 if ( labelIds.NumberOfLabels() == 0 )
						 {
							  return;
						 }
						 PropertyKeyIds.clear();
						 NodeCursor.properties( PropertyCursor );
						 while ( PropertyCursor.next() )
						 {
							  PropertyKeyIds.add( PropertyCursor.propertyKey() );
						 }
					}
					else
					{
						 throw new System.InvalidOperationException( format( "Node %d with changes should exist.", nodeId ) );
					}

					outerInstance.validateNodeProperties( nodeId, labelIds, PropertyKeyIds );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateRelationship(long id) throws org.Neo4Net.kernel.api.exceptions.schema.RelationshipPropertyExistenceException
			  internal virtual void ValidateRelationship( long id )
			  {
					if ( outerInstance.mandatoryRelationshipPropertiesByType.Empty )
					{
						 return;
					}

					int relationshipType;
					int[] required;
					Read.singleRelationship( id, RelationshipCursor );
					if ( RelationshipCursor.next() )
					{
						 relationshipType = RelationshipCursor.type();
						 required = outerInstance.mandatoryRelationshipPropertiesByType.get( relationshipType );
						 if ( required == null )
						 {
							  return;
						 }
						 PropertyKeyIds.clear();
						 RelationshipCursor.properties( PropertyCursor );
						 while ( PropertyCursor.next() )
						 {
							  PropertyKeyIds.add( PropertyCursor.propertyKey() );
						 }
					}
					else
					{
						 throw new System.InvalidOperationException( format( "Relationship %d with changes should exist.", id ) );
					}

					foreach ( int mandatory in required )
					{
						 if ( !PropertyKeyIds.contains( mandatory ) )
						 {
							  outerInstance.failRelationship( id, relationshipType, mandatory );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNodeProperties(long id, org.Neo4Net.Kernel.Api.Internal.LabelSet labelIds, org.eclipse.collections.api.set.primitive.IntSet propertyKeyIds) throws org.Neo4Net.kernel.api.exceptions.schema.NodePropertyExistenceException
		 private void ValidateNodeProperties( long id, LabelSet labelIds, IntSet propertyKeyIds )
		 {
			  int numberOfLabels = labelIds.NumberOfLabels();
			  if ( numberOfLabels > _mandatoryNodePropertiesByLabel.size() )
			  {
					for ( MutableLongIterator labels = _mandatoryNodePropertiesByLabel.Keys.longIterator(); labels.hasNext(); )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long label = labels.next();
						 long label = labels.next();
						 if ( labelIds.Contains( toIntExact( label ) ) )
						 {
							  ValidateNodeProperties( id, label, _mandatoryNodePropertiesByLabel.get( label ), propertyKeyIds );
						 }
					}
			  }
			  else
			  {
					for ( int i = 0; i < numberOfLabels; i++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long label = labelIds.label(i);
						 long label = labelIds.Label( i );
						 int[] keys = _mandatoryNodePropertiesByLabel.get( label );
						 if ( keys != null )
						 {
							  ValidateNodeProperties( id, label, keys, propertyKeyIds );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNodeProperties(long id, long label, int[] requiredKeys, org.eclipse.collections.api.set.primitive.IntSet propertyKeyIds) throws org.Neo4Net.kernel.api.exceptions.schema.NodePropertyExistenceException
		 private void ValidateNodeProperties( long id, long label, int[] requiredKeys, IntSet propertyKeyIds )
		 {
			  foreach ( int key in requiredKeys )
			  {
					if ( !propertyKeyIds.contains( key ) )
					{
						 FailNode( id, label, key );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void failNode(long id, long label, int propertyKey) throws org.Neo4Net.kernel.api.exceptions.schema.NodePropertyExistenceException
		 private void FailNode( long id, long label, int propertyKey )
		 {
			  foreach ( LabelSchemaDescriptor constraint in _nodeConstraints )
			  {
					if ( constraint.LabelId == label && Contains( constraint.PropertyIds, propertyKey ) )
					{
						 throw new NodePropertyExistenceException( constraint, VALIDATION, id );
					}
			  }
			  throw new System.InvalidOperationException( format( "Node constraint for label=%d, propertyKey=%d should exist.", label, propertyKey ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void failRelationship(long id, int relationshipType, int propertyKey) throws org.Neo4Net.kernel.api.exceptions.schema.RelationshipPropertyExistenceException
		 private void FailRelationship( long id, int relationshipType, int propertyKey )
		 {
			  foreach ( RelationTypeSchemaDescriptor constraint in _relationshipConstraints )
			  {
					if ( constraint.RelTypeId == relationshipType && Contains( constraint.PropertyIds, propertyKey ) )
					{
						 throw new RelationshipPropertyExistenceException( constraint, VALIDATION, id );
					}
			  }
			  throw new System.InvalidOperationException( format( "Relationship constraint for relationshipType=%d, propertyKey=%d should exist.", relationshipType, propertyKey ) );
		 }

		 private bool Contains( int[] list, int value )
		 {
			  foreach ( int x in list )
			  {
					if ( value == x )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}