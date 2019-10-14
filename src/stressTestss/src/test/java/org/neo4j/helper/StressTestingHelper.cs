using System;

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
namespace Neo4Net.helper
{

	using FileUtils = Neo4Net.Io.fs.FileUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getenv;

	public class StressTestingHelper
	{
		 private StressTestingHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File ensureExistsAndEmpty(java.io.File directory) throws java.io.IOException
		 public static File EnsureExistsAndEmpty( File directory )
		 {
			  FileUtils.deleteRecursively( directory );

			  if ( !directory.mkdirs() )
			  {
					throw new Exception( "Could not create directory: " + directory.AbsolutePath );
			  }
			  return directory;
		 }

		 public static string FromEnv( string environmentVariableName, string defaultValue )
		 {
			  string environmentVariableValue = getenv( environmentVariableName );
			  return string.ReferenceEquals( environmentVariableValue, null ) ? defaultValue : environmentVariableValue;
		 }
	}

}