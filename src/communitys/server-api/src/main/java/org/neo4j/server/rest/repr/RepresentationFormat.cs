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
namespace Neo4Net.Server.rest.repr
{

	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using ParameterList = Neo4Net.Server.plugins.ParameterList;
	using NodeNotFoundException = Neo4Net.Server.rest.web.NodeNotFoundException;
	using RelationshipNotFoundException = Neo4Net.Server.rest.web.RelationshipNotFoundException;

	/// <summary>
	/// Implementations of this class must be stateless. Implementations of this
	/// class must have a public no arguments constructor.
	/// </summary>
	public abstract class RepresentationFormat : InputFormat
	{
		public abstract URI ReadUri( string input );
		public abstract IList<object> ReadList( string input );
		public abstract IDictionary<string, object> ReadMap( string input, params string[] requiredKeys );
		public abstract object ReadValue( string input );
		 internal readonly MediaType MediaType;

		 public RepresentationFormat( MediaType mediaType )
		 {
			  this.MediaType = mediaType;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}[{1}]", this.GetType().Name, MediaType );
		 }

		 internal virtual string SerializeValue( RepresentationType type, object value )
		 {
			  return SerializeValue( type.ValueName, value );
		 }

		 protected internal abstract string SerializeValue( string type, object value );

		 internal virtual ListWriter SerializeList( RepresentationType type )
		 {
			  if ( string.ReferenceEquals( type.ListName, null ) )
			  {
					throw new System.InvalidOperationException( "Invalid list type: " + type );
			  }
			  return SerializeList( type.ListName );
		 }

		 protected internal abstract ListWriter SerializeList( string type );

		 internal virtual MappingWriter SerializeMapping( RepresentationType type )
		 {
			  return SerializeMapping( type.ValueName );
		 }

		 protected internal abstract MappingWriter SerializeMapping( string type );

		 /// <summary>
		 /// Will be invoked (when serialization is done) with the result retrieved
		 /// from invoking <seealso cref="serializeList(string)"/>, it is therefore safe for
		 /// this method to convert the <seealso cref="ListWriter"/> argument to the
		 /// implementation class returned by <seealso cref="serializeList(string)"/>.
		 /// </summary>
		 protected internal abstract string Complete( ListWriter serializer );

		 /// <summary>
		 /// Will be invoked (when serialization is done) with the result retrieved
		 /// from invoking <seealso cref="serializeMapping(string)"/>, it is therefore safe for
		 /// this method to convert the <seealso cref="MappingWriter"/> argument to the
		 /// implementation class returned by <seealso cref="serializeMapping(string)"/>.
		 /// </summary>
		 protected internal abstract string Complete( MappingWriter serializer );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.plugins.ParameterList readParameterList(String input) throws BadInputException
		 public override ParameterList ReadParameterList( string input )
		 {
			  return new ParameterListAnonymousInnerClass( this, ReadMap( input ) );
		 }

		 private class ParameterListAnonymousInnerClass : ParameterList
		 {
			 private readonly RepresentationFormat _outerInstance;

