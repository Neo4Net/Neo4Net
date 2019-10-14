using System;
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
namespace Neo4Net.Server.rest.domain
{

	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using PropertyValueException = Neo4Net.Server.rest.web.PropertyValueException;

	/// <summary>
	/// Responsible for setting properties on primitive types.
	/// </summary>
	public class PropertySettingStrategy
	{
		 private readonly GraphDatabaseAPI _db;

		 public PropertySettingStrategy( GraphDatabaseAPI db )
		 {
			  this._db = db;
		 }

		 /// <summary>
		 /// Set all properties on an entity, deleting any properties that existed on the entity but not in the
		 /// provided map.
		 /// </summary>
		 /// <param name="entity"> </param>
		 /// <param name="properties"> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAllProperties(org.neo4j.graphdb.PropertyContainer entity, java.util.Map<String, Object> properties) throws org.neo4j.server.rest.web.PropertyValueException
		 public virtual void SetAllProperties( PropertyContainer entity, IDictionary<string, object> properties )
		 {
			  IDictionary<string, object> propsToSet = properties == null ? new Dictionary<string, object>() : properties;

			  using ( Transaction tx = _db.beginTx() )
			  {
					SetProperties( entity, properties );
					EnsureHasOnlyTheseProperties( entity, propsToSet.Keys );

					tx.Success();
			  }
		 }

		 private void EnsureHasOnlyTheseProperties( PropertyContainer entity, ISet<string> propertiesThatShouldExist )
		 {
			  foreach ( string entityPropertyKey in entity.PropertyKeys )
			  {
					if ( !propertiesThatShouldExist.Contains( entityPropertyKey ) )
					{
						 entity.RemoveProperty( entityPropertyKey );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setProperties(org.neo4j.graphdb.PropertyContainer entity, java.util.Map<String, Object> properties) throws org.neo4j.server.rest.web.PropertyValueException
		 public virtual void SetProperties( PropertyContainer entity, IDictionary<string, object> properties )
		 {
			  if ( properties != null )
			  {
					using ( Transaction tx = _db.beginTx() )
					{
						 foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
						 {
							  SetProperty( entity, property.Key, property.Value );
						 }
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setProperty(org.neo4j.graphdb.PropertyContainer entity, String key, Object value) throws org.neo4j.server.rest.web.PropertyValueException
		 public virtual void SetProperty( PropertyContainer entity, string key, object value )
		 {
			  if ( value is System.Collections.ICollection )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (((java.util.Collection<?>) value).size() == 0)
					if ( ( ( ICollection<object> ) value ).Count == 0 )
					{
						 // Special case: Trying to set an empty array property. We cannot determine the type
						 // of the collection now, so we fall back to checking if there already is a collection
						 // on the entity, and either leave it intact if it is empty, or set it to an empty collection
						 // of the same type as the original
						 object currentValue = entity.GetProperty( key, null );
						 if ( currentValue != null && currentValue.GetType().IsArray )
						 {
							  if ( Array.getLength( currentValue ) == 0 )
							  {
									// Ok, leave it this way
									return;
							  }
							  value = EmptyArrayOfType( currentValue.GetType().GetElementType() );
						 }
						 else
						 {
							  throw new PropertyValueException( "Unable to set property '" + key + "' to an empty array, " + "because, since there are no values of any type in it, " + "and no pre-existing collection to infer type from, it is not possible " + "to determine what type of array to store." );
						 }
					}
					else
					{
						 // Non-empty collection
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: value = convertToNativeArray((java.util.Collection<?>) value);
						 value = ConvertToNativeArray( ( ICollection<object> ) value );
					}
			  }

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						entity.SetProperty( key, value );
						tx.Success();
					  }
			  }
			  catch ( System.ArgumentException )
			  {
					throw new PropertyValueException( "Could not set property \"" + key + "\", unsupported type: " + value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object convert(Object value) throws org.neo4j.server.rest.web.PropertyValueException
		 public virtual object Convert( object value )
		 {
			  if ( !( value is System.Collections.ICollection ) )
			  {
					return value;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (((java.util.Collection<?>) value).size() == 0)
			  if ( ( ( ICollection<object> ) value ).Count == 0 )
			  {
					throw new PropertyValueException( "Unable to convert '" + value + "' to an empty array, " + "because, since there are no values of any type in it, " + "and no pre-existing collection to infer type from, it is not possible " + "to determine what type of array to store." );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return convertToNativeArray((java.util.Collection<?>) value);
			  return ConvertToNativeArray( ( ICollection<object> ) value );
		 }

		 private object EmptyArrayOfType( Type cls )
		 {
			 return Array.CreateInstance( cls, 0 );
		 }

		 public static object ConvertToNativeArray<T1>( ICollection<T1> collection )
		 {
			  object[] array = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> objects = collection.iterator();
			  IEnumerator<object> objects = collection.GetEnumerator();
			  for ( int i = 0; objects.MoveNext(); i++ )
			  {
					object @object = objects.Current;
					if ( array == null )
					{
						 array = ( object[] ) Array.CreateInstance( @object.GetType(), collection.Count );
					}
					array[i] = @object;
			  }
			  return array;
		 }
	}

}