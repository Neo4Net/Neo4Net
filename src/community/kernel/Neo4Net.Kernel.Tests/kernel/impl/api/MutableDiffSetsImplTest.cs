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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using Neo4Net.Kernel.impl.util.diffsets;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsCollectionContaining.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;

	public class MutableDiffSetsImplTest
	{
		 private static readonly System.Predicate<long> _oddFilter = item => item % 2 != 0;

		 private readonly MutableDiffSetsImpl<long> _diffSets = new MutableDiffSetsImpl<long>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAdd()
		 public virtual void TestAdd()
		 {
			  // WHEN
			  _diffSets.add( 1L );
			  _diffSets.add( 2L );

			  // THEN
			  assertEquals( asSet( 1L, 2L ), _diffSets.Added );
			  assertTrue( _diffSets.Removed.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemove()
		 public virtual void TestRemove()
		 {
			  // WHEN
			  _diffSets.add( 1L );
			  _diffSets.remove( 2L );

			  // THEN
			  assertEquals( asSet( 1L ), _diffSets.Added );
			  assertEquals( asSet( 2L ), _diffSets.Removed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddRemove()
		 public virtual void TestAddRemove()
		 {
			  // WHEN
			  _diffSets.add( 1L );
			  _diffSets.remove( 1L );

			  // THEN
			  assertTrue( _diffSets.Added.Count == 0 );
			  assertTrue( _diffSets.Removed.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveAdd()
		 public virtual void TestRemoveAdd()
		 {
			  // WHEN
			  _diffSets.remove( 1L );
			  _diffSets.add( 1L );

			  // THEN
			  assertTrue( _diffSets.Added.Count == 0 );
			  assertTrue( _diffSets.Removed.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIsAddedOrRemoved()
		 public virtual void TestIsAddedOrRemoved()
		 {
			  // WHEN
			  _diffSets.add( 1L );
			  _diffSets.remove( 10L );

			  // THEN
			  assertTrue( _diffSets.isAdded( 1L ) );
			  assertTrue( !_diffSets.isAdded( 2L ) );
			  assertTrue( _diffSets.isRemoved( 10L ) );
			  assertTrue( !_diffSets.isRemoved( 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddRemoveAll()
		 public virtual void TestAddRemoveAll()
		 {
			  // WHEN
			  _diffSets.addAll( iterator( 1L, 2L ) );
			  _diffSets.removeAll( iterator( 2L, 3L ) );

			  // THEN
			  assertEquals( asSet( 1L ), _diffSets.Added );
			  assertEquals( asSet( 3L ), _diffSets.Removed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFilterAdded()
		 public virtual void TestFilterAdded()
		 {
			  // GIVEN
			  _diffSets.addAll( iterator( 1L, 2L ) );
			  _diffSets.removeAll( iterator( 3L, 4L ) );

			  // WHEN
			  MutableDiffSetsImpl<long> filtered = _diffSets.filterAdded( _oddFilter );

			  // THEN
			  assertEquals( asSet( 1L ), filtered.Added );
			  assertEquals( asSet( 3L, 4L ), filtered.Removed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReturnSourceFromApplyWithEmptyDiffSets()
		 public virtual void TestReturnSourceFromApplyWithEmptyDiffSets()
		 {
			  // WHEN
			  IEnumerator<long> result = _diffSets.apply( singletonList( 18L ).GetEnumerator() );

			  // THEN
			  assertEquals( singletonList( 18L ), asCollection( result ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAppendAddedToSourceInApply()
		 public virtual void TestAppendAddedToSourceInApply()
		 {
			  // GIVEN
			  _diffSets.add( 52L );
			  _diffSets.remove( 43L );

			  // WHEN
			  IEnumerator<long> result = _diffSets.apply( singletonList( 18L ).GetEnumerator() );

			  // THEN
			  assertEquals( asList( 18L, 52L ), asCollection( result ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFilterRemovedFromSourceInApply()
		 public virtual void TestFilterRemovedFromSourceInApply()
		 {
			  // GIVEN
			  _diffSets.remove( 43L );

			  // WHEN
			  IEnumerator<long> result = _diffSets.apply( asList( 42L, 43L, 44L ).GetEnumerator() );

			  // THEN
			  assertEquals( asList( 42L, 44L ), asCollection( result ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFilterAddedFromSourceInApply()
		 public virtual void TestFilterAddedFromSourceInApply()
		 {
			  // GIVEN
			  _diffSets.add( 42L );
			  _diffSets.add( 44L );

			  // WHEN
			  IEnumerator<long> result = _diffSets.apply( asList( 42L, 43L ).GetEnumerator() );

			  // THEN
			  ICollection<long> collectedResult = asCollection( result );
			  assertEquals( 3, collectedResult.Count );
			  assertThat( collectedResult, hasItems( 43L, 42L, 44L ) );
		 }
	}

}