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
namespace Neo4Net.Cypher.@internal.compiler.v3_5.common
{

	using IncomparableValuesException = Neo4Net.Cypher.@internal.v3_5.util.IncomparableValuesException;
	using UnorderableValueException = Neo4Net.Cypher.@internal.v3_5.util.UnorderableValueException;
	using Path = Neo4Net.Graphdb.Path;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using MathUtil = Neo4Net.Helpers.MathUtil;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using AnyValues = Neo4Net.Values.AnyValues;
	using Values = Neo4Net.Values.Storable.Values;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;

	/// <summary>
	/// Helper class for dealing with orderability in compiled code.
	/// 
	/// <h1>
	/// Orderability
	/// 
	/// <a href="https://github.com/opencypher/openCypher/blob/master/cip/1.accepted/CIP2016-06-14-Define-comparability-and-equality-as-well-as-orderability-and-equivalence.adoc">
	///   The Cypher CIP defining orderability
	/// </a>
	/// 
	/// <para>
	/// Ascending global sort order of disjoint types:
	/// 
	/// <ul>
	///   <li> MAP types
	///    <ul>
	///      <li> Regular map
	/// 
	///      <li> NODE
	/// 
	///      <li> RELATIONSHIP
	///    <ul>
	/// 
	///  <li> LIST OF ANY?
	/// 
	///  <li> PATH
	/// 
	///  <li> STRING
	/// 
	///  <li> BOOLEAN
	/// 
	///  <li> NUMBER
	///    <ul>
	///      <li> NaN values are treated as the largest numbers in orderability only (i.e. they are put after positive infinity)
	///    </ul>
	///  <li> VOID (i.e. the type of null)
	/// </ul>
	/// 
	/// TBD: POINT and GEOMETRY
	/// </para>
	/// </summary>
	public class CypherOrderability
	{
		 /// <summary>
		 /// Do not instantiate this class
		 /// </summary>
		 private CypherOrderability()
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Compare with Cypher orderability semantics for disjoint types
		 /// </summary>
		 /// <param name="lhs"> </param>
		 /// <param name="rhs">
		 /// @return </param>
		 public static int Compare( object lhs, object rhs )
		 {
			  if ( lhs == rhs )
			  {
					return 0;
			  }
			  // null is greater than any other type
			  else if ( lhs == Values.NO_VALUE || lhs == null )
			  {
					return 1;
			  }
			  else if ( rhs == Values.NO_VALUE || rhs == null )
			  {
					return -1;
			  }
			  else if ( lhs is AnyValue )
			  {
					AnyValue rhsValue = ( rhs is AnyValue ) ? ( AnyValue ) rhs : ValueUtils.of( rhs );
					return AnyValues.COMPARATOR.Compare( ( AnyValue ) lhs, rhsValue );
			  }
			  else if ( rhs is AnyValue )
			  {
					AnyValue lhsValue = ValueUtils.of( lhs );
					return AnyValues.COMPARATOR.Compare( lhsValue, ( AnyValue ) rhs );
			  }
			  // Compare the types
			  // TODO: Test coverage for the Orderability CIP
			  SuperType leftType = SuperType.ofValue( lhs );
			  SuperType rightType = SuperType.ofValue( rhs );

			  int typeComparison = SuperType.TYPE_ID_COMPARATOR.compare( leftType, rightType );
			  if ( typeComparison != 0 )
			  {
					// Types are different an decides the order
					return typeComparison;
			  }

			  return leftType.comparator.compare( lhs, rhs );
		 }

		 public sealed class SuperType
		 {
			  public static readonly SuperType Map = new SuperType( "Map", InnerEnum.Map, 0, _fallbackComparator );
			  public static readonly SuperType Node = new SuperType( "Node", InnerEnum.Node, 1, _nodeComparator );
			  public static readonly SuperType Relationship = new SuperType( "Relationship", InnerEnum.Relationship, 2, _relationshipComparator );
			  public static readonly SuperType List = new SuperType( "List", InnerEnum.List, 3, LIST_COMPARATOR );
			  public static readonly SuperType Path = new SuperType( "Path", InnerEnum.Path, 4, _pathComparator );
			  public static readonly SuperType String = new SuperType( "String", InnerEnum.String, 5, _stringComparator );
			  public static readonly SuperType Boolean = new SuperType( "Boolean", InnerEnum.Boolean, 6, _booleanComparator );
			  public static readonly SuperType Number = new SuperType( "Number", InnerEnum.Number, 7, _numberComparator );
			  public static readonly SuperType Void = new SuperType( "Void", InnerEnum.Void, 8, _voidComparator );

