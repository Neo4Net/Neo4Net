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

	public class MemberIdSet : RaftGroup<MemberId>
	{
		 private readonly ISet<MemberId> _members;

		 public MemberIdSet( ISet<MemberId> members )
		 {
			  this._members = members;
		 }

		 public override string ToString()
		 {
			  return "MemberIdSet{ members=" + _members + '}';
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

			  MemberIdSet that = ( MemberIdSet ) o;

			  return !( _members != null ?!_members.SetEquals( that._members ) : that._members != null );

		 }

		 public override int GetHashCode()
		 {
			  return _members != null ? _members.GetHashCode() : 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }
	}

}