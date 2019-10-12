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
namespace Org.Neo4j.Server.rest.causalclustering
{
	using JsonSerialize = org.codehaus.jackson.map.annotate.JsonSerialize;


	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonSerialize(include = JsonSerialize.Inclusion.NON_NULL) public class ClusterStatusResponse
	public class ClusterStatusResponse
	{
		 private readonly bool _isCore;
		 private readonly long _lastAppliedRaftIndex;
		 private readonly bool _isParticipatingInRaftGroup;
		 private readonly ICollection<string> _votingMembers;
		 private readonly bool _isHealthy;
		 private readonly string _memberId;
		 private readonly string _leader;
		 private readonly long? _millisSinceLastLeaderMessage;

		 internal ClusterStatusResponse( long lastAppliedRaftIndex, bool isParticipatingInRaftGroup, ICollection<MemberId> votingMembers, bool isHealthy, MemberId memberId, MemberId leader, Duration millisSinceLastLeaderMessage, bool isCore )
		 {
			  this._lastAppliedRaftIndex = lastAppliedRaftIndex;
			  this._isParticipatingInRaftGroup = isParticipatingInRaftGroup;
			  this._votingMembers = votingMembers.Select( member => member.Uuid.ToString() ).OrderBy(c => c).ToList();
			  this._isHealthy = isHealthy;
			  this._memberId = memberId.Uuid.ToString();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  this._leader = Optional.ofNullable( leader ).map( MemberId::getUuid ).map( System.Guid.toString ).orElse( null );
			  this._millisSinceLastLeaderMessage = Optional.ofNullable( millisSinceLastLeaderMessage ).map( Duration.toMillis ).orElse( null );
			  this._isCore = isCore;
		 }

		 /// <summary>
		 /// Transactions are associated with raft log indexes. By tracking this value across a cluster you will be able to evaluate with whether
		 /// the cluster is caught up and functioning as expected.
		 /// </summary>
		 /// <returns> the latest transaction id available on this node </returns>
		 public virtual long LastAppliedRaftIndex
		 {
			 get
			 {
				  return _lastAppliedRaftIndex;
			 }
		 }

		 /// <summary>
		 /// A node is considered participating if it believes it is caught up and knows who the leader is. Leader timeouts will prevent this value from being true
		 /// even if the core is caught up. This is always false for replicas, since they never participate in raft. The refuse to be leader flag does not affect this
		 /// logic (i.e. if a core proposes itself to be leader, it still doesn't know who the leader is since it the leader has not been voted in)
		 /// </summary>
		 /// <returns> true if the core is in a "good state" (up to date and part of raft). For cores this is likely the flag you will want to look at </returns>
		 public virtual bool ParticipatingInRaftGroup
		 {
			 get
			 {
				  return _isParticipatingInRaftGroup;
			 }
		 }

		 /// <summary>
		 /// For cores, this will list all known live core members. Read replicas also include all known read replicas.
		 /// Users will want to monitor this field (size or values) when performing rolling upgrades for read replicas.
		 /// </summary>
		 /// <returns> a list of discovery addresses ("hostname:port") that are part of this node's membership set </returns>
		 public virtual ICollection<string> VotingMembers
		 {
			 get
			 {
				  return _votingMembers;
			 }
		 }

		 public virtual bool Healthy
		 {
			 get
			 {
				  return _isHealthy;
			 }
		 }

		 public virtual string MemberId
		 {
			 get
			 {
				  return _memberId;
			 }
		 }

		 public virtual string Leader
		 {
			 get
			 {
				  return _leader;
			 }
		 }

		 public virtual long? MillisSinceLastLeaderMessage
		 {
			 get
			 {
				  return _millisSinceLastLeaderMessage;
			 }
		 }

		 public virtual bool Core
		 {
			 get
			 {
				  return _isCore;
			 }
		 }
	}

}