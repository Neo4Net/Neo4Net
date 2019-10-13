using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;


	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexLimitation = Neo4Net.@internal.Kernel.Api.IndexLimitation;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Neo4Net.@internal.Kernel.Api.IndexValueCapability;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class UnionIndexCapabilityTest
	{
		 private static readonly IndexOrder[] _orderCapabilitiesAll = new IndexOrder[]{ IndexOrder.ASCENDING, IndexOrder.DESCENDING };
		 private static readonly IndexOrder[] _orderCapabilitiesOnlyAsc = new IndexOrder[]{ IndexOrder.ASCENDING };
		 private static readonly IndexOrder[] _orderCapabilitiesOnlyDes = new IndexOrder[]{ IndexOrder.DESCENDING };
		 private static readonly IndexOrder[] _orderCapabilitiesNone = new IndexOrder[0];

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUnionOfOrderCapabilities()
		 public virtual void ShouldCreateUnionOfOrderCapabilities()
		 {
			  // given
			  UnionIndexCapability union;
			  union = UnionOfOrderCapabilities( _orderCapabilitiesNone, _orderCapabilitiesOnlyAsc );

			  // then
			  AssertOrderCapability( union, _orderCapabilitiesOnlyAsc );

			  // given
			  union = UnionOfOrderCapabilities( _orderCapabilitiesNone, _orderCapabilitiesAll );

			  // then
			  AssertOrderCapability( union, _orderCapabilitiesAll );

			  // given
			  union = UnionOfOrderCapabilities( _orderCapabilitiesOnlyAsc, _orderCapabilitiesOnlyDes );

			  // then
			  AssertOrderCapability( union, _orderCapabilitiesAll );

			  // given
			  union = UnionOfOrderCapabilities( _orderCapabilitiesOnlyAsc, _orderCapabilitiesAll );

			  // then
			  AssertOrderCapability( union, _orderCapabilitiesAll );

			  // given
			  union = UnionOfOrderCapabilities( _orderCapabilitiesOnlyAsc, _orderCapabilitiesOnlyAsc );

			  // then
			  AssertOrderCapability( union, _orderCapabilitiesOnlyAsc );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUnionOfValueCapability()
		 public virtual void ShouldCreateUnionOfValueCapability()
		 {
			  UnionIndexCapability union;

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.NO, IndexValueCapability.NO );

			  // then
			  AssertValueCapability( union, IndexValueCapability.NO );

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.NO, IndexValueCapability.PARTIAL );

			  // then
			  AssertValueCapability( union, IndexValueCapability.PARTIAL );

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.NO, IndexValueCapability.YES );

			  // then
			  AssertValueCapability( union, IndexValueCapability.YES );

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.PARTIAL, IndexValueCapability.PARTIAL );

			  // then
			  AssertValueCapability( union, IndexValueCapability.PARTIAL );

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.PARTIAL, IndexValueCapability.YES );

			  // then
			  AssertValueCapability( union, IndexValueCapability.YES );

			  // given
			  union = UnionOfValueCapabilities( IndexValueCapability.YES, IndexValueCapability.YES );

			  // then
			  AssertValueCapability( union, IndexValueCapability.YES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUnionOfIndexLimitations()
		 public virtual void ShouldCreateUnionOfIndexLimitations()
		 {
			  UnionIndexCapability union;

			  // given
			  union = UnionOfIndexLimitations( Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.LimitiationNone, Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.LimitiationNone );

			  // then
			  assertEquals( Collections.emptySet(), asSet(union.Limitations()) );

			  // given
			  union = UnionOfIndexLimitations( Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.LimitiationNone, array( IndexLimitation.SLOW_CONTAINS ) );

			  // then
			  assertEquals( asSet( IndexLimitation.SLOW_CONTAINS ), asSet( union.Limitations() ) );

			  // given
			  union = UnionOfIndexLimitations( array( IndexLimitation.SLOW_CONTAINS ), array( IndexLimitation.SLOW_CONTAINS ) );

			  // then
			  assertEquals( asSet( IndexLimitation.SLOW_CONTAINS ), asSet( union.Limitations() ) );
		 }

		 private UnionIndexCapability UnionOfIndexLimitations( params IndexLimitation[][] limitations )
		 {
			  IList<IndexCapability> capabilities = new List<IndexCapability>();
			  foreach ( IndexLimitation[] limitation in limitations )
			  {
					capabilities.Add( CapabilityWithIndexLimitations( limitation ) );
			  }
			  return new UnionIndexCapability( capabilities );
		 }

		 private IndexCapability CapabilityWithIndexLimitations( IndexLimitation[] limitations )
		 {
			  IndexCapability mock = MockedIndexCapability();
			  when( mock.Limitations() ).thenReturn(limitations);
			  return mock;
		 }

		 private UnionIndexCapability UnionOfValueCapabilities( params IndexValueCapability[] valueCapabilities )
		 {
			  IList<IndexCapability> capabilities = new List<IndexCapability>( valueCapabilities.Length );
			  foreach ( IndexValueCapability valueCapability in valueCapabilities )
			  {
					capabilities.Add( CapabilityWithValue( valueCapability ) );
			  }
			  return new UnionIndexCapability( capabilities );
		 }

		 private UnionIndexCapability UnionOfOrderCapabilities( params IndexOrder[][] indexOrders )
		 {
			  IList<IndexCapability> capabilities = new List<IndexCapability>( indexOrders.Length );
			  foreach ( IndexOrder[] indexOrder in indexOrders )
			  {
					capabilities.Add( CapabilityWithOrder( indexOrder ) );
			  }
			  return new UnionIndexCapability( capabilities );
		 }

		 private IndexCapability CapabilityWithValue( IndexValueCapability valueCapability )
		 {
			  IndexCapability mock = MockedIndexCapability();
			  when( mock.ValueCapability( any() ) ).thenReturn(valueCapability);
			  return mock;
		 }

		 private IndexCapability MockedIndexCapability()
		 {
			  IndexCapability mock = mock( typeof( IndexCapability ) );
			  when( mock.Limitations() ).thenReturn(Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.LimitiationNone);
			  return mock;
		 }

		 private IndexCapability CapabilityWithOrder( IndexOrder[] indexOrder )
		 {
			  IndexCapability mock = MockedIndexCapability();
			  when( mock.OrderCapability( any() ) ).thenReturn(indexOrder);
			  return mock;
		 }

		 private void AssertValueCapability( UnionIndexCapability union, IndexValueCapability expectedValueCapability )
		 {
			  IndexValueCapability actual = union.ValueCapability( SomeValueCategory() );
			  assertEquals( expectedValueCapability, actual );
		 }

		 private void AssertOrderCapability( UnionIndexCapability union, params IndexOrder[] expected )
		 {
			  IndexOrder[] actual = union.OrderCapability( SomeValueCategory() );
			  assertTrue( "Actual contains all expected", ArrayUtil.containsAll( expected, actual ) );
			  assertTrue( "Actual contains nothing else than expected", ArrayUtil.containsAll( actual, expected ) );
		 }

		 private ValueCategory SomeValueCategory()
		 {
			  return ValueCategory.TEXT;
		 }
	}

}