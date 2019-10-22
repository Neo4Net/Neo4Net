using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.core.replication
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	public class LeaderProviderTest
	{

		 private static readonly MemberId _memberId = new MemberId( System.Guid.randomUUID() );
		 private readonly ExecutorService _executorService = Executors.newCachedThreadPool();
		 private readonly LeaderProvider _leaderProvider = new LeaderProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _leaderProvider.Leader = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveCurrentLeaderIfAvailable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveCurrentLeaderIfAvailable()
		 {
			  _leaderProvider.Leader = _memberId;
			  assertEquals( _leaderProvider.currentLeader(), _memberId );
			  assertEquals( _leaderProvider.awaitLeader(), _memberId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitForNonNullValue() throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForNonNullValue()
		 {
			  // given
			  int threads = 3;
			  assertNull( _leaderProvider.currentLeader() );

			  // when
			  CompletableFuture<List<MemberId>> futures = CompletableFuture.completedFuture( new List<List<MemberId>>() );
			  for ( int i = 0; i < threads; i++ )
			  {
					CompletableFuture<MemberId> future = CompletableFuture.supplyAsync( CurrentLeader, _executorService );
					futures = futures.thenCombine(future, (completableFutures, memberId) =>
					{
					 completableFutures.add( memberId );
					 return completableFutures;
					});
			  }

			  // then
			  Thread.Sleep( 100 );
			  assertFalse( futures.Done );

			  // when
			  _leaderProvider.Leader = _memberId;

			  List<MemberId> memberIds = futures.get( 5, TimeUnit.SECONDS );

			  // then
			  assertTrue( memberIds.All( memberId => memberId.Equals( _memberId ) ) );
		 }

		 private System.Func<MemberId> CurrentLeader
		 {
			 get
			 {
				  return () =>
				  {
					try
					{
						 return _leaderProvider.awaitLeader();
					}
					catch ( InterruptedException )
					{
						 throw new Exception( "Interrupted" );
					}
				  };
			 }
		 }
	}

}