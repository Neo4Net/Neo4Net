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
namespace Neo4Net.Consistency.checking
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.ByteArrayBitsManipulator.MAX_BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.ByteArrayBitsManipulator.MAX_SLOT_BITS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.ByteArrayBitsManipulator.MAX_SLOT_VALUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class ByteArrayBitsManipulatorTest
	internal class ByteArrayBitsManipulatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected Neo4Net.test.rule.RandomRule random;
		 protected internal RandomRule Random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMaxSlotSize()
		 internal virtual void ShouldHandleMaxSlotSize()
		 {
			  // given
			  ByteArrayBitsManipulator manipulator = new ByteArrayBitsManipulator( MAX_SLOT_BITS, 1 );
			  long[][] actual = new long[1_000][];
			  using ( ByteArray array = NumberArrayFactory.HEAP.newByteArray( actual.Length, new sbyte[MAX_BYTES] ) )
			  {
					// when
					for ( int i = 0; i < actual.Length; i++ )
					{
						 actual[i] = new long[] { Random.nextLong( MAX_SLOT_VALUE + 1 ), Random.nextBoolean() ? -1 : 0 };
						 Put( manipulator, array, i, actual[i] );
					}

					for ( int i = 0; i < actual.Length; i++ )
					{
						 Verify( manipulator, array, i, actual[i] );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTwoMaxSlotsAndSomeBooleans()
		 internal virtual void ShouldHandleTwoMaxSlotsAndSomeBooleans()
		 {
			  // given
			  ByteArrayBitsManipulator manipulator = new ByteArrayBitsManipulator( MAX_SLOT_BITS, MAX_SLOT_BITS, 1, 1, 1, 1 );
			  long[][] actual = new long[1_000][];
			  using ( ByteArray array = NumberArrayFactory.HEAP.newByteArray( actual.Length, new sbyte[MAX_BYTES] ) )
			  {
					// when
					for ( int i = 0; i < actual.Length; i++ )
					{
						 actual[i] = new long[] { Random.nextLong( MAX_SLOT_VALUE + 1 ), Random.nextLong( MAX_SLOT_VALUE + 1 ), Random.nextBoolean() ? -1 : 0, Random.nextBoolean() ? -1 : 0, Random.nextBoolean() ? -1 : 0, Random.nextBoolean() ? -1 : 0 };
						 Put( manipulator, array, i, actual[i] );
					}

					// then
					for ( int i = 0; i < actual.Length; i++ )
					{
						 Verify( manipulator, array, i, actual[i] );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMinusOne()
		 internal virtual void ShouldHandleMinusOne()
		 {
			  // given
			  ByteArrayBitsManipulator manipulator = new ByteArrayBitsManipulator( MAX_SLOT_BITS, 1 );
			  using ( ByteArray array = NumberArrayFactory.HEAP.newByteArray( 2, new sbyte[MAX_BYTES] ) )
			  {
					// when
					Put( manipulator, array, 0, -1, 0 );
					Put( manipulator, array, 1, -1, -1 );

					// then
					Verify( manipulator, array, 0, -1, 0 );
					Verify( manipulator, array, 1, -1, -1 );
			  }
		 }

		 private void Verify( ByteArrayBitsManipulator manipulator, ByteArray array, long index, params long[] values )
		 {
			  for ( int i = 0; i < values.Length; i++ )
			  {
					assertEquals( values[i], manipulator.Get( array, index, i ) );
			  }
		 }

		 private void Put( ByteArrayBitsManipulator manipulator, ByteArray array, long index, params long[] values )
		 {
			  for ( int i = 0; i < values.Length; i++ )
			  {
					manipulator.Set( array, index, i, values[i] );
			  }
		 }
	}

}