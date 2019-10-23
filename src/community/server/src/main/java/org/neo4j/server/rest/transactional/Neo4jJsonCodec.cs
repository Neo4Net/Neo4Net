using System;
using System.Collections;
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
namespace Neo4Net.Server.rest.transactional
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using SerializationConfig = org.codehaus.jackson.map.SerializationConfig;


	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using CRS = Neo4Net.GraphDb.Spatial.CRS;
	using Coordinate = Neo4Net.GraphDb.Spatial.Coordinate;
	using Geometry = Neo4Net.GraphDb.Spatial.Geometry;
	using Point = Neo4Net.GraphDb.Spatial.Point;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.genericMap;

	public class Neo4NetJsonCodec : ObjectMapper
	{
		 private sealed class Neo4NetJsonMetaType
		 {
			  public static readonly Neo4NetJsonMetaType Node = new Neo4NetJsonMetaType( "Node", InnerEnum.Node, "node" );
			  public static readonly Neo4NetJsonMetaType Relationship = new Neo4NetJsonMetaType( "Relationship", InnerEnum.Relationship, "relationship" );
			  public static readonly Neo4NetJsonMetaType DateTime = new Neo4NetJsonMetaType( "DateTime", InnerEnum.DateTime, "datetime" );
			  public static readonly Neo4NetJsonMetaType Time = new Neo4NetJsonMetaType( "Time", InnerEnum.Time, "time" );
			  public static readonly Neo4NetJsonMetaType LocalDateTime = new Neo4NetJsonMetaType( "LocalDateTime", InnerEnum.LocalDateTime, "localdatetime" );
			  public static readonly Neo4NetJsonMetaType Date = new Neo4NetJsonMetaType( "Date", InnerEnum.Date, "date" );
			  public static readonly Neo4NetJsonMetaType LocalTime = new Neo4NetJsonMetaType( "LocalTime", InnerEnum.LocalTime, "localtime" );
			  public static readonly Neo4NetJsonMetaType Duration = new Neo4NetJsonMetaType( "Duration", InnerEnum.Duration, "duration" );
			  public static readonly Neo4NetJsonMetaType Point = new Neo4NetJsonMetaType( "Point", InnerEnum.Point, "point" );

			  private static readonly IList<Neo4NetJsonMetaType> valueList = new List<Neo4NetJsonMetaType>();

			  static Neo4NetJsonMetaType()
			  {
				  valueList.Add( Node );
				  valueList.Add( Relationship );
				  valueList.Add( DateTime );
				  valueList.Add( Time );
				  valueList.Add( LocalDateTime );
				  valueList.Add( Date );
				  valueList.Add( LocalTime );
				  valueList.Add( Duration );
				  valueList.Add( Point );
			  }

			  public enum InnerEnum
			  {
				  Node,
				  Relationship,
				  DateTime,
				  Time,
				  LocalDateTime,
				  Date,
				  LocalTime,
				  Duration,
				  Point
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: Neo4NetJsonMetaType(final String code)
			  internal Neo4NetJsonMetaType( string name, InnerEnum innerEnum, string code )
			  {
					this._code = code;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal string Code()
			  {
					return this._code;
			  }

			 public static IList<Neo4NetJsonMetaType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Neo4NetJsonMetaType ValueOf( string name )
			 {
				 foreach ( Neo4NetJsonMetaType enumInstance in Neo4NetJsonMetaType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private TransitionalPeriodTransactionMessContainer _container;

		 public Neo4NetJsonCodec( TransitionalPeriodTransactionMessContainer container ) : this()
		 {
			  this._container = container;
		 }

		 public Neo4NetJsonCodec()
		 {
			  SerializationConfig.without( SerializationConfig.Feature.FLUSH_AFTER_WRITE_VALUE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeValue(org.codehaus.jackson.JsonGenerator out, Object value) throws java.io.IOException
		 public override void WriteValue( JsonGenerator @out, object value )
		 {
			  if ( value is IPropertyContainer )
			  {
					using ( TransactionStateChecker txStateChecker = TransactionStateChecker.Create( _container ) )
					{
						 WritePropertyContainer( @out, ( IPropertyContainer ) value, txStateChecker );
					}
			  }
			  else if ( value is Path )
			  {
					using ( TransactionStateChecker txStateChecker = TransactionStateChecker.Create( _container ) )
					{
						 WritePath( @out, ( ( Path ) value ).GetEnumerator(), txStateChecker );
					}
			  }
			  else if ( value is System.Collections.IEnumerable )
			  {
					WriteIterator( @out, ( ( System.Collections.IEnumerable ) value ).GetEnumerator() );
			  }
			  else if ( value is sbyte[] )
			  {
					WriteByteArray( @out, ( sbyte[] ) value );
			  }
			  else if ( value is System.Collections.IDictionary )
			  {
					WriteMap( @out, ( System.Collections.IDictionary ) value );
			  }
			  else if ( value is Geometry )
			  {
					Geometry geom = ( Geometry ) value;
					object coordinates = ( geom is Point ) ? ( ( Point ) geom ).Coordinate : geom.Coordinates;
					WriteMap( @out, genericMap( new LinkedHashMap<>(), "type", geom.GeometryType, "coordinates", coordinates, "crs", geom.CRS ) );
			  }
			  else if ( value is Coordinate )
			  {
					Coordinate coordinate = ( Coordinate ) value;
					WriteIterator( @out, coordinate.GetCoordinate().GetEnumerator() );
			  }
			  else if ( value is CRS )
			  {
					CRS crs = ( CRS ) value;
					WriteMap( @out, genericMap( new LinkedHashMap<>(), "srid", crs.Code, "name", crs.Type, "type", "link", "properties", genericMap(new LinkedHashMap<>(), "href", crs.Href + "ogcwkt/", "type", "ogcwkt") ) );
			  }
			  else if ( value is Temporal || value is TemporalAmount )
			  {
					base.WriteValue( @out, value.ToString() );
			  }
			  else if ( value != null && value.GetType().IsArray && SupportedArrayType(value.GetType().GetElementType()) )
			  {
					WriteReflectiveArray( @out, value );
			  }
			  else
			  {
					base.WriteValue( @out, value );
			  }
		 }

		 private bool SupportedArrayType( Type valueClass )
		 {
			  return valueClass.IsAssignableFrom( typeof( Geometry ) ) || valueClass.IsAssignableFrom( typeof( CRS ) ) || valueClass.IsAssignableFrom( typeof( Temporal ) ) || valueClass.IsAssignableFrom( typeof( TemporalAmount ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReflectiveArray(org.codehaus.jackson.JsonGenerator out, Object array) throws java.io.IOException
		 private void WriteReflectiveArray( JsonGenerator @out, object array )
		 {
			  @out.writeStartArray();
			  try
			  {
					int length = Array.getLength( array );
					for ( int i = 0; i < length; i++ )
					{
						 WriteValue( @out, Array.get( array, i ) );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeMap(org.codehaus.jackson.JsonGenerator out, java.util.Map value) throws java.io.IOException
		 private void WriteMap( JsonGenerator @out, System.Collections.IDictionary value )
		 {
			  @out.writeStartObject();
			  try
			  {
					ISet<DictionaryEntry> set = value.SetOfKeyValuePairs();
					foreach ( DictionaryEntry e in set )
					{
						 object key = e.Key;
						 @out.writeFieldName( key == null ? "null" : key.ToString() );
						 WriteValue( @out, e.Value );
					}
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeIterator(org.codehaus.jackson.JsonGenerator out, java.util.Iterator value) throws java.io.IOException
		 private void WriteIterator( JsonGenerator @out, System.Collections.IEnumerator value )
		 {
			  @out.writeStartArray();
			  try
			  {
					while ( value.MoveNext() )
					{
						 WriteValue( @out, value.Current );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePath(org.codehaus.jackson.JsonGenerator out, java.util.Iterator<org.Neo4Net.graphdb.PropertyContainer> value, TransactionStateChecker txStateChecker) throws java.io.IOException
		 private void WritePath( JsonGenerator @out, IEnumerator<PropertyContainer> value, TransactionStateChecker txStateChecker )
		 {
			  @out.writeStartArray();
			  try
			  {
					while ( value.MoveNext() )
					{
						 WritePropertyContainer( @out, value.Current, txStateChecker );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePropertyContainer(org.codehaus.jackson.JsonGenerator out, org.Neo4Net.graphdb.PropertyContainer value, TransactionStateChecker txStateChecker) throws java.io.IOException
		 private void WritePropertyContainer( JsonGenerator @out, IPropertyContainer value, TransactionStateChecker txStateChecker )
		 {
			  if ( value is Node )
			  {
					WriteNodeOrRelationship( @out, value, txStateChecker.IsNodeDeletedInCurrentTx( ( ( Node ) value ).Id ) );
			  }
			  else if ( value is Relationship )
			  {
					WriteNodeOrRelationship( @out, value, txStateChecker.IsRelationshipDeletedInCurrentTx( ( ( Relationship ) value ).Id ) );
			  }
			  else
			  {
					throw new System.ArgumentException( "Expected a Node or Relationship, but got a " + value.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNodeOrRelationship(org.codehaus.jackson.JsonGenerator out, org.Neo4Net.graphdb.PropertyContainer IEntity, boolean isDeleted) throws java.io.IOException
		 private void WriteNodeOrRelationship( JsonGenerator @out, IPropertyContainer IEntity, bool isDeleted )
		 {
			  @out.writeStartObject();
			  try
			  {
					if ( !isDeleted )
					{
						 foreach ( KeyValuePair<string, object> property in IEntity.AllProperties.SetOfKeyValuePairs() )
						 {
							  @out.writeObjectField( property.Key, property.Value );
						 }
					}
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeByteArray(org.codehaus.jackson.JsonGenerator out, byte[] bytes) throws java.io.IOException
		 private void WriteByteArray( JsonGenerator @out, sbyte[] bytes )
		 {
			  @out.writeStartArray();
			  try
			  {
					foreach ( sbyte b in bytes )
					{
						 @out.writeNumber( ( int ) b );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeMeta(org.codehaus.jackson.JsonGenerator out, Object value) throws java.io.IOException
		 internal virtual void WriteMeta( JsonGenerator @out, object value )
		 {
			  if ( value is Node )
			  {
					Node node = ( Node ) value;
					using ( TransactionStateChecker stateChecker = TransactionStateChecker.Create( _container ) )
					{
						 WriteNodeOrRelationshipMeta( @out, node.Id, Neo4NetJsonMetaType.Node, stateChecker.IsNodeDeletedInCurrentTx( node.Id ) );
					}
			  }
			  else if ( value is Relationship )
			  {
					Relationship relationship = ( Relationship ) value;
					using ( TransactionStateChecker transactionStateChecker = TransactionStateChecker.Create( _container ) )
					{
						 WriteNodeOrRelationshipMeta( @out, relationship.Id, Neo4NetJsonMetaType.Relationship, transactionStateChecker.IsRelationshipDeletedInCurrentTx( relationship.Id ) );
					}
			  }
			  else if ( value is Path )
			  {
					WriteMetaPath( @out, ( Path ) value );
			  }
			  else if ( value is System.Collections.IEnumerable )
			  {
					foreach ( object v in ( System.Collections.IEnumerable ) value )
					{
						 WriteMeta( @out, v );
					}
			  }
			  else if ( value is System.Collections.IDictionary )
			  {
					System.Collections.IDictionary map = ( System.Collections.IDictionary ) value;
					foreach ( object key in map.Keys )
					{
						 WriteMeta( @out, map[key] );
					}
			  }
			  else if ( value is Geometry )
			  {
					WriteObjectMeta( @out, ParseGeometryType( ( Geometry ) value ) );
			  }
			  else if ( value is Temporal )
			  {
					WriteObjectMeta( @out, ParseTemporalType( ( Temporal ) value ) );
			  }
			  else if ( value is TemporalAmount )
			  {
					WriteObjectMeta( @out, Neo4NetJsonMetaType.Duration );
			  }
			  else
			  {
					@out.writeNull();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4NetJsonMetaType parseGeometryType(org.Neo4Net.GraphDb.Spatial.Geometry value) throws java.io.IOException
		 private Neo4NetJsonMetaType ParseGeometryType( Geometry value )
		 {
			  Neo4NetJsonMetaType type = null;
			  if ( value is Point )
			  {
					type = Neo4NetJsonMetaType.Point;
			  }
			  if ( type == null )
			  {
					throw new System.ArgumentException( string.Format( "Unsupported Geometry type: type={0}, value={1}", value.GetType().Name, value ) );
			  }
			  return type;
		 }

		 private Neo4NetJsonMetaType ParseTemporalType( Temporal value )
		 {
			  Neo4NetJsonMetaType type = null;
			  if ( value is ZonedDateTime )
			  {
					type = Neo4NetJsonMetaType.DateTime;
			  }
			  else if ( value is LocalDate )
			  {
					type = Neo4NetJsonMetaType.Date;
			  }
			  else if ( value is OffsetTime )
			  {
					type = Neo4NetJsonMetaType.Time;
			  }
			  else if ( value is DateTime )
			  {
					type = Neo4NetJsonMetaType.LocalDateTime;
			  }
			  else if ( value is LocalTime )
			  {
					type = Neo4NetJsonMetaType.LocalTime;
			  }
			  if ( type == null )
			  {
					throw new System.ArgumentException( string.Format( "Unsupported Temporal type: type={0}, value={1}", value.GetType().Name, value ) );
			  }
			  return type;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeMetaPath(org.codehaus.jackson.JsonGenerator out, org.Neo4Net.graphdb.Path value) throws java.io.IOException
		 private void WriteMetaPath( JsonGenerator @out, Path value )
		 {
			  @out.writeStartArray();
			  try
			  {
					foreach ( IPropertyContainer element in value )
					{
						 WriteMeta( @out, element );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeObjectMeta(org.codehaus.jackson.JsonGenerator out, Neo4NetJsonMetaType type) throws java.io.IOException
		 private void WriteObjectMeta( JsonGenerator @out, Neo4NetJsonMetaType type )
		 {
			  requireNonNull( type, "The meta type cannot be null for known types." );
			  @out.writeStartObject();
			  try
			  {
					@out.writeStringField( "type", type.code() );
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNodeOrRelationshipMeta(org.codehaus.jackson.JsonGenerator out, long id, Neo4NetJsonMetaType type, boolean isDeleted) throws java.io.IOException
		 private void WriteNodeOrRelationshipMeta( JsonGenerator @out, long id, Neo4NetJsonMetaType type, bool isDeleted )
		 {
			  requireNonNull( type, "The meta type could not be null for node or relationship." );
			  @out.writeStartObject();
			  try
			  {
					@out.writeNumberField( "id", id );
					@out.writeStringField( "type", type.code() );
					@out.writeBooleanField( "deleted", isDeleted );
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }
	}

}