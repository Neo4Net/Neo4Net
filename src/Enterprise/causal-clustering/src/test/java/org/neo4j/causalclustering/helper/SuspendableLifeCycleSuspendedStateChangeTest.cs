using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using SuspendedState = Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.SuspendedState;
	using LifeCycleState = Neo4Net.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helper.SuspendableLifecycleStateTestHelpers.setInitialState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SuspendableLifeCycleSuspendedStateChangeTest
	public class SuspendableLifeCycleSuspendedStateChangeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.neo4j.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState fromState;
		 public LifeCycleState FromState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.neo4j.causalclustering.helper.SuspendableLifecycleStateTestHelpers.SuspendedState fromSuspendedState;
		 public SuspendedState FromSuspendedState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public org.neo4j.causalclustering.helper.SuspendableLifecycleStateTestHelpers.SuspendedState toSuspendedState;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public SuspendedState ToSuspendedStateConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public org.neo4j.causalclustering.helper.SuspendableLifecycleStateTestHelpers.LifeCycleState shouldEndInState;
		 public LifeCycleState ShouldEndInState;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "From {0} and {1} to {2} should end in {3}") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  IList<object[]> @params = new List<object[]>();
			  foreach ( LifeCycleState lifeCycleState in LifeCycleState.values() )
			  {
					foreach ( SuspendedState suspendedState in SuspendedState.values() )
					{
						 foreach ( SuspendedState toSuspendedState in toSuspendedState() )
						 {
							  @params.Add( new object[]{ lifeCycleState, suspendedState, toSuspendedState, ExpectedResult( lifeCycleState, suspendedState, toSuspendedState ) } );
						 }
					}
			  }
			  return @params;
		 }

		 private StateAwareSuspendableLifeCycle _lifeCycle;

		 private static SuspendedState[] ToSuspendedState()
		 {
			  return new SuspendedState[]{ SuspendedState.Enabled, SuspendedState.Disabled };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUpServer()
		 {
			  _lifeCycle = new StateAwareSuspendableLifeCycle( NullLogProvider.Instance.getLog( "log" ) );
			  setInitialState( _lifeCycle, FromState );
			  FromSuspendedState.set( _lifeCycle );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeSuspendedState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeSuspendedState()
		 {
			  ToSuspendedStateConflict.set( _lifeCycle );
			  assertEquals( ShouldEndInState, _lifeCycle.status );
		 }

		 private static LifeCycleState ExpectedResult( LifeCycleState fromState, SuspendedState fromSuspendedState, SuspendedState toSuspendedState )
		 {
			  if ( toSuspendedState == SuspendedState.Disabled )
			  {
					return LifeCycleState.Stop;
			  }
			  else if ( toSuspendedState == SuspendedState.Enabled )
			  {
					if ( fromSuspendedState == SuspendedState.Disabled )
					{
						 if ( fromState == LifeCycleState.Init || fromState == LifeCycleState.Shutdown )
						 {
							  return LifeCycleState.Stop;
						 }
					}
					return fromState;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Should not transition to any other state got: " + toSuspendedState );
			  }
		 }
	}

}