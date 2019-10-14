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
namespace Neo4Net.Collections
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveArrays.union;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class PrimitiveArraysUnionTest
	public class PrimitiveArraysUnionTest
	{
		 private static readonly long _seed = ThreadLocalRandom.current().nextLong();
		 private static readonly Random _random = new Random( _seed );
		 private const int MINIMUM_RANDOM_SIZE = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> parameters()
		 public static IEnumerable<object[]> Parameters()
		 {
			  IList<object[]> inputs = Stream.generate( PrimitiveArraysUnionTest.randomInput ).limit( 300 ).collect( Collectors.toList() );
			  IList<object[]> manuallyDefinedValues = Arrays.asList( Lhs( 1, 2, 3 ).rhs( 1, 2, 3 ).expectLhs(), Lhs(1, 2, 3).rhs(1).expectLhs(), Lhs(1, 2, 3).rhs(2).expectLhs(), Lhs(1, 2, 3).rhs(3).expectLhs(), Lhs(1).rhs(1, 2, 3).expectRhs(), Lhs(2).rhs(1, 2, 3).expectRhs(), Lhs(3).rhs(1, 2, 3).expectRhs(), Lhs(1, 2, 3).rhs(4, 5, 6).expect(1, 2, 3, 4, 5, 6), Lhs(1, 3, 5).rhs(2, 4, 6).expect(1, 2, 3, 4, 5, 6), Lhs(1, 2, 3, 5).rhs(2, 4, 6).expect(1, 2, 3, 4, 5, 6), Lhs(2, 3, 4, 7, 8, 9, 12, 16, 19).rhs(4, 6, 9, 11, 12, 15).expect(2, 3, 4, 6, 7, 8, 9, 11, 12, 15, 16, 19), Lhs(10, 13).rhs(13, 18).expect(10, 13, 18), Lhs(13, 18).rhs(10, 13).expect(10, 13, 18) );
			  ( ( IList<object[]> )inputs ).AddRange( manuallyDefinedValues );
			  return inputs;
		 }

		 private readonly int[] _lhs;
		 private readonly int[] _rhs;
		 private readonly int[] _expected;

		 public PrimitiveArraysUnionTest( Input input )
		 {
			  this._lhs = input.Lhs;
			  this._rhs = input.Rhs;
			  this._expected = input.Expected;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUnion()
		 public virtual void TestUnion()
		 {
			  int[] actual = union( _lhs, _rhs );
			  if ( _lhs == _expected || _rhs == _expected )
			  {
					assertSame( _expected, actual );
			  }
			  else
			  {
					assertArrayEquals( "Arrays should be equal. Test seed value: " + _seed, _expected, actual );
			  }
		 }

		 private static Input.Lhs Lhs( params int[] lhs )
		 {
			  return new Input.Lhs( lhs );
		 }

		 internal class Input
		 {
			  internal readonly int[] Lhs;
			  internal readonly int[] Rhs;
			  internal readonly int[] Expected;

			  internal Input( int[] lhs, int[] rhs, int[] expected )
			  {
					this.Lhs = lhs;
					this.Rhs = rhs;
					this.Expected = expected;
			  }

			  public override string ToString()
			  {
					return string.Format( "{{lhs={0}, rhs={1}, expected={2}}}", Arrays.ToString( Lhs ), Arrays.ToString( Rhs ), Arrays.ToString( Expected ) );
			  }

			  internal class Lhs
			  {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
					internal readonly int[] LhsConflict;

					internal Lhs( int[] lhs )
					{
						 this.LhsConflict = lhs;
					}

					internal virtual Rhs Rhs( params int[] rhs )
					{
						 return new Rhs( LhsConflict, rhs );
					}
			  }

			  internal class Rhs
			  {
					internal readonly int[] Lhs;
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
					internal readonly int[] RhsConflict;

					internal Rhs( int[] lhs, int[] rhs )
					{

						 this.Lhs = lhs;
						 this.RhsConflict = rhs;
					}

					internal virtual object[] Expect( params int[] expected )
					{
						 return new object[] { new Input( Lhs, RhsConflict, expected ) };
					}

					internal virtual object[] ExpectLhs()
					{
						 return new object[] { new Input( Lhs, RhsConflict, Lhs ) };
					}

					internal virtual object[] ExpectRhs()
					{
						 return new object[] { new Input( Lhs, RhsConflict, RhsConflict ) };
					}
			  }
		 }

		 private static object[] RandomInput()
		 {
			  int randomArraySize = MINIMUM_RANDOM_SIZE + _random.Next( 100 );
			  int lhsSize = _random.Next( randomArraySize );
			  int rhsSize = randomArraySize - lhsSize;

			  int[] resultValues = new int[randomArraySize];
			  int[] lhs = new int[lhsSize];
			  int[] rhs = new int[rhsSize];

			  int lhsSideItems = 0;
			  int rhsSideItems = 0;

			  int index = 0;
			  int value = _random.Next( 10 );
			  do
			  {
					if ( _random.nextBoolean() )
					{
						 if ( rhsSideItems < rhsSize )
						 {
							  rhs[rhsSideItems++] = value;
						 }
						 else
						 {
							  lhs[lhsSideItems++] = value;
						 }
					}
					else
					{
						 if ( lhsSideItems < lhsSize )
						 {
							  lhs[lhsSideItems++] = value;
						 }
						 else
						 {
							  rhs[rhsSideItems++] = value;
						 }
					}
					resultValues[index++] = value;
					value += 1 + _random.Next( 10 );
			  } while ( index < randomArraySize );
			  return new object[]{ new Input( lhs, rhs, resultValues ) };
		 }
	}

}