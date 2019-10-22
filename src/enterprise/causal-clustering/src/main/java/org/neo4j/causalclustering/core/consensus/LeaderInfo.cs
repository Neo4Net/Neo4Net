using System;

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
namespace Neo4Net.causalclustering.core.consensus
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	[Serializable]
	public class LeaderInfo
	{
		 private const long SERIAL_VERSION_UID = 7983780359510842910L;

		 public static readonly LeaderInfo Initial = new LeaderInfo( null, -1 );

		 private readonly MemberId _memberId;
		 private readonly long _term;
		 private readonly bool _isSteppingDown;

		 public LeaderInfo( MemberId memberId, long term ) : this( memberId, term, false )
		 {
		 }

		 private LeaderInfo( MemberId memberId, long term, bool isSteppingDown )
		 {
			  this._memberId = memberId;
			  this._term = term;
			  this._isSteppingDown = isSteppingDown;
		 }

		 /// <summary>
		 /// Produces a new LeaderInfo object for a step down event, setting memberId to null but maintaining the current term.
		 /// </summary>
		 public virtual LeaderInfo StepDown()
		 {
			  return new LeaderInfo( null, this._term, true );
		 }

		 public virtual bool SteppingDown
		 {
			 get
			 {
				  return _isSteppingDown;
			 }
		 }

		 public virtual MemberId MemberId()
		 {
			  return _memberId;
		 }

		 public virtual long Term()
		 {
			  return _term;
		 }

		 public override string ToString()
		 {
			  return "LeaderInfo{" + "memberId=" + _memberId + ", term=" + _term + '}';
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
			  LeaderInfo that = ( LeaderInfo ) o;
			  return _term == that._term && _isSteppingDown == that._isSteppingDown && Objects.Equals( _memberId, that._memberId );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _memberId, _term, _isSteppingDown );
		 }
	}

}