/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.consensus
{
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;

	public interface LeaderListener
	{
		 /// <summary>
		 /// Allows listeners to handle a leader step down for the given term.
		 /// Note: actions taken as a result of a step down should typically happen *before* any
		 /// actions taken as a result of the leader switch which has also, implicitly, taken place.
		 /// </summary>
		 /// <param name="stepDownTerm"> the term in which the the step down event occurred. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void onLeaderStepDown(long stepDownTerm)
	//	 {
	//	 }

		 void OnLeaderSwitch( LeaderInfo leaderInfo );

		 /// <summary>
		 /// Standard catch-all method which delegates leader events to their appropriate handlers
		 /// in the appropriate order, i.e. calls step down logic (if necessary) befor leader switch
		 /// logic.
		 /// </summary>
		 /// <param name="outcome"> The outcome which contains details of the leader event </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void onLeaderEvent(org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome)
	//	 {
	//		  outcome.stepDownTerm().ifPresent(this::onLeaderStepDown);
	//		  onLeaderSwitch(new LeaderInfo(outcome.getLeader(), outcome.getTerm()));
	//	 }
	}

}