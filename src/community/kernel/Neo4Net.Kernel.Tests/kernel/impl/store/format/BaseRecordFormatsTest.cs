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
namespace Neo4Net.Kernel.impl.store.format
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.array;

	public class BaseRecordFormatsTest
	{
		 private static readonly Capability[] _capabilities = Capability.values();
		 private static readonly CapabilityType[] _capabilityTypes = Enum.GetValues( typeof( CapabilityType ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCompatibilityBetweenTwoEqualSetsOfCapabilities()
		 public virtual void ShouldReportCompatibilityBetweenTwoEqualSetsOfCapabilities()
		 {
			  // given
			  Capability[] capabilities = Random.selection( _capabilities, _capabilities.Length / 2, _capabilities.Length, false );

			  // then
			  AssertCompatibility( capabilities, capabilities, true, _capabilityTypes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCompatibilityForAdditiveAdditionalCapabilities()
		 public virtual void ShouldReportCompatibilityForAdditiveAdditionalCapabilities()
		 {
			  // given
			  Capability[] from = array( Capability.Schema );
			  Capability[] to = array( Capability.Schema, Capability.PointProperties, Capability.TemporalProperties );

			  // then
			  AssertCompatibility( from, to, true, _capabilityTypes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIncompatibilityForChangingAdditionalCapabilities()
		 public virtual void ShouldReportIncompatibilityForChangingAdditionalCapabilities()
		 {
			  // given
			  Capability[] from = array( Capability.Schema );
			  Capability[] to = array( Capability.Schema, Capability.DenseNodes );

			  // then
			  AssertCompatibility( from, to, false, CapabilityType.Store );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIncompatibilityForAdditiveRemovedCapabilities()
		 public virtual void ShouldReportIncompatibilityForAdditiveRemovedCapabilities()
		 {
			  // given
			  Capability[] from = array( Capability.Schema, Capability.PointProperties, Capability.TemporalProperties );
			  Capability[] to = array( Capability.Schema );

			  // then
			  AssertCompatibility( from, to, false, CapabilityType.Store );
		 }

		 private void AssertCompatibility( Capability[] from, Capability[] to, bool compatible, params CapabilityType[] capabilityTypes )
		 {
			  foreach ( CapabilityType type in capabilityTypes )
			  {
					assertEquals( compatible, Format( from ).hasCompatibleCapabilities( Format( to ), type ) );
			  }
		 }

		 private RecordFormats Format( params Capability[] capabilities )
		 {
			  RecordFormats formats = mock( typeof( BaseRecordFormats ) );
			  when( formats.Capabilities() ).thenReturn(capabilities);
			  when( formats.HasCompatibleCapabilities( any( typeof( RecordFormats ) ), any( typeof( CapabilityType ) ) ) ).thenCallRealMethod();
			  return formats;
		 }
	}

}