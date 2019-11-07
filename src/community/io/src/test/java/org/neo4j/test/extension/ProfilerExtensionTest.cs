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
namespace Neo4Net.Test.extension
{
	using Test = org.junit.jupiter.api.Test;
	using Launcher = org.junit.platform.launcher.Launcher;
	using LauncherDiscoveryRequest = org.junit.platform.launcher.LauncherDiscoveryRequest;
	using TestExecutionListener = org.junit.platform.launcher.TestExecutionListener;
	using LauncherDiscoveryRequestBuilder = org.junit.platform.launcher.core.LauncherDiscoveryRequestBuilder;
	using LauncherFactory = org.junit.platform.launcher.core.LauncherFactory;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.platform.engine.discovery.DiscoverySelectors.selectMethod;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.extension.DirectoryExtensionLifecycleVerificationTest.ConfigurationParameterCondition.TEST_TOGGLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.extension.ExecutionSharedContext.CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.extension.ProfilerExtensionVerificationTest.TEST_DIR;

	internal class ProfilerExtensionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void passingTestsMustNotProduceProfilerOutput()
		 internal virtual void PassingTestsMustNotProduceProfilerOutput()
		 {
			  CONTEXT.clear();
			  Execute( "testThatPasses" );
			  File testDir = CONTEXT.getValue( TEST_DIR );
			  assertFalse( testDir.exists() ); // The TestDirectory extension deletes the test directory when the test passes.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failingTestsMustProduceProfilerOutput() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FailingTestsMustProduceProfilerOutput()
		 {
			  CONTEXT.clear();
			  Execute( "testThatFails" );
			  File testDir = CONTEXT.getValue( TEST_DIR );
			  assertTrue( testDir.exists() );
			  assertTrue( testDir.Directory );
			  File profileData = new File( testDir, "profiler-output.txt" );
			  assertTrue( profileData.exists() );
			  assertTrue( profileData.File );
			  using ( Stream<string> lines = Files.lines( profileData.toPath() ) )
			  {
					assertTrue( lines.anyMatch( line => line.contains( "someVeryExpensiveComputation" ) ) );
			  }
		 }

		 private static void Execute( string testName, params TestExecutionListener[] testExecutionListeners )
		 {
			  LauncherDiscoveryRequest discoveryRequest = LauncherDiscoveryRequestBuilder.request().selectors(selectMethod(typeof(ProfilerExtensionVerificationTest), testName)).configurationParameter(TEST_TOGGLE, "true").build();
			  Launcher launcher = LauncherFactory.create();
			  launcher.execute( discoveryRequest, testExecutionListeners );
		 }
	}

}