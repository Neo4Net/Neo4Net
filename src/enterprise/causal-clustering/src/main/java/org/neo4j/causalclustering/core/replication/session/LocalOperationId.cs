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
	/// Uniquely identifies an operation as performed under a global session. </summary>
	public class LocalOperationId
	{
		 private readonly long _localSessionId;
		 private readonly long _sequenceNumber;

		 public LocalOperationId( long localSessionId, long sequenceNumber )
		 {
			  this._localSessionId = localSessionId;
			  this._sequenceNumber = sequenceNumber;
		 }

		 public virtual long LocalSessionId()
		 {
			  return _localSessionId;
		 }

		 public virtual long SequenceNumber()
		 {
			  return _sequenceNumber;
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

			  LocalOperationId that = ( LocalOperationId ) o;

			  if ( _localSessionId != that._localSessionId )
			  {
					return false;
			  }
			  return _sequenceNumber == that._sequenceNumber;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _localSessionId ^ ( ( long )( ( ulong )_localSessionId >> 32 ) ) );
			  result = 31 * result + ( int )( _sequenceNumber ^ ( ( long )( ( ulong )_sequenceNumber >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return format( "LocalOperationId{localSessionId=%d, sequenceNumber=%d}", _localSessionId, _sequenceNumber );
		 }
	}

}