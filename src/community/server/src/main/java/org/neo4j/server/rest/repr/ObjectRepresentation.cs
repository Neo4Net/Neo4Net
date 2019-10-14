using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Server.rest.repr
{

	public abstract class ObjectRepresentation : MappingRepresentation
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_serialization = _serialization( this.GetType() );
		}

		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 protected internal class Mapping : System.Attribute
		 {
			 private readonly ObjectRepresentation _outerInstance;

			 public Mapping;
			 {
			 }

			  internal string value;

			 public Mapping( public Mapping, String value )
			 {
				 this.Mapping = Mapping;
				 this.value = value;
			 }
		 }

		 private static readonly ConcurrentDictionary<Type, IDictionary<string, PropertyGetter>> _serializations = new ConcurrentDictionary<Type, IDictionary<string, PropertyGetter>>();

		 private IDictionary<string, PropertyGetter> _serialization;

		 internal ObjectRepresentation( RepresentationType type ) : base( type )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
		 }

		 public ObjectRepresentation( string type ) : base( type )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
		 }

		 private static IDictionary<string, PropertyGetter> Serialization( Type type )
		 {
			  IDictionary<string, PropertyGetter> serialization = _serializations.computeIfAbsent( type, ObjectRepresentation.buildSerialization );
			  return serialization;
		 }

		 private static IDictionary<string, PropertyGetter> BuildSerialization( Type type )
		 {
			  IDictionary<string, PropertyGetter> serialization;
			  serialization = new Dictionary<string, PropertyGetter>();
			  foreach ( System.Reflection.MethodInfo method in type.GetMethods() )
			  {
					Mapping property = method.getAnnotation( typeof( Mapping ) );
					if ( property != null )
					{
						 serialization[property.value()] = Getter(method);
					}
			  }
			  return serialization;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static PropertyGetter getter(final Method method)
		 private static PropertyGetter Getter( System.Reflection.MethodInfo method )
		 {
			  // If this turns out to be a bottle neck we could use a byte code
			  // generation library, such as ASM, instead of reflection.
			  return new PropertyGetterAnonymousInnerClass( method );
		 }

		 private class PropertyGetterAnonymousInnerClass : PropertyGetter
		 {
			 private System.Reflection.MethodInfo _method;

			 public PropertyGetterAnonymousInnerClass( System.Reflection.MethodInfo method ) : base( method )
			 {
				 this._method = method;
			 }

			 internal override object get( ObjectRepresentation @object )
			 {
				  Exception e;
				  try
				  {
						return _method.invoke( @object );
				  }
				  catch ( InvocationTargetException ex )
				  {
						e = ex.TargetException;
						if ( e is Exception )
						{
							 throw ( Exception ) e;
						}
						else if ( e is Exception )
						{
							 throw ( Exception ) e;
						}
				  }
				  catch ( Exception ex )
				  {
						e = ex;
				  }
				  throw new System.InvalidOperationException( "Serialization failure", e );
			 }
		 }

		 private abstract class PropertyGetter
		 {
			  internal PropertyGetter( System.Reflection.MethodInfo method )
			  {
					if ( method.ParameterTypes.length != 0 )
					{
						 throw new System.InvalidOperationException( "Property getter method may not have any parameters." );
					}
					if ( !method.ReturnType.IsAssignableFrom( typeof( Representation ) ) )
					{
						 throw new System.InvalidOperationException( "Property getter must return Representation object." );
					}
			  }

			  internal virtual void PutTo( MappingSerializer serializer, ObjectRepresentation @object, string key )
			  {
					object value = Get( @object );
					if ( value != null )
					{
						 ( ( Representation ) value ).PutTo( serializer, key );
					}
			  }

			  internal abstract object Get( ObjectRepresentation @object );
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  foreach ( KeyValuePair<string, PropertyGetter> property in _serialization.SetOfKeyValuePairs() )
			  {
					property.Value.putTo( serializer, this, property.Key );
			  }
			  ExtraData( serializer );
		 }

		 internal virtual void ExtraData( MappingSerializer serializer )
		 {
		 }
	}

}