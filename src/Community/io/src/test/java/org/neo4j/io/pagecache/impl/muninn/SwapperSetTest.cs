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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;
	using Matcher = org.hamcrest.Matcher;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using DummyPageSwapper = Neo4Net.Io.pagecache.tracing.DummyPageSwapper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class SwapperSetTest
	{
		 private SwapperSet _set;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _set = new SwapperSet();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReturnAllocationWithSwapper()
		 internal virtual void MustReturnAllocationWithSwapper()
		 {
			  DummyPageSwapper a = new DummyPageSwapper( "a", 42 );
			  DummyPageSwapper b = new DummyPageSwapper( "b", 43 );
			  int idA = _set.allocate( a );
			  int idB = _set.allocate( b );
			  SwapperSet.SwapperMapping allocA = _set.getAllocation( idA );
			  SwapperSet.SwapperMapping allocB = _set.getAllocation( idB );
			  assertThat( allocA.Swapper, @is( a ) );
			  assertThat( allocB.Swapper, @is( b ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void accessingFreedAllocationMustReturnNull()
		 internal virtual void AccessingFreedAllocationMustReturnNull()
		 {
			  int id = _set.allocate( new DummyPageSwapper( "a", 42 ) );
			  _set.free( id );
			  assertNull( _set.getAllocation( id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doubleFreeMustThrow()
		 internal virtual void DoubleFreeMustThrow()
		 {
			  int id = _set.allocate( new DummyPageSwapper( "a", 42 ) );
			  _set.free( id );
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _set.free(id) );
			  assertThat( exception.Message, containsString( "double free" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freedIdsMustNotBeReusedBeforeVacuum()
		 internal virtual void FreedIdsMustNotBeReusedBeforeVacuum()
		 {
			  PageSwapper swapper = new DummyPageSwapper( "a", 42 );
			  MutableIntSet ids = new IntHashSet( 10_000 );
			  for ( int i = 0; i < 10_000; i++ )
			  {
					AllocateFreeAndAssertNotReused( swapper, ids, i );
			  }
		 }

		 private void AllocateFreeAndAssertNotReused( PageSwapper swapper, MutableIntSet ids, int i )
		 {
			  int id = _set.allocate( swapper );
			  _set.free( id );
			  if ( !ids.add( id ) )
			  {
					fail( "Expected ids.add( id ) to return true for id " + id + " in iteration " + i + " but it instead returned false" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freedAllocationsMustBecomeAvailableAfterVacuum()
		 internal virtual void FreedAllocationsMustBecomeAvailableAfterVacuum()
		 {
			  MutableIntSet allocated = new IntHashSet();
			  MutableIntSet freed = new IntHashSet();
			  MutableIntSet vacuumed = new IntHashSet();
			  MutableIntSet reused = new IntHashSet();
			  PageSwapper swapper = new DummyPageSwapper( "a", 42 );

			  AllocateAndAddTenThousand( allocated, swapper );

			  allocated.forEach(id =>
			  {
				_set.free( id );
				freed.add( id );
			  });
			  _set.vacuum( vacuumed.addAll );

			  AllocateAndAddTenThousand( reused, swapper );

			  assertThat( allocated, @is( equalTo( freed ) ) );
			  assertThat( allocated, @is( equalTo( vacuumed ) ) );
			  assertThat( allocated, @is( equalTo( reused ) ) );
		 }

		 private void AllocateAndAddTenThousand( MutableIntSet allocated, PageSwapper swapper )
		 {
			  for ( int i = 0; i < 10_000; i++ )
			  {
					AllocateAndAdd( allocated, swapper );
			  }
		 }

		 private void AllocateAndAdd( MutableIntSet allocated, PageSwapper swapper )
		 {
			  int id = _set.allocate( swapper );
			  allocated.add( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void vacuumMustNotDustOffAnyIdsWhenNoneHaveBeenFreed()
		 internal virtual void VacuumMustNotDustOffAnyIdsWhenNoneHaveBeenFreed()
		 {
			  PageSwapper swapper = new DummyPageSwapper( "a", 42 );
			  for ( int i = 0; i < 100; i++ )
			  {
					_set.allocate( swapper );
			  }
			  MutableIntSet vacuumedIds = new IntHashSet();
			  _set.vacuum( vacuumedIds.addAll );
			  if ( !vacuumedIds.Empty )
			  {
					throw new AssertionError( "Vacuum found id " + vacuumedIds + " when it should have found nothing" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotUseZeroAsSwapperId()
		 internal virtual void MustNotUseZeroAsSwapperId()
		 {
			  PageSwapper swapper = new DummyPageSwapper( "a", 42 );
			  Matcher<int> isNotZero = @is( not( 0 ) );
			  for ( int i = 0; i < 10_000; i++ )
			  {
					assertThat( _set.allocate( swapper ), isNotZero );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void gettingAllocationZeroMustThrow()
		 internal virtual void GettingAllocationZeroMustThrow()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _set.getAllocation((short) 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freeOfIdZeroMustThrow()
		 internal virtual void FreeOfIdZeroMustThrow()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _set.free(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustKeepTrackOfAvailableSwapperIds()
		 internal virtual void MustKeepTrackOfAvailableSwapperIds()
		 {
			  PageSwapper swapper = new DummyPageSwapper( "a", 42 );
			  int initial = ( 1 << 21 ) - 2;
			  assertThat( _set.countAvailableIds(), @is(initial) );
			  int id = _set.allocate( swapper );
			  assertThat( _set.countAvailableIds(), @is(initial - 1) );
			  _set.free( id );
			  assertThat( _set.countAvailableIds(), @is(initial - 1) );
			  _set.vacuum(x =>
			  {
			  });
			  assertThat( _set.countAvailableIds(), @is(initial) );
		 }
	}

}