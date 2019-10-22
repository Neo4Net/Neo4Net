using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using SuspendedState = Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.SuspendedState;
	using LifeCycleState = Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.setInitialState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SuspendableLifeCycleLifeStateChangeTest
	public class SuspendableLifeCycleLifeStateChangeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState fromState;
		 public LifeCycleState FromState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.SuspendedState fromSuspendedState;
		 public SuspendedState FromSuspendedState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public org.Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState toLifeCycleState;
		 public LifeCycleState ToLifeCycleState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public org.Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState shouldBeRunning;
		 public LifeCycleState ShouldBeRunning;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "From {0} and {1} to {2} should end in {3}") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  IList<object[]> @params = new List<object[]>();
			  foreach ( LifeCycleState lifeCycleState in LifeCycleState.values() )
			  {
					foreach ( SuspendedState suspendedState in SuspendedState.values() )
					{
						 foreach ( LifeCycleState toState in LifeCycleOperation() )
						 {
							  @params.Add( new object[]{ lifeCycleState, suspendedState, toState, ExpectedResult( suspendedState, toState ) } );
						 }
					}
			  }
			  return @params;
		 }

		 private StateAwareSuspendableLifeCycle _lifeCycle;

		 private static LifeCycleState[] LifeCycleOperation()
		 {
			  return new LifeCycleState[]{ LifeCycleState.Start, LifeCycleState.Stop };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUpServer()
		 {
			  _lifeCycle = new StateAwareSuspendableLifeCycle( ( new AssertableLogProvider( false ) ).getLog( "log" ) );
			  setInitialState( _lifeCycle, FromState );
			  FromSuspendedState.set( _lifeCycle );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeLifeState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeLifeState()
		 {
			  ToLifeCycleState.set( _lifeCycle );
			  assertEquals( ShouldBeRunning, _lifeCycle.status );
		 }

		 private static LifeCycleState ExpectedResult( SuspendedState state, LifeCycleState toLifeCycle )
		 {
			  if ( state == SuspendedState.Untouched || state == SuspendedState.Enabled )
			  {
					return toLifeCycle;
			  }
			  else if ( state == SuspendedState.Disabled )
			  {
					if ( toLifeCycle == LifeCycleState.Shutdown )
					{
						 return toLifeCycle;
					}
					else
					{
						 return LifeCycleState.Stop;
					}
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Unknown state " + state );
			  }
		 }
	}

}