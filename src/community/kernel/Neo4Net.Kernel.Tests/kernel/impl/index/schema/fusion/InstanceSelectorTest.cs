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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.STRING;

	public class InstanceSelectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelect()
		 public virtual void ShouldSelect()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0", STRING, "1" );

			  // when
			  string select0 = selector.Select( NUMBER );
			  // then
			  assertEquals( "0", select0 );

			  // when
			  string select1 = selector.Select( STRING );
			  // then
			  assertEquals( "1", select1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnNonInstantiatedSelect()
		 public virtual void ShouldThrowOnNonInstantiatedSelect()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0" );

			  try
			  {
					// when
					selector.Select( STRING );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnNonInstantiatedFlatMap()
		 public virtual void ShouldThrowOnNonInstantiatedFlatMap()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0" );

			  // when
			  try
			  {
					selector.Transform( int?.parseInt );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnNonInstantiatedMap()
		 public virtual void ShouldThrowOnNonInstantiatedMap()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0" );

			  // when
			  try
			  {
					selector.Map( int?.parseInt );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlatMap()
		 public virtual void ShouldFlatMap()
		 {
			  // given
			  InstanceSelector<string> selector = SelectorFilledWithOrdinal();

			  // when
			  IList<int> actual = Iterables.asList( selector.Transform( int?.parseInt ) );
			  IList<int> expected = java.util.Enum.GetValues( typeof( IndexSlot ) ).Select( Enum.ordinal ).ToList();

			  // then
			  assertEquals( expected.Count, actual.Count );
			  foreach ( int? i in expected )
			  {
					assertTrue( actual.Contains( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMap()
		 public virtual void ShouldMap()
		 {
			  // given
			  InstanceSelector<string> selector = SelectorFilledWithOrdinal();

			  // when
			  Dictionary<IndexSlot, int> actual = selector.Map( int?.parseInt );

			  // then
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					assertEquals( ( int )slot, actual[slot] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ResultOfMethodCallIgnored") @Test public void shouldThrowOnNonInstantiatedForAll()
		 public virtual void ShouldThrowOnNonInstantiatedForAll()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0" );

			  // when
			  try
			  {
					selector.ForAll( int?.parseInt );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForAll()
		 public virtual void ShouldForAll()
		 {
			  // given
			  InstanceSelector<string> selector = SelectorFilledWithOrdinal();

			  // when
			  MutableInt count = new MutableInt();
			  selector.ForAll( s => count.increment() );

			  // then
			  assertEquals( Enum.GetValues( typeof( IndexSlot ) ).length, count.intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ResultOfMethodCallIgnored") @Test public void shouldNotThrowOnNonInstantiatedClose()
		 public virtual void ShouldNotThrowOnNonInstantiatedClose()
		 {
			  // given
			  InstanceSelector<string> selector = selector( NUMBER, "0" );

			  // when
			  selector.Close( int?.parseInt );

			  // then
			  // good
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAll()
		 public virtual void ShouldCloseAll()
		 {
			  // given
			  InstanceSelector<string> selector = SelectorFilledWithOrdinal();

			  // when
			  MutableInt count = new MutableInt();
			  selector.Close( s => count.increment() );

			  // then
			  assertEquals( Enum.GetValues( typeof( IndexSlot ) ).length, count.intValue() );
		 }

		 private InstanceSelector<string> Selector( params object[] mapping )
		 {
			  Dictionary<IndexSlot, string> map = new Dictionary<IndexSlot, string>( typeof( IndexSlot ) );
			  int i = 0;
			  while ( i < mapping.Length )
			  {
					map[( IndexSlot ) mapping[i++]] = ( string ) mapping[i++];
			  }
			  return new InstanceSelector<string>( map );
		 }

		 private InstanceSelector<string> SelectorFilledWithOrdinal()
		 {
			  Dictionary<IndexSlot, string> map = new Dictionary<IndexSlot, string>( typeof( IndexSlot ) );
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					map[slot] = Convert.ToString( ( int )slot );
			  }
			  return new InstanceSelector<string>( map );
		 }
	}

}