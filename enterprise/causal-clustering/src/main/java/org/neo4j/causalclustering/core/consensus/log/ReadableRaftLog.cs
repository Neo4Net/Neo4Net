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

	public interface ReadableRaftLog
	{
		 /// <returns> The index of the last appended entry. </returns>
		 long AppendIndex();

		 /// <returns> The index immediately preceding entries in the log. </returns>
		 long PrevIndex();

		 /// <summary>
		 /// Reads the term associated with the entry at the supplied index.
		 /// </summary>
		 /// <param name="logIndex"> The index of the log entry. </param>
		 /// <returns> The term of the entry, or -1 if the entry does not exist </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long readEntryTerm(long logIndex) throws java.io.IOException;
		 long ReadEntryTerm( long logIndex );

		 /// <summary>
		 /// Returns a <seealso cref="RaftLogCursor"/> of <seealso cref="RaftLogEntry"/>s from the specified index until the end of the log </summary>
		 /// <param name="fromIndex"> The log index at which the cursor should be positioned </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RaftLogCursor getEntryCursor(long fromIndex) throws java.io.IOException;
		 RaftLogCursor GetEntryCursor( long fromIndex );
	}

}