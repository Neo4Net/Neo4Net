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
namespace Org.Neo4j.causalclustering.helper
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ExponentialBackoffStrategyTest
	{
		 private const int NUMBER_OF_ACCESSES = 5;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoubleEachTime()
		 public virtual void ShouldDoubleEachTime()
		 {
			  // given
			  ExponentialBackoffStrategy strategy = new ExponentialBackoffStrategy( 1, 1 << NUMBER_OF_ACCESSES, MILLISECONDS );
			  TimeoutStrategy_Timeout timeout = strategy.NewTimeout();

			  // when
			  for ( int i = 0; i < NUMBER_OF_ACCESSES; i++ )
			  {
					timeout.Increment();
			  }

			  // then
			  assertEquals( 1 << NUMBER_OF_ACCESSES, timeout.Millis );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvidePreviousTimeout()
		 public virtual void ShouldProvidePreviousTimeout()
		 {
			  // given
			  ExponentialBackoffStrategy strategy = new ExponentialBackoffStrategy( 1, 1 << NUMBER_OF_ACCESSES, MILLISECONDS );
			  TimeoutStrategy_Timeout timeout = strategy.NewTimeout();

			  // when
			  for ( int i = 0; i < NUMBER_OF_ACCESSES; i++ )
			  {
					timeout.Increment();
			  }

			  // then
			  assertEquals( 1 << NUMBER_OF_ACCESSES, timeout.Millis );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectUpperBound()
		 public virtual void ShouldRespectUpperBound()
		 {
			  // given
			  long upperBound = ( 1 << NUMBER_OF_ACCESSES ) - 5;
			  ExponentialBackoffStrategy strategy = new ExponentialBackoffStrategy( 1, upperBound, MILLISECONDS );
			  TimeoutStrategy_Timeout timeout = strategy.NewTimeout();

			  // when
			  for ( int i = 0; i < NUMBER_OF_ACCESSES; i++ )
			  {
					timeout.Increment();
			  }

			  assertEquals( upperBound, timeout.Millis );

			  // additional increments
			  timeout.Increment();
			  timeout.Increment();
			  timeout.Increment();

			  // then
			  assertEquals( upperBound, timeout.Millis );
		 }
	}

}