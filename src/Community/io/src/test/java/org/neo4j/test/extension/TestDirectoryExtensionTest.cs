using System;

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
namespace Neo4Net.Test.extension
{
	using Test = org.junit.jupiter.api.Test;
	using EnabledOnOs = org.junit.jupiter.api.condition.EnabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using TestExecutionResult = org.junit.platform.engine.TestExecutionResult;
	using Launcher = org.junit.platform.launcher.Launcher;
	using LauncherDiscoveryRequest = org.junit.platform.launcher.LauncherDiscoveryRequest;
	using TestExecutionListener = org.junit.platform.launcher.TestExecutionListener;
	using TestIdentifier = org.junit.platform.launcher.TestIdentifier;
	using LauncherDiscoveryRequestBuilder = org.junit.platform.launcher.core.LauncherDiscoveryRequestBuilder;
	using LauncherFactory = org.junit.platform.launcher.core.LauncherFactory;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.platform.engine.TestExecutionResult.Status.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.platform.engine.discovery.DiscoverySelectors.selectMethod;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.DirectoryExtensionLifecycleVerificationTest.ConfigurationParameterCondition.TEST_TOGGLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.FAILED_TEST_FILE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.LOCKED_TEST_FILE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.SUCCESSFUL_TEST_FILE_KEY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class TestDirectoryExtensionTest
	internal class TestDirectoryExtensionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject DefaultFileSystemAbstraction fileSystem;
		 internal DefaultFileSystemAbstraction FileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDirectoryInjectionWorks()
		 internal virtual void TestDirectoryInjectionWorks()
		 {
			  assertNotNull( TestDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDirectoryInitialisedForUsage()
		 internal virtual void TestDirectoryInitialisedForUsage()
		 {
			  File directory = TestDirectory.directory();
			  assertNotNull( directory );
			  assertTrue( directory.exists() );
			  Path targetTestData = Paths.get( "target", "test data" );
			  assertTrue( directory.AbsolutePath.contains( targetTestData.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDirectoryUsesFileSystemFromExtension()
		 internal virtual void TestDirectoryUsesFileSystemFromExtension()
		 {
			  assertSame( FileSystem, TestDirectory.FileSystem );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createTestFile()
		 internal virtual void CreateTestFile()
		 {
			  File file = TestDirectory.createFile( "a" );
			  assertEquals( "a", file.Name );
			  assertTrue( FileSystem.fileExists( file ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failedTestShouldKeepDirectory()
		 internal virtual void FailedTestShouldKeepDirectory()
		 {
			  CONTEXT.clear();
			  Execute( "failAndKeepDirectory" );
			  File failedFile = CONTEXT.getValue( FAILED_TEST_FILE_KEY );
			  assertNotNull( failedFile );
			  assertTrue( failedFile.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void successfulTestShouldCleanupDirectory()
		 internal virtual void SuccessfulTestShouldCleanupDirectory()
		 {
			  CONTEXT.clear();
			  Execute( "executeAndCleanupDirectory" );
			  File greenTestFail = CONTEXT.getValue( SUCCESSFUL_TEST_FILE_KEY );
			  assertNotNull( greenTestFail );
			  assertFalse( greenTestFail.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @EnabledOnOs(org.junit.jupiter.api.condition.OS.LINUX) void exceptionOnDirectoryDeletionIncludeTestDisplayName() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ExceptionOnDirectoryDeletionIncludeTestDisplayName()
		 {
			  CONTEXT.clear();
			  FailedTestExecutionListener failedTestListener = new FailedTestExecutionListener();
			  Execute( "lockFileAndFailToDeleteDirectory", failedTestListener );
			  File lockedFile = CONTEXT.getValue( LOCKED_TEST_FILE_KEY );

			  assertNotNull( lockedFile );
			  assertTrue( lockedFile.setReadable( true, true ) );
			  FileUtils.deleteRecursively( lockedFile );
			  failedTestListener.AssertTestObserver();
		 }

		 private static void Execute( string testName, params TestExecutionListener[] testExecutionListeners )
		 {
			  LauncherDiscoveryRequest discoveryRequest = LauncherDiscoveryRequestBuilder.request().selectors(selectMethod(typeof(DirectoryExtensionLifecycleVerificationTest), testName)).configurationParameter(TEST_TOGGLE, "true").build();
			  Launcher launcher = LauncherFactory.create();
			  launcher.execute( discoveryRequest, testExecutionListeners );
		 }

		 private class FailedTestExecutionListener : TestExecutionListener
		 {
			  internal int ResultsObserved;

			  public override void ExecutionFinished( TestIdentifier testIdentifier, TestExecutionResult testExecutionResult )
			  {
					if ( testExecutionResult.Status == FAILED )
					{
						 ResultsObserved++;
						 string exceptionMessage = testExecutionResult.Throwable.map( Exception.getMessage ).orElse( EMPTY );
						 assertThat( exceptionMessage, containsString( "Fail to cleanup test directory for lockFileAndFailToDeleteDirectory" ) );
					}
			  }

			  internal virtual void AssertTestObserver()
			  {
					assertEquals( 1, ResultsObserved );
			  }
		 }
	}

}