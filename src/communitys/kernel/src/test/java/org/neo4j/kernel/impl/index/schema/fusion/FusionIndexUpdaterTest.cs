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


	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using SwallowingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.without;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.fill;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.remove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionVersion.v00;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionVersion.v10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionVersion.v20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.LUCENE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.SPATIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.TEMPORAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FusionIndexUpdaterTest
	public class FusionIndexUpdaterTest
	{
		 private IndexUpdater[] _aliveUpdaters;
		 private Dictionary<IndexSlot, IndexUpdater> _updaters;
		 private FusionIndexUpdater _fusionIndexUpdater;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public RandomRule Random = new RandomRule();
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
			  InitiateMocks();
		 }

		 private void InitiateMocks()
		 {
			  IndexSlot[] activeSlots = FusionVersion.aliveSlots();
			  _updaters = new Dictionary<IndexSlot, IndexUpdater>( typeof( IndexSlot ) );
			  fill( _updaters, SwallowingIndexUpdater.INSTANCE );
			  _aliveUpdaters = new IndexUpdater[activeSlots.Length];
			  for ( int i = 0; i < activeSlots.Length; i++ )
			  {
					IndexUpdater mock = mock( typeof( IndexUpdater ) );
					_aliveUpdaters[i] = mock;
					switch ( activeSlots[i] )
					{
					case STRING:
						 _updaters[STRING] = mock;
						 break;
					case NUMBER:
						 _updaters[NUMBER] = mock;
						 break;
					case SPATIAL:
						 _updaters[SPATIAL] = mock;
						 break;
					case TEMPORAL:
						 _updaters[TEMPORAL] = mock;
						 break;
					case LUCENE:
						 _updaters[LUCENE] = mock;
						 break;
					default:
						 throw new Exception();
					}
			  }
			  _fusionIndexUpdater = new FusionIndexUpdater( FusionVersion.slotSelector(), new LazyInstanceSelector<IndexUpdater>(_updaters, ThrowingFactory()) );
		 }

		 private System.Func<IndexSlot, IndexUpdater> ThrowingFactory()
		 {
			  return i =>
			  {
				throw new System.InvalidOperationException( "All updaters should exist already" );
			  };
		 }

		 private void ResetMocks()
		 {
			  foreach ( IndexUpdater updater in _aliveUpdaters )
			  {
					reset( updater );
			  }
		 }

		 /* process */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processMustSelectCorrectForAdd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessMustSelectCorrectForAdd()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  Value[] allValues = FusionIndexTestHelp.AllValues();

			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					foreach ( Value value in values[slot] )
					{
						 // then
						 VerifyAddWithCorrectUpdater( OrLucene( _updaters[slot] ), value );
					}
			  }

			  // when value is composite
			  foreach ( Value firstValue in allValues )
			  {
					foreach ( Value secondValue in allValues )
					{
						 VerifyAddWithCorrectUpdater( _updaters[LUCENE], firstValue, secondValue );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processMustSelectCorrectForRemove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessMustSelectCorrectForRemove()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  Value[] allValues = FusionIndexTestHelp.AllValues();

			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					foreach ( Value value in values[slot] )
					{
						 // then
						 VerifyRemoveWithCorrectUpdater( OrLucene( _updaters[slot] ), value );
					}
			  }

			  // when value is composite
			  foreach ( Value firstValue in allValues )
			  {
					foreach ( Value secondValue in allValues )
					{
						 VerifyRemoveWithCorrectUpdater( _updaters[LUCENE], firstValue, secondValue );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processMustSelectCorrectForChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessMustSelectCorrectForChange()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();

			  // when
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					foreach ( Value before in values[slot] )
					{
						 foreach ( Value after in values[slot] )
						 {
							  VerifyChangeWithCorrectUpdaterNotMixed( OrLucene( _updaters[slot] ), before, after );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processMustSelectCorrectForChangeFromOneGroupToAnother() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessMustSelectCorrectForChangeFromOneGroupToAnother()
		 {
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  foreach ( IndexSlot from in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					// given
					foreach ( IndexSlot to in Enum.GetValues( typeof( IndexSlot ) ) )
					{
						 if ( from != to )
						 {
							  // when
							  VerifyChangeWithCorrectUpdaterMixed( OrLucene( _updaters[from] ), OrLucene( _updaters[to] ), values[from], values[to] );
						 }
						 else
						 {
							  VerifyChangeWithCorrectUpdaterNotMixed( OrLucene( _updaters[from] ), values[from] );
						 }
						 ResetMocks();
					}
			  }
		 }

		 private IndexUpdater OrLucene( IndexUpdater updater )
		 {
			  return updater != SwallowingIndexUpdater.INSTANCE ? updater : _updaters[LUCENE];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyAddWithCorrectUpdater(org.neo4j.kernel.api.index.IndexUpdater correctPopulator, org.neo4j.values.storable.Value... numberValues) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void VerifyAddWithCorrectUpdater( IndexUpdater correctPopulator, params Value[] numberValues )
		 {
			  IndexEntryUpdate<LabelSchemaDescriptor> update = add( numberValues );
			  _fusionIndexUpdater.process( update );
			  verify( correctPopulator, times( 1 ) ).process( update );
			  foreach ( IndexUpdater populator in _aliveUpdaters )
			  {
					if ( populator != correctPopulator )
					{
						 verify( populator, never() ).process(update);
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRemoveWithCorrectUpdater(org.neo4j.kernel.api.index.IndexUpdater correctPopulator, org.neo4j.values.storable.Value... numberValues) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void VerifyRemoveWithCorrectUpdater( IndexUpdater correctPopulator, params Value[] numberValues )
		 {
			  IndexEntryUpdate<LabelSchemaDescriptor> update = FusionIndexTestHelp.Remove( numberValues );
			  _fusionIndexUpdater.process( update );
			  verify( correctPopulator, times( 1 ) ).process( update );
			  foreach ( IndexUpdater populator in _aliveUpdaters )
			  {
					if ( populator != correctPopulator )
					{
						 verify( populator, never() ).process(update);
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyChangeWithCorrectUpdaterNotMixed(org.neo4j.kernel.api.index.IndexUpdater correctPopulator, org.neo4j.values.storable.Value before, org.neo4j.values.storable.Value after) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void VerifyChangeWithCorrectUpdaterNotMixed( IndexUpdater correctPopulator, Value before, Value after )
		 {
			  IndexEntryUpdate<LabelSchemaDescriptor> update = FusionIndexTestHelp.Change( before, after );
			  _fusionIndexUpdater.process( update );
			  verify( correctPopulator, times( 1 ) ).process( update );
			  foreach ( IndexUpdater populator in _aliveUpdaters )
			  {
					if ( populator != correctPopulator )
					{
						 verify( populator, never() ).process(update);
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyChangeWithCorrectUpdaterNotMixed(org.neo4j.kernel.api.index.IndexUpdater updater, org.neo4j.values.storable.Value[] supportedValues) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void VerifyChangeWithCorrectUpdaterNotMixed( IndexUpdater updater, Value[] supportedValues )
		 {
			  foreach ( Value before in supportedValues )
			  {
					foreach ( Value after in supportedValues )
					{
						 VerifyChangeWithCorrectUpdaterNotMixed( updater, before, after );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyChangeWithCorrectUpdaterMixed(org.neo4j.kernel.api.index.IndexUpdater expectRemoveFrom, org.neo4j.kernel.api.index.IndexUpdater expectAddTo, org.neo4j.values.storable.Value[] beforeValues, org.neo4j.values.storable.Value[] afterValues) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void VerifyChangeWithCorrectUpdaterMixed( IndexUpdater expectRemoveFrom, IndexUpdater expectAddTo, Value[] beforeValues, Value[] afterValues )
		 {
			  for ( int beforeIndex = 0; beforeIndex < beforeValues.Length; beforeIndex++ )
			  {
					Value before = beforeValues[beforeIndex];
					for ( int afterIndex = 0; afterIndex < afterValues.Length; afterIndex++ )
					{
						 Value after = afterValues[afterIndex];

						 IndexEntryUpdate<LabelSchemaDescriptor> change = change( before, after );
						 _fusionIndexUpdater.process( change );

						 if ( expectRemoveFrom != expectAddTo )
						 {
							  verify( expectRemoveFrom, times( afterIndex + 1 ) ).process( remove( before ) );
							  verify( expectAddTo, times( beforeIndex + 1 ) ).process( add( after ) );
						 }
						 else
						 {
							  verify( expectRemoveFrom, times( 1 ) ).process( change( before, after ) );
						 }
					}
			  }
		 }

		 /* close */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustCloseAll()
		 {
			  // when
			  _fusionIndexUpdater.close();

			  // then
			  foreach ( IndexUpdater updater in _aliveUpdaters )
			  {
					verify( updater, times( 1 ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfAnyThrow()
		 {
			  foreach ( IndexSlot indexSlot in FusionVersion.aliveSlots() )
			  {
					FusionIndexTestHelp.VerifyFusionCloseThrowOnSingleCloseThrow( _updaters[indexSlot], _fusionIndexUpdater );
					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseOthersIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustCloseOthersIfAnyThrow()
		 {
			  foreach ( IndexSlot indexSlot in FusionVersion.aliveSlots() )
			  {
					IndexUpdater failingUpdater = _updaters[indexSlot];
					FusionIndexTestHelp.VerifyOtherIsClosedOnSingleThrow( failingUpdater, _fusionIndexUpdater, without( _aliveUpdaters, failingUpdater ) );
					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfAllThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfAllThrow()
		 {
			  FusionIndexTestHelp.VerifyFusionCloseThrowIfAllThrow( _fusionIndexUpdater, _aliveUpdaters );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstantiatePartLazilyForSpecificValueGroupUpdates() throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstantiatePartLazilyForSpecificValueGroupUpdates()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  foreach ( IndexSlot i in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					if ( _updaters[i] != SwallowingIndexUpdater.INSTANCE )
					{
						 // when
						 Value value = values[i][0];
						 _fusionIndexUpdater.process( add( value ) );
						 foreach ( IndexSlot j in Enum.GetValues( typeof( IndexSlot ) ) )
						 {
							  // then
							  if ( _updaters[j] != SwallowingIndexUpdater.INSTANCE )
							  {
									if ( i == j )
									{
										 verify( _updaters[i] ).process( any( typeof( IndexEntryUpdate ) ) );
									}
									else
									{
										 verifyNoMoreInteractions( _updaters[j] );
									}
							  }
						 }
					}

					InitiateMocks();
			  }
		 }
	}

}