using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{
	using Test = org.junit.Test;


	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class MultiRetryStrategyTest
	{
		 private static readonly System.Predicate<int> _alwaysValid = i => true;
		 private static readonly System.Predicate<int> _neverValid = i => false;
		 private static readonly System.Predicate<int> VALID_ON_SECOND_TIME = new PredicateAnonymousInnerClass();

		 private class PredicateAnonymousInnerClass : System.Predicate<int>
		 {
			 private bool nextSuccessful;
			 public override bool test( int? integer )
			 {
				  if ( !nextSuccessful )
				  {
						nextSuccessful = true;
						return false;
				  }
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successOnRetryCausesNoDelay()
		 public virtual void SuccessOnRetryCausesNoDelay()
		 {
			  // given
			  CountingSleeper countingSleeper = new CountingSleeper();
			  int retries = 10;
			  MultiRetryStrategy<int, int> subject = new MultiRetryStrategy<int, int>( 0, retries, NullLogProvider.Instance, countingSleeper );

			  // when
			  int? result = subject.Apply( 3, System.Func.identity(), _alwaysValid );

			  // then
			  assertEquals( 0, countingSleeper.InvocationCount() );
			  assertEquals( "Function identity should be used to retrieve the expected value", 3, result.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void numberOfIterationsDoesNotExceedMaximum()
		 public virtual void NumberOfIterationsDoesNotExceedMaximum()
		 {
			  // given
			  CountingSleeper countingSleeper = new CountingSleeper();
			  int retries = 5;
			  MultiRetryStrategy<int, int> subject = new MultiRetryStrategy<int, int>( 0, retries, NullLogProvider.Instance, countingSleeper );

			  // when
			  subject.Apply( 3, System.Func.identity(), _neverValid );

			  // then
			  assertEquals( retries, countingSleeper.InvocationCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulRetriesBreakTheRetryLoop()
		 public virtual void SuccessfulRetriesBreakTheRetryLoop()
		 {
			  CountingSleeper countingSleeper = new CountingSleeper();
			  int retries = 5;
			  MultiRetryStrategy<int, int> subject = new MultiRetryStrategy<int, int>( 0, retries, NullLogProvider.Instance, countingSleeper );

			  // when
			  subject.Apply( 3, System.Func.identity(), VALID_ON_SECOND_TIME );

			  // then
			  assertEquals( 1, countingSleeper.InvocationCount() );
		 }

		 public class CountingSleeper : System.Action<long>
		 {
			  internal int Counter;

			  public override void Accept( long l )
			  {
					Counter++;
			  }

			  public virtual int InvocationCount()
			  {
					return Counter;
			  }

		 }

		 public static MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> TestRetryStrategy( int numRetries )
		 {
			  return new MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>>( 0, numRetries, NullLogProvider.Instance, new CountingSleeper() );
		 }
	}

}