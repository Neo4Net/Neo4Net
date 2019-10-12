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
namespace Neo4Net.causalclustering.core.consensus.explorer
{

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using Leader = Neo4Net.causalclustering.core.consensus.roles.Leader;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;

	public class ClusterSafetyViolations
	{
		 private ClusterSafetyViolations()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.List<Violation> violations(ClusterState state) throws java.io.IOException
		 public static IList<Violation> Violations( ClusterState state )
		 {
			  IList<Violation> invariantsViolated = new List<Violation>();

			  if ( MultipleLeadersInSameTerm( state ) )
			  {
					invariantsViolated.Add( Violation.MultipleLeaders );
			  }

			  if ( InconsistentCommittedLogEntries( state ) )
			  {
					invariantsViolated.Add( Violation.DivergedLog );
			  }

			  return invariantsViolated;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean inconsistentCommittedLogEntries(ClusterState state) throws java.io.IOException
		 public static bool InconsistentCommittedLogEntries( ClusterState state )
		 {
			  int index = 0;
			  bool moreLog = true;
			  while ( moreLog )
			  {
					moreLog = false;
					RaftLogEntry clusterLogEntry = null;
					foreach ( ComparableRaftState memberState in state.States.Values )
					{
						 if ( index <= memberState.CommitIndex() )
						 {
							  RaftLogEntry memberLogEntry = readLogEntry( memberState.EntryLog(), index );
							  if ( clusterLogEntry == null )
							  {
									clusterLogEntry = memberLogEntry;
							  }
							  else
							  {
									if ( !clusterLogEntry.Equals( memberLogEntry ) )
									{
										 return true;
									}
							  }
						 }
						 if ( index < memberState.CommitIndex() )
						 {
							  moreLog = true;
						 }
					}
					index++;
			  }
			  return false;
		 }

		 public static bool MultipleLeadersInSameTerm( ClusterState state )
		 {
			  ISet<long> termThatHaveALeader = new HashSet<long>();
			  foreach ( KeyValuePair<MemberId, Role> entry in state.Roles.SetOfKeyValuePairs() )
			  {
					RaftMessageHandler role = entry.Value.handler;
					if ( role is Leader )
					{
						 long term = state.States[entry.Key].term();
						 if ( termThatHaveALeader.Contains( term ) )
						 {
							  return true;
						 }
						 else
						 {
							  termThatHaveALeader.Add( term );
						 }
					}
			  }
			  return false;
		 }

		 public enum Violation
		 {
			  DivergedLog,
			  MultipleLeaders
		 }

	}

}