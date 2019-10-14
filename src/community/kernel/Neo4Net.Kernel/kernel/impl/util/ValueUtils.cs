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
namespace Neo4Net.Kernel.impl.util
{

	using Entity = Neo4Net.Graphdb.Entity;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Geometry = Neo4Net.Graphdb.spatial.Geometry;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using AnyValue = Neo4Net.Values.AnyValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public sealed class ValueUtils
	{
		 private ValueUtils()
		 {
			  throw new System.NotSupportedException( "do not instantiate" );
		 }

		 /// <summary>
		 /// Creates an AnyValue by doing type inspection. Do not use in production code where performance is important.
		 /// </summary>
		 /// <param name="object"> the object to turned into a AnyValue </param>
		 /// <returns> the AnyValue corresponding to object. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static org.neo4j.values.AnyValue of(Object object)
		 public static AnyValue Of( object @object )
		 {
			  Value value = Values.unsafeOf( @object, true );
			  if ( value != null )
			  {
					return value;
			  }
			  else
			  {
					if ( @object is Entity )
					{
						 if ( @object is Node )
						 {
							  return FromNodeProxy( ( Node ) @object );
						 }
						 else if ( @object is Relationship )
						 {
							  return FromRelationshipProxy( ( Relationship ) @object );
						 }
						 else
						 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							  throw new System.ArgumentException( "Unknown entity + " + @object.GetType().FullName );
						 }
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (object instanceof Iterable<?>)
					else if ( @object is IEnumerable<object> )
					{
						 if ( @object is Path )
						 {
							  return FromPath( ( Path ) @object );
						 }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (object instanceof java.util.List<?>)
						 else if ( @object is IList<object> )
						 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return asListValue((java.util.List<?>) object);
							  return AsListValue( ( IList<object> ) @object );
						 }
						 else
						 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return asListValue((Iterable<?>) object);
							  return AsListValue( ( IEnumerable<object> ) @object );
						 }
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (object instanceof java.util.Map<?,?>)
					else if ( @object is IDictionary<object, ?> )
					{
						 return AsMapValue( ( IDictionary<string, object> ) @object );
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (object instanceof java.util.Iterator<?>)
					else if ( @object is IEnumerator<object> )
					{
						 List<object> objects = new List<object>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> iterator = (java.util.Iterator<?>) object;
						 IEnumerator<object> iterator = ( IEnumerator<object> ) @object;
						 while ( iterator.MoveNext() )
						 {
							  objects.Add( iterator.Current );
						 }
						 return AsListValue( objects );
					}
					else if ( @object is object[] )
					{
						 object[] array = ( object[] ) @object;
						 AnyValue[] anyValues = new AnyValue[array.Length];
						 for ( int i = 0; i < array.Length; i++ )
						 {
							  anyValues[i] = ValueUtils.Of( array[i] );
						 }
						 return VirtualValues.list( anyValues );
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (object instanceof java.util.stream.Stream<?>)
					else if ( @object is Stream<object> )
					{
						 return AsListValue( ( ( Stream<object> ) @object ).collect( Collectors.toList() ) );
					}
					else if ( @object is Geometry )
					{
						 return AsGeometryValue( ( Geometry ) @object );
					}
					else if ( @object is VirtualNodeValue || @object is VirtualRelationshipValue )
					{
						 return ( AnyValue ) @object;
					}
					else
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new System.ArgumentException( string.Format( "Cannot convert {0} to AnyValue", @object.GetType().FullName ) );
					}
			  }
		 }

		 public static PointValue AsPointValue( Point point )
		 {
			  return ToPoint( point );
		 }

		 public static PointValue AsGeometryValue( Geometry geometry )
		 {
			  if ( !geometry.GeometryType.Equals( "Point" ) )
			  {
					throw new System.ArgumentException( "Cannot handle geometry type: " + geometry.CRS.Type );
			  }
			  return ToPoint( geometry );
		 }

		 private static PointValue ToPoint( Geometry geometry )
		 {
			  IList<double> coordinate = geometry.Coordinates[0].Coordinate;
			  double[] primitiveCoordinate = new double[coordinate.Count];
			  for ( int i = 0; i < coordinate.Count; i++ )
			  {
					primitiveCoordinate[i] = coordinate[i];
			  }

			  return Values.pointValue( CoordinateReferenceSystem.get( geometry.CRS ), primitiveCoordinate );
		 }

		 public static ListValue AsListValue<T1>( IList<T1> collection )
		 {
			  List<AnyValue> values = new List<AnyValue>( collection.Count );
			  foreach ( object o in collection )
			  {
					values.Add( ValueUtils.Of( o ) );
			  }
			  return VirtualValues.fromList( values );
		 }

		 public static ListValue AsListValue<T1>( IEnumerable<T1> collection )
		 {
			  List<AnyValue> values = new List<AnyValue>();
			  foreach ( object o in collection )
			  {
					values.Add( ValueUtils.Of( o ) );
			  }
			  return VirtualValues.fromList( values );
		 }

		 public static AnyValue AsNodeOrEdgeValue( PropertyContainer container )
		 {
			  if ( container is Node )
			  {
					return FromNodeProxy( ( Node ) container );
			  }
			  else if ( container is Relationship )
			  {
					return FromRelationshipProxy( ( Relationship ) container );
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.ArgumentException( "Cannot produce a node or edge from " + container.GetType().FullName );
			  }
		 }

		 public static ListValue AsListOfEdges( IEnumerable<Relationship> rels )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return VirtualValues.list( StreamSupport.stream( rels.spliterator(), false ).map(ValueUtils.fromRelationshipProxy).toArray(RelationshipValue[]::new) );
		 }

