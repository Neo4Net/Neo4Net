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
namespace Neo4Net.causalclustering.core.consensus.election
{
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;

	/// <summary>
	/// A test suite that is used for measuring the election performance and
	/// guarding against regressions in this area. The outcome assertions are very
	/// relaxed so that false positives are avoided in CI and adjustments of the
	/// limits should be made by looking at statistics and reasoning about what
	/// type of performance should be expected, taking all parameters into account.
	/// 
	/// Major regressions that severely affect the election performance and the
	/// ability to perform an election at all should be caught by this test. Very
	/// rare false positives should not be used as an indication for increasing the
	/// limits.
	/// </summary>
	public class ElectionPerformanceIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("This belongs better in a benchmarking suite.") @Test public void electionPerformance_NormalConditions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ElectionPerformanceNormalConditions()
		 {
			  /* This test runs with with few iterations. Hence it does not have the power to catch
			   * regressions efficiently. Its purpose is mainly to run elections using real-world
			   * parameters and catch very obvious regressions while not contributing overly much to the
			   * regression test suites total runtime. */

			  // given parameters
			  const long networkLatency = 15L;
			  const long electionTimeout = 500L;
			  const long heartbeatInterval = 250L;
			  const int iterations = 10;

			  TestNetwork net = new TestNetwork<>( ( i, o ) => networkLatency );
			  ISet<MemberId> members = asSet( member( 0 ), member( 1 ), member( 2 ) );
			  Fixture fixture = new Fixture( members, net, electionTimeout, heartbeatInterval );
			  DisconnectLeaderScenario scenario = new DisconnectLeaderScenario( fixture, electionTimeout );

			  try
			  {
					// when running scenario
					fixture.Boot();
					scenario.Run( iterations, 10 * electionTimeout );
			  }
			  finally
			  {
					fixture.TearDown();
			  }

			  DisconnectLeaderScenario.Result result = scenario.Result();

			  /* These bounds have been experimentally established and should have a very low
			   * likelihood for false positives without an actual major regression. If this test fails
			   * then the recommended action is to run the test manually and interpret the results
			   * to guide further action. Perhaps the power of the test has to be improved, but
			   * the intention here is not to catch anything but the most major of regressions. */

			  assertThat( result.NonCollidingAverage, lessThan( 2.0 * electionTimeout ) );
			  if ( result.CollisionCount > 3 )
			  {
					assertThat( result.CollidingAverage, lessThan( 6.0 * electionTimeout ) );
			  }
			  assertThat( result.TimeoutCount, @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("This belongs better in a benchmarking suite.") @Test public void electionPerformance_RapidConditions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ElectionPerformanceRapidConditions()
		 {
			  // given parameters
			  const long networkLatency = 1L;
			  const long electionTimeout = 30L;
			  const long heartbeatInterval = 15L;
			  const int iterations = 100;

			  TestNetwork net = new TestNetwork<>( ( i, o ) => networkLatency );
			  ISet<MemberId> members = asSet( member( 0 ), member( 1 ), member( 2 ) );
			  Fixture fixture = new Fixture( members, net, electionTimeout, heartbeatInterval );
			  DisconnectLeaderScenario scenario = new DisconnectLeaderScenario( fixture, electionTimeout );

			  try
			  {
					// when running scenario
					fixture.Boot();
					scenario.Run( iterations, 10 * electionTimeout );
			  }
			  finally
			  {
					fixture.TearDown();
			  }

			  DisconnectLeaderScenario.Result result = scenario.Result();

			  /* These bounds have been experimentally established and should have a very low
			   * likelihood for false positives without an actual major regression. If this test fails
			   * then the recommended action is to run the test manually and interpret the results
			   * to guide further action. Perhaps the power of the test has to be improved, but
			   * the intention here is not to catch anything but the most major of regressions. */

			  assertThat( result.NonCollidingAverage, lessThan( 2.0 * electionTimeout ) );

			  // because of the high number of iterations, it is possible to assert on the collision rate
			  assertThat( result.CollisionRate, lessThan( 0.50d ) );

			  if ( result.CollisionCount > 10 )
			  {
					assertThat( result.CollidingAverage, lessThan( 5.0 * electionTimeout ) );
			  }
			  assertThat( result.TimeoutCount, lessThanOrEqualTo( 1L ) ); // for GC or whatever reason
		 }
	}

}