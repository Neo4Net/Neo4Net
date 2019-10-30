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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;

	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LearnerContextTest
	{
		 private readonly LogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAllowHigherLastLearnedInstanceId()
		 public virtual void ShouldOnlyAllowHigherLastLearnedInstanceId()
		 {
			  // Given

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext mpCtx = new MultiPaxosContext( null, Iterables.empty(), mock(typeof(ClusterConfiguration)), null, NullLogProvider.Instance, null, null, null, null, null, config );
			  LearnerContext state = mpCtx.LearnerContext;

			  // When
			  state.SetLastKnownLearnedInstanceInCluster( 1, new InstanceId( 2 ) );
			  state.SetLastKnownLearnedInstanceInCluster( 0, new InstanceId( 3 ) );

			  // Then
			  assertThat( state.LastKnownLearnedInstanceInCluster, equalTo( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackLastKnownUpToDateAliveInstance()
		 public virtual void ShouldTrackLastKnownUpToDateAliveInstance()
		 {
			  // Given

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext mpCtx = new MultiPaxosContext( null, Iterables.empty(), mock(typeof(ClusterConfiguration)), null, NullLogProvider.Instance, null, null, null, null, null, config );
			  LearnerContext state = mpCtx.LearnerContext;

			  // When
			  state.SetLastKnownLearnedInstanceInCluster( 1, new InstanceId( 2 ) );
			  state.SetLastKnownLearnedInstanceInCluster( 1, new InstanceId( 3 ) );
			  state.SetLastKnownLearnedInstanceInCluster( 0, new InstanceId( 4 ) );

			  // Then
			  assertThat( state.LastKnownLearnedInstanceInCluster, equalTo( 1L ) );
			  assertThat( state.LastKnownAliveUpToDateInstance, equalTo( new InstanceId( 3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void settingLastLearnedInstanceToNegativeOneShouldAlwaysWin()
		 public virtual void SettingLastLearnedInstanceToNegativeOneShouldAlwaysWin()
		 {
			  // Given
			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext mpCtx = new MultiPaxosContext( null, Iterables.empty(), mock(typeof(ClusterConfiguration)), null, NullLogProvider.Instance, null, null, null, null, null, config );
			  LearnerContext state = mpCtx.LearnerContext;

			  // When
			  state.SetLastKnownLearnedInstanceInCluster( 1, new InstanceId( 2 ) );
			  state.SetLastKnownLearnedInstanceInCluster( -1, null );

			  // Then
			  assertThat( state.LastKnownLearnedInstanceInCluster, equalTo( -1L ) );
			  assertThat( state.LastKnownAliveUpToDateInstance, equalTo( new InstanceId( 2 ) ) );
		 }
	}

}