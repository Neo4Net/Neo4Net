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
namespace Neo4Net.Test.extension
{
	using Test = org.junit.jupiter.api.Test;
	using ConditionEvaluationResult = org.junit.jupiter.api.extension.ConditionEvaluationResult;
	using ExecutionCondition = org.junit.jupiter.api.extension.ExecutionCondition;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;

	using Profiler = Neo4Net.Resources.Profiler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.extension.ConditionEvaluationResult.disabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.extension.ExecutionSharedContext.CONTEXT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, ProfilerExtension.class}) @ExtendWith(ProfilerExtensionVerificationTest.ConfigurationParameterCondition.class) class ProfilerExtensionVerificationTest
	internal class ProfilerExtensionVerificationTest
	{
		 internal const string TEST_DIR = "test dir";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject Profiler profiler;
		 internal Profiler Profiler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testThatPasses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestThatPasses()
		 {
			  CONTEXT.clear();
			  CONTEXT.setValue( TEST_DIR, TestDirectory.absolutePath() );
			  Profiler.profile();
			  SomeVeryExpensiveComputation();
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testThatFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestThatFails()
		 {
			  CONTEXT.clear();
			  CONTEXT.setValue( TEST_DIR, TestDirectory.absolutePath() );
			  Profiler.profile();
			  SomeVeryExpensiveComputation();
			  fail( "This is exactly like that 'worst movie death scene ever' from the Turkish film Kareteci Kiz." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void someVeryExpensiveComputation() throws InterruptedException
		 private void SomeVeryExpensiveComputation()
		 {
			  Thread.Sleep( 1000 );
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