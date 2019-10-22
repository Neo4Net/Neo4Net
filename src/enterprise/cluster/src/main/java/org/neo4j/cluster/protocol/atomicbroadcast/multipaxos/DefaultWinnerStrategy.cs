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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using ElectionContextImpl = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.ElectionContextImpl;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;


	public class DefaultWinnerStrategy : WinnerStrategy
	{
		 private ClusterContext _electionContext;

		 public DefaultWinnerStrategy( ClusterContext electionContext )
		 {
			  this._electionContext = electionContext;
		 }

		 public override Neo4Net.cluster.InstanceId PickWinner( ICollection<Vote> votes )
		 {
			  IList<Vote> eligibleVotes = ElectionContextImpl.removeBlankVotes( votes );

			  MoveMostSuitableCandidatesToTop( eligibleVotes );

			  LogElectionOutcome( votes, eligibleVotes );

			  foreach ( Vote vote in eligibleVotes )
			  {
					return vote.SuggestedNode;
			  }

			  return null;
		 }

		 private void MoveMostSuitableCandidatesToTop( IList<Vote> eligibleVotes )
		 {
			  eligibleVotes.Sort();
			  eligibleVotes.Reverse();
		 }

		 private void LogElectionOutcome( ICollection<Vote> votes, IList<Vote> eligibleVotes )
		 {
			  string electionOutcome = string.Format( "Election: received votes {0}, eligible votes {1}", votes, eligibleVotes );
			  _electionContext.getLog( this.GetType() ).debug(electionOutcome);
		 }
	}

}