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
namespace Neo4Net.causalclustering.catchup
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using UpstreamDatabaseSelectionException = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException;
	using UpstreamDatabaseSelectionStrategy = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UpstreamStrategyAddressSupplierTest
	{
		 private MemberId _defaultMember = new MemberId( System.Guid.randomUUID() );
		 private MemberId _firstMember = new MemberId( System.Guid.randomUUID() );
		 private MemberId _secondMember = new MemberId( System.Guid.randomUUID() );
		 private AdvertisedSocketAddress _defaultAddress = new AdvertisedSocketAddress( "Default", 123 );
		 private AdvertisedSocketAddress _firstAddress = new AdvertisedSocketAddress( "First", 456 );
		 private AdvertisedSocketAddress _secondAddress = new AdvertisedSocketAddress( "Second", 789 );
		 private TopologyService _topologyService = mock( typeof( TopologyService ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _topologyService.findCatchupAddress( eq( _defaultMember ) ) ).thenReturn( _defaultAddress );
			  when( _topologyService.findCatchupAddress( eq( _firstMember ) ) ).thenReturn( _firstAddress );
			  when( _topologyService.findCatchupAddress( eq( _secondMember ) ) ).thenReturn( _secondAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectionPrioritiesAreKept() throws CatchupAddressResolutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectionPrioritiesAreKept()
		 {
			  // given various strategies with different priorities
			  UpstreamDatabaseStrategySelector upstreamDatabaseStrategySelector = new UpstreamDatabaseStrategySelector( new CountedSelectionStrategy( this, _defaultMember, 5 ), Arrays.asList( new CountedSelectionStrategy( this, _firstMember, 1 ), new CountedSelectionStrategy( this, _secondMember, 1 ) ), NullLogProvider.Instance );

			  // and
			  UpstreamStrategyAddressSupplier upstreamStrategyAddressSupplier = new UpstreamStrategyAddressSupplier( upstreamDatabaseStrategySelector, _topologyService );

			  // when
			  AdvertisedSocketAddress firstResult = upstreamStrategyAddressSupplier.Get();
			  AdvertisedSocketAddress secondResult = upstreamStrategyAddressSupplier.Get();
			  AdvertisedSocketAddress thirdResult = upstreamStrategyAddressSupplier.Get();

			  // then
			  assertEquals( _firstAddress, firstResult );
			  assertEquals( _secondAddress, secondResult );
			  assertEquals( _defaultAddress, thirdResult );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionWhenStrategiesFail() throws CatchupAddressResolutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExceptionWhenStrategiesFail()
		 {
			  // given a guaranteed fail strategy
			  UpstreamDatabaseStrategySelector upstreamDatabaseStrategySelector = new UpstreamDatabaseStrategySelector( new CountedSelectionStrategy( this, _defaultMember, 0 ) );

			  // and
			  UpstreamStrategyAddressSupplier upstreamStrategyAddressSupplier = new UpstreamStrategyAddressSupplier( upstreamDatabaseStrategySelector, _topologyService );

			  // then
			  ExpectedException.expect( typeof( CatchupAddressResolutionException ) );

			  // when
			  upstreamStrategyAddressSupplier.Get();
		 }

		 private class CountedSelectionStrategy : UpstreamDatabaseSelectionStrategy
		 {
			 private readonly UpstreamStrategyAddressSupplierTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal MemberId UpstreamDatabaseConflict;
			  internal int NumberOfIterations;

			  internal CountedSelectionStrategy( UpstreamStrategyAddressSupplierTest outerInstance, MemberId upstreamDatabase, int numberOfIterations ) : base( typeof( CountedSelectionStrategy ).FullName )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  this._outerInstance = outerInstance;
					this.UpstreamDatabaseConflict = upstreamDatabase;
					this.NumberOfIterations = numberOfIterations;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.Neo4Net.causalclustering.identity.MemberId> upstreamDatabase() throws org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException
			  public override Optional<MemberId> UpstreamDatabase()
			  {
					MemberId consumed = UpstreamDatabaseConflict;
					NumberOfIterations--;
					if ( NumberOfIterations < 0 )
					{
						 UpstreamDatabaseConflict = null;
					}
					return Optional.ofNullable( consumed );
			  }

			  public override int GetHashCode()
			  {
					return base.GetHashCode() + (UpstreamDatabaseConflict.GetHashCode() * 17) + (31 * NumberOfIterations);
			  }

			  public override bool Equals( object o )
			  {
					if ( o == null || !( o is CountedSelectionStrategy ) )
					{
						 return false;
					}
					CountedSelectionStrategy other = ( CountedSelectionStrategy ) o;
					return this.UpstreamDatabaseConflict.Equals( other.UpstreamDatabaseConflict ) && this.NumberOfIterations == other.NumberOfIterations;
			  }
		 }
	}

}