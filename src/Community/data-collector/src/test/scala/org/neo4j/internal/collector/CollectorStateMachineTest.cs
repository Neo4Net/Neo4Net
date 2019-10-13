using System.Collections.Generic;
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
namespace Neo4Net.@internal.Collector
{
	using Test = org.junit.jupiter.api.Test;


	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.*;

	internal class CollectorStateMachineTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleStress() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleStress()
		 {
			  // given
			  int n = 1000;
			  TestStateMachine stateMachine = new TestStateMachine();
			  ExecutorService executor = Executors.newFixedThreadPool( 3 );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> collect = executor.submit(stress(n, () -> collect(stateMachine)));
			  Future<object> collect = executor.submit( Stress( n, () => collect(stateMachine) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> stop = executor.submit(stress(n, () -> stateMachine.stop(Long.MAX_VALUE)));
			  Future<object> stop = executor.submit( Stress( n, () => stateMachine.Stop(long.MaxValue) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> clear = executor.submit(stress(n, stateMachine::clear));
			  Future<object> clear = executor.submit( Stress( n, stateMachine.clear ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> status = executor.submit(stress(n, stateMachine::status));
			  Future<object> status = executor.submit( Stress( n, stateMachine.status ) );

			  // then without illegal transitions or exceptions
			  collect.get();
			  stop.get();
			  clear.get();
			  status.get();
			  executor.shutdown();
		 }

		 private ThreadStart Stress<T>( int n, System.Func<T> action )
		 {
			  return () =>
			  {
				for ( int i = 0; i < n; i++ )
				{
					 action();
				}
			  };
		 }

		 public virtual CollectorStateMachine.Result Collect( CollectorStateMachine stateMachine )
		 {
			  try
			  {
					return stateMachine.collect( Collections.emptyMap() );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					throw new System.InvalidOperationException( e );
			  }
		 }

		 internal class TestStateMachine : CollectorStateMachine<string>
		 {
			  internal TestStateMachine() : base(false)
			  {
			  }

			  internal enum State
			  {
					Idle,
					Collecting
			  }

			  internal volatile State State = State.Idle;

			  protected internal override Result DoCollect( IDictionary<string, object> config, long collectionId )
			  {
					assertSame( State, State.Idle );
					State = State.Collecting;
					return null;
			  }

			  protected internal override Result DoStop()
			  {
					assertSame( State, State.Collecting );
					State = State.Idle;
					return null;
			  }

			  protected internal override Result DoClear()
			  {
					assertSame( State, State.Idle );
					return null;
			  }

			  protected internal override string DoGetData()
			  {
					assertSame( State, State.Idle );
					return "Data";
			  }
		 }
	}

}