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
namespace Neo4Net.Io.pagecache.stresstests
{
	using Test = org.junit.Test;

	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using PageCacheStressTest = Neo4Net.Io.pagecache.stress.PageCacheStressTest;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.ensureExistsAndEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.fromEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.stress.Conditions.timePeriod;

	/// <summary>
	/// Notice the class name: this is _not_ going to be run as part of the main build.
	/// </summary>
	public class PageCacheStressTesting
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveCorrectlyUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveCorrectlyUnderStress()
		 {
			  int durationInMinutes = parseInt( fromEnv( "PAGE_CACHE_STRESS_DURATION", "1" ) );
			  int numberOfPages = parseInt( fromEnv( "PAGE_CACHE_STRESS_NUMBER_OF_PAGES", "10000" ) );
			  int numberOfThreads = parseInt( fromEnv( "PAGE_CACHE_STRESS_NUMBER_OF_THREADS", "8" ) );
			  int numberOfCachePages = parseInt( fromEnv( "PAGE_CACHE_STRESS_NUMBER_OF_CACHE_PAGES", "1000" ) );
			  File baseDir = new File( fromEnv( "PAGE_CACHE_STRESS_WORKING_DIRECTORY", getProperty( "java.io.tmpdir" ) ) );

			  File workingDirectory = new File( baseDir, "working" );

			  DefaultPageCacheTracer monitor = new DefaultPageCacheTracer();
			  PageCacheStressTest runner = ( new PageCacheStressTest.Builder() ).with(timePeriod(durationInMinutes, MINUTES)).withNumberOfPages(numberOfPages).withNumberOfThreads(numberOfThreads).withNumberOfCachePages(numberOfCachePages).withWorkingDirectory(ensureExistsAndEmpty(workingDirectory)).with(monitor).build();

			  runner.Run();

			  long faults = monitor.Faults();
			  long evictions = monitor.Evictions();
			  long pins = monitor.Pins();
			  long unpins = monitor.Unpins();
			  long flushes = monitor.Flushes();
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: System.out.printf(" - page faults: %d%n - evictions: %d%n - pins: %d%n - unpins: %d%n - flushes: %d%n", faults, evictions, pins, unpins, flushes);
			  Console.Write( " - page faults: %d%n - evictions: %d%n - pins: %d%n - unpins: %d%n - flushes: %d%n", faults, evictions, pins, unpins, flushes );

			  // let's cleanup disk space when everything went well
			  FileUtils.deleteRecursively( workingDirectory );
		 }
	}

}