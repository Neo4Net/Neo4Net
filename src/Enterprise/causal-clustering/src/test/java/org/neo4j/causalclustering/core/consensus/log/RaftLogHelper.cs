﻿/*
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
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	public class RaftLogHelper
	{
		 private RaftLogHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static RaftLogEntry readLogEntry(ReadableRaftLog raftLog, long index) throws java.io.IOException
		 public static RaftLogEntry ReadLogEntry( ReadableRaftLog raftLog, long index )
		 {
			  using ( RaftLogCursor cursor = raftLog.GetEntryCursor( index ) )
			  {
					if ( cursor.Next() )
					{
						 return cursor.get();
					}
			  }

			  //todo: do not do this and update RaftLogContractTest to not depend on this exception.
			  throw new IOException( "Asked for raft log entry at index " + index + " but it was not found" );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super RaftLog> hasNoContent(long index)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> HasNoContent( long index )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( index );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<RaftLog>
		 {
			 private long _index;

			 public TypeSafeMatcherAnonymousInnerClass( long index )
			 {
				 this._index = index;
			 }

			 protected internal override bool matchesSafely( RaftLog log )
			 {
				  try
				  {
						ReadLogEntry( log, _index );
				  }
				  catch ( IOException )
				  {
						// oh well...
						return true;
				  }
				  return false;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Log should not contain entry at index " ).appendValue( _index );
			 }
		 }
	}

}