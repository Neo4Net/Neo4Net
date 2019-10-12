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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RawBitsTest
	public class RawBitsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public NumberLayout layout;
		 public NumberLayout Layout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> layouts()
		 public static IList<object[]> Layouts()
		 {
			  return new IList<object[]>
			  {
				  new object[]{ "Unique", new NumberLayoutUnique() },
				  new object[]{ "NonUnique", new NumberLayoutNonUnique() }
			  };
		 }

		 internal readonly IList<object> Objects = Arrays.asList( double.NegativeInfinity, -double.MaxValue, long.MinValue, long.MinValue + 1, int.MinValue, short.MinValue, sbyte.MinValue, 0, double.Epsilon, Double.MIN_NORMAL, float.Epsilon, Float.MIN_NORMAL, 1L, 1.1d, 1.2f, Math.E, Math.PI, ( sbyte ) 10, ( short ) 20, sbyte.MaxValue, short.MaxValue, int.MaxValue, 33554432, 33554432F, 33554433, 33554433F, 33554434, 33554434F, 9007199254740991L, 9007199254740991D, 9007199254740992L, 9007199254740992D, 9007199254740993L, 9007199254740993D, 9007199254740994L, 9007199254740994D, long.MaxValue, float.MaxValue, double.MaxValue, double.PositiveInfinity, Double.NaN, Math.nextDown( Math.E ), Math.nextUp( Math.E ), Math.nextDown( Math.PI ), Math.nextUp( Math.PI ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSortInSameOrderAsValueComparator()
		 public virtual void MustSortInSameOrderAsValueComparator()
		 {
			  // given
			  IList<Value> values = AsValueObjects( Objects );
			  IList<NumberIndexKey> numberIndexKeys = AsNumberIndexKeys( values );
			  Collections.shuffle( values );
			  Collections.shuffle( numberIndexKeys );

			  // when
			  values.sort( Values.COMPARATOR );
			  numberIndexKeys.sort( Layout );
			  IList<Value> actual = AsValues( numberIndexKeys );

			  // then
			  AssertSameOrder( actual, values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompareAllValuesToAllOtherValuesLikeValueComparator()
		 public virtual void ShouldCompareAllValuesToAllOtherValuesLikeValueComparator()
		 {
			  // given
			  IList<Value> values = AsValueObjects( Objects );
			  IList<NumberIndexKey> numberIndexKeys = AsNumberIndexKeys( values );
			  values.sort( Values.COMPARATOR );

			  // when
			  foreach ( NumberIndexKey numberKey in numberIndexKeys )
			  {
					IList<NumberIndexKey> withoutThisOne = new List<NumberIndexKey>( numberIndexKeys );
					assertTrue( withoutThisOne.Remove( numberKey ) );
					withoutThisOne = unmodifiableList( withoutThisOne );
					for ( int i = 0; i < withoutThisOne.Count; i++ )
					{
						 IList<NumberIndexKey> withThisOneInWrongPlace = new List<NumberIndexKey>( withoutThisOne );
						 withThisOneInWrongPlace.Insert( i, numberKey );
						 withThisOneInWrongPlace.sort( Layout );
						 IList<Value> actual = AsValues( withThisOneInWrongPlace );

						 // then
						 AssertSameOrder( actual, values );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSameCompareResultsAsValueCompare()
		 public virtual void ShouldHaveSameCompareResultsAsValueCompare()
		 {
			  // given
			  IList<Value> values = AsValueObjects( Objects );
			  IList<NumberIndexKey> numberIndexKeys = AsNumberIndexKeys( values );

			  // when
			  for ( int i = 0; i < values.Count; i++ )
			  {
					Value value1 = values[i];
					NumberIndexKey numberIndexKey1 = numberIndexKeys[i];
					for ( int j = 0; j < values.Count; j++ )
					{
						 // then
						 Value value2 = values[j];
						 NumberIndexKey numberIndexKey2 = numberIndexKeys[j];
						 assertEquals( Values.COMPARATOR.Compare( value1, value2 ), Layout.compare( numberIndexKey1, numberIndexKey2 ) );
						 assertEquals( Values.COMPARATOR.Compare( value2, value1 ), Layout.compare( numberIndexKey2, numberIndexKey1 ) );
					}
			  }
		 }

		 private IList<Value> AsValues( IList<NumberIndexKey> numberIndexKeys )
		 {
			  return numberIndexKeys.Select( k => RawBits.AsNumberValue( k.rawValueBits, k.type ) ).ToList();
		 }

		 private void AssertSameOrder( IList<Value> actual, IList<Value> values )
		 {
			  assertEquals( actual.Count, values.Count );
			  for ( int i = 0; i < actual.Count; i++ )
			  {
					Number actualAsNumber = ( Number ) actual[i].AsObject();
					Number valueAsNumber = ( Number ) values[i].AsObject();
					//noinspection StatementWithEmptyBody
					if ( double.IsNaN( actualAsNumber.doubleValue() ) && double.IsNaN(valueAsNumber.doubleValue()) )
					{
						 // Don't compare equals because NaN does not equal itself
					}
					else
					{
						 assertEquals( actual[i], values[i] );
					}
			  }
		 }

		 private IList<Value> AsValueObjects( IList<object> objects )
		 {
			  IList<Value> values = new List<Value>();
			  foreach ( object @object in objects )
			  {
					values.Add( Values.of( @object ) );
			  }
			  return values;
		 }

		 private IList<NumberIndexKey> AsNumberIndexKeys( IList<Value> values )
		 {
			  IList<NumberIndexKey> numberIndexKeys = new List<NumberIndexKey>();
			  foreach ( Value value in values )
			  {
					NumberIndexKey key = new NumberIndexKey();
					key.From( value );
					numberIndexKeys.Add( key );
			  }
			  return numberIndexKeys;
		 }
	}

}