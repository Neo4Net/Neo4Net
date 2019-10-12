using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
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
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.fill;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.verifyCallFail;
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
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FusionIndexPopulatorTest
	public class FusionIndexPopulatorTest
	{
		 private IndexPopulator[] _alivePopulators;
		 private Dictionary<IndexSlot, IndexPopulator> _populators;
		 private FusionIndexPopulator _fusionIndexPopulator;
		 private readonly long _indexId = 8;
		 private readonly IndexDropAction _dropAction = mock( typeof( IndexDropAction ) );

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
			  IndexSlot[] aliveSlots = FusionVersion.aliveSlots();
			  _populators = new Dictionary<IndexSlot, IndexPopulator>( typeof( IndexSlot ) );
			  fill( _populators, Neo4Net.Kernel.Api.Index.IndexPopulator_Fields.Empty );
			  _alivePopulators = new IndexPopulator[aliveSlots.Length];
			  for ( int i = 0; i < aliveSlots.Length; i++ )
			  {
					IndexPopulator mock = mock( typeof( IndexPopulator ) );
					_alivePopulators[i] = mock;
					switch ( aliveSlots[i] )
					{
					case STRING:
						 _populators[STRING] = mock;
						 break;
					case NUMBER:
						 _populators[NUMBER] = mock;
						 break;
					case SPATIAL:
						 _populators[SPATIAL] = mock;
						 break;
					case TEMPORAL:
						 _populators[TEMPORAL] = mock;
						 break;
					case LUCENE:
						 _populators[LUCENE] = mock;
						 break;
					default:
						 throw new Exception();
					}
			  }
			  _fusionIndexPopulator = new FusionIndexPopulator( FusionVersion.slotSelector(), new InstanceSelector<IndexPopulator>(_populators), _indexId, _dropAction, false );
		 }

		 private void ResetMocks()
		 {
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					reset( alivePopulator );
			  }
		 }

		 /* create */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createMustCreateAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateMustCreateAll()
		 {
			  // when
			  _fusionIndexPopulator.create();

			  // then
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					verify( alivePopulator, times( 1 ) ).create();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createRemoveAnyLeftOversThatWasThereInIndexDirectoryBeforePopulation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateRemoveAnyLeftOversThatWasThereInIndexDirectoryBeforePopulation()
		 {
			  _fusionIndexPopulator.create();

			  verify( _dropAction ).drop( _indexId, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createMustThrowIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateMustThrowIfAnyThrow()
		 {
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					// given
					UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
					doThrow( failure ).when( alivePopulator ).create();

					verifyCallFail(failure, () =>
					{
					 _fusionIndexPopulator.create();
					 return null;
					});

					// reset throw for testing of next populator
					doAnswer( invocation => null ).when( alivePopulator ).create();
			  }
		 }

		 /* drop */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustDropAll()
		 public virtual void DropMustDropAll()
		 {
			  // when
			  _fusionIndexPopulator.drop();

			  // then
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					verify( alivePopulator, times( 1 ) ).drop();
			  }
			  verify( _dropAction ).drop( _indexId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustThrowIfAnyDropThrow()
		 public virtual void DropMustThrowIfAnyDropThrow()
		 {
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					// given
					UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
					doThrow( failure ).when( alivePopulator ).drop();

					verifyCallFail(failure, () =>
					{
					 _fusionIndexPopulator.drop();
					 return null;
					});

					// reset throw for testing of next populator
					doAnswer( invocation => null ).when( alivePopulator ).drop();
			  }
		 }

		 /* add */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addMustSelectCorrectPopulator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddMustSelectCorrectPopulator()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  Value[] allValues = FusionIndexTestHelp.AllValues();

			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					foreach ( Value value in values[slot] )
					{
						 VerifyAddWithCorrectPopulator( OrLucene( _populators[slot] ), value );
					}
			  }

			  // All composite values should go to lucene
			  foreach ( Value firstValue in allValues )
			  {
					foreach ( Value secondValue in allValues )
					{
						 VerifyAddWithCorrectPopulator( _populators[LUCENE], firstValue, secondValue );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyAddWithCorrectPopulator(org.neo4j.kernel.api.index.IndexPopulator correctPopulator, org.neo4j.values.storable.Value... numberValues) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void VerifyAddWithCorrectPopulator( IndexPopulator correctPopulator, params Value[] numberValues )
		 {
			  ICollection<IndexEntryUpdate<LabelSchemaDescriptor>> update = Collections.singletonList( add( numberValues ) );
			  _fusionIndexPopulator.add( update );
			  verify( correctPopulator, times( 1 ) ).add( update );
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					if ( alivePopulator != correctPopulator )
					{
						 verify( alivePopulator, never() ).add(update);
					}
			  }
		 }

		 /* verifyDeferredConstraints */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyDeferredConstraintsMustThrowIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyDeferredConstraintsMustThrowIfAnyThrow()
		 {
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					// given
					IndexEntryConflictException failure = mock( typeof( IndexEntryConflictException ) );
					doThrow( failure ).when( alivePopulator ).verifyDeferredConstraints( any() );

					verifyCallFail(failure, () =>
					{
					 _fusionIndexPopulator.verifyDeferredConstraints( null );
					 return null;
					});

					// reset throw for testing of next populator
					doAnswer( invocation => null ).when( alivePopulator ).verifyDeferredConstraints( any() );
			  }
		 }

		 /* close */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustCloseAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulCloseMustCloseAll()
		 {
			  // when
			  CloseAndVerifyPropagation( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustCloseAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnsuccessfulCloseMustCloseAll()
		 {
			  // when
			  CloseAndVerifyPropagation( false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeAndVerifyPropagation(boolean populationCompletedSuccessfully) throws java.io.IOException
		 private void CloseAndVerifyPropagation( bool populationCompletedSuccessfully )
		 {
			  _fusionIndexPopulator.close( populationCompletedSuccessfully );

			  // then
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					verify( alivePopulator, times( 1 ) ).close( populationCompletedSuccessfully );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfCloseAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfCloseAnyThrow()
		 {
			  foreach ( IndexSlot aliveSlot in FusionVersion.aliveSlots() )
			  {
					// given
					UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
					doThrow( failure ).when( _populators[aliveSlot] ).close( anyBoolean() );

					verifyCallFail(failure, () =>
					{
					 _fusionIndexPopulator.close( anyBoolean() );
					 return null;
					});

					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyOtherCloseOnThrow(org.neo4j.kernel.api.index.IndexPopulator throwingPopulator) throws Exception
		 private void VerifyOtherCloseOnThrow( IndexPopulator throwingPopulator )
		 {
			  // given
			  UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
			  doThrow( failure ).when( throwingPopulator ).close( anyBoolean() );

			  // when
			  try
			  {
					_fusionIndexPopulator.close( true );
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException )
			  {
			  }

			  // then
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					verify( alivePopulator, times( 1 ) ).close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseOthersIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustCloseOthersIfAnyThrow()
		 {
			  foreach ( IndexSlot throwingSlot in FusionVersion.aliveSlots() )
			  {
					VerifyOtherCloseOnThrow( _populators[throwingSlot] );
					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfAllThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfAllThrow()
		 {
			  // given
			  IList<UncheckedIOException> failures = new List<UncheckedIOException>();
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					UncheckedIOException failure = new UncheckedIOException( new IOException( "FAILURE[" + alivePopulator + "]" ) );
					failures.Add( failure );
					doThrow( failure ).when( alivePopulator ).close( anyBoolean() );
			  }

			  try
			  {
					// when
					_fusionIndexPopulator.close( anyBoolean() );
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException e )
			  {
					// then
					if ( !failures.Contains( e ) )
					{
						 fail( "Thrown exception didn't match any of the expected failures: " + failures );
					}
			  }
		 }

		 /* markAsFailed */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markAsFailedMustMarkAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkAsFailedMustMarkAll()
		 {
			  // when
			  string failureMessage = "failure";
			  _fusionIndexPopulator.markAsFailed( failureMessage );

			  // then
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					verify( alivePopulator, times( 1 ) ).markAsFailed( failureMessage );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markAsFailedMustThrowIfAnyThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkAsFailedMustThrowIfAnyThrow()
		 {
			  foreach ( IndexPopulator alivePopulator in _alivePopulators )
			  {
					// given
					UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
					doThrow( failure ).when( alivePopulator ).markAsFailed( anyString() );

					// then
					verifyCallFail(failure, () =>
					{
					 _fusionIndexPopulator.markAsFailed( anyString() );
					 return null;
					});

					// reset throw for testing of next populator
					doAnswer( invocation => null ).when( alivePopulator ).markAsFailed( anyString() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeSampleOnCorrectPopulator()
		 public virtual void ShouldIncludeSampleOnCorrectPopulator()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();

			  foreach ( IndexSlot activeSlot in FusionVersion.aliveSlots() )
			  {
					VerifySampleToCorrectPopulator( values[activeSlot], _populators[activeSlot] );
			  }
		 }

		 private void VerifySampleToCorrectPopulator( Value[] values, IndexPopulator populator )
		 {
			  foreach ( Value value in values )
			  {
					// when
					IndexEntryUpdate<LabelSchemaDescriptor> update = add( value );
					_fusionIndexPopulator.includeSample( update );

					// then
					verify( populator ).includeSample( update );
					reset( populator );
			  }
		 }

		 private IndexPopulator OrLucene( IndexPopulator populator )
		 {
			  return populator != Neo4Net.Kernel.Api.Index.IndexPopulator_Fields.Empty ? populator : _populators[LUCENE];
		 }
	}

}