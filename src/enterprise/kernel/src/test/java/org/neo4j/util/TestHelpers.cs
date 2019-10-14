using System.Collections.Generic;

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
namespace Neo4Net.Utils
{

	using AdminTool = Neo4Net.CommandLine.Admin.AdminTool;
	using ProcessStreamHandler = Neo4Net.Test.ProcessStreamHandler;
	using StreamConsumer = Neo4Net.Test.StreamConsumer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.proc.ProcessUtil.getJavaExecutable;

	public class TestHelpers
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static int runBackupToolFromOtherJvmToGetExitCode(java.io.File neo4jHome, String... args) throws Exception
		 public static int RunBackupToolFromOtherJvmToGetExitCode( File neo4jHome, params string[] args )
		 {
			  return RunBackupToolFromOtherJvmToGetExitCode( neo4jHome, System.out, System.err, true, args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static int runBackupToolFromOtherJvmToGetExitCode(java.io.File neo4jHome, java.io.PrintStream outPrintStream, java.io.PrintStream errPrintStream, boolean debug, String... args) throws Exception
		 public static int RunBackupToolFromOtherJvmToGetExitCode( File neo4jHome, PrintStream outPrintStream, PrintStream errPrintStream, bool debug, params string[] args )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  IList<string> allArgs = new List<string>( Arrays.asList( JavaExecutable.ToString(), "-cp", ClassPath, typeof(AdminTool).FullName ) );
			  allArgs.Add( "backup" );
			  ( ( IList<string> )allArgs ).AddRange( Arrays.asList( args ) );

			  ProcessBuilder processBuilder = ( new ProcessBuilder() ).command(allArgs.ToArray());
			  processBuilder.environment().put("NEO4J_HOME", neo4jHome.AbsolutePath);
			  if ( debug )
			  {
					processBuilder.environment().put("NEO4J_DEBUG", "anything_works");
			  }
			  Process process = processBuilder.start();
			  ProcessStreamHandler processStreamHandler = new ProcessStreamHandler( process, false, "", StreamConsumer.IGNORE_FAILURES, outPrintStream, errPrintStream );
			  return processStreamHandler.WaitForResult();
		 }
	}


}