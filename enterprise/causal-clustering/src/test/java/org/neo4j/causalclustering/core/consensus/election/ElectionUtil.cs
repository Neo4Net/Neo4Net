﻿using System;
using System.Collections.Generic;
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
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

	public class ElectionUtil
	{
		 private ElectionUtil()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.causalclustering.identity.MemberId waitForLeaderAgreement(Iterable<org.neo4j.causalclustering.core.consensus.RaftMachine> validRafts, long maxTimeMillis) throws InterruptedException, java.util.concurrent.TimeoutException
		 public static MemberId WaitForLeaderAgreement( IEnumerable<RaftMachine> validRafts, long maxTimeMillis )
		 {
			  long viewCount = Iterables.count( validRafts );

			  IDictionary<MemberId, MemberId> leaderViews = new Dictionary<MemberId, MemberId>();
			  CompletableFuture<MemberId> futureAgreedLeader = new CompletableFuture<MemberId>();

			  ICollection<ThreadStart> destructors = new List<ThreadStart>();
			  foreach ( RaftMachine raft in validRafts )
			  {
					destructors.Add( LeaderViewUpdatingListener( raft, validRafts, leaderViews, viewCount, futureAgreedLeader ) );
			  }

			  try
			  {
					try
					{
						 return futureAgreedLeader.get( maxTimeMillis, TimeUnit.MILLISECONDS );
					}
					catch ( ExecutionException e )
					{
						 throw new Exception( e );
					}
			  }
			  finally
			  {
					destructors.forEach( ThreadStart.run );
			  }
		 }

		 private static ThreadStart LeaderViewUpdatingListener( RaftMachine raft, IEnumerable<RaftMachine> validRafts, IDictionary<MemberId, MemberId> leaderViews, long viewCount, CompletableFuture<MemberId> futureAgreedLeader )
		 {
			  LeaderListener listener = newLeader =>
			  {
				lock ( leaderViews )
				{
					 leaderViews[raft.Identity()] = newLeader.memberId();

					 bool leaderIsValid = false;
					 foreach ( RaftMachine validRaft in validRafts )
					 {
						  if ( validRaft.Identity().Equals(newLeader.memberId()) )
						  {
								leaderIsValid = true;
						  }
					 }

					 if ( newLeader.memberId() != null && leaderIsValid && AllAgreeOnLeader(leaderViews, viewCount, newLeader.memberId()) )
					 {
						  futureAgreedLeader.complete( newLeader.memberId() );
					 }
				}
			  };

			  raft.RegisterListener( listener );
			  return () => raft.unregisterListener(listener);
		 }

		 private static bool AllAgreeOnLeader<T>( IDictionary<T, T> leaderViews, long viewCount, T leader )
		 {
			  if ( leaderViews.Count != viewCount )
			  {
					return false;
			  }

			  foreach ( T leaderView in leaderViews.Values )
			  {
					if ( !leader.Equals( leaderView ) )
					{
						 return false;
					}
			  }

			  return true;
		 }
	}

}