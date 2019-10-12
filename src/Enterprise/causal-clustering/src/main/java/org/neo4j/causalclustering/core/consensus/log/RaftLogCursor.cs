using System;

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
namespace Neo4Net.causalclustering.core.consensus.log
{

	using Neo4Net.Cursors;

	public interface RaftLogCursor : RawCursor<RaftLogEntry, Exception>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean next() throws java.io.IOException;
		 bool Next();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException;
		 void Close();

		 long Index();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RaftLogCursor empty()
	//	 {
	//		  return new RaftLogCursor()
	//		  {
	//				@@Override public boolean next()
	//				{
	//					 return false;
	//				}
	//
	//				@@Override public void close()
	//				{
	//				}
	//
	//				@@Override public long index()
	//				{
	//					 return -1;
	//				}
	//
	//				@@Override public RaftLogEntry get()
	//				{
	//					 throw new IllegalStateException();
	//				}
	//		  };
	//	 }
	}

}