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
namespace Neo4Net.causalclustering.core.replication.session
{

	/// <summary>
	/// Holds the state for a local session. </summary>
	public class LocalSession
	{
		 private long _localSessionId;
		 private long _currentSequenceNumber;

		 public LocalSession( long localSessionId )
		 {
			  this._localSessionId = localSessionId;
		 }

		 /// <summary>
		 /// Consumes and returns an operation id under this session. </summary>
		 protected internal virtual LocalOperationId NextOperationId()
		 {
			  return new LocalOperationId( _localSessionId, _currentSequenceNumber++ );
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

			  LocalSession that = ( LocalSession ) o;
			  return _localSessionId == that._localSessionId;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( _localSessionId ^ ( ( long )( ( ulong )_localSessionId >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return format( "LocalSession{localSessionId=%d, sequenceNumber=%d}", _localSessionId, _currentSequenceNumber );
		 }
	}

}