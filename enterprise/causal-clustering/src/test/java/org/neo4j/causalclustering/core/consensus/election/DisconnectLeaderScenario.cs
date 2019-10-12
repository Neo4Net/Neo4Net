using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.core.consensus.election
{

	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.Helpers.Collection;

	/// <summary>
	/// In this scenario we disconnect the current leader and measure how long time it
	/// takes until the remaining members agree on a new leader.
	/// </summary>
	public class DisconnectLeaderScenario
	{
		 private readonly Fixture _fixture;
		 private readonly long _electionTimeout;
		 private readonly IList<long> _electionTimeResults = new List<long>();
		 private long _timeoutCount;

		 public DisconnectLeaderScenario( Fixture fixture, long electionTimeout )
		 {
			  this._fixture = fixture;
			  this._electionTimeout = electionTimeout;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(long iterations, long leaderStabilityMaxTimeMillis) throws InterruptedException
		 public virtual void Run( long iterations, long leaderStabilityMaxTimeMillis )
		 {
			  for ( int i = 0; i < iterations; i++ )
			  {
					long electionTime;
					try
					{
						 electionTime = OneIteration( leaderStabilityMaxTimeMillis );
						 _electionTimeResults.Add( electionTime );
					}
					catch ( TimeoutException )
					{
						 _timeoutCount++;
					}
					_fixture.net.reset();
					Thread.Sleep( ThreadLocalRandom.current().nextLong(_electionTimeout) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long oneIteration(long leaderStabilityMaxTimeMillis) throws InterruptedException, java.util.concurrent.TimeoutException
		 private long OneIteration( long leaderStabilityMaxTimeMillis )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<RaftMachine> rafts = _fixture.rafts.Select( Fixture.RaftFixture::raftMachine ).ToList();
			  MemberId oldLeader = ElectionUtil.WaitForLeaderAgreement( rafts, leaderStabilityMaxTimeMillis );
			  long startTime = DateTimeHelper.CurrentUnixTimeMillis();

			  _fixture.net.disconnect( oldLeader );
			  MemberId newLeader = ElectionUtil.WaitForLeaderAgreement( new FilteringIterable<RaftMachine>( rafts, raft => !raft.identity().Equals(oldLeader) ), leaderStabilityMaxTimeMillis );
			  Debug.Assert( !newLeader.Equals( oldLeader ) ); // this should be guaranteed by the waitForLeaderAgreement call

			  return DateTimeHelper.CurrentUnixTimeMillis() - startTime;
		 }

		 private bool HadOneOrMoreCollisions( long result )
		 {
			  /* This is just a simple heuristic to classify the results into colliding and
			   * non-colliding groups. It is not entirely accurate and doesn't have to be. */
			  return result > ( _electionTimeout * 2 );
		 }

		 public class Result
		 {
			 private readonly DisconnectLeaderScenario _outerInstance;

			 public Result( DisconnectLeaderScenario outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal double NonCollidingAverage;
			  internal double CollidingAverage;
			  internal double CollisionRate;
			  internal long CollisionCount;
			  internal long TimeoutCount;

			  public override string ToString()
			  {
					return string.Format( "Result{{nonCollidingAverage={0}, collidingAverage={1}, collisionRate={2}, " + "collisionCount={3:D}, timeoutCount={4:D}}}", NonCollidingAverage, CollidingAverage, CollisionRate, CollisionCount, TimeoutCount );
			  }
		 }

		 public virtual Result Result()
		 {
			  Result result = new Result( this );

			  long collidingRuns = 0;
			  long collidingSum = 0;

			  long nonCollidingRuns = 0;
			  long nonCollidingSum = 0;

			  foreach ( long electionTime in _electionTimeResults )
			  {
					if ( HadOneOrMoreCollisions( electionTime ) )
					{
						 collidingRuns++;
						 collidingSum += electionTime;
					}
					else
					{
						 nonCollidingRuns++;
						 nonCollidingSum += electionTime;
					}
			  }

			  result.CollidingAverage = collidingSum / ( double ) collidingRuns;
			  result.NonCollidingAverage = nonCollidingSum / ( double ) nonCollidingRuns;
			  result.CollisionRate = collidingRuns / ( double ) _electionTimeResults.Count;
			  result.CollisionCount = collidingRuns;
			  result.TimeoutCount = _timeoutCount;

			  return result;
		 }
	}

}