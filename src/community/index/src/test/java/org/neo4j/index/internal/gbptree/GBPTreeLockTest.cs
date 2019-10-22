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
namespace Neo4Net.Index.Internal.gbptree
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Helpers.Collections;
	using Race = Neo4Net.Test.Race;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class GBPTreeLockTest
	{
		 // Lock can be in following states and this test verify transitions back and forth between states
		 // and also verify expected behaviour after each transition.
		 //            Writer   | Cleaner
		 // State UU - unlocked | unlocked
		 // State UL - unlocked | locked
		 // State LU - locked   | unlocked
		 // State LL - locked   | locked

		 private readonly GBPTreeLock @lock = new GBPTreeLock();
		 private GBPTreeLock _copy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> executor = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> Executor = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_UU_UL_UU() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestUUULUU()
		 {
			  // given
			  AssertUU();

			  // then
			  @lock.CleanerLock();
			  AssertUL();

			  @lock.CleanerUnlock();
			  AssertUU();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_UL_LL_UL() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestULLLUL()
		 {
			  // given
			  @lock.CleanerLock();
			  AssertUL();

			  // then
			  @lock.WriterLock();
			  AssertLL();

			  @lock.WriterUnlock();
			  AssertUL();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_LL_LU_LL() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestLLLULL()
		 {
			  // given
			  @lock.WriterLock();
			  @lock.CleanerLock();
			  AssertLL();

			  // then
			  @lock.CleanerUnlock();
			  AssertLU();

			  @lock.CleanerLock();
			  AssertLL();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_LU_UU_LU() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestLUUULU()
		 {
			  // given
			  @lock.WriterLock();
			  AssertLU();

			  // then
			  @lock.WriterUnlock();
			  AssertUU();

			  @lock.WriterLock();
			  AssertLU();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_UU_LL_UU() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestUULLUU()
		 {
			  // given
			  AssertUU();

			  // then
			  @lock.WriterAndCleanerLock();
			  AssertLL();

			  @lock.WriterAndCleanerUnlock();
			  AssertUU();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void test_race_ULvsUL() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceULvsUL()
		 {
			  AssertOnlyOneSucceeds( @lock.cleanerLock, @lock.cleanerLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test_race_ULvsLU() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceULvsLU()
		 {
			  AssertBothSucceeds( @lock.cleanerLock, @lock.writerLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void test_race_ULvsLL() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceULvsLL()
		 {
			  AssertOnlyOneSucceeds( @lock.cleanerLock, @lock.writerAndCleanerLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void test_race_LUvsLU() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceLUvsLU()
		 {
			  AssertOnlyOneSucceeds( @lock.writerLock, @lock.writerLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void test_race_LUvsLL() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceLUvsLL()
		 {
			  AssertOnlyOneSucceeds( @lock.writerLock, @lock.writerAndCleanerLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void test_race_LLvsLL() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRaceLLvsLL()
		 {
			  AssertOnlyOneSucceeds( @lock.writerAndCleanerLock, @lock.writerAndCleanerLock );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertOnlyOneSucceeds(Runnable lockAction1, Runnable lockAction2) throws Throwable
		 private void AssertOnlyOneSucceeds( ThreadStart lockAction1, ThreadStart lockAction2 )
		 {
			  AssertUU();
			  Race race = new Race();
			  LockContestant c1 = new LockContestant( lockAction1 );
			  LockContestant c2 = new LockContestant( lockAction2 );

			  // when
			  race.AddContestant( c1 );
			  race.AddContestant( c2 );

			  race.GoAsync();
			  while ( !( c1.LockAcquired() || c2.LockAcquired() ) || !(c1.Started() && c2.Started()) )
			  {
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 1 ) );
			  }

			  // then
			  Pair<bool, bool> c1State = c1.State();
			  Pair<bool, bool> c2State = c2.State();
			  assertNotEquals( WithState( "Expected exactly one to acquire lock.", c1State, c2State ), c1State.First(), c2State.First() );
			  assertTrue( WithState( "Expected both to be started.", c1State, c2State ), c1State.Other() && c2State.Other() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertBothSucceeds(Runnable lockAction1, Runnable lockAction2) throws Throwable
		 private void AssertBothSucceeds( ThreadStart lockAction1, ThreadStart lockAction2 )
		 {
			  AssertUU();
			  Race race = new Race();
			  LockContestant c1 = new LockContestant( lockAction1 );
			  LockContestant c2 = new LockContestant( lockAction2 );

			  // when
			  race.AddContestant( c1 );
			  race.AddContestant( c2 );

			  race.Go();

			  // then
			  Pair<bool, bool> c1State = c1.State();
			  Pair<bool, bool> c2State = c2.State();
			  assertTrue( WithState( "Expected both to acquire lock.", c1State, c2State ), c1State.First() && c2State.First() );
			  assertTrue( WithState( "Expected both to be started.", c1State, c2State ), c1State.Other() && c2State.Other() );
		 }

		 private string WithState( string message, Pair<bool, bool> c1State, Pair<bool, bool> c2State )
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("%s c1.lockAcquired=%b, c1.started=%b, c2.lockAcquired=%b, c2.started=%b", message, c1State.first(), c1State.other(), c2State.first(), c2State.other());
			  return string.Format( "%s c1.lockAcquired=%b, c1.started=%b, c2.lockAcquired=%b, c2.started=%b", message, c1State.First(), c1State.Other(), c2State.First(), c2State.Other() );
		 }

		 private class LockContestant : ThreadStart
		 {
			  internal readonly ThreadStart LockAction;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicBoolean LockAcquiredConflict = new AtomicBoolean();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicBoolean StartedConflict = new AtomicBoolean();

			  internal LockContestant( ThreadStart lockAction )
			  {
					this.LockAction = lockAction;
			  }

			  public override void Run()
			  {
					StartedConflict.set( true );
					LockAction.run();
					LockAcquiredConflict.set( true );
			  }

			  internal virtual Pair<bool, bool> State()
			  {
					return Pair.of( LockAcquired(), Started() );
			  }

			  internal virtual bool LockAcquired()
			  {
					return LockAcquiredConflict.get();
			  }

			  internal virtual bool Started()
			  {
					return StartedConflict.get();
			  }
		 }

		 private void AssertThrow( ThreadStart unlock )
		 {
			  try
			  {
					unlock.run();
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertBlock(Runnable runLock, Runnable runUnlock) throws Exception
		 private void AssertBlock( ThreadStart runLock, ThreadStart runUnlock )
		 {
			  Future<object> future = Executor.execute(state =>
			  {
				runLock.run();
				return null;
			  });
			  Executor.get().waitUntilWaiting(details => details.isAt(typeof(GBPTreeLock), "doLock"));
			  runUnlock.run();
			  future.get();
		 }

		 private void AssertUU()
		 {
			  AssertThrow( @lock.writerUnlock );
			  AssertThrow( @lock.cleanerUnlock );
			  AssertThrow( @lock.writerAndCleanerUnlock );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertUL() throws Exception
		 private void AssertUL()
		 {
			  AssertThrow( @lock.writerUnlock );
			  AssertThrow( @lock.writerAndCleanerUnlock );
			  _copy = @lock.Copy();
			  AssertBlock( _copy.cleanerLock, _copy.cleanerUnlock );
			  _copy = @lock.Copy();
			  AssertBlock( _copy.writerAndCleanerLock, _copy.cleanerUnlock );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLU() throws Exception
		 private void AssertLU()
		 {
			  AssertThrow( @lock.cleanerUnlock );
			  AssertThrow( @lock.writerAndCleanerUnlock );
			  _copy = @lock.Copy();
			  AssertBlock( _copy.writerLock, _copy.writerUnlock );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLL() throws Exception
		 private void AssertLL()
		 {
			  _copy = @lock.Copy();
			  AssertBlock( _copy.writerLock, _copy.writerUnlock );
			  _copy = @lock.Copy();
			  AssertBlock( _copy.cleanerLock, _copy.cleanerUnlock );
			  _copy = @lock.Copy();
			  AssertBlock( _copy.writerAndCleanerLock, _copy.writerAndCleanerUnlock );
		 }
	}

}