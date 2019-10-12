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
namespace Org.Neo4j.causalclustering.core.consensus.log
{

	/// <summary>
	/// Persists entries that are coordinated through RAFT, i.e. this is the log
	/// of user data.
	/// <p/>
	/// All write operations in this interface must be durably persisted before
	/// returning from the respective functions.
	/// </summary>
	public interface RaftLog : ReadableRaftLog
	{

		 /// <summary>
		 /// Appends entry to the end of the log. The first log index is 0.
		 /// <p/>
		 /// The entries must be uniquely identifiable and already appended
		 /// entries must not be re-appended (unless they have been removed
		 /// through truncation).
		 /// </summary>
		 /// <param name="entry"> The log entry. </param>
		 /// <returns> the index at which the entry was appended. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long append(RaftLogEntry... entry) throws java.io.IOException;
		 long Append( params RaftLogEntry[] entry );

		 /// <summary>
		 /// Truncates the log starting from the supplied index. Committed
		 /// entries can never be truncated.
		 /// </summary>
		 /// <param name="fromIndex"> The start index (inclusive). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void truncate(long fromIndex) throws java.io.IOException;
		 void Truncate( long fromIndex );

		 /// <summary>
		 /// Attempt to prune (delete) a prefix of the log, no further than the safeIndex.
		 /// <p/>
		 /// Implementations can choose to prune a shorter prefix if this is convenient for
		 /// their storage mechanism. The return value tells the caller how much was actually pruned.
		 /// </summary>
		 /// <param name="safeIndex"> Highest index that may be pruned.
		 /// </param>
		 /// <returns> The new prevIndex for the log, which will be at most safeIndex. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long prune(long safeIndex) throws java.io.IOException;
		 long Prune( long safeIndex );

		 /// <summary>
		 /// Skip up to the supplied index if it is not already present.
		 /// <p/>
		 /// If the entry was not present then it gets defined with the
		 /// supplied term, but without content, and thus can be used
		 /// only for log matching from a later index.
		 /// <p/>
		 /// This is useful when a snapshot starting from a later index
		 /// has been downloaded and thus earlier entries are irrelevant
		 /// and possibly non-existent in the cluster.
		 /// </summary>
		 /// <param name="index"> the index we want to skip to </param>
		 /// <param name="term"> the term of the index
		 /// </param>
		 /// <returns> The appendIndex after this call, which
		 ///         will be at least index. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long skip(long index, long term) throws java.io.IOException;
		 long Skip( long index, long term );
	}

	public static class RaftLog_Fields
	{
		  public const string RAFT_LOG_DIRECTORY_NAME = "raft-log";
	}

}