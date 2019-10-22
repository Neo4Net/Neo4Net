using System;

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
namespace Neo4Net.Test
{
	using Test = org.junit.jupiter.api.Test;


	using Runnables = Neo4Net.Utils.Concurrent.Runnables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.Race.throwing;

	/// <summary>
	/// Test of a test utility <seealso cref="Race"/>.
	/// </summary>
	internal class RaceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWaitForAllContestantsToComplete() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWaitForAllContestantsToComplete()
		 {
			  // GIVEN
			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger completed = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger completed = new AtomicInteger();
			  int count = 5;
			  race.AddContestants(count, throwing(() =>
			  {
				sleep( current().Next(100) );
				completed.incrementAndGet();
			  }));

			  // WHEN
			  race.Go();

			  // THEN
			  assertEquals( count, completed.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConsultEndCondition() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldConsultEndCondition()
		 {
			  // GIVEN
			  CallCountBooleanSupplier endCondition = new CallCountBooleanSupplier( 100 );
			  Race race = ( new Race() ).WithEndCondition(endCondition);
			  race.AddContestants( 20, throwing( () => sleep(10) ) );

			  // WHEN
			  race.Go();

			  // THEN
			  assertTrue( endCondition.CallCount.get() >= 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveMultipleEndConditions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHaveMultipleEndConditions()
		 {
			  // GIVEN
			  ControlledBooleanSupplier endCondition1 = spy( new ControlledBooleanSupplier( false ) );
			  ControlledBooleanSupplier endCondition2 = spy( new ControlledBooleanSupplier( false ) );
			  ControlledBooleanSupplier endCondition3 = spy( new ControlledBooleanSupplier( false ) );
			  Race race = ( new Race() ).WithEndCondition(endCondition1, endCondition2, endCondition3);
			  race.AddContestant( () => endCondition2.set(true) );
			  race.AddContestants( 3, Runnables.EMPTY_RUNNABLE );

			  // WHEN
			  race.Go();

			  // THEN
			  verify( endCondition1, atLeast( 4 ) ).AsBoolean;
			  verify( endCondition2, atLeast( 4 ) ).AsBoolean;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBreakOnError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBreakOnError()
		 {
			  // GIVEN
			  string error = "Noooo";
			  Race race = new Race();
			  race.WithEndCondition( () => false ); // <-- never end
			  race.AddContestant(() =>
			  {
				throw new Exception( error );
			  });
			  race.AddContestants(3, () =>
			  {
			  });

			  // WHEN
			  Exception exception = assertThrows( typeof( Exception ), () => race.go() );
			  assertEquals( error, exception.Message );
		 }

		 public class ControlledBooleanSupplier : System.Func<bool>
		 {
			  internal volatile bool Value;

			  internal ControlledBooleanSupplier( bool initialValue )
			  {
					this.Value = initialValue;
			  }

			  public virtual void Set( bool value )
			  {
					this.Value = value;
			  }

			  public override bool AsBoolean
			  {
				  get
				  {
						return Value;
				  }
			  }
		 }

		 public class CallCountBooleanSupplier : System.Func<bool>
		 {
			  internal readonly int CallCountTriggeringTrueEndCondition;
			  internal readonly AtomicInteger CallCount = new AtomicInteger();

			  internal CallCountBooleanSupplier( int callCountTriggeringTrueEndCondition )
			  {
					this.CallCountTriggeringTrueEndCondition = callCountTriggeringTrueEndCondition;
			  }

			  public override bool AsBoolean
			  {
				  get
				  {
						return CallCount.incrementAndGet() >= CallCountTriggeringTrueEndCondition;
				  }
			  }
		 }
	}

}