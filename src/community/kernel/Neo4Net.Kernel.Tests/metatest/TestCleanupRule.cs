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
namespace Neo4Net.Metatest
{
	using Test = org.junit.Test;
	using Statement = org.junit.runners.model.Statement;
	using InOrder = org.mockito.InOrder;

	using CleanupRule = Neo4Net.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class TestCleanupRule
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanupAutoCloseable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanupAutoCloseable()
		 {
			  // GIVEN
			  CleanupRule rule = new CleanupRule();
			  AutoCloseable toClose = rule.Add( mock( typeof( AutoCloseable ) ) );

			  // WHEN
			  SimulateTestExecution( rule );

			  // THEN
			  verify( toClose ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanupObjectWithAppropriateCloseMethod() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanupObjectWithAppropriateCloseMethod()
		 {
			  // GIVEN
			  CleanupRule rule = new CleanupRule();
			  Dirt toClose = rule.Add( mock( typeof( Dirt ) ) );

			  // WHEN
			  SimulateTestExecution( rule );

			  // THEN
			  verify( toClose ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanupMultipleObjectsInReverseAddedOrder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanupMultipleObjectsInReverseAddedOrder()
		 {
			  // GIVEN
			  CleanupRule rule = new CleanupRule();
			  AutoCloseable closeable = rule.Add( mock( typeof( AutoCloseable ) ) );
			  Dirt dirt = rule.Add( mock( typeof( Dirt ) ) );

			  // WHEN
			  SimulateTestExecution( rule );

			  // THEN
			  InOrder inOrder = inOrder( dirt, closeable );
			  inOrder.verify( dirt, times( 1 ) ).shutdown();
			  inOrder.verify( closeable, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellUserIllegalArgumentIfSo()
		 public virtual void ShouldTellUserIllegalArgumentIfSo()
		 {
			  // GIVEN
			  CleanupRule rule = new CleanupRule();
			  try
			  {
					rule.Add( new object() );
					fail( "Should not accept this object" );
			  }
			  catch ( System.ArgumentException )
			  { // OK, good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void simulateTestExecution(org.neo4j.test.rule.CleanupRule rule) throws Throwable
		 private void SimulateTestExecution( CleanupRule rule )
		 {
			  rule.apply(new StatementAnonymousInnerClass(this)
			 , null).evaluate();
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly TestCleanupRule _outerInstance;

			 public StatementAnonymousInnerClass( TestCleanupRule outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void evaluate()
			 {
			 }
		 }

		 private interface Dirt
		 {
			  void Shutdown();
		 }
	}

}