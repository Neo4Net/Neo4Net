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
namespace Org.Neo4j.Kernel.impl.core
{

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using DatabaseShutdownException = Org.Neo4j.Graphdb.DatabaseShutdownException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using NotInTransactionException = Org.Neo4j.Graphdb.NotInTransactionException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using PropertyKeyIdNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using AutoIndexingKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using Org.Neo4j.Storageengine.Api;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	public class RelationshipProxy : Relationship, RelationshipVisitor<Exception>
	{
		 private readonly EmbeddedProxySPI _spi;
		 private long _id = AbstractBaseRecord.NO_ID;
		 private long _startNode = AbstractBaseRecord.NO_ID;
		 private long _endNode = AbstractBaseRecord.NO_ID;
		 private int _type;

		 public RelationshipProxy( EmbeddedProxySPI spi, long id, long startNode, int type, long endNode )
		 {
			  this._spi = spi;
			  Visit( id, type, startNode, endNode );
		 }

		 public RelationshipProxy( EmbeddedProxySPI spi, long id )
		 {
			  this._spi = spi;
			  this._id = id;
		 }

		 public static bool IsDeletedInCurrentTransaction( Relationship relationship )
		 {
			  if ( relationship is RelationshipProxy )
			  {
					RelationshipProxy proxy = ( RelationshipProxy ) relationship;
					KernelTransaction ktx = proxy._spi.kernelTransaction();
					using ( Statement ignore = ktx.AcquireStatement() )
					{
						 return ktx.DataRead().relationshipDeletedInTransaction(proxy._id);
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void visit(long id, int type, long startNode, long endNode) throws RuntimeException
		 public override void Visit( long id, int type, long startNode, long endNode )
		 {
			  this._id = id;
			  this._type = type;
			  this._startNode = startNode;
			  this._endNode = endNode;
		 }

		 public virtual bool InitializeData()
		 {
			  // It enough to check only start node, since it's absence will indicate that data was not yet loaded.
			  if ( _startNode == AbstractBaseRecord.NO_ID )
			  {
					KernelTransaction transaction = _spi.kernelTransaction();
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
						 transaction.DataRead().singleRelationship(_id, relationships);
						 // At this point we don't care if it is there or not just load what we got.
						 bool wasPresent = relationships.Next();
						 this._type = relationships.Type();
						 this._startNode = relationships.SourceNodeReference();
						 this._endNode = relationships.TargetNodeReference();
						 // But others might care, e.g. the Bolt server needs to know for serialisation purposes.
						 return wasPresent;
					}
			  }
			  return true;
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return _id;
			 }
		 }

		 private int TypeId()
		 {
			  InitializeData();
			  return _type;
		 }

		 private long SourceId()
		 {
			  InitializeData();
			  return _startNode;
		 }

		 private long TargetId()
		 {
			  InitializeData();
			  return _endNode;
		 }

		 public virtual GraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return _spi.GraphDatabase;
			 }
		 }

