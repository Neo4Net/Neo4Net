using System.Text;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Diagnostics.utils
{

	public class DumpUtils
	{
		 private DumpUtils()
		 {
		 }

		 /// <summary>
		 /// Creates threads dump and try to mimic JVM stack trace as much as possible to allow existing analytics tools to be used
		 /// </summary>
		 /// <returns> string that contains thread dump </returns>
		 public static string ThreadDump()
		 {
			  ThreadMXBean threadMxBean = ManagementFactory.ThreadMXBean;
			  Properties systemProperties = System.Properties;

			  return ThreadDump( threadMxBean, systemProperties );
		 }

		 /// <summary>
		 /// Creates threads dump and try to mimic JVM stack trace as much as possible to allow existing analytics tools to be used
		 /// </summary>
		 /// <param name="threadMxBean"> bean to use for thread dump </param>
		 /// <param name="systemProperties"> dumped vm system properties </param>
		 /// <returns> string that contains thread dump </returns>
		 public static string ThreadDump( ThreadMXBean threadMxBean, Properties systemProperties )
		 {
			  ThreadInfo[] threadInfos = threadMxBean.dumpAllThreads( true, true );

			  // Reproduce JVM stack trace as far as possible to allow existing analytics tools to be used
			  string vmName = systemProperties.getProperty( "java.vm.name" );
			  string vmVersion = systemProperties.getProperty( "java.vm.version" );
			  string vmInfoString = systemProperties.getProperty( "java.vm.info" );

			  StringBuilder sb = new StringBuilder();
			  sb.Append( string.Format( "Full thread dump {0} ({1} {2}):\n\n", vmName, vmVersion, vmInfoString ) );
			  foreach ( ThreadInfo threadInfo in threadInfos )
			  {
					sb.Append( string.Format( "\"{0}\" #{1:D}\n", threadInfo.ThreadName, threadInfo.ThreadId ) );
					sb.Append( "   java.lang.Thread.State: " ).Append( threadInfo.ThreadState ).Append( "\n" );

					StackTraceElement[] stackTrace = threadInfo.StackTrace;
					for ( int i = 0; i < stackTrace.Length; i++ )
					{
						 StackTraceElement e = stackTrace[i];
						 sb.Append( "\tat " ).Append( e.ToString() ).Append('\n');

						 // First stack element info can be found in the thread state
						 if ( i == 0 && threadInfo.LockInfo != null )
						 {
							  Thread.State ts = threadInfo.ThreadState;
							  switch ( ts )
							  {
							  case BLOCKED:
									sb.Append( "\t-  blocked on " ).Append( threadInfo.LockInfo ).Append( '\n' );
									break;
							  case WAITING:
									sb.Append( "\t-  waiting on " ).Append( threadInfo.LockInfo ).Append( '\n' );
									break;
							  case TIMED_WAITING:
									sb.Append( "\t-  waiting on " ).Append( threadInfo.LockInfo ).Append( '\n' );
									break;
							  default:
						  break;
							  }
						 }
						 foreach ( MonitorInfo mi in threadInfo.LockedMonitors )
						 {
							  if ( mi.LockedStackDepth == i )
							  {
									sb.Append( "\t-  locked " ).Append( mi ).Append( '\n' );
							  }
						 }
					}
			  }

			  return sb.ToString();
		 }
	}

}