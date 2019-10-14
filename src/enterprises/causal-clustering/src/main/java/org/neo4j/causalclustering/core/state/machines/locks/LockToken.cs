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
namespace Neo4Net.causalclustering.core.state.machines.locks
{
	/// <summary>
	/// Represents a lock token by an pairing an id together with its owner. A lock token
	/// is held by a single server at any point in time and the token holder is the
	/// only one that should be using locks. It is used as an ordering primitive
	/// in the consensus machinery to mark local lock validity by using it as
	/// the cluster lock session id.
	/// 
	/// The reason for calling it a token is to clarify the fact that there logically
	/// is just a single valid token at any point in time, which gets requested and
	/// logically passed around. When bound to a transaction the id gets used as a
	/// lock session id in the cluster.
	/// </summary>
	internal interface LockToken
	{

		 /// <summary>
		 /// Convenience method for retrieving a valid candidate id for a
		 /// lock token request.
		 /// </summary>
		 ///  <returns> A suitable candidate id for a token request. </returns>
		 /// <param name="currentId"> </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static int nextCandidateId(int currentId)
	//	 {
	//		  int candidateId = currentId + 1;
	//		  if (candidateId == INVALID_LOCK_TOKEN_ID)
	//		  {
	//				candidateId++;
	//		  }
	//		  return candidateId;
	//	 }

		 /// <summary>
		 /// The id of the lock token.
		 /// </summary>
		 int Id();

		 /// <summary>
		 /// The owner of this lock token.
		 /// </summary>
		 object Owner();
	}

	public static class LockToken_Fields
	{
		 public const int INVALID_LOCK_TOKEN_ID = -1;
	}

}