			 public ParameterListAnonymousInnerClass( RepresentationFormat outerInstance, UnknownType readMap ) : base( readMap )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<bool> convertBoolean(Object value) throws BadInputException
			 protected internal override bool? convertBoolean( object value )
			 {
				  return _outerInstance.convertBoolean( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<sbyte> convertByte(Object value) throws BadInputException
			 protected internal override sbyte? convertByte( object value )
			 {
				  return _outerInstance.convertByte( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<char> convertCharacter(Object value) throws BadInputException
			 protected internal override char? convertCharacter( object value )
			 {
				  return _outerInstance.convertCharacter( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<double> convertDouble(Object value) throws BadInputException
			 protected internal override double? convertDouble( object value )
			 {
				  return _outerInstance.convertDouble( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<float> convertFloat(Object value) throws BadInputException
			 protected internal override float? convertFloat( object value )
			 {
				  return _outerInstance.convertFloat( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<int> convertInteger(Object value) throws BadInputException
			 protected internal override int? convertInteger( object value )
			 {
				  return _outerInstance.convertInteger( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<long> convertLong(Object value) throws BadInputException
			 protected internal override long? convertLong( object value )
			 {
				  return _outerInstance.convertLong( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Node convertNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws BadInputException
			 protected internal override Node convertNode( GraphDatabaseAPI graphDb, object value )
			 {
				  return _outerInstance.convertNode( graphDb, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Relationship convertRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws BadInputException
			 protected internal override Relationship convertRelationship( GraphDatabaseAPI graphDb, object value )
			 {
				  return _outerInstance.convertRelationship( graphDb, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<short> convertShort(Object value) throws BadInputException
			 protected internal override short? convertShort( object value )
			 {
				  return _outerInstance.convertShort( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected String convertString(Object value) throws BadInputException
			 protected internal override string convertString( object value )
			 {
				  return _outerInstance.convertString( value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.net.URI convertURI(Object value) throws BadInputException
			 protected internal override URI convertURI( object value )
			 {
				  return _outerInstance.convertURI( value );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Relationship convertRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws BadInputException
		 protected internal virtual Relationship ConvertRelationship( GraphDatabaseAPI graphDb, object value )
		 {
			  if ( value is Relationship )
			  {
					return ( Relationship ) value;
			  }
			  if ( value is URI )
			  {
					try
					{
						 return GetRelationship( graphDb, ( URI ) value );
					}
					catch ( RelationshipNotFoundException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  if ( value is string )
			  {
					try
					{
						 return GetRelationship( graphDb, ( string ) value );
					}
					catch ( RelationshipNotFoundException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Node convertNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object value) throws BadInputException
		 protected internal virtual Node ConvertNode( GraphDatabaseAPI graphDb, object value )
		 {
			  if ( value is Node )
			  {
					return ( Node ) value;
			  }
			  if ( value is URI )
			  {
					try
					{
						 return GetNode( graphDb, ( URI ) value );
					}
					catch ( NodeNotFoundException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  if ( value is string )
			  {
					try
					{
						 return GetNode( graphDb, ( string ) value );
					}
					catch ( NodeNotFoundException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Node getNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String value) throws BadInputException, org.neo4j.server.rest.web.NodeNotFoundException
		 protected internal virtual Node GetNode( GraphDatabaseAPI graphDb, string value )
		 {
			  try
			  {
					return GetNode( graphDb, new URI( value ) );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Node getNode(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, java.net.URI uri) throws BadInputException, org.neo4j.server.rest.web.NodeNotFoundException
		 protected internal virtual Node GetNode( GraphDatabaseAPI graphDb, URI uri )
		 {
			  try
			  {
					return graphDb.GetNodeById( ExtractId( uri ) );
			  }
			  catch ( NotFoundException e )
			  {
					throw new NodeNotFoundException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long extractId(java.net.URI uri) throws BadInputException
		 private long ExtractId( URI uri )
		 {
			  string[] path = uri.Path.Split( "/" );
			  try
			  {
					return long.Parse( path[path.Length - 1] );
			  }
			  catch ( System.FormatException e )
			  {
					throw new BadInputException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Relationship getRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, String value) throws BadInputException, org.neo4j.server.rest.web.RelationshipNotFoundException
		 private Relationship GetRelationship( GraphDatabaseAPI graphDb, string value )
		 {
			  try
			  {
					return GetRelationship( graphDb, new URI( value ) );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.Relationship getRelationship(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, java.net.URI uri) throws BadInputException, org.neo4j.server.rest.web.RelationshipNotFoundException
		 protected internal virtual Relationship GetRelationship( GraphDatabaseAPI graphDb, URI uri )
		 {
			  try
			  {
					return graphDb.GetRelationshipById( ExtractId( uri ) );
			  }
			  catch ( NotFoundException e )
			  {
					throw new RelationshipNotFoundException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.net.URI convertURI(Object value) throws BadInputException
		 protected internal virtual URI ConvertURI( object value )
		 {
			  if ( value is URI )
			  {
					return ( URI ) value;
			  }
			  if ( value is string )
			  {
					try
					{
						 return new URI( ( string ) value );
					}
					catch ( URISyntaxException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected String convertString(Object value) throws BadInputException
		 protected internal virtual string ConvertString( object value )
		 {
			  if ( value is string )
			  {
					return ( string ) value;
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<short> convertShort(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual short? ConvertShort( object value )
		 {
			  if ( value is Number && !( value is float? || value is double? ) )
			  {
					short primitive = ( ( Number ) value ).shortValue();
					if ( primitive != ( ( Number ) value ).longValue() )
					{
						 throw new BadInputException( "Input did not fit in short" );
					}
					return primitive;
			  }
			  if ( value is string )
			  {
					try
					{
						 return short.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<long> convertLong(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual long? ConvertLong( object value )
		 {
			  if ( value is Number && !( value is float? || value is double? ) )
			  {
					long primitive = ( ( Number ) value ).longValue();
					return primitive;
			  }
			  if ( value is string )
			  {
					try
					{
						 return long.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<int> convertInteger(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual int? ConvertInteger( object value )
		 {
			  if ( value is Number && !( value is float? || value is double? ) )
			  {
					int primitive = ( ( Number ) value ).intValue();
					if ( primitive != ( ( Number ) value ).longValue() )
					{
						 throw new BadInputException( "Input did not fit in int" );
					}
					return primitive;
			  }
			  if ( value is string )
			  {
					try
					{
						 return int.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<float> convertFloat(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual float? ConvertFloat( object value )
		 {
			  if ( value is Number )
			  {
					return ( ( Number ) value ).floatValue();
			  }
			  if ( value is string )
			  {
					try
					{
						 return float.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<double> convertDouble(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual double? ConvertDouble( object value )
		 {
			  if ( value is Number )
			  {
					return ( ( Number ) value ).doubleValue();
			  }
			  if ( value is string )
			  {
					try
					{
						 return double.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<char> convertCharacter(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual char? ConvertCharacter( object value )
		 {
			  if ( value is char? )
			  {
					return ( char? ) value;
			  }
			  if ( value is Number )
			  {
					int primitive = ( ( Number ) value ).intValue();
					if ( primitive != ( ( Number ) value ).longValue() || (primitive > 0xFFFF) )
					{
						 throw new BadInputException( "Input did not fit in char" );
					}
					return Convert.ToChar( ( char ) primitive );
			  }
			  if ( value is string && ( ( string ) value ).Length == 1 )
			  {
					return ( ( string ) value )[0];
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<sbyte> convertByte(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual sbyte? ConvertByte( object value )
		 {
			  if ( value is Number )
			  {
					sbyte primitive = ( ( Number ) value ).byteValue();
					if ( primitive != ( ( Number ) value ).longValue() )
					{
						 throw new BadInputException( "Input did not fit in byte" );
					}
					return primitive;
			  }
			  if ( value is string )
			  {
					try
					{
						 return sbyte.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected System.Nullable<bool> convertBoolean(Object value) throws BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual bool? ConvertBoolean( object value )
		 {
			  if ( value is bool? )
			  {
					return ( bool? ) value;
			  }
			  if ( value is string )
			  {
					try
					{
						 return bool.Parse( ( string ) value );
					}
					catch ( System.FormatException e )
					{
						 throw new BadInputException( e );
					}
			  }
			  throw new BadInputException( "Could not convert!" );
		 }

		 public virtual void Complete()
		 {
		 }
	}

}