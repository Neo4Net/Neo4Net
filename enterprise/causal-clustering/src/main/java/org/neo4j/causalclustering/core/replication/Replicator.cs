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
namespace Org.Neo4j.causalclustering.core.replication
{

	/// <summary>
	/// Replicate content across a cluster of servers.
	/// </summary>
	public interface Replicator
	{
		 /// <summary>
		 /// Submit content for replication. This method does not guarantee that the content
		 /// actually gets replicated, it merely makes an attempt at replication. Other
		 /// mechanisms must be used to achieve required delivery semantics.
		 /// </summary>
		 /// <param name="content">      The content to replicated. </param>
		 /// <param name="trackResult">  Whether to track the result for this operation.
		 /// </param>
		 /// <returns> A future that will receive the result when available. Only valid if trackResult is set. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.concurrent.Future<Object> replicate(ReplicatedContent content, boolean trackResult) throws ReplicationFailureException;
		 Future<object> Replicate( ReplicatedContent content, bool trackResult );
	}

}