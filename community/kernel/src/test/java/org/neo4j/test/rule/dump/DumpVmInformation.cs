using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Test.rule.dump
{
	using State = Thread.State;

	public class DumpVmInformation
	{
		 private DumpVmInformation()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void dumpVmInfo(java.io.File directory) throws java.io.IOException
		 public static void DumpVmInfo( File directory )
		 {
			  File file = new File( directory, "main-vm-dump-" + DateTimeHelper.CurrentUnixTimeMillis() );
			  PrintStream @out = null;
			  try
			  {
					@out = new PrintStream( file );
					DumpVmInfo( @out );
			  }
			  finally
			  {
					if ( @out != null )
					{
						 @out.close();
					}
			  }
		 }

		 public static void DumpVmInfo( PrintStream @out )
		 {
			  // Find the top thread group
			  ThreadGroup topThreadGroup = Thread.CurrentThread.ThreadGroup;
			  while ( topThreadGroup.Parent != null )
			  {
					topThreadGroup = topThreadGroup.Parent;
			  }

			  // Get all the thread groups under the top.
			  ThreadGroup[] allGroups = new ThreadGroup[1000];
			  topThreadGroup.enumerate( allGroups, true );

			  // Dump the info.
			  foreach ( ThreadGroup group in allGroups )
			  {
					if ( group == null )
					{
						 break;
					}
					DumpThreadGroupInfo( group, @out );
			  }
			  DumpThreadGroupInfo( topThreadGroup, @out );
		 }

		 public static void DumpThreadGroupInfo( ThreadGroup tg, PrintStream @out )
		 {
			  string parentName = tg.Parent == null ? null : tg.Parent.Name;
			  // Dump thread group info.
			  @out.println( "---- GROUP:" + tg.Name + ( !string.ReferenceEquals( parentName, null ) ? " parent:" + parentName : "" ) + ( tg.Daemon ? " daemon" : "" ) + ( tg.Destroyed ? " destroyed" : "" ) + " ----" );
			  // Dump info for each thread.
			  Thread[] allThreads = new Thread[1000];
			  tg.enumerate( allThreads, false );
			  foreach ( Thread thread in allThreads )
			  {
					if ( thread == null )
					{
						 break;
					}
					@out.println( "\"" + thread.Name + "\"" + ( thread.Daemon ? " daemon" : "" ) + " prio=" + thread.Priority + " tid=" + thread.Id + " " + thread.State.name().ToLower() );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					@out.println( "  " + typeof( Thread.State ).FullName + ": " + thread.State.name() );
					foreach ( StackTraceElement element in thread.StackTrace )
					{
						 @out.println( "\tat " + element );
					}
			  }
		 }
	}

}