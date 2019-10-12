using System;

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
namespace Neo4Net.Kernel.impl.locking
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class LockClientStateHolderTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowIncrementDecrementClientsWhileNotClosed()
		 public virtual void ShouldAllowIncrementDecrementClientsWhileNotClosed()
		 {
			  // given
			  LockClientStateHolder lockClientStateHolder = new LockClientStateHolder();

			  // expect
			  assertFalse( lockClientStateHolder.HasActiveClients() );
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  assertTrue( lockClientStateHolder.HasActiveClients() );
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.DecrementActiveClients();
			  lockClientStateHolder.DecrementActiveClients();
			  lockClientStateHolder.DecrementActiveClients();
			  assertFalse( lockClientStateHolder.HasActiveClients() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNewClientsWhenClosed()
		 public virtual void ShouldNotAllowNewClientsWhenClosed()
		 {
			  // given
			  LockClientStateHolder lockClientStateHolder = new LockClientStateHolder();

			  // when
			  lockClientStateHolder.StopClient();

			  // then
			  assertFalse( lockClientStateHolder.HasActiveClients() );
			  try
			  {
					lockClientStateHolder.incrementActiveClients( new NoOpClient() );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDecrementActiveItemAndDetectWhenFree()
		 public virtual void ShouldBeAbleToDecrementActiveItemAndDetectWhenFree()
		 {
			  // given
			  LockClientStateHolder lockClientStateHolder = new LockClientStateHolder();

			  // when
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.DecrementActiveClients();
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );

			  // expect
			  assertTrue( lockClientStateHolder.HasActiveClients() );

			  // and when
			  lockClientStateHolder.StopClient();

			  // expect
			  assertTrue( lockClientStateHolder.HasActiveClients() );
			  lockClientStateHolder.DecrementActiveClients();
			  assertTrue( lockClientStateHolder.HasActiveClients() );
			  lockClientStateHolder.DecrementActiveClients();
			  assertFalse( lockClientStateHolder.HasActiveClients() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToResetAndReuseClientState()
		 public virtual void ShouldBeAbleToResetAndReuseClientState()
		 {
			  // given
			  LockClientStateHolder lockClientStateHolder = new LockClientStateHolder();

			  // when
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  lockClientStateHolder.DecrementActiveClients();

			  // expect
			  assertTrue( lockClientStateHolder.HasActiveClients() );

			  // and when
			  lockClientStateHolder.StopClient();

			  // expect
			  assertTrue( lockClientStateHolder.HasActiveClients() );
			  assertTrue( lockClientStateHolder.Stopped );

			  // and when
			  lockClientStateHolder.Reset();

			  // expect
			  assertFalse( lockClientStateHolder.HasActiveClients() );
			  assertFalse( lockClientStateHolder.Stopped );

			  // when
			  lockClientStateHolder.incrementActiveClients( new NoOpClient() );
			  assertTrue( lockClientStateHolder.HasActiveClients() );
			  assertFalse( lockClientStateHolder.Stopped );
		 }

	}

}