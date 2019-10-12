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
namespace Org.Neo4j.Test.extension
{
	using Test = org.junit.jupiter.api.Test;
	using ConditionEvaluationResult = org.junit.jupiter.api.extension.ConditionEvaluationResult;
	using ExecutionCondition = org.junit.jupiter.api.extension.ExecutionCondition;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;


	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.extension.ConditionEvaluationResult.disabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.FAILED_TEST_FILE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.LOCKED_TEST_FILE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.SUCCESSFUL_TEST_FILE_KEY;

	/// <summary>
	/// This test is disabled by default and not executed directly by test runner.
	/// It will be executed by a specific test executor as part of extensions lifecycle testing. </summary>
	/// <seealso cref= TestDirectoryExtensionTest#failedTestShouldKeepDirectory() </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) @ExtendWith(DirectoryExtensionLifecycleVerificationTest.ConfigurationParameterCondition.class) class DirectoryExtensionLifecycleVerificationTest
	internal class DirectoryExtensionLifecycleVerificationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void executeAndCleanupDirectory()
		 internal virtual void ExecuteAndCleanupDirectory()
		 {
			  File file = _directory.createFile( "a" );
			  assertTrue( file.exists() );
			  CONTEXT.setValue( SUCCESSFUL_TEST_FILE_KEY, file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failAndKeepDirectory()
		 internal virtual void FailAndKeepDirectory()
		 {
			  File file = _directory.createFile( "b" );
			  CONTEXT.setValue( FAILED_TEST_FILE_KEY, file );
			  throw new Exception( "simulate test failure" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lockFileAndFailToDeleteDirectory()
		 internal virtual void LockFileAndFailToDeleteDirectory()
		 {
			  File nonDeletableDirectory = _directory.directory( "c" );
			  CONTEXT.setValue( LOCKED_TEST_FILE_KEY, nonDeletableDirectory );
			  assertTrue( nonDeletableDirectory.setReadable( false, false ) );
		 }

		 internal class ConfigurationParameterCondition : ExecutionCondition
		 {
			  internal const string TEST_TOGGLE = "testToggle";

			  public override ConditionEvaluationResult EvaluateExecutionCondition( ExtensionContext context )
			  {
					Optional<string> option = context.getConfigurationParameter( TEST_TOGGLE );
					return option.map( ConditionEvaluationResult.enabled ).orElseGet( () => disabled("configuration parameter not present") );
			  }
		 }
	}

}