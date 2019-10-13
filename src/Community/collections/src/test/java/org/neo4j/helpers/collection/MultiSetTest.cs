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
namespace Neo4Net.Helpers.Collections
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class MultiSetTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void anEmptySetContainsNothing()
		 public virtual void AnEmptySetContainsNothing()
		 {
			  // given
			  object aValue = new object();

			  // when
			  MultiSet<object> emptyMultiSet = new MultiSet<object>();

			  // then
			  assertTrue( emptyMultiSet.Empty );
			  assertEquals( 0, emptyMultiSet.Size() );
			  assertEquals( 0, emptyMultiSet.UniqueSize() );
			  assertFalse( emptyMultiSet.Contains( aValue ) );
			  assertEquals( 0, emptyMultiSet.Count( aValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddAnElementToTheMultiSet()
		 public virtual void ShouldAddAnElementToTheMultiSet()
		 {
			  // given
			  MultiSet<object> multiSet = new MultiSet<object>();
			  object value = new object();

			  // when
			  long count = multiSet.Add( value );

			  // then
			  assertEquals( 1, count );
			  assertFalse( multiSet.Empty );
			  assertEquals( 1, multiSet.Size() );
			  assertEquals( 1, multiSet.UniqueSize() );
			  assertTrue( multiSet.Contains( value ) );
			  assertEquals( 1, multiSet.Count( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveAnElementFromTheMultiSet()
		 public virtual void ShouldRemoveAnElementFromTheMultiSet()
		 {
			  // given
			  MultiSet<object> multiSet = new MultiSet<object>();
			  object value = new object();
			  multiSet.Add( value );

			  // when
			  long count = multiSet.Remove( value );

			  // then
			  assertEquals( 0, count );
			  assertTrue( multiSet.Empty );
			  assertEquals( 0, multiSet.Size() );
			  assertEquals( 0, multiSet.UniqueSize() );
			  assertFalse( multiSet.Contains( value ) );
			  assertEquals( 0, multiSet.Count( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddAnElementTwice()
		 public virtual void ShouldAddAnElementTwice()
		 {
			  // given
			  MultiSet<object> multiSet = new MultiSet<object>();
			  object value = new object();
			  multiSet.Add( value );

			  // when
			  long count = multiSet.Add( value );

			  // then
			  assertEquals( 2, count );
			  assertFalse( multiSet.Empty );
			  assertEquals( 2, multiSet.Size() );
			  assertEquals( 1, multiSet.UniqueSize() );
			  assertTrue( multiSet.Contains( value ) );
			  assertEquals( 2, multiSet.Count( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveAnElementWhenMultiElementArePresentInTheMultiSet()
		 public virtual void ShouldRemoveAnElementWhenMultiElementArePresentInTheMultiSet()
		 {
			  // given
			  MultiSet<object> multiSet = new MultiSet<object>();
			  object value = new object();
			  multiSet.Add( value );
			  multiSet.Add( value );

			  // when
			  long count = multiSet.Remove( value );

			  // then
			  assertEquals( 1, count );
			  assertFalse( multiSet.Empty );
			  assertEquals( 1, multiSet.Size() );
			  assertEquals( 1, multiSet.UniqueSize() );
			  assertTrue( multiSet.Contains( value ) );
			  assertEquals( 1, multiSet.Count( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTheMultiSet()
		 public virtual void ShouldClearTheMultiSet()
		 {
			  // given
			  MultiSet<object> multiSet = new MultiSet<object>();
			  object value = new object();
			  multiSet.Add( value );
			  multiSet.Add( value );
			  multiSet.Add( new object() );

			  // when
			  multiSet.Clear();

			  // then
			  assertTrue( multiSet.Empty );
			  assertEquals( 0, multiSet.Size() );
			  assertEquals( 0, multiSet.UniqueSize() );
			  assertFalse( multiSet.Contains( value ) );
			  assertEquals( 0, multiSet.Count( value ) );
		 }
	}

}