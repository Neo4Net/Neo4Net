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
namespace Neo4Net.Server.preflight
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.SuppressOutput.suppressAll;

	public class TestPreflightTasks
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassWithNoRules()
		 public virtual void ShouldPassWithNoRules()
		 {
			  PreFlightTasks check = new PreFlightTasks( NullLogProvider.Instance );
			  assertTrue( check.Run() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunAllHealthChecksToCompletionIfNonFail()
		 public virtual void ShouldRunAllHealthChecksToCompletionIfNonFail()
		 {
			  PreFlightTasks check = new PreFlightTasks( NullLogProvider.Instance, PassingRules );
			  assertTrue( check.Run() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfOneOrMoreHealthChecksFail()
		 public virtual void ShouldFailIfOneOrMoreHealthChecksFail()
		 {
			  PreFlightTasks check = new PreFlightTasks( NullLogProvider.Instance, WithOneFailingRule );
			  assertFalse( check.Run() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailedRule()
		 public virtual void ShouldLogFailedRule()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  PreFlightTasks check = new PreFlightTasks( logProvider, WithOneFailingRule );
			  check.Run();

			  logProvider.AssertExactly( inLog( typeof( PreFlightTasks ) ).error( "blah blah" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseFailedRule()
		 public virtual void ShouldAdvertiseFailedRule()
		 {
			  PreFlightTasks check = new PreFlightTasks( NullLogProvider.Instance, WithOneFailingRule );
			  check.Run();
			  assertNotNull( check.FailedTask() );
		 }

		 private PreflightTask[] WithOneFailingRule
		 {
			 get
			 {
				  PreflightTask[] rules = new PreflightTask[5];
   
				  for ( int i = 0; i < rules.Length; i++ )
				  {
						rules[i] = new PreflightTaskAnonymousInnerClass( this );
				  }
   
				  rules[rules.Length / 2] = new PreflightTaskAnonymousInnerClass2( this );
   
				  return rules;
			 }
		 }

		 private class PreflightTaskAnonymousInnerClass : PreflightTask
		 {
			 private readonly TestPreflightTasks _outerInstance;

			 public PreflightTaskAnonymousInnerClass( TestPreflightTasks outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool run()
			 {
				  return true;
			 }

			 public string FailureMessage
			 {
				 get
				 {
					  return "blah blah";
				 }
			 }
		 }

		 private class PreflightTaskAnonymousInnerClass2 : PreflightTask
		 {
			 private readonly TestPreflightTasks _outerInstance;

			 public PreflightTaskAnonymousInnerClass2( TestPreflightTasks outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool run()
			 {
				  return false;
			 }

			 public string FailureMessage
			 {
				 get
				 {
					  return "blah blah";
				 }
			 }
		 }

		 private PreflightTask[] PassingRules
		 {
			 get
			 {
				  PreflightTask[] rules = new PreflightTask[5];
   
				  for ( int i = 0; i < rules.Length; i++ )
				  {
						rules[i] = new PreflightTaskAnonymousInnerClass3( this );
				  }
   
				  return rules;
			 }
		 }

		 private class PreflightTaskAnonymousInnerClass3 : PreflightTask
		 {
			 private readonly TestPreflightTasks _outerInstance;

			 public PreflightTaskAnonymousInnerClass3( TestPreflightTasks outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool run()
			 {
				  return true;
			 }

			 public string FailureMessage
			 {
				 get
				 {
					  return "blah blah";
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();
	}

}