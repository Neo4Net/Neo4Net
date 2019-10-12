/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class SharedLockTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpgradeToUpdateLock()
		 public virtual void ShouldUpgradeToUpdateLock()
		 {
			  // Given
			  ForsetiClient clientA = mock( typeof( ForsetiClient ) );
			  ForsetiClient clientB = mock( typeof( ForsetiClient ) );

			  SharedLock @lock = new SharedLock( clientA );
			  @lock.Acquire( clientB );

			  // When
			  assertTrue( @lock.TryAcquireUpdateLock( clientA ) );

			  // Then
			  assertThat( @lock.NumberOfHolders(), equalTo(2) );
			  assertThat( @lock.UpdateLock, equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseSharedLock()
		 public virtual void ShouldReleaseSharedLock()
		 {
			  // Given
			  ForsetiClient clientA = mock( typeof( ForsetiClient ) );
			  SharedLock @lock = new SharedLock( clientA );

			  // When
			  assertTrue( @lock.Release( clientA ) );

			  // Then
			  assertThat( @lock.NumberOfHolders(), equalTo(0) );
			  assertThat( @lock.UpdateLock, equalTo( false ) );
		 }

	}

}