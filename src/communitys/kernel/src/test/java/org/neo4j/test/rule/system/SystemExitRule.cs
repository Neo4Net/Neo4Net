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
namespace Neo4Net.Test.rule.system
{
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class SystemExitRule : ExternalResource
	{
		 private int? _expectedExitStatusCode;
		 private SecurityManager _originalSecurityManager;

		 private SystemExitRule()
		 {
		 }

		 public static SystemExitRule None()
		 {
			  return new SystemExitRule();
		 }

		 public virtual void ExpectExit( int statusCode )
		 {
			  this._expectedExitStatusCode = statusCode;
		 }

		 protected internal override void Before()
		 {
			  _originalSecurityManager = System.SecurityManager;
			  TestSecurityManager testSecurityManager = new TestSecurityManager( _originalSecurityManager );
			  System.SecurityManager = testSecurityManager;
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.junit.runners.model.Statement externalRuleStatement = super.apply(super, description);
			  Statement externalRuleStatement = base.Apply( @base, description );
			  return new StatementAnonymousInnerClass( this, externalRuleStatement );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly SystemExitRule _outerInstance;

			 private Statement _externalRuleStatement;

			 public StatementAnonymousInnerClass( SystemExitRule outerInstance, Statement externalRuleStatement )
			 {
				 this.outerInstance = outerInstance;
				 this._externalRuleStatement = externalRuleStatement;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  try
				  {
						_externalRuleStatement.evaluate();
						if ( outerInstance.exitWasExpected() )
						{
							 fail( "System exit call was expected, but not invoked." );
						}
				  }
				  catch ( SystemExitError e )
				  {
						int exceptionStatusCode = e.StatusCode;
						if ( outerInstance.exitWasExpected() )
						{
							 int expectedCode = _outerInstance.expectedExitStatusCode.Value;
							 assertEquals( string.Format( "Expected system exit code:{0:D} but was: {1:D}.", expectedCode, exceptionStatusCode ), expectedCode, exceptionStatusCode );
						}
						else
						{
							 fail( "System exit call was not expected, but was invoked. Exit status code: " + exceptionStatusCode );
						}

				  }
			 }
		 }

		 protected internal override void After()
		 {
			  System.SecurityManager = _originalSecurityManager;
		 }

		 private bool ExitWasExpected()
		 {
			  return _expectedExitStatusCode != null;
		 }
	}

}