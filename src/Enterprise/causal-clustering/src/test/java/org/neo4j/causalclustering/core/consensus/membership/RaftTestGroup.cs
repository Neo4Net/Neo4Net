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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class RaftTestGroup : RaftGroup<MemberId>
	{
		 private readonly ISet<MemberId> _members = new HashSet<MemberId>();

		 public RaftTestGroup( ISet<MemberId> members )
		 {
			  this._members.addAll( members );
		 }

		 public RaftTestGroup( params int[] memberIds )
		 {
			  foreach ( int memberId in memberIds )
			  {
					this._members.Add( member( memberId ) );
			  }
		 }

		 public RaftTestGroup( params MemberId[] memberIds )
		 {
			  this._members.addAll( Arrays.asList( memberIds ) );
		 }

		 public virtual ISet<MemberId> Members
		 {
			 get
			 {
				  return _members;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  RaftTestGroup that = ( RaftTestGroup ) o;

			  return _members.SetEquals( that._members );

		 }

		 public override int GetHashCode()
		 {
			  return _members.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return format( "RaftTestGroup{members=%s}", _members );
		 }

		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  throw new System.NotSupportedException( "No handler for this " + this.GetType() );
		 }
	}

}