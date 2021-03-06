﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.identity
{
	using OperationTimeoutException = com.hazelcast.core.OperationTimeoutException;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreBootstrapper = Org.Neo4j.causalclustering.core.state.CoreBootstrapper;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;
	using Org.Neo4j.causalclustering.core.state.storage;
	using CoreServerInfo = Org.Neo4j.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using TestTopology = Org.Neo4j.causalclustering.discovery.TestTopology;
	using Org.Neo4j.Helpers.Collection;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ClusterBinderTest
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterBinderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_minCoreHosts = _config.get( CausalClusteringSettings.minimum_core_cluster_size_at_formation );
			_dbName = _config.get( CausalClusteringSettings.database );
		}

		 private readonly CoreBootstrapper _coreBootstrapper = mock( typeof( CoreBootstrapper ) );
		 private readonly FakeClock _clock = Clocks.fakeClock();

		 private readonly Config _config = Config.defaults();
		 private int _minCoreHosts;
		 private string _dbName;

		 private ClusterBinder ClusterBinder( SimpleStorage<ClusterId> clusterIdStorage, CoreTopologyService topologyService )
		 {
			  return new ClusterBinder( clusterIdStorage, new StubSimpleStorage<DatabaseName>( this ), topologyService, _clock, () => _clock.forward(1, TimeUnit.SECONDS), Duration.of(3_000, MILLIS), _coreBootstrapper, _dbName, _minCoreHosts, new Monitors() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetryWhenPublishFailsWithTransientErrors() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRetryWhenPublishFailsWithTransientErrors()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<MemberId, CoreServerInfo> members = IntStream.range( 0, _minCoreHosts ).mapToObj( i => Pair.of( new MemberId( System.Guid.randomUUID() ), TestTopology.addressesForCore(i, false) ) ).collect(Collectors.toMap(Pair::first, Pair::other));

			  CoreTopology bootstrappableTopology = new CoreTopology( null, true, members );
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  when( topologyService.SetClusterId( any(), anyString() ) ).thenThrow(typeof(OperationTimeoutException)).thenReturn(true); // Then succeed
			  when( topologyService.LocalCoreServers() ).thenReturn(bootstrappableTopology);

			  ClusterBinder binder = ClusterBinder( new StubSimpleStorage<ClusterId>( this ), topologyService );

			  // when
			  binder.BindToCluster();

			  // then
			  verify( topologyService, atLeast( 2 ) ).setClusterId( any(), anyString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.util.concurrent.TimeoutException.class) public void shouldTimeoutIfPublishContinuallyFailsWithTransientErrors() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfPublishContinuallyFailsWithTransientErrors()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<MemberId, CoreServerInfo> members = IntStream.range( 0, _minCoreHosts ).mapToObj( i => Pair.of( new MemberId( System.Guid.randomUUID() ), TestTopology.addressesForCore(i, false) ) ).collect(Collectors.toMap(Pair::first, Pair::other));

			  CoreTopology bootstrappableTopology = new CoreTopology( null, true, members );
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  when( topologyService.SetClusterId( any(), anyString() ) ).thenThrow(typeof(OperationTimeoutException)); // Causes a retry
			  when( topologyService.LocalCoreServers() ).thenReturn(bootstrappableTopology);

			  ClusterBinder binder = ClusterBinder( new StubSimpleStorage<ClusterId>( this ), topologyService );

			  // when
			  binder.BindToCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutWhenNotBootstrappableAndNobodyElsePublishesClusterId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutWhenNotBootstrappableAndNobodyElsePublishesClusterId()
		 {
			  // given
			  CoreTopology unboundTopology = new CoreTopology( null, false, emptyMap() );
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );
			  when( topologyService.LocalCoreServers() ).thenReturn(unboundTopology);

			  ClusterBinder binder = ClusterBinder( new StubSimpleStorage<ClusterId>( this ), topologyService );

			  try
			  {
					// when
					binder.BindToCluster();
					fail( "Should have timed out" );
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }

			  // then
			  verify( topologyService, atLeast( 2 ) ).localCoreServers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBindToClusterIdPublishedByAnotherMember() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBindToClusterIdPublishedByAnotherMember()
		 {
			  // given
			  ClusterId publishedClusterId = new ClusterId( System.Guid.randomUUID() );
			  CoreTopology unboundTopology = new CoreTopology( null, false, emptyMap() );
			  CoreTopology boundTopology = new CoreTopology( publishedClusterId, false, emptyMap() );

			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );
			  when( topologyService.LocalCoreServers() ).thenReturn(unboundTopology).thenReturn(boundTopology);

			  ClusterBinder binder = ClusterBinder( new StubSimpleStorage<ClusterId>( this ), topologyService );

			  // when
			  binder.BindToCluster();

			  // then
			  Optional<ClusterId> clusterId = binder.Get();
			  assertTrue( clusterId.Present );
			  assertEquals( publishedClusterId, clusterId.get() );
			  verify( topologyService, atLeast( 2 ) ).localCoreServers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPublishStoredClusterIdIfPreviouslyBound() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPublishStoredClusterIdIfPreviouslyBound()
		 {
			  // given
			  ClusterId previouslyBoundClusterId = new ClusterId( System.Guid.randomUUID() );

			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );
			  when( topologyService.SetClusterId( previouslyBoundClusterId, "default" ) ).thenReturn( true );

			  StubSimpleStorage<ClusterId> clusterIdStorage = new StubSimpleStorage<ClusterId>( this );
			  clusterIdStorage.WriteState( previouslyBoundClusterId );

			  ClusterBinder binder = ClusterBinder( clusterIdStorage, topologyService );

			  // when
			  binder.BindToCluster();

			  // then
			  verify( topologyService ).setClusterId( previouslyBoundClusterId, "default" );
			  Optional<ClusterId> clusterId = binder.Get();
			  assertTrue( clusterId.Present );
			  assertEquals( previouslyBoundClusterId, clusterId.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToPublishMismatchingStoredClusterId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToPublishMismatchingStoredClusterId()
		 {
			  // given
			  ClusterId previouslyBoundClusterId = new ClusterId( System.Guid.randomUUID() );

			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );
			  when( topologyService.SetClusterId( previouslyBoundClusterId, "default" ) ).thenReturn( false );

			  StubSimpleStorage<ClusterId> clusterIdStorage = new StubSimpleStorage<ClusterId>( this );
			  clusterIdStorage.WriteState( previouslyBoundClusterId );

			  ClusterBinder binder = ClusterBinder( clusterIdStorage, topologyService );

			  // when
			  try
			  {
					binder.BindToCluster();
					fail( "Should have thrown exception" );
			  }
			  catch ( BindingException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBootstrapWhenBootstrappable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBootstrapWhenBootstrappable()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<MemberId, CoreServerInfo> members = IntStream.range( 0, _minCoreHosts ).mapToObj( i => Pair.of( new MemberId( System.Guid.randomUUID() ), TestTopology.addressesForCore(i, false) ) ).collect(Collectors.toMap(Pair::first, Pair::other));

			  CoreTopology bootstrappableTopology = new CoreTopology( null, true, members );

			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );
			  when( topologyService.LocalCoreServers() ).thenReturn(bootstrappableTopology);
			  when( topologyService.SetClusterId( any(), eq("default") ) ).thenReturn(true);
			  CoreSnapshot snapshot = mock( typeof( CoreSnapshot ) );
			  when( _coreBootstrapper.bootstrap( any() ) ).thenReturn(snapshot);

			  ClusterBinder binder = ClusterBinder( new StubSimpleStorage<ClusterId>( this ), topologyService );

			  // when
			  BoundState boundState = binder.BindToCluster();

			  // then
			  verify( _coreBootstrapper ).bootstrap( any() );
			  Optional<ClusterId> clusterId = binder.Get();
			  assertTrue( clusterId.Present );
			  verify( topologyService ).setClusterId( clusterId.get(), "default" );
			  assertTrue( boundState.Snapshot().Present );
			  assertEquals( boundState.Snapshot().get(), snapshot );
		 }

		 private class StubSimpleStorage<T> : SimpleStorage<T>
		 {
			 private readonly ClusterBinderTest _outerInstance;

			 public StubSimpleStorage( ClusterBinderTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal T State;

			  public override bool Exists()
			  {
					return State != default( T );
			  }

			  public override T ReadState()
			  {
					return State;
			  }

			  public override void WriteState( T state )
			  {
					this.State = state;
			  }
		 }
	}

}