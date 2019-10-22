using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.CodeGen
{

	using AnyValue = Neo4Net.Values.AnyValue;


	public class TypeReference
	{
		 public static Bound Extending( Type type )
		 {
			  return Extending( TypeReferenceConflict( type ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Bound extending(final TypeReference type)
		 public static Bound Extending( TypeReference type )
		 {
			  return new BoundAnonymousInnerClass( type );
		 }

		 private class BoundAnonymousInnerClass : Bound
		 {
			 private Neo4Net.CodeGen.TypeReference _type;

			 public BoundAnonymousInnerClass( Neo4Net.CodeGen.TypeReference type ) : base( type )
			 {
				 this._type = type;
			 }

			 public override TypeReference extendsBound()
			 {
				  return _type;
			 }

			 public override TypeReference superBound()
			 {
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static TypeReference TypeReferenceConflict( Type type )
		 {
			  if ( type == typeof( void ) )
			  {
					return VoidConflict;
			  }
			  if ( type == typeof( object ) )
			  {
					return Object;
			  }
			  string packageName = "";
			  string name;
			  string declaringClassName = "";

			  Type innerType = type.IsArray ? type.GetElementType() : type;

			  if ( innerType.IsPrimitive )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					name = innerType.FullName;
					switch ( name )
					{
					case "boolean":
						 return type.IsArray ? BooleanArray : Boolean;
					case "int":
						 return type.IsArray ? IntArray : Int;
					case "long":
						 return type.IsArray ? LongArray : Long;
					case "double":
						 return type.IsArray ? DoubleArray : Double;
					default:
						 // continue through the normal path
				break;
					}
			  }
			  else
			  {
				packageName = innerType.Assembly.GetName().Name;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					string canonicalName = innerType.FullName;
					Type declaringClass = innerType.DeclaringType;
					if ( declaringClass != null )
					{
						 declaringClassName = declaringClass.Name;
						 name = canonicalName.Substring( packageName.Length + declaringClassName.Length + 2 );
					}
					else
					{
						 name = canonicalName.Substring( packageName.Length + 1 );
					}
			  }
			  return new TypeReference( packageName, name, type.IsPrimitive, type.IsArray, false, declaringClassName, type.Modifiers );
		 }

		 public static TypeReference TypeParameter( string name )
		 {
			  return new TypeReference( "", name, false, false, true, "", Modifier.PUBLIC );
		 }

		 public static TypeReference ArrayOf( TypeReference type )
		 {
			  return new TypeReference( type._packageName, type._name, false, true, false, type._declaringClassName, type._modifiers );
		 }

		 public static TypeReference ParameterizedType( Type @base, params Type[] parameters )
		 {
			  return ParameterizedType( TypeReferenceConflict( @base ), TypeReferences( parameters ) );
		 }

		 public static TypeReference ParameterizedType( Type @base, params TypeReference[] parameters )
		 {
			  return ParameterizedType( TypeReferenceConflict( @base ), parameters );
		 }

		 public static TypeReference ParameterizedType( TypeReference @base, params TypeReference[] parameters )
		 {
			  return new TypeReference( @base._packageName, @base._name, false, @base.Array, false, @base._declaringClassName, @base._modifiers, parameters );
		 }

		 public static TypeReference[] TypeReferences( Type first, Type[] more )
		 {
			  TypeReference[] result = new TypeReference[more.Length + 1];
			  result[0] = TypeReferenceConflict( first );
			  for ( int i = 0; i < more.Length; i++ )
			  {
					result[i + 1] = TypeReferenceConflict( more[i] );
			  }
			  return result;
		 }

		 public static TypeReference[] TypeReferences( Type[] types )
		 {
			  TypeReference[] result = new TypeReference[types.Length];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = TypeReferenceConflict( types[i] );
			  }
			  return result;
		 }

		 private readonly string _packageName;
		 private readonly string _name;
		 private readonly TypeReference[] _parameters;
		 private readonly bool _isPrimitive;
		 private readonly bool _isArray;
		 private readonly bool _isTypeParameter;
		 private readonly string _declaringClassName;
		 private readonly int _modifiers;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public static readonly TypeReference VoidConflict = new TypeReference( "", "void", true, false, false, "", typeof( void ).Modifiers );
		 public static readonly TypeReference Object = new TypeReference( "java.lang", "Object", false, false, false, "", typeof( object ).Modifiers );
		 public static readonly TypeReference Boolean = new TypeReference( "", "boolean", true, false, false, "", typeof( bool ).Modifiers );
		 public static readonly TypeReference Int = new TypeReference( "", "int", true, false, false, "", typeof( int ).Modifiers );
		 public static readonly TypeReference Long = new TypeReference( "", "long", true, false, false, "", typeof( long ).Modifiers );
		 public static readonly TypeReference Double = new TypeReference( "", "double", true, false, false, "", typeof( double ).Modifiers );
		 public static readonly TypeReference BooleanArray = new TypeReference( "", "boolean", false, true, false, "", typeof( bool ).Modifiers );
		 public static readonly TypeReference IntArray = new TypeReference( "", "int", false, true, false, "", typeof( int ).Modifiers );
		 public static readonly TypeReference LongArray = new TypeReference( "", "long", false, true, false, "", typeof( long ).Modifiers );
		 public static readonly TypeReference DoubleArray = new TypeReference( "", "double", false, true, false, "", typeof( double ).Modifiers );
		 public static readonly TypeReference Value = new TypeReference( "org.Neo4Net.values", "AnyValue", false, false, false, "", typeof( AnyValue ).Modifiers );
		 internal static readonly TypeReference[] NoTypes = new TypeReference[0];

		 internal TypeReference( string packageName, string name, bool isPrimitive, bool isArray, bool isTypeParameter, string declaringClassName, int modifiers, params TypeReference[] parameters )
		 {
			  this._packageName = packageName;
			  this._name = name;
			  this._isPrimitive = isPrimitive;
			  this._isArray = isArray;
			  this._isTypeParameter = isTypeParameter;
			  this._declaringClassName = declaringClassName;
			  this._modifiers = modifiers;
			  this._parameters = parameters;
		 }

		 public virtual string PackageName()
		 {
			  return _packageName;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual string SimpleName()
		 {
			  return _isArray ? _name + "[]" : _name;
		 }

		 public virtual bool Primitive
		 {
			 get
			 {
				  return _isPrimitive;
			 }
		 }

		 public virtual bool TypeParameter
		 {
			 get
			 {
				  return _isTypeParameter;
			 }
		 }

		 public virtual bool Generic
		 {
			 get
			 {
				  return _parameters == null || _parameters.Length > 0;
			 }
		 }

		 public virtual IList<TypeReference> Parameters()
		 {
			  return unmodifiableList( asList( _parameters ) );
		 }

		 public virtual string FullName()
		 {
			  return WriteTo( new StringBuilder() ).ToString();
		 }

		 public virtual bool Array
		 {
			 get
			 {
				  return _isArray;
			 }
		 }

		 public virtual bool Void
		 {
			 get
			 {
				  return this == VoidConflict;
			 }
		 }

		 public virtual bool InnerClass
		 {
			 get
			 {
				  return _declaringClassName.Length > 0;
			 }
		 }

		 public virtual string DeclaringClassName()
		 {
			  return _declaringClassName;
		 }

		 public virtual int Modifiers()
		 {
			  return _modifiers;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  TypeReference reference = ( TypeReference ) o;

			  if ( _isPrimitive != reference._isPrimitive )
			  {
					return false;
			  }
			  if ( _isArray != reference._isArray )
			  {
					return false;
			  }
			  if ( _isTypeParameter != reference._isTypeParameter )
			  {
					return false;
			  }
			  if ( _modifiers != reference._modifiers )
			  {
					return false;
			  }
			  if ( !string.ReferenceEquals( _packageName, null ) ?!_packageName.Equals( reference._packageName ) :!string.ReferenceEquals( reference._packageName, null ) )
			  {
					return false;
			  }
			  if ( !string.ReferenceEquals( _name, null ) ?!_name.Equals( reference._name ) :!string.ReferenceEquals( reference._name, null ) )
			  {
					return false;
			  }
			  // Probably incorrect - comparing Object[] arrays with Arrays.equals
			  if ( !Arrays.Equals( _parameters, reference._parameters ) )
			  {
					return false;
			  }
			  return !string.ReferenceEquals( _declaringClassName, null ) ? _declaringClassName.Equals( reference._declaringClassName ) : string.ReferenceEquals( reference._declaringClassName, null );

		 }

		 public override int GetHashCode()
		 {
			  int result = !string.ReferenceEquals( _packageName, null ) ? _packageName.GetHashCode() : 0;
			  result = 31 * result + ( !string.ReferenceEquals( _name, null ) ? _name.GetHashCode() : 0 );
			  result = 31 * result + Arrays.GetHashCode( _parameters );
			  result = 31 * result + ( _isPrimitive ? 1 : 0 );
			  result = 31 * result + ( _isArray ? 1 : 0 );
			  result = 31 * result + ( _isTypeParameter ? 1 : 0 );
			  result = 31 * result + ( !string.ReferenceEquals( _declaringClassName, null ) ? _declaringClassName.GetHashCode() : 0 );
			  result = 31 * result + _modifiers;
			  return result;
		 }

		 public override string ToString()
		 {
			  return WriteTo( ( new StringBuilder() ).Append("TypeReference[") ).Append(']').ToString();
		 }

		 internal virtual StringBuilder WriteTo( StringBuilder result )
		 {
			  if ( _packageName.Length > 0 )
			  {
					result.Append( _packageName ).Append( '.' );
			  }
			  if ( _declaringClassName.Length > 0 )
			  {
					result.Append( _declaringClassName ).Append( '.' );
			  }
			  result.Append( _name );
			  if ( _isArray )
			  {
					result.Append( "[]" );
			  }
			  if ( !( _parameters == null || _parameters.Length == 0 ) )
			  {
					result.Append( '<' );
					string sep = "";
					foreach ( TypeReference parameter in _parameters )
					{
						 parameter.WriteTo( result.Append( sep ) );
						 sep = ",";
					}
					result.Append( '>' );
			  }
			  return result;
		 }

		 public abstract class Bound
		 {
			  internal readonly TypeReference Type;

			  internal Bound( TypeReference type )
			  {
					this.Type = type;
			  }

			  public abstract TypeReference ExtendsBound();

			  public abstract TypeReference SuperBound();
		 }
	}

}