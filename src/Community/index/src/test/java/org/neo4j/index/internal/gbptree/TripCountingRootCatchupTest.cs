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
namespace Neo4Net.Index.@internal.gbptree
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TripCountingRootCatchupTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustThrowOnConsecutiveCatchupsFromSamePage()
		 public virtual void MustThrowOnConsecutiveCatchupsFromSamePage()
		 {
			  // Given
			  TripCountingRootCatchup tripCountingRootCatchup = TripCounter;

			  // When
			  try
			  {
					for ( int i = 0; i < TripCountingRootCatchup.MaxTripCount; i++ )
					{
						 tripCountingRootCatchup.CatchupFrom( 10 );
					}
					fail( "Expected to throw" );
			  }
			  catch ( TreeInconsistencyException )
			  {
					// Then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotThrowOnInterleavedCatchups()
		 public virtual void MustNotThrowOnInterleavedCatchups()
		 {
			  // given
			  TripCountingRootCatchup tripCountingRootCatchup = TripCounter;

			  // When
			  for ( int i = 0; i < TripCountingRootCatchup.MaxTripCount * 4; i++ )
			  {
					tripCountingRootCatchup.CatchupFrom( i % 2 );
			  }

			  // then this should be fine
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReturnRootUsingProvidedSupplier()
		 public virtual void MustReturnRootUsingProvidedSupplier()
		 {
			  // given
			  Root expectedRoot = new Root( 1, 2 );
			  System.Func<Root> rootSupplier = () => expectedRoot;
			  TripCountingRootCatchup tripCountingRootCatchup = new TripCountingRootCatchup( rootSupplier );

			  // when
			  Root actualRoot = tripCountingRootCatchup.CatchupFrom( 10 );

			  // then
			  assertSame( expectedRoot, actualRoot );
		 }

		 private TripCountingRootCatchup TripCounter
		 {
			 get
			 {
				  Root root = new Root( 1, 2 );
				  return new TripCountingRootCatchup( () => root );
			 }
		 }
	}

}