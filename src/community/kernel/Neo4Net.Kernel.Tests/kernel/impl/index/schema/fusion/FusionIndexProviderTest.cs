using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.FusionIndexBase.GROUP_OF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.FusionIndexTestHelp.fill;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.FusionVersion.v00;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.FusionVersion.v10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.FusionVersion.v20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.LUCENE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.SPATIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.TEMPORAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FusionIndexProviderTest
	public class FusionIndexProviderTest
	{
		 private static readonly IndexProviderDescriptor _descriptor = new IndexProviderDescriptor( "test-fusion", "1" );
		 public static readonly StoreIndexDescriptor AnIndex = IndexDescriptorFactory.forSchema( forLabel( 0, 0 ), PROVIDER_DESCRIPTOR ).withId( 0 );

		 private Dictionary<IndexSlot, IndexProvider> _providers;
		 private IndexProvider[] _aliveProviders;
		 private IndexProvider _fusionIndexProvider;
		 private SlotSelector _slotSelector;
		 private InstanceSelector<IndexProvider> _instanceSelector;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static FusionVersion[] versions()
		 public static FusionVersion[] Versions()
		 {
			  return new FusionVersion[] { v00, v10, v20 };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public static FusionVersion fusionVersion;
		 public static FusionVersion FusionVersion;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _slotSelector = FusionVersion.slotSelector();
			  SetupMocks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public RandomRule Random = new RandomRule();

		 private void SetupMocks()
		 {
			  IndexSlot[] aliveSlots = FusionVersion.aliveSlots();
			  _aliveProviders = new IndexProvider[aliveSlots.Length];
			  _providers = new Dictionary<IndexSlot, IndexProvider>( typeof( IndexSlot ) );
			  fill( _providers, IndexProvider.EMPTY );
			  for ( int i = 0; i < aliveSlots.Length; i++ )
			  {
					switch ( aliveSlots[i] )
					{
					case STRING:
						 IndexProvider @string = MockProvider( typeof( StringIndexProvider ), "string" );
						 _providers[STRING] = @string;
						 _aliveProviders[i] = @string;
						 break;
					case NUMBER:
						 IndexProvider number = MockProvider( typeof( NumberIndexProvider ), "number" );
						 _providers[NUMBER] = number;
						 _aliveProviders[i] = number;
						 break;
					case SPATIAL:
						 IndexProvider spatial = MockProvider( typeof( SpatialIndexProvider ), "spatial" );
						 _providers[SPATIAL] = spatial;
						 _aliveProviders[i] = spatial;
						 break;
					case TEMPORAL:
						 IndexProvider temporal = MockProvider( typeof( TemporalIndexProvider ), "temporal" );
						 _providers[TEMPORAL] = temporal;
						 _aliveProviders[i] = temporal;
						 break;
					case LUCENE:
						 IndexProvider lucene = MockProvider( typeof( IndexProvider ), "lucene" );
						 _providers[LUCENE] = lucene;
						 _aliveProviders[i] = lucene;
						 break;
					default:
						 throw new Exception();
					}
			  }
			  _fusionIndexProvider = new FusionIndexProvider( _providers[STRING], _providers[NUMBER], _providers[SPATIAL], _providers[TEMPORAL], _providers[LUCENE], FusionVersion.slotSelector(), _descriptor, NONE, mock(typeof(FileSystemAbstraction)), false );
			  _instanceSelector = new InstanceSelector<IndexProvider>( _providers );
		 }

		 private static IndexProvider MockProvider( System.Type providerClass, string name )
		 {
			  IndexProvider mock = mock( providerClass );
			  when( mock.ProviderDescriptor ).thenReturn( new IndexProviderDescriptor( name, "1" ) );
			  return mock;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectCorrectTargetForAllGivenValueCombinations()
		 public virtual void MustSelectCorrectTargetForAllGivenValueCombinations()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  Value[] allValues = FusionIndexTestHelp.AllValues();

			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					Value[] group = values[slot];
					foreach ( Value value in group )
					{
						 // when
						 IndexProvider selected = _instanceSelector.select( _slotSelector.selectSlot( array( value ), GROUP_OF ) );

						 // then
						 assertSame( OrLucene( _providers[slot] ), selected );
					}
			  }

			  // All composite values should go to lucene
			  foreach ( Value firstValue in allValues )
			  {
					foreach ( Value secondValue in allValues )
					{
						 // when
						 IndexProvider selected = _instanceSelector.select( _slotSelector.selectSlot( array( firstValue, secondValue ), GROUP_OF ) );

						 // then
						 assertSame( _providers[LUCENE], selected );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCombineSamples()
		 public virtual void MustCombineSamples()
		 {
			  // given
			  int sumIndexSize = 0;
			  int sumUniqueValues = 0;
			  int sumSampleSize = 0;
			  IndexSample[] samples = new IndexSample[_providers.Count];
			  for ( int i = 0; i < samples.Length; i++ )
			  {
					int indexSize = Random.Next( 0, 1_000_000 );
					int uniqueValues = Random.Next( 0, 1_000_000 );
					int sampleSize = Random.Next( 0, 1_000_000 );
					samples[i] = new IndexSample( indexSize, uniqueValues, sampleSize );
					sumIndexSize += indexSize;
					sumUniqueValues += uniqueValues;
					sumSampleSize += sampleSize;
			  }

			  // when
			  IndexSample fusionSample = FusionIndexSampler.CombineSamples( Arrays.asList( samples ) );

			  // then
			  assertEquals( sumIndexSize, fusionSample.IndexSize() );
			  assertEquals( sumUniqueValues, fusionSample.UniqueValues() );
			  assertEquals( sumSampleSize, fusionSample.SampleSize() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustThrowIfNoFailure()
		 public virtual void getPopulationFailureMustThrowIfNoFailure()
		 {
			  // when
			  // ... no failure
			  System.InvalidOperationException failure = new System.InvalidOperationException( "not failed" );
			  foreach ( IndexProvider provider in _aliveProviders )
			  {
					when( provider.GetPopulationFailure( any( typeof( StoreIndexDescriptor ) ) ) ).thenThrow( failure );
			  }

			  // then
			  try
			  {
					_fusionIndexProvider.getPopulationFailure( AnIndex );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  { // good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustReportFailureWhenAnyFailed()
		 public virtual void getPopulationFailureMustReportFailureWhenAnyFailed()
		 {
			  foreach ( IndexProvider failingProvider in _aliveProviders )
			  {
					// when
					string failure = "failure";
					System.InvalidOperationException exception = new System.InvalidOperationException( "not failed" );
					foreach ( IndexProvider provider in _aliveProviders )
					{
						 if ( provider == failingProvider )
						 {
							  when( provider.GetPopulationFailure( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( failure );
						 }
						 else
						 {
							  when( provider.GetPopulationFailure( any( typeof( StoreIndexDescriptor ) ) ) ).thenThrow( exception );
						 }
					}

					// then
					assertThat( _fusionIndexProvider.getPopulationFailure( AnIndex ), containsString( failure ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustReportFailureWhenMultipleFail()
		 public virtual void getPopulationFailureMustReportFailureWhenMultipleFail()
		 {
			  // when
			  IList<string> failureMessages = new List<string>();
			  foreach ( IndexProvider aliveProvider in _aliveProviders )
			  {
					string failureMessage = "FAILURE[" + aliveProvider + "]";
					failureMessages.Add( failureMessage );
					when( aliveProvider.GetPopulationFailure( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( failureMessage );
			  }

			  // then
			  string populationFailure = _fusionIndexProvider.getPopulationFailure( AnIndex );
			  foreach ( string failureMessage in failureMessages )
			  {
					assertThat( populationFailure, containsString( failureMessage ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFailedIfAnyIsFailed()
		 public virtual void ShouldReportFailedIfAnyIsFailed()
		 {
			  // given
			  IndexProvider provider = _fusionIndexProvider;

			  foreach ( InternalIndexState state in Enum.GetValues( typeof( InternalIndexState ) ) )
			  {
					foreach ( IndexProvider failedProvider in _aliveProviders )
					{
						 // when
						 foreach ( IndexProvider aliveProvider in _aliveProviders )
						 {
							  SetInitialState( aliveProvider, failedProvider == aliveProvider ? InternalIndexState.FAILED : state );
						 }
						 InternalIndexState initialState = provider.GetInitialState( AnIndex );

						 // then
						 assertEquals( InternalIndexState.FAILED, initialState );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPopulatingIfAnyIsPopulating()
		 public virtual void ShouldReportPopulatingIfAnyIsPopulating()
		 {
			  // given
			  foreach ( InternalIndexState state in array( InternalIndexState.ONLINE, InternalIndexState.POPULATING ) )
			  {
					foreach ( IndexProvider populatingProvider in _aliveProviders )
					{
						 // when
						 foreach ( IndexProvider aliveProvider in _aliveProviders )
						 {
							  SetInitialState( aliveProvider, populatingProvider == aliveProvider ? InternalIndexState.POPULATING : state );
						 }
						 InternalIndexState initialState = _fusionIndexProvider.getInitialState( AnIndex );

						 // then
						 assertEquals( InternalIndexState.POPULATING, initialState );
					}
			  }
		 }

		 private void SetInitialState( IndexProvider mockedProvider, InternalIndexState state )
		 {
			  when( mockedProvider.GetInitialState( any( typeof( StoreIndexDescriptor ) ) ) ).thenReturn( state );
		 }

		 private IndexProvider OrLucene( IndexProvider provider )
		 {
			  return provider != IndexProvider.EMPTY ? provider : _providers[LUCENE];
		 }
	}

}