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
namespace Org.Neo4j.causalclustering.core.consensus.log.debug
{


	public class LogPrinter
	{
		 private readonly ReadableRaftLog _raftLog;

		 public LogPrinter( ReadableRaftLog raftLog )
		 {
			  this._raftLog = raftLog;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void print(java.io.PrintStream out) throws java.io.IOException
		 public virtual void Print( PrintStream @out )
		 {
			  @out.println( string.Format( "{0,8} {1,5}  {2,2} {3}", "Index", "Term", "C?", "Content" ) );
			  long index = 0L;
			  using ( RaftLogCursor cursor = _raftLog.getEntryCursor( 0 ) )
			  {
					while ( cursor.Next() )
					{
						 RaftLogEntry raftLogEntry = cursor.get();
						 @out.printf( "%8d %5d %s", index, raftLogEntry.Term(), raftLogEntry.Content() );
						 index++;
					}
			  }
		 }
	}

}