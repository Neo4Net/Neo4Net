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
namespace Neo4Net.causalclustering.core.consensus.log
{

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;

	public class RaftLogEntry
	{
		 public static readonly RaftLogEntry[] Empty = new RaftLogEntry[0];

		 private readonly long _term;
		 private readonly ReplicatedContent _content;

		 public RaftLogEntry( long term, ReplicatedContent content )
		 {
			  Objects.requireNonNull( content );
			  this._term = term;
			  this._content = content;
		 }

		 public virtual long Term()
		 {
			  return this._term;
		 }

		 public virtual ReplicatedContent Content()
		 {
			  return this._content;
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

			  RaftLogEntry that = ( RaftLogEntry ) o;

			  return _term == that._term && _content.Equals( that._content );
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _term ^ ( ( long )( ( ulong )_term >> 32 ) ) );
			  result = 31 * result + _content.GetHashCode();
			  return result;
		 }

		 public override string ToString()
		 {
			  return format( "{term=%d, content=%s}", _term, _content );
		 }

	}

}