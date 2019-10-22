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
namespace Neo4Net.Kernel.Api.Index
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Strings = Neo4Net.Helpers.Strings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.createConstraint;

	/*
	 * The purpose of this test class is to make sure all index providers produce the same results.
	 *
	 * Indexes should always produce the same result as scanning all nodes and checking properties. By extending this
	 * class in the index provider module, all value types will be checked against the index provider.
	 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(value = Parameterized.class) public abstract class SchemaConstraintProviderApprovalTest
	public abstract class SchemaConstraintProviderApprovalTest
	{
		 // These are the values that will be checked.
		 public sealed class TestValue
		 {
			  public static readonly TestValue BooleanTrue = new TestValue( "BooleanTrue", InnerEnum.BooleanTrue, true );
			  public static readonly TestValue BooleanFalse = new TestValue( "BooleanFalse", InnerEnum.BooleanFalse, false );
			  public static readonly TestValue StringTrue = new TestValue( "StringTrue", InnerEnum.StringTrue, "true" );
			  public static readonly TestValue StringFalse = new TestValue( "StringFalse", InnerEnum.StringFalse, "false" );
			  public static readonly TestValue StringUpperA = new TestValue( "StringUpperA", InnerEnum.StringUpperA, "A" );
			  public static readonly TestValue StringLowerA = new TestValue( "StringLowerA", InnerEnum.StringLowerA, "a" );
			  public static readonly TestValue CharUpperA = new TestValue( "CharUpperA", InnerEnum.CharUpperA, 'B' );
			  public static readonly TestValue CharLowerA = new TestValue( "CharLowerA", InnerEnum.CharLowerA, 'b' );
			  public static readonly TestValue Int_42 = new TestValue( "Int_42", InnerEnum.Int_42, 42 );
			  public static readonly TestValue Long_42 = new TestValue( "Long_42", InnerEnum.Long_42, ( long ) 43 );
			  public static readonly TestValue LargeLong_1 = new TestValue( "LargeLong_1", InnerEnum.LargeLong_1, 4611686018427387905L );
			  public static readonly TestValue LargeLong_2 = new TestValue( "LargeLong_2", InnerEnum.LargeLong_2, 4611686018427387907L );
			  public static readonly TestValue Byte_42 = new TestValue( "Byte_42", InnerEnum.Byte_42, ( sbyte ) 44 );
			  public static readonly TestValue Double_42 = new TestValue( "Double_42", InnerEnum.Double_42, ( double ) 41 );
			  public static readonly TestValue DOUBLE_42andAHalf = new TestValue( "DOUBLE_42andAHalf", InnerEnum.DOUBLE_42andAHalf, 42.5d );
			  public static readonly TestValue Short_42 = new TestValue( "Short_42", InnerEnum.Short_42, ( short ) 45 );
			  public static readonly TestValue Float_42 = new TestValue( "Float_42", InnerEnum.Float_42, ( float ) 46 );
			  public static readonly TestValue FLOAT_42andAHalf = new TestValue( "FLOAT_42andAHalf", InnerEnum.FLOAT_42andAHalf, 41.5f );
			  public static readonly TestValue Point_123456Gps = new TestValue( "Point_123456Gps", InnerEnum.Point_123456Gps, Neo4Net.Values.Storable.Values.PointValue( Neo4Net.Values.Storable.CoordinateReferenceSystem.Wgs84, 12.3, 45.6 ) );
			  public static readonly TestValue Point_123456Car = new TestValue( "Point_123456Car", InnerEnum.Point_123456Car, Neo4Net.Values.Storable.Values.PointValue( Neo4Net.Values.Storable.CoordinateReferenceSystem.Cartesian, 123, 456 ) );
			  public static readonly TestValue Point_123456Gps_3d = new TestValue( "Point_123456Gps_3d", InnerEnum.Point_123456Gps_3d, Neo4Net.Values.Storable.Values.PointValue( Neo4Net.Values.Storable.CoordinateReferenceSystem.Wgs84_3d, 12.3, 45.6, 78.9 ) );
			  public static readonly TestValue Point_123456Car_3d = new TestValue( "Point_123456Car_3d", InnerEnum.Point_123456Car_3d, Neo4Net.Values.Storable.Values.PointValue( Neo4Net.Values.Storable.CoordinateReferenceSystem.Cartesian_3D, 123, 456, 789 ) );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_INTS(new int[]{1, 2, 3}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_LONGS(new long[]{4, 5, 6}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_LARGE_LONGS_1(new long[] { 4611686018427387905L }),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_LARGE_LONGS_2(new long[] { 4611686018427387906L }),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_LARGE_LONGS_3(new System.Nullable<long>[] { 4611686018425387907L }),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_LARGE_LONGS_4(new System.Nullable<long>[] { 4611686018425387908L }),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_BOOL_LIKE_STRING(new String[]{"true", "false", "true"}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_BOOLS(new boolean[]{true, false, true}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_DOUBLES(new double[]{7, 8, 9}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_STRING(new String[]{"a", "b", "c"}),
			  public static readonly TestValue EmptyArrayOfString = new TestValue( "EmptyArrayOfString", InnerEnum.EmptyArrayOfString, new string[0] );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ONE(new String[]{"", "||"}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OTHER(new String[]{"||", ""}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ANOTHER_ARRAY_OF_STRING(new String[]{"1|2|3"}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_CHAR(new char[]{'d', 'e', 'f'}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_POINTS_GPS(new org.Neo4Net.values.storable.PointValue[]{org.Neo4Net.values.storable.Values.pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, 12.3, 45.6)}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_POINTS_CAR(new org.Neo4Net.values.storable.PointValue[]{org.Neo4Net.values.storable.Values.pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian, 123, 456)}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_POINTS_GPS_3D(new org.Neo4Net.values.storable.PointValue[]{org.Neo4Net.values.storable.Values.pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84_3D, 12.3, 45.6, 78.9)}),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY_OF_POINTS_CAR_3D(new org.Neo4Net.values.storable.PointValue[]{org.Neo4Net.values.storable.Values.pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D, 123, 456, 789)});

			  private static readonly IList<TestValue> valueList = new List<TestValue>();

			  static TestValue()
			  {
				  valueList.Add( BooleanTrue );
				  valueList.Add( BooleanFalse );
				  valueList.Add( StringTrue );
				  valueList.Add( StringFalse );
				  valueList.Add( StringUpperA );
				  valueList.Add( StringLowerA );
				  valueList.Add( CharUpperA );
				  valueList.Add( CharLowerA );
				  valueList.Add( Int_42 );
				  valueList.Add( Long_42 );
				  valueList.Add( LargeLong_1 );
				  valueList.Add( LargeLong_2 );
				  valueList.Add( Byte_42 );
				  valueList.Add( Double_42 );
				  valueList.Add( DOUBLE_42andAHalf );
				  valueList.Add( Short_42 );
				  valueList.Add( Float_42 );
				  valueList.Add( FLOAT_42andAHalf );
				  valueList.Add( Point_123456Gps );
				  valueList.Add( Point_123456Car );
				  valueList.Add( Point_123456Gps_3d );
				  valueList.Add( Point_123456Car_3d );
				  valueList.Add( ARRAY_OF_INTS );
				  valueList.Add( ARRAY_OF_LONGS );
				  valueList.Add( ARRAY_OF_LARGE_LONGS_1 );
				  valueList.Add( ARRAY_OF_LARGE_LONGS_2 );
				  valueList.Add( ARRAY_OF_LARGE_LONGS_3 );
				  valueList.Add( ARRAY_OF_LARGE_LONGS_4 );
				  valueList.Add( ARRAY_OF_BOOL_LIKE_STRING );
				  valueList.Add( ARRAY_OF_BOOLS );
				  valueList.Add( ARRAY_OF_DOUBLES );
				  valueList.Add( ARRAY_OF_STRING );
				  valueList.Add( EmptyArrayOfString );
				  valueList.Add( ONE );
				  valueList.Add( OTHER );
				  valueList.Add( ANOTHER_ARRAY_OF_STRING );
				  valueList.Add( ARRAY_OF_CHAR );
				  valueList.Add( ARRAY_OF_POINTS_GPS );
				  valueList.Add( ARRAY_OF_POINTS_CAR );
				  valueList.Add( ARRAY_OF_POINTS_GPS_3D );
				  valueList.Add( ARRAY_OF_POINTS_CAR_3D );
			  }

			  public enum InnerEnum
			  {
				  BooleanTrue,
				  BooleanFalse,
				  StringTrue,
				  StringFalse,
				  StringUpperA,
				  StringLowerA,
				  CharUpperA,
				  CharLowerA,
				  Int_42,
				  Long_42,
				  LargeLong_1,
				  LargeLong_2,
				  Byte_42,
				  Double_42,
				  DOUBLE_42andAHalf,
				  Short_42,
				  Float_42,
				  FLOAT_42andAHalf,
				  Point_123456Gps,
				  Point_123456Car,
				  Point_123456Gps_3d,
				  Point_123456Car_3d,
				  ARRAY_OF_INTS,
				  ARRAY_OF_LONGS,
				  ARRAY_OF_LARGE_LONGS_1,
				  ARRAY_OF_LARGE_LONGS_2,
				  ARRAY_OF_LARGE_LONGS_3,
				  ARRAY_OF_LARGE_LONGS_4,
				  ARRAY_OF_BOOL_LIKE_STRING,
				  ARRAY_OF_BOOLS,
				  ARRAY_OF_DOUBLES,
				  ARRAY_OF_STRING,
				  EmptyArrayOfString,
				  ONE,
				  OTHER,
				  ANOTHER_ARRAY_OF_STRING,
				  ARRAY_OF_CHAR,
				  ARRAY_OF_POINTS_GPS,
				  ARRAY_OF_POINTS_CAR,
				  ARRAY_OF_POINTS_GPS_3D,
				  ARRAY_OF_POINTS_CAR_3D
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal readonly object value;

			  internal TestValue( string name, InnerEnum innerEnum, object value )
			  {
					this._value = value;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<TestValue> values()
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

			 public static TestValue valueOf( string name )
			 {
				 foreach ( TestValue enumInstance in TestValue.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private static IDictionary<TestValue, ISet<object>> _noIndexRun;
		 private static IDictionary<TestValue, ISet<object>> _constraintRun;

		 private readonly TestValue _currentValue;

		 public SchemaConstraintProviderApprovalTest( TestValue value )
		 {
			  _currentValue = value;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.Collection<TestValue> data()
		 public static ICollection<TestValue> Data()
		 {
			  return Arrays.asList( TestValue.values() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void init()
		 public static void Init()
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  foreach ( TestValue value in TestValue.values() )
			  {
					CreateNode( db, PROPERTY_KEY, value.value );
			  }

			  _noIndexRun = RunFindByLabelAndProperty( db );
			  createConstraint( db, label( LABEL ), PROPERTY_KEY );
			  _constraintRun = RunFindByLabelAndProperty( db );
			  Db.shutdown();
		 }

		 public const string LABEL = "Person";
		 public const string PROPERTY_KEY = "name";
		 public static readonly System.Func<Node, object> PropertyExtractor = node =>
		 {
		  object value = node.getProperty( PROPERTY_KEY );
		  if ( value.GetType().IsArray )
		  {
				return new ArrayEqualityObject( value );
		  }
		  return value;
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test()
		 public virtual void Test()
		 {
			  ISet<object> noIndexResult = Iterables.asSet( _noIndexRun[_currentValue] );
			  ISet<object> constraintResult = Iterables.asSet( _constraintRun[_currentValue] );

			  string errorMessage = _currentValue.ToString();

			  assertEquals( errorMessage, noIndexResult, constraintResult );
		 }

		 private static IDictionary<TestValue, ISet<object>> RunFindByLabelAndProperty( IGraphDatabaseService db )
		 {
			  Dictionary<TestValue, ISet<object>> results = new Dictionary<TestValue, ISet<object>>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( TestValue value in TestValue.values() )
					{
						 AddToResults( db, results, value );
					}
					tx.Success();
			  }
			  return results;
		 }

		 private static Node CreateNode( IGraphDatabaseService db, string propertyKey, object value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( LABEL ) );
					node.SetProperty( propertyKey, value );
					tx.Success();
					return node;
			  }
		 }

		 private static void AddToResults( IGraphDatabaseService db, Dictionary<TestValue, ISet<object>> results, TestValue value )
		 {
			  ResourceIterator<Node> foundNodes = Db.findNodes( label( LABEL ), PROPERTY_KEY, value.value );
			  ISet<object> propertyValues = asSet( Iterators.map( PropertyExtractor, foundNodes ) );
			  results[value] = propertyValues;
		 }

		 private class ArrayEqualityObject
		 {
			  internal readonly object Array;

			  internal ArrayEqualityObject( object array )
			  {
					this.Array = array;
			  }

			  public override int GetHashCode()
			  {
					return ArrayUtil.GetHashCode( Array );
			  }

			  public override bool Equals( object obj )
			  {
					return obj is ArrayEqualityObject && ArrayUtil.Equals( Array, ( ( ArrayEqualityObject ) obj ).Array );
			  }

			  public override string ToString()
			  {
					return Strings.prettyPrint( Array );
			  }
		 }
	}

}