using System;
using System.Collections.Generic;

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
namespace Neo4Net.Storageengine.Api
{

	using PrimitiveLongResourceIterator = Neo4Net.Collection.PrimitiveLongResourceIterator;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using EntityNotFoundException = Neo4Net.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using Register = Neo4Net.Register.Register;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY;

	/// <summary>
	/// Implementation of <seealso cref="StorageReader"/> with focus on making testing the storage read cursors easy without resorting to mocking.
	/// </summary>
	public class StubStorageCursors : StorageReader
	{
		private bool InstanceFieldsInitialized = false;

		public StubStorageCursors()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_propertyKeyTokenHolder = new DelegatingTokenHolder( name => toIntExact( _nextTokenId.AndIncrement ), TYPE_PROPERTY_KEY );
		}

		 private const long NO_ID = -1;

		 private readonly AtomicLong _nextPropertyId = new AtomicLong();
		 private readonly AtomicLong _nextTokenId = new AtomicLong();
		 private TokenHolder _propertyKeyTokenHolder;

		 private readonly IDictionary<long, NodeData> _nodeData = new Dictionary<long, NodeData>();
		 private readonly IDictionary<string, long> _labelByName = new Dictionary<string, long>();
		 private readonly IDictionary<long, string> _labelById = new Dictionary<long, string>();
		 private readonly IDictionary<string, long> _propertyKeyByName = new Dictionary<string, long>();
		 private readonly IDictionary<long, string> _propertyKeyById = new Dictionary<long, string>();
		 private readonly IDictionary<string, long> _relationshipTypeByName = new Dictionary<string, long>();
		 private readonly IDictionary<long, string> _relationshipTypeById = new Dictionary<long, string>();
		 private readonly IDictionary<long, PropertyData> _propertyData = new Dictionary<long, PropertyData>();
		 private readonly IDictionary<long, RelationshipData> _relationshipData = new Dictionary<long, RelationshipData>();

		 public virtual void WithNode( long id )
		 {
			  WithNode( id, new long[0] );
		 }

		 public virtual void WithNode( long id, long[] labels )
		 {
			  WithNode( id, labels, Collections.emptyMap() );
		 }

		 public virtual void WithNode( long id, long[] labels, IDictionary<string, Value> properties )
		 {
			  _nodeData[id] = new NodeData( id, labels, NO_ID, PropertyIdOf( properties ) );
		 }

		 public virtual void WithRelationship( long id, long startNode, int type, long endNode )
		 {
			  WithRelationship( id, startNode, type, endNode, Collections.emptyMap() );
		 }

		 public virtual void WithRelationship( long id, long startNode, int type, long endNode, IDictionary<string, Value> properties )
		 {
			  _relationshipData[id] = new RelationshipData( id, startNode, type, endNode, PropertyIdOf( properties ) );
		 }

		 private long PropertyIdOf( IDictionary<string, Value> properties )
		 {
			  if ( properties.Count == 0 )
			  {
					return NO_ID;
			  }
			  long propertyId = _nextPropertyId.incrementAndGet();
			  _propertyData[propertyId] = new PropertyData( properties );
			  properties.Keys.forEach( _propertyKeyTokenHolder.getOrCreateId );
			  return propertyId;
		 }

		 public override void Acquire()
		 {
		 }

		 public override void Release()
		 {
		 }

		 public override void Close()
		 {
		 }

		 public virtual TokenHolder PropertyKeyTokenHolder()
		 {
			  return _propertyKeyTokenHolder;
		 }