		 public static ListValue AsListOfEdges( Relationship[] rels )
		 {
			  RelationshipValue[] relValues = new RelationshipValue[rels.Length];
			  for ( int i = 0; i < relValues.Length; i++ )
			  {
					relValues[i] = FromRelationshipProxy( rels[i] );
			  }
			  return VirtualValues.list( relValues );
		 }

		 public static MapValue AsMapValue( IDictionary<string, object> map )
		 {
			  MapValueBuilder builder = new MapValueBuilder( map.Count );
			  foreach ( KeyValuePair<string, object> entry in map.SetOfKeyValuePairs() )
			  {
					builder.Add( entry.Key, ValueUtils.Of( entry.Value ) );
			  }
			  return builder.Build();
		 }

		 public static MapValue AsParameterMapValue( IDictionary<string, object> map )
		 {
			  MapValueBuilder builder = new MapValueBuilder( map.Count );
			  foreach ( KeyValuePair<string, object> entry in map.SetOfKeyValuePairs() )
			  {
					try
					{
						 builder.Add( entry.Key, ValueUtils.Of( entry.Value ) );
					}
					catch ( System.ArgumentException e )
					{
						 builder.Add( entry.Key, VirtualValues.error( e ) );
					}
			  }

			  return builder.Build();
		 }

		 public static NodeValue FromNodeProxy( Node node )
		 {
			  return new NodeProxyWrappingNodeValue( node );
		 }

		 public static RelationshipValue FromRelationshipProxy( Relationship relationship )
		 {
			  return new RelationshipProxyWrappingValue( relationship );
		 }

		 public static PathValue FromPath( Path path )
		 {
			  return new PathWrappingPathValue( path );
		 }

		 /// <summary>
		 /// Creates a <seealso cref="Value"/> from the given object, or if it is already a Value it is returned as it is.
		 /// <para>
		 /// This is different from <seealso cref="Values.of"/> which often explicitly fails or creates a new copy
		 /// if given a Value.
		 /// </para>
		 /// </summary>
		 public static Value AsValue( object value )
		 {
			  if ( value is Value )
			  {
					return ( Value ) value;
			  }
			  return Values.of( value );
		 }

		 /// <summary>
		 /// Creates an <seealso cref="AnyValue"/> from the given object, or if it is already an AnyValue it is returned as it is.
		 /// <para>
		 /// This is different from <seealso cref="ValueUtils.of"/> which often explicitly fails or creates a new copy
		 /// if given an AnyValue.
		 /// </para>
		 /// </summary>
		 public static AnyValue AsAnyValue( object value )
		 {
			  if ( value is AnyValue )
			  {
					return ( AnyValue ) value;
			  }
			  return ValueUtils.Of( value );
		 }

		 public static NodeValue AsNodeValue( object @object )
		 {
			  if ( @object is NodeValue )
			  {
					return ( NodeValue ) @object;
			  }
			  if ( @object is Node )
			  {
					return FromNodeProxy( ( Node ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a node from " + @object.GetType().FullName );
		 }

		 public static RelationshipValue AsRelationshipValue( object @object )
		 {
			  if ( @object is RelationshipValue )
			  {
					return ( RelationshipValue ) @object;
			  }
			  if ( @object is Relationship )
			  {
					return FromRelationshipProxy( ( Relationship ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a relationship from " + @object.GetType().FullName );
		 }

		 public static LongValue AsLongValue( object @object )
		 {
			  if ( @object is LongValue )
			  {
					return ( LongValue ) @object;
			  }
			  if ( @object is long? )
			  {
					return Values.longValue( ( long ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a long from " + @object.GetType().FullName );
		 }

		 public static DoubleValue AsDoubleValue( object @object )
		 {
			  if ( @object is DoubleValue )
			  {
					return ( DoubleValue ) @object;
			  }
			  if ( @object is double? )
			  {
					return Values.doubleValue( ( double ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a double from " + @object.GetType().FullName );
		 }

		 public static BooleanValue AsBooleanValue( object @object )
		 {
			  if ( @object is BooleanValue )
			  {
					return ( BooleanValue ) @object;
			  }
			  if ( @object is bool? )
			  {
					return Values.booleanValue( ( bool ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a boolean from " + @object.GetType().FullName );
		 }

		 public static TextValue AsTextValue( object @object )
		 {
			  if ( @object is TextValue )
			  {
					return ( TextValue ) @object;
			  }
			  if ( @object is string )
			  {
					return Values.stringValue( ( string ) @object );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Cannot produce a string from " + @object.GetType().FullName );
		 }

	}


}