using System;
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
namespace Org.Neo4j.cluster.protocol.election
{

	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;

	/// <summary>
	/// Context used by <seealso cref="ElectionState"/>.
	/// </summary>
	public interface ElectionContext : TimeoutsContext, LoggingContext, ConfigurationContext
	{

		 void Created();

		 IList<ElectionRole> PossibleRoles { get; }

		 /*
		  * Removes all roles from the provided node. This is expected to be the first call when receiving a demote
		  * message for a node, since it is the way to ensure that election will happen for each role that node had
		  */
		 void NodeFailed( InstanceId node );

		 IEnumerable<string> GetRoles( InstanceId server );

		 bool IsElectionProcessInProgress( string role );

		 void StartElectionProcess( string role );

		 bool Voted( string role, InstanceId suggestedNode, ElectionCredentials suggestionCredentials, long electionVersion );

		 InstanceId GetElectionWinner( string role );

		 ElectionCredentials GetCredentialsForRole( string role );

		 int GetVoteCount( string role );

		 int NeededVoteCount { get; }

		 void ForgetElection( string role );

		 IEnumerable<string> RolesRequiringElection { get; }

		 bool ElectionOk();

		 bool InCluster { get; }

		 IEnumerable<InstanceId> Alive { get; }

		 bool Elector { get; }

		 bool IsFailed( InstanceId key );

		 InstanceId GetElected( string roleName );

		 bool HasCurrentlyElectedVoted( string role, InstanceId currentElected );

		 ISet<InstanceId> Failed { get; }

		 ClusterMessage.VersionedConfigurationStateChange NewConfigurationStateChange();

		 ElectionContext_VoteRequest VoteRequestForRole( ElectionRole role );
	}

	 [Serializable]
	 public class ElectionContext_VoteRequest
	 {
		  internal const long SERIAL_VERSION_UID = -715604979485263049L;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal string RoleConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long VersionConflict;

		  public ElectionContext_VoteRequest( string role, long version )
		  {
				this.RoleConflict = role;
				this.VersionConflict = version;
		  }

		  public virtual string Role
		  {
			  get
			  {
					return RoleConflict;
			  }
		  }

		  public virtual long Version
		  {
			  get
			  {
					return VersionConflict;
			  }
		  }

		  public override string ToString()
		  {
				return "VoteRequest{role='" + RoleConflict + "', version=" + VersionConflict + "}";
		  }
	 }

}