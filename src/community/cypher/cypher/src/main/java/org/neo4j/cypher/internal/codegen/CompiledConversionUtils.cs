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
namespace Neo4Net.Cypher.Internal.codegen
{

	using CypherTypeException = Neo4Net.Cypher.Internal.v3_5.util.CypherTypeException;
	using IncomparableValuesException = Neo4Net.Cypher.Internal.v3_5.util.IncomparableValuesException;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxyWrappingNodeValue = Neo4Net.Kernel.impl.util.NodeProxyWrappingNodeValue;
	using RelationshipProxyWrappingValue = Neo4Net.Kernel.impl.util.RelationshipProxyWrappingValue;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Neo4Net.Values.Storable;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.SequenceValue_IterationPreference.RANDOM_ACCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	// Class with static methods used by compiled execution plans
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public abstract class CompiledConversionUtils
	public abstract class CompiledConversionUtils
	{
		 public static bool CoerceToPredicate( object value )
		 {
			  if ( value == null || value == Values.NO_VALUE )
			  {
					return false;
			  }
			  if ( value is BooleanValue )
			  {
					return ( ( BooleanValue ) value ).booleanValue();
			  }
			  if ( value is bool? )
			  {
					return ( bool ) value;
			  }
			  if ( value is ArrayValue )
			  {
					return ( ( ArrayValue ) value ).length() > 0;
			  }
			  if ( value.GetType().IsArray )
			  {
					return Array.getLength( value ) > 0;
			  }
			  throw new CypherTypeException( "Don't know how to treat that as a predicate: " + value.ToString(), null );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static java.util.Set<?> toSet(Object value)
		 public static ISet<object> ToSet( object value )
		 {
			  if ( value == null || value == NO_VALUE )
			  {
					return Collections.emptySet();
			  }
			  else if ( value is SequenceValue )
			  {
					SequenceValue sequenceValue = ( SequenceValue ) value;
					IEnumerator<AnyValue> iterator = sequenceValue.GetEnumerator();
					ISet<AnyValue> set;
					if ( sequenceValue.IterationPreference() == RANDOM_ACCESS )
					{
						 // If we have a random access sequence value length() should be cheap and we can optimize the initial capacity
						 int length = sequenceValue.Length();
						 set = new HashSet<AnyValue>( length );
						 for ( int i = 0; i < length; i++ )
						 {
							  set.Add( sequenceValue.Value( i ) );
						 }
					}
					else
					{
						 set = new HashSet<AnyValue>();
						 while ( iterator.MoveNext() )
						 {
							  AnyValue element = iterator.Current;
							  set.Add( element );
						 }
					}
					return set;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (value instanceof java.util.Collection<?>)
			  else if ( value is ICollection<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new java.util.HashSet<>((java.util.Collection<?>) value);
					return new HashSet<object>( ( ICollection<object> ) value );
			  }
			  else if ( value is LongStream )
			  {
					LongStream stream = ( LongStream ) value;
					return stream.boxed().collect(Collectors.toSet());
			  }
			  else if ( value is IntStream )
			  {
					IntStream stream = ( IntStream ) value;
					return stream.boxed().collect(Collectors.toSet());
			  }
			  else if ( value is DoubleStream )
			  {
					DoubleStream stream = ( DoubleStream ) value;
					return stream.boxed().collect(Collectors.toSet());
			  }
			  else if ( value.GetType().IsArray )
			  {
					int len = Array.getLength( value );
					HashSet<object> collection = new HashSet<object>( len );
					for ( int i = 0; i < len; i++ )
					{
						 collection.Add( Array.get( value, i ) );
					}
					return collection;
			  }

			  throw new CypherTypeException( "Don't know how to create a set out of " + value.GetType().Name, null );
		 }

		 public static CompositeKey CompositeKey( params long[] keys )
		 {
			  return new CompositeKey( keys );
		 }

		 public class CompositeKey
		 {
			  internal readonly long[] Key;

			  internal CompositeKey( long[] key )
			  {
					this.Key = key;
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

					CompositeKey that = ( CompositeKey ) o;

					return Arrays.Equals( Key, that.Key );

			  }

			  public override int GetHashCode()
			  {
					return Arrays.GetHashCode( Key );
			  }
		 }

		 /// <summary>
		 /// Checks equality according to OpenCypher </summary>
		 /// <returns> true if equal, false if not equal and null if incomparable </returns>
		 public static bool? Equals( object lhs, object rhs )
		 {
			  if ( lhs == null || rhs == null || lhs == NO_VALUE || rhs == NO_VALUE )
			  {
					return null;
			  }

			  bool lhsVirtualNodeValue = lhs is VirtualNodeValue;
			  if ( lhsVirtualNodeValue || rhs is VirtualNodeValue || lhs is VirtualRelationshipValue || rhs is VirtualRelationshipValue )
			  {
					if ( ( lhsVirtualNodeValue && !( rhs is VirtualNodeValue ) ) || ( rhs is VirtualNodeValue && !lhsVirtualNodeValue ) || ( lhs is VirtualRelationshipValue && !( rhs is VirtualRelationshipValue ) ) || ( rhs is VirtualRelationshipValue && !( lhs is VirtualRelationshipValue ) ) )
					{
						 throw new IncomparableValuesException( lhs.GetType().Name, rhs.GetType().Name );
					}
					return lhs.Equals( rhs );
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : ValueUtils.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : ValueUtils.of( rhs );

			  return lhsValue.TernaryEquals( rhsValue );
		 }

		 // Ternary OR
		 public static bool? Or( object lhs, object rhs )
		 {
			  bool? l = ToBooleanOrNull( lhs );
			  bool? r = ToBooleanOrNull( rhs );

			  if ( l == null && r == null )
			  {
					return null;
			  }
			  else if ( l == null )
			  {
					return r ? true : null;
			  }
			  else if ( r == null )
			  {
					return l ? true : null;
			  }
			  return l || r;
		 }

		 // Ternary NOT
		 public static bool? Not( object predicate )
		 {
			  bool? b = ToBooleanOrNull( predicate );
			  if ( b == null )
			  {
					return null;
			  }
			  return !b;
		 }

		 private static bool? ToBooleanOrNull( object o )
		 {
			  if ( o == null || o == NO_VALUE )
			  {
					return null;
			  }
			  else if ( o is bool? )
			  {
					return ( bool? ) o;
			  }
			  else if ( o is BooleanValue )
			  {
					return ( ( BooleanValue ) o ).booleanValue();
			  }
			  throw new CypherTypeException( "Don't know how to treat that as a boolean: " + o.ToString(), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "WeakerAccess"}) public static org.neo4j.values.AnyValue materializeAnyResult(org.neo4j.kernel.impl.core.EmbeddedProxySPI proxySpi, Object anyValue)
		 public static AnyValue MaterializeAnyResult( EmbeddedProxySPI proxySpi, object anyValue )
		 {
			  if ( anyValue == null || anyValue == NO_VALUE )
			  {
					return NO_VALUE;
			  }
			  else if ( anyValue is AnyValue )
			  {
					return MaterializeAnyValueResult( proxySpi, anyValue );
			  }
			  else if ( anyValue is System.Collections.IList )
			  {
					return VirtualValues.fromList( ( IList<AnyValue> )( ( ( System.Collections.IList ) anyValue ).Select( v => MaterializeAnyResult( proxySpi, v ) ).ToList() ) );
			  }
			  else if ( anyValue is System.Collections.IDictionary )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String,?> incoming = (java.util.Map<String,?>) anyValue;
					IDictionary<string, ?> incoming = ( IDictionary<string, ?> ) anyValue;
					MapValueBuilder builder = new MapValueBuilder( incoming.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<String,?> entry : incoming.entrySet())
					foreach ( KeyValuePair<string, ?> entry in incoming.SetOfKeyValuePairs() )
					{
						 builder.Add( entry.Key, MaterializeAnyResult( proxySpi, entry.Value ) );
					}
					return builder.Build();
			  }
			  else if ( anyValue is PrimitiveNodeStream )
			  {
					return VirtualValues.fromList( ( ( PrimitiveNodeStream ) anyValue ).LongStream().mapToObj(id => (AnyValue) ValueUtils.fromNodeProxy(proxySpi.NewNodeProxy(id))).collect(Collectors.toList()) );
			  }
			  else if ( anyValue is PrimitiveRelationshipStream )
			  {
					return VirtualValues.fromList( ( ( PrimitiveRelationshipStream ) anyValue ).LongStream().mapToObj(id => (AnyValue) ValueUtils.fromRelationshipProxy(proxySpi.NewRelationshipProxy(id))).collect(Collectors.toList()) );
			  }
			  else if ( anyValue is LongStream )
			  {
					long[] array = ( ( LongStream ) anyValue ).toArray();
					return Values.longArray( array );
			  }
			  else if ( anyValue is DoubleStream )
			  {
					double[] array = ( ( DoubleStream ) anyValue ).toArray();
					return Values.doubleArray( array );
			  }
			  else if ( anyValue is IntStream )
			  {
					// IntStream is only used for list of primitive booleans
					return VirtualValues.fromList( ( ( IntStream ) anyValue ).mapToObj( i => Values.booleanValue( i != 0 ) ).collect( Collectors.toList() ) );
			  }
			  else if ( anyValue.GetType().IsArray )
			  {
					Type componentType = anyValue.GetType().GetElementType();
					int length = Array.getLength( anyValue );

					if ( componentType.IsPrimitive )
					{
						 object copy = Array.CreateInstance( componentType, length );
						 //noinspection SuspiciousSystemArraycopy
						 Array.Copy( anyValue, 0, copy, 0, length );
						 return ValueUtils.of( copy );
					}
					else if ( anyValue is string[] )
					{
						 return Values.stringArray( ( string[] ) anyValue );
					}
					else
					{
						 AnyValue[] copy = new AnyValue[length];
						 for ( int i = 0; i < length; i++ )
						 {
							  copy[i] = MaterializeAnyResult( proxySpi, Array.get( anyValue, i ) );
						 }
						 return VirtualValues.list( copy );
					}
			  }
			  else
			  {
					return ValueUtils.of( anyValue );
			  }
		 }

		 // NOTE: This assumes anyValue is an instance of AnyValue
		 public static AnyValue MaterializeAnyValueResult( EmbeddedProxySPI proxySpi, object anyValue )
		 {
			  if ( anyValue is VirtualNodeValue )
			  {
					if ( anyValue is NodeValue )
					{
						 return ( AnyValue ) anyValue;
					}
					return ValueUtils.fromNodeProxy( proxySpi.NewNodeProxy( ( ( VirtualNodeValue ) anyValue ).id() ) );
			  }
			  if ( anyValue is VirtualRelationshipValue )
			  {
					if ( anyValue is RelationshipValue )
					{
						 return ( AnyValue ) anyValue;
					}
					return ValueUtils.fromRelationshipProxy( proxySpi.NewRelationshipProxy( ( ( VirtualRelationshipValue ) anyValue ).id() ) );
			  }
			  // If it is a list or map, run it through a ValueMapper that will create proxy objects for entities if needed.
			  // This will first do a dry run and return as it is if no conversion is needed.
			  // If in the future we will always create proxy objects directly whenever we create values we can skip this
			  // Doing this conversion lazily instead, by wrapping with TransformedListValue or TransformedMapValue is probably not a
			  // good idea because of the complexities involved (see TOMBSTONE in VirtualValues about why TransformedListValue was killed).
			  // NOTE: There is also a case where a ListValue can be storable (ArrayValueListValue) where no conversion is needed
			  if ( ( anyValue is ListValue && !( ( ListValue ) anyValue ).storable() ) || anyValue is MapValue )
			  {
					return CompiledMaterializeValueMapper.MapAnyValue( proxySpi, ( AnyValue ) anyValue );
			  }
			  return ( AnyValue ) anyValue;
		 }

		 public static NodeValue MaterializeNodeValue( EmbeddedProxySPI proxySpi, object anyValue )
		 {
			  // Null check has to be done outside by the generated code
			  if ( anyValue is NodeValue )
			  {
					return ( NodeValue ) anyValue;
			  }
			  else if ( anyValue is VirtualNodeValue )
			  {
					return ValueUtils.fromNodeProxy( proxySpi.NewNodeProxy( ( ( VirtualNodeValue ) anyValue ).id() ) );
			  }
			  else if ( anyValue is Node )
			  {
					return ValueUtils.fromNodeProxy( ( Node ) anyValue );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Do not know how to materialize node value from type " + anyValue.GetType().FullName );
		 }

		 public static RelationshipValue MaterializeRelationshipValue( EmbeddedProxySPI proxySpi, object anyValue )
		 {
			  // Null check has to be done outside by the generated code
			  if ( anyValue is RelationshipValue )
			  {
					return ( RelationshipValue ) anyValue;
			  }
			  else if ( anyValue is VirtualRelationshipValue )
			  {
					return ValueUtils.fromRelationshipProxy( proxySpi.NewRelationshipProxy( ( ( VirtualRelationshipValue ) anyValue ).id() ) );
			  }
			  else if ( anyValue is Relationship )
			  {
					return ValueUtils.fromRelationshipProxy( ( Relationship ) anyValue );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "Do not know how to materialize relationship value from type " + anyValue.GetType().FullName );
		 }

		 public static System.Collections.IEnumerator IteratorFrom( object iterable )
		 {
			  if ( iterable is System.Collections.IEnumerable )
			  {
					return ( ( System.Collections.IEnumerable ) iterable ).GetEnumerator();
			  }
			  else if ( iterable is PrimitiveEntityStream )
			  {
					return ( ( PrimitiveEntityStream ) iterable ).GetEnumerator();
			  }
			  else if ( iterable is LongStream )
			  {
					return ( ( LongStream ) iterable ).GetEnumerator();
			  }
			  else if ( iterable is DoubleStream )
			  {
					return ( ( DoubleStream ) iterable ).GetEnumerator();
			  }
			  else if ( iterable is IntStream )
			  {
					return ( ( IntStream ) iterable ).GetEnumerator();
			  }
			  else if ( iterable == null || iterable == NO_VALUE )
			  {
					return Collections.emptyIterator();
			  }
			  else if ( iterable.GetType().IsArray )
			  {
					return new ArrayIterator( iterable );
			  }
			  else
			  {
					return Stream.of( iterable ).GetEnumerator();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static java.util.stream.LongStream toLongStream(Object list)
		 public static LongStream ToLongStream( object list )
		 {
			  if ( list == null )
			  {
					return LongStream.empty();
			  }
			  else if ( list is SequenceValue )
			  {
					throw new System.ArgumentException( "Need to implement support for SequenceValue in CompiledConversionUtils.toLongStream" );
			  }
			  else if ( list is System.Collections.IList )
			  {
					return ( ( System.Collections.IList ) list ).Select( n => ( ( Number ) n ).longValue() );
			  }
			  else if ( list.GetType().IsAssignableFrom(typeof(object[])) )
			  {
					return java.util.( object[] ) list.Select( n => ( ( Number ) n ).longValue() );
			  }
			  else if ( list is sbyte[] )
			  {
					sbyte[] array = ( sbyte[] ) list;
					return IntStream.range( 0, array.Length ).mapToLong( i => array[i] );
			  }
			  else if ( list is short[] )
			  {
					short[] array = ( short[] ) list;
					return IntStream.range( 0, array.Length ).mapToLong( i => array[i] );
			  }
			  else if ( list is int[] )
			  {
					return IntStream.of( ( int[] ) list ).mapToLong( i => i );
			  }
			  else if ( list is long[] )
			  {
					return LongStream.of( ( long[] ) list );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "Can not be converted to stream: %s", list.GetType().FullName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static java.util.stream.DoubleStream toDoubleStream(Object list)
		 public static DoubleStream ToDoubleStream( object list )
		 {
			  if ( list == null )
			  {
					return DoubleStream.empty();
			  }
			  else if ( list is SequenceValue )
			  {
					throw new System.ArgumentException( "Need to implement support for SequenceValue in CompiledConversionUtils.toDoubleStream" );
			  }
			  else if ( list is System.Collections.IList )
			  {
					return ( ( System.Collections.IList ) list ).Select( n => ( ( Number ) n ).doubleValue() );
			  }
			  else if ( list.GetType().IsAssignableFrom(typeof(object[])) )
			  {
					return java.util.( object[] ) list.Select( n => ( ( Number ) n ).doubleValue() );
			  }
			  else if ( list is float[] )
			  {
					float[] array = ( float[] ) list;
					return IntStream.range( 0, array.Length ).mapToDouble( i => array[i] );
			  }
			  else if ( list is double[] )
			  {
					return DoubleStream.of( ( double[] ) list );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "Can not be converted to stream: %s", list.GetType().FullName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static java.util.stream.IntStream toBooleanStream(Object list)
		 public static IntStream ToBooleanStream( object list )
		 {
			  if ( list == null )
			  {
					return IntStream.empty();
			  }
			  else if ( list is SequenceValue )
			  {
					throw new System.ArgumentException( "Need to implement support for SequenceValue in CompiledConversionUtils.toBooleanStream" );
			  }
			  else if ( list is System.Collections.IList )
			  {
					return ( ( System.Collections.IList ) list ).Select( n => ( ( Number ) n ).intValue() );
			  }
			  else if ( list.GetType().IsAssignableFrom(typeof(object[])) )
			  {
					return java.util.( object[] ) list.Select( n => ( ( Number ) n ).intValue() );
			  }
			  else if ( list is bool[] )
			  {
					bool[] array = ( bool[] ) list;
					return IntStream.range( 0, array.Length ).map( i => ( array[i] ) ? 1 : 0 );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "Can not be converted to stream: %s", list.GetType().FullName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static long unboxNodeOrNull(org.neo4j.values.virtual.VirtualNodeValue value)
		 public static long UnboxNodeOrNull( VirtualNodeValue value )
		 {
			  if ( value == null )
			  {
					return -1L;
			  }
			  return value.Id();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static long unboxRelationshipOrNull(org.neo4j.values.virtual.VirtualRelationshipValue value)
		 public static long UnboxRelationshipOrNull( VirtualRelationshipValue value )
		 {
			  if ( value == null )
			  {
					return -1L;
			  }
			  return value.Id();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static long extractLong(Object obj)
		 public static long ExtractLong( object obj )
		 {
			  if ( obj == null || obj == NO_VALUE )
			  {
					return -1L;
			  }
			  else if ( obj is VirtualNodeValue )
			  {
					return ( ( VirtualNodeValue ) obj ).id();
			  }
			  else if ( obj is VirtualRelationshipValue )
			  {
					return ( ( VirtualRelationshipValue ) obj ).id();
			  }
			  else if ( obj is long? )
			  {
					return ( long? ) obj.Value;
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.ArgumentException( format( "Can not be converted to long: %s", obj.GetType().FullName ) );
			  }
		 }

		 //In the store we only support storable Value types and arrays thereof.
		 //In cypher we must make an effort to transform Cypher lists to appropriate arrays whenever
		 //we are using sending values down to the store or to an index.
		 public static object MakeValueNeoSafe( object @object )
		 {
			  AnyValue value = @object is AnyValue ? ( ( AnyValue ) @object ) : ValueUtils.of( @object );
			  return Neo4Net.Cypher.Internal.runtime.interpreted.makeValueNeoSafe.apply( value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static Object mapGetProperty(Object object, String key)
		 public static object MapGetProperty( object @object, string key )
		 {
			  if ( @object == NO_VALUE )
			  {
					return NO_VALUE;
			  }
			  if ( @object is MapValue )
			  {
					MapValue map = ( MapValue ) @object;
					return map.Get( key );
			  }
			  if ( @object is NodeProxyWrappingNodeValue )
			  {
					return Values.of( ( ( NodeProxyWrappingNodeValue ) @object ).nodeProxy().getProperty(key) );
			  }
			  if ( @object is RelationshipProxyWrappingValue )
			  {
					return Values.of( ( ( RelationshipProxyWrappingValue ) @object ).relationshipProxy().getProperty(key) );
			  }
			  if ( @object is PropertyContainer ) // Entity that is not wrapped by an AnyValue
			  {
					return Values.of( ( ( PropertyContainer ) @object ).getProperty( key ) );
			  }
			  if ( @object is NodeValue )
			  {
					return ( ( NodeValue ) @object ).properties().get(key);
			  }
			  if ( @object is RelationshipValue )
			  {
					return ( ( RelationshipValue ) @object ).properties().get(key);
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (object instanceof java.util.Map<?,?>)
			  if ( @object is IDictionary<object, ?> )
			  {
					IDictionary<string, object> map = ( IDictionary<string, object> ) @object;
					return map[key];
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (object instanceof org.neo4j.values.storable.TemporalValue<?,?>)
			  if ( @object is TemporalValue<object, ?> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return ((org.neo4j.values.storable.TemporalValue<?,?>) object).get(key);
					return ( ( TemporalValue<object, ?> ) @object ).get( key );
			  }
			  if ( @object is DurationValue )
			  {
					return ( ( DurationValue ) @object ).get( key );
			  }
			  if ( @object is PointValue )
			  {
					return ( ( PointValue ) @object ).get( key );
			  }

			  // NOTE: VirtualNodeValue and VirtualRelationshipValue will fall through to here
			  // To handle these we would need specialized cursor code
			  throw new CypherTypeException( string.Format( "Type mismatch: expected a map but was {0}", @object ), null );
		 }

		 internal class ArrayIterator : System.Collections.IEnumerator
		 {
			  internal int Position;
			  internal readonly int Len;
			  internal readonly object Array;

			  internal ArrayIterator( object array )
			  {
					this.Position = 0;
					this.Len = Array.getLength( array );
					this.Array = array;
			  }

			  public override bool HasNext()
			  {
					return Position < Len;
			  }

			  public override object Next()
			  {
					if ( Position >= Len )
					{
						 throw new NoSuchElementException();
					}
					int offset = Position++;
					return Array.get( Array, offset );
			  }
		 }
	}

}