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
namespace Neo4Net.Kernel.impl.core
{

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException;
	using IllegalTokenNameException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IllegalTokenNameException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public class GraphPropertiesProxy : GraphProperties
	{
		 private readonly EmbeddedProxySPI _actions;

		 public GraphPropertiesProxy( EmbeddedProxySPI actions )
		 {
			  this._actions = actions;
		 }

		 public virtual IGraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return _actions.GraphDatabase;
			 }
		 }

		 public override bool HasProperty( string key )
		 {
			  if ( null == key )
			  {
					return false;
			  }

			  KernelTransaction transaction = SafeAcquireTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
			  {
					return false;
			  }

			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  transaction.DataRead().graphProperties(properties);
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override object GetProperty( string key )
		 {
			  if ( null == key )
			  {
					throw new System.ArgumentException( "(null) property key is not allowed" );
			  }
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
			  {
					throw new NotFoundException( format( "No such property, '%s'.", key ) );
			  }

			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  transaction.DataRead().graphProperties(properties);

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
			  KernelTransaction transaction = SafeAcquireTransaction();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
			  {
					return defaultValue;
			  }
			  transaction.DataRead().graphProperties(properties);
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

		 public override void SetProperty( string key, object value )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
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
						transaction.DataWrite().graphSetProperty(propertyKeyId, Values.of(value, false));
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override object RemoveProperty( string key )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
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
						return transaction.DataWrite().graphRemoveProperty(propertyKeyId).asObjectCopy();
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public virtual IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
				  IList<string> keys = new List<string>();
				  try
				  {
						PropertyCursor properties = transaction.AmbientPropertyCursor();
						TokenRead token = transaction.TokenRead();
						transaction.DataRead().graphProperties(properties);
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

		 public override IDictionary<string, object> GetProperties( params string[] names )
		 {
			  Objects.requireNonNull( names, "Properties keys should be not null array." );

			  if ( names.Length == 0 )
			  {
					return Collections.emptyMap();
			  }

			  KernelTransaction transaction = SafeAcquireTransaction();

			  int itemsToReturn = names.Length;
			  IDictionary<string, object> properties = new Dictionary<string, object>( itemsToReturn );
			  TokenRead token = transaction.TokenRead();

			  //Find ids, note we are betting on that the number of keys
			  //is small enough not to use a set here.
			  int[] propertyIds = new int[itemsToReturn];
			  for ( int i = 0; i < itemsToReturn; i++ )
			  {
					string key = names[i];
					if ( string.ReferenceEquals( key, null ) )
					{
						 throw new System.NullReferenceException( string.Format( "Key {0:D} was null", i ) );
					}
					propertyIds[i] = token.PropertyKey( key );
			  }

			  PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
			  transaction.DataRead().graphProperties(propertyCursor);
			  int propertiesToFind = itemsToReturn;
			  while ( propertiesToFind > 0 && propertyCursor.Next() )
			  {
					//Do a linear check if this is a property we are interested in.
					int currentKey = propertyCursor.PropertyKey();
					for ( int i = 0; i < itemsToReturn; i++ )
					{
						 if ( propertyIds[i] == currentKey )
						 {
							  properties[names[i]] = propertyCursor.PropertyValue().asObjectCopy();
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
				  KernelTransaction transaction = SafeAcquireTransaction();
				  IDictionary<string, object> properties = new Dictionary<string, object>();
   
				  try
				  {
						PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
						TokenRead token = transaction.TokenRead();
						transaction.DataRead().graphProperties(propertyCursor);
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

		 public override bool Equals( object o )
		 {
			  // Yeah, this is breaking transitive equals, but should be OK anyway.
			  // Also, we're checking == (not .equals) on GDS since that seems to be what the tests are asserting
			  return o is GraphPropertiesProxy && _actions.GraphDatabase == ( ( GraphPropertiesProxy )o )._actions.GraphDatabase;
		 }

		 public override int GetHashCode()
		 {
			  return _actions.GraphDatabase.GetHashCode();
		 }

		 private KernelTransaction SafeAcquireTransaction()
		 {
			  KernelTransaction transaction = _actions.kernelTransaction();
			  if ( transaction.Terminated )
			  {
					Status terminationReason = transaction.ReasonIfTerminated.orElse( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
					throw new TransactionTerminatedException( terminationReason );
			  }
			  return transaction;
		 }
	}

}