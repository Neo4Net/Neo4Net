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
namespace Org.Neo4j.Server.plugins
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public abstract class ParameterList
	{
		 private readonly IDictionary<string, object> _data;

		 public ParameterList( IDictionary<string, object> data )
		 {
			  this._data = data;
		 }

		 private abstract class Converter<T>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract T convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException;
			  internal abstract T Convert( GraphDatabaseAPI graphDb, object value );

			  internal abstract T[] NewArray( int size );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> T[] getList(String name, org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Converter<T> converter) throws org.neo4j.server.rest.repr.BadInputException
		 private T[] GetList<T>( string name, GraphDatabaseAPI graphDb, Converter<T> converter )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  IList<T> result = new List<T>();
			  if ( value is object[] )
			  {
					foreach ( object element in ( object[] ) value )
					{
						 result.Add( converter.Convert( graphDb, element ) );
					}
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (value instanceof Iterable<?>)
			  else if ( value is IEnumerable<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Object element : (Iterable<?>) value)
					foreach ( object element in ( IEnumerable<object> ) value )
					{
						 result.Add( converter.Convert( graphDb, element ) );
					}
			  }
			  else
			  {
					throw new BadInputException( name + " is not a list" );
			  }
			  return result.toArray( converter.NewArray( result.Count ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getString(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual string GetString( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertString( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String[] getStringList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual string[] GetStringList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass( this ) );
		 }

		 private class ConverterAnonymousInnerClass : Converter<string>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override string convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertString( value );
			 }

			 internal override string[] newArray( int size )
			 {
				  return new string[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract String convertString(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract string ConvertString( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int> getInteger(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual int? GetInteger( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertInteger( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int>[] getIntegerList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual int?[] GetIntegerList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass2( this ) );
		 }

		 private class ConverterAnonymousInnerClass2 : Converter<int>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass2( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Integer convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Integer convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertInteger( value ).Value;
			 }

			 internal override Integer[] newArray( int size )
			 {
				  return new int?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<int> convertInteger(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract int? ConvertInteger( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> getLong(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual long? GetLong( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertLong( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long>[] getLongList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual long?[] GetLongList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass3( this ) );
		 }

		 private class ConverterAnonymousInnerClass3 : Converter<long>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass3( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Long convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Long convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertLong( value ).Value;
			 }

			 internal override Long[] newArray( int size )
			 {
				  return new long?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<long> convertLong(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract long? ConvertLong( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<sbyte> getByte(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual sbyte? GetByte( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertByte( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<sbyte>[] getByteList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual sbyte?[] GetByteList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass4( this ) );
		 }

		 private class ConverterAnonymousInnerClass4 : Converter<sbyte>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass4( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Byte convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Byte convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertByte( value );
			 }

			 internal override Byte[] newArray( int size )
			 {
				  return new sbyte?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<sbyte> convertByte(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract sbyte? ConvertByte( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<char> getCharacter(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual char? GetCharacter( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertCharacter( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<char>[] getCharacterList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual char?[] GetCharacterList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass5( this ) );
		 }

		 private class ConverterAnonymousInnerClass5 : Converter<char>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass5( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Character convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Character convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertCharacter( value ).Value;
			 }

			 internal override Character[] newArray( int size )
			 {
				  return new char?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<char> convertCharacter(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract char? ConvertCharacter( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<bool> getBoolean(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual bool? GetBoolean( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertBoolean( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<bool>[] getBooleanList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual bool?[] GetBooleanList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass6( this ) );
		 }

		 private class ConverterAnonymousInnerClass6 : Converter<bool>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass6( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Boolean convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Boolean convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertBoolean( value ).Value;
			 }

			 internal override Boolean[] newArray( int size )
			 {
				  return new bool?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<bool> convertBoolean(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract bool? ConvertBoolean( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<short> getShort(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual short? GetShort( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertShort( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<short>[] getShortList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual short?[] GetShortList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass7( this ) );
		 }

		 private class ConverterAnonymousInnerClass7 : Converter<short>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass7( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Short convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Short convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertShort( value ).Value;
			 }

			 internal override Short[] newArray( int size )
			 {
				  return new short?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<short> convertShort(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract short? ConvertShort( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<float> getFloat(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual float? GetFloat( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertFloat( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<float>[] getFloatList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual float?[] GetFloatList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass8( this ) );
		 }

		 private class ConverterAnonymousInnerClass8 : Converter<float>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass8( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Float convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Float convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertFloat( value ).Value;
			 }

			 internal override Float[] newArray( int size )
			 {
				  return new float?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<float> convertFloat(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract float? ConvertFloat( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<double> getDouble(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual double? GetDouble( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertDouble( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<double>[] getDoubleList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual double?[] GetDoubleList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass9( this ) );
		 }

		 private class ConverterAnonymousInnerClass9 : Converter<double>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass9( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Double convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Double convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertDouble( value ).Value;
			 }

			 internal override Double[] newArray( int size )
			 {
				  return new double?[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract System.Nullable<double> convertDouble(Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract double? ConvertDouble( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Node getNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual Node GetNode( GraphDatabaseAPI graphDb, string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertNode( graphDb, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Node[] getNodeList(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual Node[] GetNodeList( GraphDatabaseAPI graphDb, string name )
		 {
			  return GetList( name, graphDb, new ConverterAnonymousInnerClass10( this, graphDb ) );
		 }

		 private class ConverterAnonymousInnerClass10 : Converter<Node>
		 {
			 private readonly ParameterList _outerInstance;

			 private GraphDatabaseAPI _graphDb;

			 public ConverterAnonymousInnerClass10( ParameterList outerInstance, GraphDatabaseAPI graphDb )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Node convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Node convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertNode( graphDb, value );
			 }

			 internal override Node[] newArray( int size )
			 {
				  return new Node[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.neo4j.graphdb.Node convertNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract Node ConvertNode( GraphDatabaseAPI graphDb, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Relationship getRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual Relationship GetRelationship( GraphDatabaseAPI graphDb, string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertRelationship( graphDb, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Relationship[] getRelationshipList(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual Relationship[] GetRelationshipList( GraphDatabaseAPI graphDb, string name )
		 {
			  return GetList( name, graphDb, new ConverterAnonymousInnerClass11( this, graphDb ) );
		 }

		 private class ConverterAnonymousInnerClass11 : Converter<Relationship>
		 {
			 private readonly ParameterList _outerInstance;

			 private GraphDatabaseAPI _graphDb;

			 public ConverterAnonymousInnerClass11( ParameterList outerInstance, GraphDatabaseAPI graphDb )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Relationship convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override Relationship convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertRelationship( graphDb, value );
			 }

			 internal override Relationship[] newArray( int size )
			 {
				  return new Relationship[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.neo4j.graphdb.Relationship convertRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException;
		 [Obsolete]
		 protected internal abstract Relationship ConvertRelationship( GraphDatabaseAPI graphDb, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI getUri(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual URI GetUri( string name )
		 {
			  object value = _data[name];
			  if ( value == null )
			  {
					return null;
			  }
			  return ConvertURI( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI[] getUriList(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual URI[] GetUriList( string name )
		 {
			  return GetList( name, null, new ConverterAnonymousInnerClass12( this ) );
		 }

		 private class ConverterAnonymousInnerClass12 : Converter<URI>
		 {
			 private readonly ParameterList _outerInstance;

			 public ConverterAnonymousInnerClass12( ParameterList outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: URI convert(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws org.neo4j.server.rest.repr.BadInputException
			 internal override URI convert( GraphDatabaseAPI graphDb, object value )
			 {
				  return outerInstance.ConvertURI( value );
			 }

			 internal override URI[] newArray( int size )
			 {
				  return new URI[size];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.net.URI convertURI(Object value) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 protected internal virtual URI ConvertURI( object value )
		 {
			  try
			  {
					return new URI( ConvertString( value ) );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map getMap(String name) throws org.neo4j.server.rest.repr.BadInputException
		 [Obsolete]
		 public virtual System.Collections.IDictionary GetMap( string name )
		 {
			  object value = _data[name];
			  if ( value is System.Collections.IDictionary )
			  {
					return ( System.Collections.IDictionary ) value;
			  }
			  else if ( value is string )
			  {
					throw new BadInputException( "Maps encoded as Strings not supported" );
			  }
			  return null;
		 }
	}

}