		 public override void Delete()
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  try
			  {
					bool deleted = transaction.DataWrite().relationshipDelete(_id);
					if ( !deleted )
					{
						 throw new NotFoundException( "Unable to delete relationship[" + Id + "] since it is already deleted." );
					}
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while deleting the relationship: " + e.Message, e );
			  }
		 }

		 public virtual Node[] Nodes
		 {
			 get
			 {
				  _spi.assertInUnterminatedTransaction();
				  return new Node[]{ _spi.newNodeProxy( SourceId() ), _spi.newNodeProxy(TargetId()) };
			 }
		 }

		 public override Node GetOtherNode( Node node )
		 {
			  _spi.assertInUnterminatedTransaction();
			  return _spi.newNodeProxy( GetOtherNodeId( node.Id ) );
		 }

		 public virtual Node StartNode
		 {
			 get
			 {
				  _spi.assertInUnterminatedTransaction();
				  return _spi.newNodeProxy( SourceId() );
			 }
		 }

		 public virtual Node EndNode
		 {
			 get
			 {
				  _spi.assertInUnterminatedTransaction();
				  return _spi.newNodeProxy( TargetId() );
			 }
		 }

		 public virtual long StartNodeId
		 {
			 get
			 {
				  return SourceId();
			 }
		 }

		 public virtual long EndNodeId
		 {
			 get
			 {
				  return TargetId();
			 }
		 }

		 public override long GetOtherNodeId( long id )
		 {
			  long start = SourceId();
			  long end = TargetId();
			  if ( start == id )
			  {
					return end;
			  }
			  if ( end == id )
			  {
					return start;
			  }
			  throw new NotFoundException( "Node[" + id + "] not connected to this relationship[" + Id + "]" );
		 }

		 public virtual RelationshipType Type
		 {
			 get
			 {
				  _spi.assertInUnterminatedTransaction();
				  return _spi.getRelationshipTypeById( TypeId() );
			 }
		 }

		 public virtual IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  KernelTransaction transaction = _spi.kernelTransaction();
				  IList<string> keys = new List<string>();
				  try
				  {
						RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
						PropertyCursor properties = transaction.AmbientPropertyCursor();
						SingleRelationship( transaction, relationships );
						TokenRead token = transaction.TokenRead();
						relationships.Properties( properties );
						while ( properties.Next() )
						{
							 keys.Add( token.PropertyKeyName( properties.PropertyKey() ) );
						}
				  }
				  catch ( PropertyKeyIdNotFoundKernelException e )
				  {
						throw new System.InvalidOperationException( "Property key retrieved through kernel API should exist.", e );
				  }
				  return keys;
			 }
		 }

		 public override IDictionary<string, object> GetProperties( params string[] keys )
		 {
			  Objects.requireNonNull( keys, "Properties keys should be not null array." );

			  if ( keys.Length == 0 )
			  {
					return Collections.emptyMap();
			  }

			  KernelTransaction transaction = _spi.kernelTransaction();

			  int itemsToReturn = keys.Length;
			  TokenRead token = transaction.TokenRead();

			  //Find ids, note we are betting on that the number of keys
			  //is small enough not to use a set here.
			  int[] propertyIds = new int[itemsToReturn];
			  for ( int i = 0; i < itemsToReturn; i++ )
			  {
					string key = keys[i];
					if ( string.ReferenceEquals( key, null ) )
					{
						 throw new System.NullReferenceException( string.Format( "Key {0:D} was null", i ) );
					}
					propertyIds[i] = token.PropertyKey( key );
			  }

			  IDictionary<string, object> properties = new Dictionary<string, object>( itemsToReturn );
			  RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
			  PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
			  SingleRelationship( transaction, relationships );
			  relationships.Properties( propertyCursor );
			  int propertiesToFind = itemsToReturn;
			  while ( propertiesToFind > 0 && propertyCursor.Next() )
			  {
					//Do a linear check if this is a property we are interested in.
					int currentKey = propertyCursor.PropertyKey();
					for ( int i = 0; i < itemsToReturn; i++ )
					{
						 if ( propertyIds[i] == currentKey )
						 {
							  properties[keys[i]] = propertyCursor.PropertyValue().asObjectCopy();
							  propertiesToFind--;
							  break;
						 }
					}
			  }
			  return properties;
		 }

		 public virtual IDictionary<string, object> AllProperties
		 {
			 get
			 {
				  KernelTransaction transaction = _spi.kernelTransaction();
				  IDictionary<string, object> properties = new Dictionary<string, object>();
   
				  try
				  {
						RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
						PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
						TokenRead token = transaction.TokenRead();
						SingleRelationship( transaction, relationships );
						relationships.Properties( propertyCursor );
						while ( propertyCursor.Next() )
						{
							 properties[token.PropertyKeyName( propertyCursor.PropertyKey() )] = propertyCursor.PropertyValue().asObjectCopy();
						}
				  }
				  catch ( PropertyKeyIdNotFoundKernelException e )
				  {
						throw new System.InvalidOperationException( "Property key retrieved through kernel API should exist.", e );
				  }
				  return properties;
			 }
		 }

		 public override object GetProperty( string key )
		 {
			  if ( null == key )
			  {
					throw new System.ArgumentException( "(null) property key is not allowed" );
			  }
			  KernelTransaction transaction = _spi.kernelTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					throw new NotFoundException( format( "No such property, '%s'.", key ) );
			  }

			  RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  SingleRelationship( transaction, relationships );
			  relationships.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 Value value = properties.PropertyValue();
						 if ( value == Values.NO_VALUE )
						 {
							  throw new NotFoundException( format( "No such property, '%s'.", key ) );
						 }
						 return value.AsObjectCopy();
					}
			  }
			  throw new NotFoundException( format( "No such property, '%s'.", key ) );
		 }

		 public override object GetProperty( string key, object defaultValue )
		 {
			  if ( null == key )
			  {
					throw new System.ArgumentException( "(null) property key is not allowed" );
			  }
			  KernelTransaction transaction = _spi.kernelTransaction();
			  RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					return defaultValue;
			  }
			  SingleRelationship( transaction, relationships );
			  relationships.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 Value value = properties.PropertyValue();
						 return value == Values.NO_VALUE ? defaultValue : value.AsObjectCopy();
					}
			  }
			  return defaultValue;
		 }

		 public override bool HasProperty( string key )
		 {
			  if ( null == key )
			  {
					return false;
			  }

			  KernelTransaction transaction = _spi.kernelTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					return false;
			  }

			  RelationshipScanCursor relationships = transaction.AmbientRelationshipCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  SingleRelationship( transaction, relationships );
			  relationships.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void SetProperty( string key, object value )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  int propertyKeyId;
			  try
			  {
					propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new System.ArgumentException( format( "Invalid property key '%s'.", key ), e );
			  }

			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						transaction.DataWrite().relationshipSetProperty(_id, propertyKeyId, Values.of(value, false));
					  }
			  }
			  catch ( System.ArgumentException e )
			  {
					// Trying to set an illegal value is a critical error - fail this transaction
					_spi.failTransaction();
					throw e;
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while setting property: " + e.Message, e );
			  }

		 }

		 public override object RemoveProperty( string key )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
						return transaction.DataWrite().relationshipRemoveProperty(_id, propertyKeyId).asObjectCopy();
					  }
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( e );
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new System.ArgumentException( format( "Invalid property key '%s'.", key ), e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while removing property: " + e.Message, e );
			  }
		 }

		 public override bool IsType( RelationshipType type )
		 {
			  _spi.assertInUnterminatedTransaction();
			  return _spi.getRelationshipTypeById( TypeId() ).name().Equals(type.Name());
		 }

		 public virtual int CompareTo( object rel )
		 {
			  Relationship r = ( Relationship ) rel;
			  return Long.compare( this.Id, r.Id );
		 }

		 public override bool Equals( object o )
		 {
			  return o is Relationship && this.Id == ( ( Relationship ) o ).Id;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( ( ( long )( ( ulong )Id >> 32 ) ) ^ Id );
		 }

		 public override string ToString()
		 {
			  string relType;
			  try
			  {
					relType = _spi.getRelationshipTypeById( TypeId() ).name();
					return format( "(%d)-[%s,%d]->(%d)", SourceId(), relType, Id, TargetId() );
			  }
			  catch ( Exception e ) when ( e is NotInTransactionException || e is DatabaseShutdownException )
			  {
					// We don't keep the rel-name lookup if the database is shut down. Source ID and target ID also requires
					// database access in a transaction. However, failing on toString would be uncomfortably evil, so we fall
					// back to noting the relationship type id.
			  }
			  relType = "RELTYPE(" + _type + ")";
			  return format( "(?)-[%s,%d]->(?)", relType, Id );
		 }

		 private void SingleRelationship( KernelTransaction transaction, RelationshipScanCursor relationships )
		 {
			  transaction.DataRead().singleRelationship(_id, relationships);
			  if ( !relationships.Next() )
			  {
					throw new NotFoundException( new EntityNotFoundException( EntityType.RELATIONSHIP, _id ) );
			  }
		 }
	}

}