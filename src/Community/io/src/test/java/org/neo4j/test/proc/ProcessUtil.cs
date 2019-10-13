using System.Collections.Generic;

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
namespace Neo4Net.Test.proc
{

	/// <summary>
	/// Utility methods for accessing information about the current Java process.
	/// </summary>
	public class ProcessUtil
	{
		 private ProcessUtil()
		 {
		 }

		 /// <summary>
		 /// Get the path to the {@code java} executable that is running this Java program.
		 /// <para>
		 /// This is useful for starting other Java programs using the same exact version of Java.
		 /// </para>
		 /// <para>
		 /// This value is computed from the {@code java.home} system property.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The path to the {@code java} executable that launched this Java process. </returns>
		 public static Path JavaExecutable
		 {
			 get
			 {
				  string javaHome = System.getProperty( "java.home" );
				  return Paths.get( javaHome, "bin", "java" );
			 }
		 }

		 /// <summary>
		 /// Get the list of command line arguments that were passed to the Java runtime, as opposed to the Java program.
		 /// </summary>
		 /// <seealso cref= RuntimeMXBean#getInputArguments() </seealso>
		 /// <returns> The list of arguments, as Strings, that were given to the Java runtime. </returns>
		 public static IList<string> JavaExecutableArguments
		 {
			 get
			 {
				  RuntimeMXBean runtimeMxBean = ManagementFactory.RuntimeMXBean;
				  return runtimeMxBean.InputArguments;
			 }
		 }

		 /// <summary>
		 /// Get the current classpath as a list of file names. </summary>
		 /// <returns> The list of file names that makes the classpath. </returns>
		 public static IList<string> ClassPathList
		 {
			 get
			 {
				  return Arrays.asList( ClassPath.Split( File.pathSeparator, true ) );
			 }
		 }

		 /// <summary>
		 /// Get the classpath as a single string of all the classpath file entries, separated by the path separator.
		 /// 
		 /// This is based on the {@code java.class.path} system property. </summary>
		 /// <seealso cref= File#pathSeparator </seealso>
		 /// <returns> The current classpath. </returns>
		 public static string ClassPath
		 {
			 get
			 {
				  return System.getProperty( "java.class.path" );
			 }
		 }
	}

}