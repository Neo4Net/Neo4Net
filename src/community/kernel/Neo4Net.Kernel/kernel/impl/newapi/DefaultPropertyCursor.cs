using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using AssertOpen = Neo4Net.Kernel.api.AssertOpen;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using IPropertyContainerState = Neo4Net.Storageengine.Api.txstate.PropertyContainerState;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	public class DefaultPropertyCursor : PropertyCursor
	{
		 private Read _read;
		 private StoragePropertyCursor _storeCursor;
		 private IPropertyContainerState _propertiesState;
		 private IEnumerator<StorageProperty> _txStateChangedProperties;
		 private StorageProperty _txStateValue;
		 private AssertOpen _assertOpen;
		 private readonly DefaultCursors _pool;

		 internal DefaultPropertyCursor( DefaultCursors pool, StoragePropertyCursor storeCursor )
		 {
			  this._pool = pool;
			  this._storeCursor = storeCursor;
		 }

		 internal virtual void InitNode( long nodeReference, long reference, Read read, AssertOpen assertOpen )
		 {
			  Debug.Assert( nodeReference != NO_ID );

			  Init( reference, read, assertOpen );

			  // Transaction state
			  if ( read.HasTxStateWithChanges() )
			  {
					this._propertiesState = read.TxState().getNodeState(nodeReference);
					this._txStateChangedProperties = this._propertiesState.addedAndChangedProperties();
			  }
		 }

		 internal virtual void InitRelationship( long relationshipReference, long reference, Read read, AssertOpen assertOpen )
		 {
			  Debug.Assert( relationshipReference != NO_ID );

			  Init( reference, read, assertOpen );

			  // Transaction state
			  if ( read.HasTxStateWithChanges() )
			  {
					this._propertiesState = read.TxState().getRelationshipState(relationshipReference);
					this._txStateChangedProperties = this._propertiesState.addedAndChangedProperties();
			  }
		 }

		 internal virtual void InitGraph( long reference, Read read, AssertOpen assertOpen )
		 {
			  Init( reference, read, assertOpen );

			  // Transaction state
			  if ( read.HasTxStateWithChanges() )
			  {
					this._propertiesState = read.TxState().GraphState;
					if ( this._propertiesState != null )
					{
						 this._txStateChangedProperties = this._propertiesState.addedAndChangedProperties();
					}
			  }
		 }

		 private void Init( long reference, Read read, AssertOpen assertOpen )
		 {
			  this._assertOpen = assertOpen;
			  this._read = read;
			  this._storeCursor.init( reference );
		 }

		 public override bool Next()
		 {
			  bool hasNext;
			  do
			  {
					hasNext = InnerNext();
			  } while ( hasNext && !Allowed( PropertyKey() ) );
			  return hasNext;
		 }

		 private bool Allowed( int propertyKey )
		 {
			  return _read.ktx.securityContext().mode().allowsPropertyReads(propertyKey);
		 }

		 private bool InnerNext()
		 {
			  if ( _txStateChangedProperties != null )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( _txStateChangedProperties.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 _txStateValue = _txStateChangedProperties.next();
						 return true;
					}
					else
					{
						 _txStateChangedProperties = null;
						 _txStateValue = null;
					}
			  }

			  while ( _storeCursor.next() )
			  {
					bool skip = _propertiesState != null && _propertiesState.isPropertyChangedOrRemoved( _storeCursor.propertyKey() );
					if ( !skip )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					_propertiesState = null;
					_txStateChangedProperties = null;
					_txStateValue = null;
					_read = null;
					_storeCursor.reset();

					_pool.accept( this );
			  }
		 }

		 public override int PropertyKey()
		 {
			  if ( _txStateValue != null )
			  {
					return _txStateValue.propertyKeyId();
			  }
			  return _storeCursor.propertyKey();
		 }

		 public override ValueGroup PropertyType()
		 {
			  if ( _txStateValue != null )
			  {
					return _txStateValue.value().valueGroup();
			  }
			  return _storeCursor.propertyType();
		 }

		 public override Value PropertyValue()
		 {
			  if ( _txStateValue != null )
			  {
					return _txStateValue.value();
			  }

			  Value value = _storeCursor.propertyValue();

			  _assertOpen.assertOpen();
			  return value;
		 }

		 public override void WriteTo<E>( ValueWriter<E> target ) where E : Exception
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool BooleanValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override string StringValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long LongValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override double DoubleValue()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( long value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( double value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueEqualTo( string value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueMatches( Pattern regex )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThan( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThan( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThan( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThan( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThanOrEqualTo( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueGreaterThanOrEqualTo( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThanOrEqualTo( long number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool ValueLessThanOrEqualTo( double number )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _read == null;
			 }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "PropertyCursor[closed state]";
			  }
			  else
			  {
					return "PropertyCursor[id=" + PropertyKey() +
							 ", " + _storeCursor.ToString() + " ]";
			  }
		 }

		 public virtual void Release()
		 {
			  _storeCursor.close();
		 }
	}

}