		 public virtual LabelScanReader LabelScanReader
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Not implemented yet" );
			 }
		 }

		 public override IndexReader GetIndexReader( IndexDescriptor index )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IndexReader GetFreshIndexReader( IndexDescriptor index )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long ReserveNode()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long ReserveRelationship()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int ReserveLabelTokenId()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int ReservePropertyKeyTokenId()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int ReserveRelationshipTypeTokenId()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public virtual long GraphPropertyReference
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Not implemented yet" );
			 }
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForLabel( int labelId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForRelationshipType( int relationshipType )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override CapableIndexDescriptor IndexGetForName( string name )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetAll()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetRelatedToProperty( int propertyId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long? IndexGetOwningUniquenessConstraintId( IndexDescriptor index )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override bool ConstraintExists( ConstraintDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetAll()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override PrimitiveLongResourceIterator NodesGetForLabel( int labelId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override CapableIndexDescriptor IndexGetForSchema( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override InternalIndexState IndexGetState( IndexDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override IndexReference IndexReference( IndexDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override PopulationProgress IndexGetPopulationProgress( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override string IndexGetFailure( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Exception> void relationshipVisit(long relationshipId, RelationshipVisitor<EXCEPTION> relationshipVisitor) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException, EXCEPTION
		 public override void RelationshipVisit<EXCEPTION>( long relationshipId, RelationshipVisitor<EXCEPTION> relationshipVisitor ) where EXCEPTION : Exception
		 {
			  RelationshipData data = this._relationshipData[relationshipId];
			  if ( data == null )
			  {
					throw new EntityNotFoundException( EntityType.Relationship, relationshipId );
			  }
			  relationshipVisitor.Visit( relationshipId, data.Type, data.StartNode, data.EndNode );
		 }

		 public override void ReleaseNode( long id )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override void ReleaseRelationship( long id )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long CountsForNode( int labelId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long CountsForRelationship( int startLabelId, int typeId, int endLabelId )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long IndexSize( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override double IndexUniqueValuesPercentage( SchemaDescriptor descriptor )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long NodesGetCount()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override long RelationshipsGetCount()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int LabelCount()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int PropertyKeyCount()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override int RelationshipTypeCount()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize( SchemaDescriptor descriptor, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexSample( SchemaDescriptor descriptor, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override bool NodeExists( long id )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override bool RelationshipExists( long id )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override T GetOrCreateSchemaDependantState<T>( Type type, System.Func<StorageReader, T> factory )
		 {
				 type = typeof( T );
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override StorageNodeCursor AllocateNodeCursor()
		 {
			  return new StubStorageNodeCursor( this );
		 }

		 public override StoragePropertyCursor AllocatePropertyCursor()
		 {
			  return new StubStoragePropertyCursor( this );
		 }

		 public override StorageRelationshipGroupCursor AllocateRelationshipGroupCursor()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override StorageRelationshipTraversalCursor AllocateRelationshipTraversalCursor()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 public override StorageRelationshipScanCursor AllocateRelationshipScanCursor()
		 {
			  return new StubStorageRelationshipScanCursor( this );
		 }

		 public override StorageSchemaReader SchemaSnapshot()
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }

		 private class NodeData
		 {
			  internal readonly long Id;
			  internal readonly long[] Labels;
			  internal readonly long FirstRelationship;
			  internal readonly long PropertyId;

			  internal NodeData( long id, long[] labels, long firstRelationship, long propertyId )
			  {
					this.Id = id;
					this.Labels = labels;
					this.FirstRelationship = firstRelationship;
					this.PropertyId = propertyId;
			  }
		 }

		 private class RelationshipData
		 {
			  internal readonly long Id;
			  internal readonly long StartNode;
			  internal readonly int Type;
			  internal readonly long EndNode;
			  internal readonly long PropertyId;

			  internal RelationshipData( long id, long startNode, int type, long endNode, long propertyId )
			  {
					this.Id = id;
					this.StartNode = startNode;
					this.Type = type;
					this.EndNode = endNode;
					this.PropertyId = propertyId;
			  }
		 }

		 private class PropertyData
		 {
			  internal readonly IDictionary<string, Value> Properties;

			  internal PropertyData( IDictionary<string, Value> properties )
			  {
					this.Properties = properties;
			  }
		 }

		 private class StubStorageNodeCursor : StorageNodeCursor
		 {
			 private readonly StubStorageCursors _outerInstance;

			 public StubStorageNodeCursor( StubStorageCursors outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long NextConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal NodeData CurrentConflict;
			  internal IEnumerator<long> Iterator;

			  public override void Scan()
			  {
					this.Iterator = outerInstance.nodeData.Keys.GetEnumerator();
			  }

			  public override void Single( long reference )
			  {
					this.Iterator = null;
					this.NextConflict = reference;
			  }

			  public override long EntityReference()
			  {
					return CurrentConflict.id;
			  }

			  public override long[] Labels()
			  {
					return CurrentConflict.labels;
			  }

			  public override bool HasLabel( int label )
			  {
					return contains( CurrentConflict.labels, label );
			  }

			  public override bool HasProperties()
			  {
					return CurrentConflict.propertyId != NO_ID;
			  }

			  public override long RelationshipGroupReference()
			  {
					return CurrentConflict.firstRelationship;
			  }

			  public override long AllRelationshipsReference()
			  {
					return CurrentConflict.firstRelationship;
			  }

			  public override long PropertiesReference()
			  {
					return CurrentConflict.propertyId;
			  }

			  public override bool Next()
			  {
					if ( Iterator != null )
					{
						 // scan
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 CurrentConflict = Iterator.hasNext() ? outerInstance.nodeData[Iterator.next()] : null;
						 return true;
					}
					else
					{
						 if ( NextConflict != NO_ID )
						 {
							  CurrentConflict = outerInstance.nodeData[NextConflict];
							  NextConflict = NO_ID;
							  return true;
						 }
					}
					return false;
			  }

			  public virtual long Current
			  {
				  set
				  {
						throw new System.NotSupportedException( "Not implemented yet" );
				  }
			  }

			  public override void Reset()
			  {
					Iterator = null;
					CurrentConflict = null;
			  }

			  public virtual bool Dense
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override void Close()
			  {
					Reset();
			  }
		 }

		 private class StubStorageRelationshipScanCursor : StorageRelationshipScanCursor
		 {
			 private readonly StubStorageCursors _outerInstance;

			 public StubStorageRelationshipScanCursor( StubStorageCursors outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal IEnumerator<long> Iterator;
			  internal RelationshipData Current;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long NextConflict;

			  public override void Scan()
			  {
					Scan( -1 );
			  }

			  public override void Scan( int type )
			  {
					Iterator = outerInstance.relationshipData.Keys.GetEnumerator();
					NextConflict = NO_ID;
			  }

			  public override void Single( long reference )
			  {
					Iterator = null;
					NextConflict = reference;
			  }

			  public override long EntityReference()
			  {
					return Current.id;
			  }

			  public override int Type()
			  {
					return Current.type;
			  }

			  public override bool HasProperties()
			  {
					return Current.propertyId != NO_ID;
			  }

			  public override long SourceNodeReference()
			  {
					return Current.startNode;
			  }

			  public override long TargetNodeReference()
			  {
					return Current.endNode;
			  }

			  public override long PropertiesReference()
			  {
					return Current.propertyId;
			  }

			  public override void Visit( long relationshipId, int typeId, long startNodeId, long endNodeId )
			  {
					throw new System.NotSupportedException( "Not implemented yet" );
			  }

			  public override bool Next()
			  {
					if ( Iterator != null )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( !Iterator.hasNext() )
						 {
							  return false;
						 }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 NextConflict = Iterator.next();
					}

					if ( NextConflict != NO_ID )
					{
						 Current = outerInstance.relationshipData[NextConflict];
						 NextConflict = NO_ID;
						 return true;
					}
					return false;
			  }

			  public override void Reset()
			  {
					Current = null;
					NextConflict = NO_ID;
			  }

			  public override void Close()
			  {
					Reset();
			  }
		 }

		 private class StubStoragePropertyCursor : StoragePropertyCursor
		 {
			 private readonly StubStorageCursors _outerInstance;

			 public StubStoragePropertyCursor( StubStorageCursors outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal KeyValuePair<string, Value> Current;
			  internal IEnumerator<KeyValuePair<string, Value>> Iterator;

			  public override void Init( long reference )
			  {
					PropertyData properties = _outerInstance.propertyData[reference];
					Iterator = properties != null ? properties.Properties.SetOfKeyValuePairs().GetEnumerator() : emptyIterator();
			  }

			  public override void Close()
			  {
			  }

			  public override int PropertyKey()
			  {
					return outerInstance.propertyKeyTokenHolder.GetOrCreateId( Current.Key );
			  }

			  public override ValueGroup PropertyType()
			  {
					return Current.Value.valueGroup();
			  }

			  public override Value PropertyValue()
			  {
					return Current.Value;
			  }

			  public override void Reset()
			  {
			  }

			  public override bool Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( Iterator.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Current = Iterator.next();
						 return true;
					}
					return false;
			  }
		 }
	}

}