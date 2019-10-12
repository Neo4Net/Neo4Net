using System.Threading;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Race = Org.Neo4j.Test.Race;
	using RepeatRule = Org.Neo4j.Test.rule.RepeatRule;
	using Repeat = Org.Neo4j.Test.rule.RepeatRule.Repeat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	public class HighestIdTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RepeatRule repeater = new org.neo4j.test.rule.RepeatRule();
		 public readonly RepeatRule Repeater = new RepeatRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Repeat(times = 100) @Test public void shouldKeepHighest() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Repeat(times : 100)]
		 public virtual void ShouldKeepHighest()
		 {
			  // GIVEN
			  Race race = new Race();
			  HighestId highestId = new HighestId();
			  int threads = Runtime.Runtime.availableProcessors();
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( threads );
			  AtomicLongArray highestIds = new AtomicLongArray( threads );
			  for ( int c = 0; c < threads; c++ )
			  {
					int cc = c;
					race.AddContestant( new RunnableAnonymousInnerClass( this, highestId, latch, highestIds, cc ) );
			  }
			  race.WithEndCondition( () => latch.CurrentCount == 0 );

			  // WHEN
			  race.Go();

			  long highest = 0;
			  for ( int i = 0; i < threads; i++ )
			  {
					highest = max( highest, highestIds.get( i ) );
			  }
			  assertEquals( highest, highestId.Get() );
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly HighestIdTest _outerInstance;

			 private Org.Neo4j.@unsafe.Impl.Batchimport.HighestId _highestId;
			 private System.Threading.CountdownEvent _latch;
			 private AtomicLongArray _highestIds;
			 private int _cc;

			 public RunnableAnonymousInnerClass( HighestIdTest outerInstance, Org.Neo4j.@unsafe.Impl.Batchimport.HighestId highestId, System.Threading.CountdownEvent latch, AtomicLongArray highestIds, int cc )
			 {
				 this.outerInstance = outerInstance;
				 this._highestId = highestId;
				 this._latch = latch;
				 this._highestIds = highestIds;
				 this._cc = cc;
				 random = ThreadLocalRandom.current();
			 }

			 internal bool run;
			 internal ThreadLocalRandom random;

			 public void run()
			 {
				  if ( run )
				  {
						return;
				  }

				  long highest = 0;
				  for ( int i = 0; i < 10; i++ )
				  {
						long nextLong = random.nextLong( 100 );
						_highestId.offer( nextLong );
						highest = max( highest, nextLong );
				  }
				  _highestIds.set( _cc, highest );
				  _latch.Signal();
				  run = true;
			 }
		 }
	}

}