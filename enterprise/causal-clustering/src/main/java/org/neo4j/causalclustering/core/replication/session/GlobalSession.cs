﻿/*
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
namespace Org.Neo4j.causalclustering.core.replication.session
{

	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;

	public class GlobalSession
	{
		 private readonly System.Guid _sessionId;
		 private readonly MemberId _owner;

		 public GlobalSession( System.Guid sessionId, MemberId owner )
		 {
			  this._sessionId = sessionId;
			  this._owner = owner;
		 }

		 public virtual System.Guid SessionId()
		 {
			  return _sessionId;
		 }

		 public virtual MemberId Owner()
		 {
			  return _owner;
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

			  GlobalSession that = ( GlobalSession ) o;

			  if ( !_sessionId.Equals( that._sessionId ) )
			  {
					return false;
			  }
			  return _owner.Equals( that._owner );
		 }

		 public override int GetHashCode()
		 {
			  int result = _sessionId.GetHashCode();
			  result = 31 * result + _owner.GetHashCode();
			  return result;
		 }

		 public override string ToString()
		 {
			  return format( "GlobalSession{sessionId=%s, owner=%s}", _sessionId, _owner );
		 }
	}

}