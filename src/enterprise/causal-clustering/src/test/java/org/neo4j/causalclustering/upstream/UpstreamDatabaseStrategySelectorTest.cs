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
namespace Neo4Net.causalclustering.upstream
{
	using Test = org.junit.Test;


	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ConnectToRandomCoreServerStrategy = Neo4Net.causalclustering.upstream.strategies.ConnectToRandomCoreServerStrategy;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.iterable;

	public class UpstreamDatabaseStrategySelectorTest
	{
		 private MemberId _dummyMemberId = new MemberId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheMemberIdFromFirstSuccessfulStrategy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTheMemberIdFromFirstSuccessfulStrategy()
		 {
			  // given
			  UpstreamDatabaseSelectionStrategy badOne = mock( typeof( UpstreamDatabaseSelectionStrategy ) );
			  when( badOne.UpstreamDatabase() ).thenReturn(null);

			  UpstreamDatabaseSelectionStrategy anotherBadOne = mock( typeof( UpstreamDatabaseSelectionStrategy ) );
			  when( anotherBadOne.UpstreamDatabase() ).thenReturn(null);

			  UpstreamDatabaseSelectionStrategy goodOne = mock( typeof( UpstreamDatabaseSelectionStrategy ) );
			  MemberId theMemberId = new MemberId( System.Guid.randomUUID() );
			  when( goodOne.UpstreamDatabase() ).thenReturn(theMemberId);

			  UpstreamDatabaseStrategySelector selector = new UpstreamDatabaseStrategySelector( badOne, iterable( goodOne, anotherBadOne ), NullLogProvider.Instance );

			  // when
			  MemberId result = selector.BestUpstreamDatabase();

			  // then
			  assertEquals( theMemberId, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultToRandomCoreServerIfNoOtherStrategySpecified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDefaultToRandomCoreServerIfNoOtherStrategySpecified()
		 {
			  // given
			  TopologyService topologyService = mock( typeof( TopologyService ) );
			  MemberId memberId = new MemberId( System.Guid.randomUUID() );
			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(new ClusterId(System.Guid.randomUUID()), false, MapOf(memberId, mock(typeof(CoreServerInfo)))));

			  ConnectToRandomCoreServerStrategy defaultStrategy = new ConnectToRandomCoreServerStrategy();
			  defaultStrategy.Inject( topologyService, Config.defaults(), NullLogProvider.Instance, null );

			  UpstreamDatabaseStrategySelector selector = new UpstreamDatabaseStrategySelector( defaultStrategy );

			  // when
			  MemberId instance = selector.BestUpstreamDatabase();

			  // then
			  assertEquals( memberId, instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseSpecifiedStrategyInPreferenceToDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseSpecifiedStrategyInPreferenceToDefault()
		 {
			  // given
			  TopologyService topologyService = mock( typeof( TopologyService ) );
			  MemberId memberId = new MemberId( System.Guid.randomUUID() );
			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(new ClusterId(System.Guid.randomUUID()), false, MapOf(memberId, mock(typeof(CoreServerInfo)))));

			  ConnectToRandomCoreServerStrategy shouldNotUse = mock( typeof( ConnectToRandomCoreServerStrategy ) );

			  UpstreamDatabaseSelectionStrategy mockStrategy = mock( typeof( UpstreamDatabaseSelectionStrategy ) );
			  when( mockStrategy.UpstreamDatabase() ).thenReturn((new MemberId(System.Guid.randomUUID())));

			  UpstreamDatabaseStrategySelector selector = new UpstreamDatabaseStrategySelector( shouldNotUse, iterable( mockStrategy ), NullLogProvider.Instance );

			  // when
			  selector.BestUpstreamDatabase();

			  // then
			  verifyZeroInteractions( shouldNotUse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public static class DummyUpstreamDatabaseSelectionStrategy extends UpstreamDatabaseSelectionStrategy
		 public class DummyUpstreamDatabaseSelectionStrategy : UpstreamDatabaseSelectionStrategy
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal MemberId MemberIdConflict;

			  public DummyUpstreamDatabaseSelectionStrategy() : base("dummy")
			  {
			  }

			  public override Optional<MemberId> UpstreamDatabase()
			  {
					return Optional.ofNullable( MemberIdConflict );
			  }

			  public virtual MemberId MemberId
			  {
				  set
				  {
						this.MemberIdConflict = value;
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public static class AnotherDummyUpstreamDatabaseSelectionStrategy extends UpstreamDatabaseSelectionStrategy
		 public class AnotherDummyUpstreamDatabaseSelectionStrategy : UpstreamDatabaseSelectionStrategy
		 {
			  public AnotherDummyUpstreamDatabaseSelectionStrategy() : base("another-dummy")
			  {
			  }

			  public override Optional<MemberId> UpstreamDatabase()
			  {
					return ( new MemberId( System.Guid.randomUUID() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public static class YetAnotherDummyUpstreamDatabaseSelectionStrategy extends UpstreamDatabaseSelectionStrategy
		 public class YetAnotherDummyUpstreamDatabaseSelectionStrategy : UpstreamDatabaseSelectionStrategy
		 {
			  public YetAnotherDummyUpstreamDatabaseSelectionStrategy() : base("yet-another-dummy")
			  {
			  }

			  public override Optional<MemberId> UpstreamDatabase()
			  {
					return ( new MemberId( System.Guid.randomUUID() ) );
			  }
		 }

		 private IDictionary<MemberId, CoreServerInfo> MapOf( MemberId memberId, CoreServerInfo coreServerInfo )
		 {
			  Dictionary<MemberId, CoreServerInfo> map = new Dictionary<MemberId, CoreServerInfo>();

			  map[memberId] = coreServerInfo;

			  return map;
		 }
	}

}