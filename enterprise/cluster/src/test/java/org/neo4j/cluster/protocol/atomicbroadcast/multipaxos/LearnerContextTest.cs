﻿/*
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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;

	using MultiPaxosContext = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

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