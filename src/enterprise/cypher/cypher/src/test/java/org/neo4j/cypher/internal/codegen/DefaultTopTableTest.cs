using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Cypher.Internal.codegen
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class DefaultTopTableTest
	{
		 private static long?[] _testValues = new long?[]{ 7L, 4L, 5L, 0L, 3L, 4L, 8L, 6L, 1L, 9L, 2L };

		 private static long[] _expectedValues = new long[]{ 0L, 1L, 2L, 3L, 4L, 4L, 5L, 6L, 7L, 8L, 9L };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleAddingMoreValuesThanCapacity()
		 public virtual void ShouldHandleAddingMoreValuesThanCapacity()
		 {
			  DefaultTopTable table = new DefaultTopTable( 7 );
			  foreach ( long? i in _testValues )
			  {
					table.add( i );
			  }

			  table.sort();

			  IEnumerator<object> iterator = table.GetEnumerator();

			  for ( int i = 0; i < 7; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					long value = ( long ) iterator.next();
					assertEquals( _expectedValues[i], value );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWhenNotCompletelyFilledToCapacity()
		 public virtual void ShouldHandleWhenNotCompletelyFilledToCapacity()
		 {
			  DefaultTopTable table = new DefaultTopTable( 20 );
			  foreach ( long? i in _testValues )
			  {
					table.add( i );
			  }

			  table.sort();

			  IEnumerator<object> iterator = table.GetEnumerator();

			  for ( int i = 0; i < _testValues.Length; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					long value = ( long ) iterator.next();
					assertEquals( _expectedValues[i], value );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWhenEmpty()
		 public virtual void ShouldHandleWhenEmpty()
		 {
			  DefaultTopTable table = new DefaultTopTable( 10 );

			  table.sort();

			  IEnumerator<object> iterator = table.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnInitializeToZeroCapacity()
		 public virtual void ShouldThrowOnInitializeToZeroCapacity()
		 {
			  Exception.expect( typeof( System.ArgumentException ) );
			  new DefaultTopTable( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnInitializeToNegativeCapacity()
		 public virtual void ShouldThrowOnInitializeToNegativeCapacity()
		 {
			  Exception.expect( typeof( System.ArgumentException ) );
			  new DefaultTopTable( -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnSortNotCalledBeforeIterator()
		 public virtual void ShouldThrowOnSortNotCalledBeforeIterator()
		 {
			  DefaultTopTable table = new DefaultTopTable( 5 );
			  foreach ( long? i in _testValues )
			  {
					table.add( i );
			  }

			  // We forgot to call sort() here...

			  Exception.expect( typeof( System.InvalidOperationException ) );
			  table.GetEnumerator();
		 }
	}

}