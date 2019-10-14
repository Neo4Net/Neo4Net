using System.Threading;

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
namespace Neo4Net.Collections.Pooling
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class MarshlandPoolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotLooseObjectsWhenThreadsDie() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotLooseObjectsWhenThreadsDie()
		 {
			  // Given
			  Pool<object> delegatePool = mock( typeof( Pool ) );
			  when( delegatePool.Acquire() ).thenReturn(1337, -1);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MarshlandPool<Object> pool = new MarshlandPool<>(delegatePool);
			  MarshlandPool<object> pool = new MarshlandPool<object>( delegatePool );

			  // When
			  ClaimAndReleaseInSeparateThread( pool );

			  // Then
			  verify( delegatePool ).acquire();
			  verifyNoMoreInteractions( delegatePool );
			  AssertPoolEventuallyReturns( pool, 1337 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnToDelegatePoolIfLocalPoolIsFull()
		 internal virtual void ShouldReturnToDelegatePoolIfLocalPoolIsFull()
		 {
			  // Given
			  Pool<object> delegatePool = mock( typeof( Pool ) );
			  when( delegatePool.Acquire() ).thenReturn(1337);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MarshlandPool<Object> pool = new MarshlandPool<>(delegatePool);
			  MarshlandPool<object> pool = new MarshlandPool<object>( delegatePool );

			  object first = pool.Acquire();
			  object second = pool.Acquire();
			  object third = pool.Acquire();

			  // When
			  pool.Release( first );
			  pool.Release( second );
			  pool.Release( third );

			  // Then
			  verify( delegatePool, times( 3 ) ).acquire();
			  verify( delegatePool, times( 2 ) ).release( any() );
			  verifyNoMoreInteractions( delegatePool );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseAllSlotsOnClose()
		 internal virtual void ShouldReleaseAllSlotsOnClose()
		 {
			  // Given
			  Pool<object> delegatePool = mock( typeof( Pool ) );
			  when( delegatePool.Acquire() ).thenReturn(1337);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MarshlandPool<Object> pool = new MarshlandPool<>(delegatePool);
			  MarshlandPool<object> pool = new MarshlandPool<object>( delegatePool );

			  object first = pool.Acquire();
			  pool.Release( first );

			  // When
			  pool.Close();

			  // Then
			  verify( delegatePool, times( 1 ) ).acquire();
			  verify( delegatePool, times( 1 ) ).release( any() );
			  verifyNoMoreInteractions( delegatePool );
		 }

		 private void AssertPoolEventuallyReturns( Pool<object> pool, int expected )
		 {
			  long maxTime = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.SECONDS.toMillis(100);
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < maxTime )
			  {
					if ( pool.Acquire().Equals(expected) )
					{
						 return;
					}
			  }

			  fail( "Waited 100 seconds for pool to return object from dead thread, but it was never returned." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void claimAndReleaseInSeparateThread(final MarshlandPool<Object> pool) throws InterruptedException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void ClaimAndReleaseInSeparateThread( MarshlandPool<object> pool )
		 {
			  Thread thread = new Thread(() =>
			  {
				object obj = pool.Acquire();
				pool.Release( obj );
			  });
			  thread.Start();
			  thread.Join();
		 }
	}

}