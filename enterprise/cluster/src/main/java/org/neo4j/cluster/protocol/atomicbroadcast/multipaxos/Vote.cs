﻿using System;

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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{
	using ElectionCredentials = Org.Neo4j.cluster.protocol.election.ElectionCredentials;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;

	/// <summary>
	/// A cluster coordinator election vote. Each vote contains the id of the server and any credentials (see {@link
	/// ElectionCredentialsProvider} implementations for details).
	/// <p/>
	/// Votes are comparable so that they can be ordered to find winner. Credentials implement the comparison rules.
	/// </summary>
	public class Vote : IComparable<Vote>
	{
		 private readonly InstanceId _suggestedNode;
		 private readonly ElectionCredentials _voteCredentials;

		 public Vote( InstanceId suggestedNode, ElectionCredentials voteCredentials )
		 {
			  this._suggestedNode = suggestedNode;
			  this._voteCredentials = voteCredentials;
		 }

		 public virtual InstanceId SuggestedNode
		 {
			 get
			 {
				  return _suggestedNode;
			 }
		 }

		 public virtual IComparable<ElectionCredentials> Credentials
		 {
			 get
			 {
				  return _voteCredentials;
			 }
		 }

		 public override string ToString()
		 {
			  return _suggestedNode + ":" + _voteCredentials;
		 }

		 public override int CompareTo( Vote o )
		 {
			  return this._voteCredentials.CompareTo( o._voteCredentials );
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

			  Vote vote = ( Vote ) o;

			  if ( !_suggestedNode.Equals( vote._suggestedNode ) )
			  {
					return false;
			  }
			  return _voteCredentials.Equals( vote._voteCredentials );
		 }

		 public override int GetHashCode()
		 {
			  int result = _suggestedNode.GetHashCode();
			  result = 31 * result + _voteCredentials.GetHashCode();
			  return result;
		 }
	}

}