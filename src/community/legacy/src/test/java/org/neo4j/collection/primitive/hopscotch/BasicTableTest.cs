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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.primitive.Primitive.VALUE_MARKER;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BasicTableTest
	public class BasicTableTest
	{
		 private readonly TableFactory _factory;

		 private static readonly long _seed = currentTimeMillis();
		 private static readonly Random _random = new Random( _seed );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> result = new List<object[]>();
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass2() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass3() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass4() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass5() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass6() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass7() } );
			  result.Add( new object[]{ new TableFactoryAnonymousInnerClass8() } );
			  return result;
		 }

		 private class TableFactoryAnonymousInnerClass : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new IntKeyTable( capacity, VALUE_MARKER );
			 }

			 public bool supportsLongs()
			 {
				  return false;
			 }

			 public object sampleValue()
			 {
				  return null;
			 }
		 }

		 private class TableFactoryAnonymousInnerClass2 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyTable( capacity, VALUE_MARKER );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return null;
			 }
		 }

		 private class TableFactoryAnonymousInnerClass3 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new IntKeyUnsafeTable( capacity, VALUE_MARKER, GlobalMemoryTracker.INSTANCE );
			 }

			 public bool supportsLongs()
			 {
				  return false;
			 }

			 public object sampleValue()
			 {
				  return null;
			 }
		 }

		 private class TableFactoryAnonymousInnerClass4 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyUnsafeTable( capacity, VALUE_MARKER, GlobalMemoryTracker.INSTANCE );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return null;
			 }
		 }

		 private class TableFactoryAnonymousInnerClass5 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyIntValueTable( capacity );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return new int[]{ _random.Next( int.MaxValue ) };
			 }
		 }

		 private class TableFactoryAnonymousInnerClass6 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyLongValueTable( capacity );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return new long[]{ Math.Abs( _random.nextLong() ) };
			 }
		 }

		 private class TableFactoryAnonymousInnerClass7 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyObjectValueTable( capacity );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return new long[]{ Math.Abs( _random.nextLong() ) };
			 }
		 }

		 private class TableFactoryAnonymousInnerClass8 : TableFactory
		 {
			 public Table newTable( int capacity )
			 {
				  return new LongKeyLongValueUnsafeTable( capacity, GlobalMemoryTracker.INSTANCE );
			 }

			 public bool supportsLongs()
			 {
				  return true;
			 }

			 public object sampleValue()
			 {
				  return new long[]{ Math.Abs( _random.nextLong() ) };
			 }
		 }

		 public BasicTableTest( TableFactory factory )
		 {
			  this._factory = factory;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAndGetSmallKey()
		 public virtual void shouldSetAndGetSmallKey()
		 {
			  using ( Table table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					long nullKey = table.nullKey();
					assertEquals( nullKey, table.key( 0 ) );

					// WHEN
					long key = 12345;
					int index = 2;
					table.put( index, key, _factory.sampleValue() );

					// THEN
					assertEquals( key, table.key( index ) );

					// WHEN/THEN
					table.remove( index );
					assertEquals( nullKey, table.key( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAndGetBigKey()
		 public virtual void shouldSetAndGetBigKey()
		 {
			  assumeTrue( _factory.supportsLongs() );
			  using ( Table table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					long nullKey = table.nullKey();
					assertEquals( nullKey, table.key( 0 ) );

					// WHEN
					long key = 0x24FCFF2FFL;
					int index = 2;
					table.put( index, key, _factory.sampleValue() );

					// THEN
					assertEquals( key, table.key( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveBigKey()
		 public virtual void shouldRemoveBigKey()
		 {
			  assumeTrue( _factory.supportsLongs() );
			  using ( Table table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					long nullKey = table.nullKey();
					long key = 0x24F1FF3FEL;
					int index = 5;
					table.put( index, key, _factory.sampleValue() );
					assertEquals( key, table.key( index ) );

					// WHEN
					table.remove( index );

					// THEN
					assertEquals( nullKey, table.key( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetHopBits()
		 public virtual void shouldSetHopBits()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (Table<?> table = factory.newTable(org.Neo4Net.collection.primitive.Primitive.DEFAULT_HEAP_CAPACITY))
			  using ( Table<object> table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					int index = 10;
					long hopBits = table.HopBits( index );
					assertEquals( 0L, hopBits );

					// WHEN
					table.PutHopBit( index, 2 );
					table.PutHopBit( index, 11 );

					// THEN
					assertEquals( ( 1L << 2 ) | ( 1L << 11 ), table.HopBits( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveHopBit()
		 public virtual void shouldMoveHopBit()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (Table<?> table = factory.newTable(org.Neo4Net.collection.primitive.Primitive.DEFAULT_HEAP_CAPACITY))
			  using ( Table<object> table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					int index = 10;
					table.PutHopBit( index, 2 );
					table.PutHopBit( index, 11 );

					// WHEN
					table.MoveHopBit( index, 2, 15 ); // will end up at 17

					// THEN
					assertEquals( ( 1L << 11 ) | ( 1L << 17 ), table.HopBits( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTable()
		 public virtual void shouldClearTable()
		 {
			  using ( Table table = _factory.newTable( Primitive.DEFAULT_HEAP_CAPACITY ) )
			  {
					// GIVEN
					int index = 3;
					long key = 123L;
					object value = _factory.sampleValue();
					table.put( index, key, value );
					assertEquals( key, table.key( index ) );

					// WHEN
					table.clear();

					// THEN
					assertEquals( table.nullKey(), table.key(index) );
			  }
		 }

		 private interface TableFactory
		 {
			  Table NewTable( int capacity );

			  object SampleValue();

			  bool SupportsLongs();
		 }
	}

}