			  private static readonly IList<SuperType> valueList = new List<SuperType>();

			  static SuperType()
			  {
				  valueList.Add( Map );
				  valueList.Add( Node );
				  valueList.Add( Relationship );
				  valueList.Add( List );
				  valueList.Add( Path );
				  valueList.Add( String );
				  valueList.Add( Boolean );
				  valueList.Add( Number );
				  valueList.Add( Void );
			  }

			  public enum InnerEnum
			  {
				  Map,
				  Node,
				  Relationship,
				  List,
				  Path,
				  String,
				  Boolean,
				  Number,
				  Void
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Public readonly;
			  internal Public readonly;

			  internal SuperType( string name, InnerEnum innerEnum, int typeId, System.Collections.IComparer comparator )
			  {
					this.TypeId = typeId;
					this.Comparator = comparator;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public bool IsSuperTypeOf( object value )
			  {
					return this == OfValue( value );
			  }

			  public static SuperType OfValue( object value )
			  {
					if ( value is string || value is char? )
					{
						 return STRING;
					}
					else if ( value is Number )
					{
						 return NUMBER;
					}
					else if ( value is bool? )
					{
						 return BOOLEAN;
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (value instanceof java.util.Map<?,?>)
					else if ( value is IDictionary<object, ?> )
					{
						 return MAP;
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (value instanceof java.util.List<?> || value.getClass().isArray())
					else if ( value is IList<object> || value.GetType().IsArray )
					{
						 return LIST;
					}
					else if ( value is VirtualNodeValue )
					{
						 if ( ( ( VirtualNodeValue ) value ).id() == -1 )
						 {
							  return VOID;
						 }
						 return NODE;
					}
					else if ( value is VirtualRelationshipValue )
					{
						 if ( ( ( VirtualRelationshipValue ) value ).id() == -1 )
						 {
							  return VOID;
						 }
						 return RELATIONSHIP;
					}
					// TODO is Path really the class that compiled runtime will be using?
					else if ( value is Path )
					{
						 return PATH;
					}
					throw new UnorderableValueException( value.GetType().Name );
			  }

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           public static final java.util.Comparator<SuperType> TYPE_ID_COMPARATOR = java.util.Comparator.comparingInt(left -> left.typeId);

			  private static readonly IList<SuperType> valueList = new List<SuperType>();

			  static SuperType()
			  {
				  valueList.Add( Map );
				  valueList.Add( Node );
				  valueList.Add( Relationship );
				  valueList.Add( List );
				  valueList.Add( Path );
				  valueList.Add( String );
				  valueList.Add( Boolean );
				  valueList.Add( Number );
				  valueList.Add( Void );
				  valueList.Add( public );
			  }

			  public enum InnerEnum
			  {
				  Map,
				  Node,
				  Relationship,
				  List,
				  Path,
				  String,
				  Boolean,
				  Number,
				  Void,
				  public
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			 public static IList<SuperType> values()
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

			 public static SuperType valueOf( string name )
			 {
				 foreach ( SuperType enumInstance in SuperType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 // NOTE: nulls are handled at the top of the public compare() method
		 // so the type-specific comparators should not check arguments for null

		 private static IComparer<object> _fallbackComparator = ( lhs, rhs ) =>
		 {
		  if ( lhs.GetType().IsAssignableFrom(rhs.GetType()) && lhs is IComparable && rhs is IComparable )
		  {
				return ( ( IComparable ) lhs ).CompareTo( rhs );
		  }

		  throw new IncomparableValuesException( lhs.GetType().Name, rhs.GetType().Name );
		 };

		 private static IComparer<object> _voidComparator = ( lhs, rhs ) => 0;

		 private static IComparer<Number> _numberComparator = ( lhs, rhs ) =>
		 {
		  // If floats, compare float values. If integer types, compare long values
		  if ( lhs is double? && rhs is float? )
		  {
				return ( ( double? ) lhs ).compareTo( rhs.doubleValue() );
		  }
		  else if ( lhs is float? && rhs is double? )
		  {
				return -( ( double? ) rhs ).compareTo( lhs.doubleValue() );
		  }
		  else if ( lhs is float? && rhs is float? )
		  {
				return ( ( float? ) lhs ).compareTo( ( float? ) rhs );
		  }
		  else if ( lhs is double? && rhs is double? )
		  {
				return ( ( double? ) lhs ).compareTo( ( double? ) rhs );
		  }
		  // Right hand side is neither Float nor Double
		  else if ( lhs is double? || lhs is float? )
		  {
				return MathUtil.compareDoubleAgainstLong( lhs.doubleValue(), rhs.longValue() );
		  }
		  // Left hand side is neither Float nor Double
		  else if ( rhs is double? || rhs is float? )
		  {
				return -MathUtil.compareDoubleAgainstLong( rhs.doubleValue(), lhs.longValue() );
		  }
		  // Everything else is a long from Cypher's point-of-view
		  return Long.compare( lhs.longValue(), rhs.longValue() );
		 };

		 private static IComparer<object> _stringComparator = ( lhs, rhs ) =>
		 {
		  if ( lhs is char? && rhs is string )
		  {
				return lhs.ToString().CompareTo((string) rhs);
		  }
		  else if ( lhs is string && rhs is char? )
		  {
				return ( ( string ) lhs ).CompareTo( rhs.ToString() );
		  }
		  else
		  {
				return ( ( IComparable ) lhs ).CompareTo( rhs );
		  }
		 };

		 private static IComparer<bool> _booleanComparator = bool?.compareTo;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static IComparer<VirtualNodeValue> _nodeComparator = System.Collections.IComparer.comparingLong( VirtualNodeValue::id );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static IComparer<VirtualRelationshipValue> _relationshipComparator = System.Collections.IComparer.comparingLong( VirtualRelationshipValue::id );

		 // TODO test
		 private static IComparer<Path> _pathComparator = ( lhs, rhs ) =>
		 {
		  IEnumerator<PropertyContainer> lhsIter = lhs.GetEnumerator();
		  IEnumerator<PropertyContainer> rhsIter = rhs.GetEnumerator();
		  while ( lhsIter.hasNext() && rhsIter.hasNext() )
		  {
				int result = Compare( lhsIter.next(), rhsIter.next() );
				if ( 0 != result )
				{
					 return result;
				}
		  }
		  return ( lhsIter.hasNext() ) ? 1 : (rhsIter.hasNext()) ? -1 : 0;
		 };

		 private static IComparer<object> LIST_COMPARATOR = new ComparatorAnonymousInnerClass();

		 private class ComparatorAnonymousInnerClass : IComparer<object>
		 {
			 public int compare( object lhs, object rhs )
			 {
				  System.Collections.IEnumerator lhsIter = toIterator( lhs );
				  System.Collections.IEnumerator rhsIter = toIterator( rhs );
				  while ( lhsIter.MoveNext() && rhsIter.MoveNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						int result = CypherOrderability.Compare( lhsIter.Current, rhsIter.next() );
						if ( 0 != result )
						{
							 return result;
						}
				  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return ( lhsIter.hasNext() ) ? 1 : (rhsIter.hasNext()) ? -1 : 0;
			 }

			 private System.Collections.IEnumerator toIterator( object o )
			 {
				  Type clazz = o.GetType();
				  if ( clazz.IsAssignableFrom( typeof( System.Collections.IEnumerable ) ) )
				  {
						return ( ( System.Collections.IEnumerable ) o ).GetEnumerator();
				  }
				  else if ( clazz.IsAssignableFrom( typeof( object[] ) ) )
				  {
						return Arrays.stream( ( object[] ) o ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( int[] ) ) )
				  {
						return IntStream.of( ( int[] ) o ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( long[] ) ) )
				  {
						return LongStream.of( ( long[] ) o ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( float[] ) ) )
				  {
						return IntStream.range( 0, ( ( float[] ) o ).Length ).mapToObj( i => ( ( float[] ) o )[i] ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( double[] ) ) )
				  {
						return DoubleStream.of( ( double[] ) o ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( string[] ) ) )
				  {
						return Arrays.stream( ( string[] ) o ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( bool[] ) ) )
				  {
						// TODO Is there a better way to covert boolean[] to Iterator?
						return IntStream.range( 0, ( ( bool[] ) o ).Length ).mapToObj( i => ( ( bool[] ) o )[i] ).GetEnumerator();
				  }
				  else if ( clazz.Equals( typeof( bool?[] ) ) )
				  {
						return Arrays.stream( ( bool?[] ) o ).GetEnumerator();
				  }
				  else
				  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						throw new System.NotSupportedException( format( "Can not convert to iterator: %s", clazz.FullName ) );
				  }
			 }
		 }
	}

}