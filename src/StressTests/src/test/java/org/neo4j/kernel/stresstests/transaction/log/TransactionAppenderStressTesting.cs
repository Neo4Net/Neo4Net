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
namespace Neo4Net.Kernel.stresstests.transaction.log
{
	using Test = org.junit.Test;


	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Builder = Neo4Net.Kernel.impl.transaction.log.stresstest.TransactionAppenderStressTest.Builder;
	using TransactionIdChecker = Neo4Net.Kernel.impl.transaction.log.stresstest.TransactionAppenderStressTest.TransactionIdChecker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.untilTimeExpired;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.ensureExistsAndEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.fromEnv;

	/// <summary>
	/// Notice the class name: this is _not_ going to be run as part of the main build.
	/// </summary>
	public class TransactionAppenderStressTesting
	{
		 private const string DEFAULT_DURATION_IN_MINUTES = "5";
		 private static readonly string _defaultWorkingDir = new File( getProperty( "java.io.tmpdir" ), "working" ).Path;
		 private const string DEFAULT_NUM_THREADS = "10";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveCorrectlyUnderStress() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveCorrectlyUnderStress()
		 {
			  int durationInMinutes = parseInt( fromEnv( "TX_APPENDER_STRESS_DURATION", DEFAULT_DURATION_IN_MINUTES ) );
			  File workingDirectory = new File( fromEnv( "TX_APPENDER_WORKING_DIRECTORY", _defaultWorkingDir ) );
			  int threads = parseInt( fromEnv( "TX_APPENDER_NUM_THREADS", DEFAULT_NUM_THREADS ) );

			  Callable<long> runner = ( new Builder() ).with(untilTimeExpired(durationInMinutes, MINUTES)).withWorkingDirectory(DatabaseLayout.of(ensureExistsAndEmpty(workingDirectory))).withNumThreads(threads).build();

			  long appendedTxs = runner.call();

			  assertEquals( ( new TransactionIdChecker( workingDirectory ) ).parseAllTxLogs(), appendedTxs );

			  // let's cleanup disk space when everything went well
			  FileUtils.deleteRecursively( workingDirectory );
		 }
	}

}