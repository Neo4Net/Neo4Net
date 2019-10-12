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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.fusion
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using Org.Neo4j.Helpers.Collection;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ReporterFactories = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactories;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.verification.VerificationModeFactory.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.without;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.fill;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.verifyFusionCloseThrowIfAllThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.verifyFusionCloseThrowOnSingleCloseThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.verifyOtherIsClosedOnSingleThrow;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FusionIndexAccessorTest
	public class FusionIndexAccessorTest
	{
		private bool InstanceFieldsInitialized = false;

		public FusionIndexAccessorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_indexDescriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 42 ) ).withId( _indexId );
		}

		 private FusionIndexAccessor _fusionIndexAccessor;
		 private readonly long _indexId = 0;
		 private readonly IndexDropAction _dropAction = mock( typeof( IndexDropAction ) );
		 private Dictionary<IndexSlot, IndexAccessor> _accessors;
		 private IndexAccessor[] _aliveAccessors;
		 private StoreIndexDescriptor _indexDescriptor;

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
			  _accessors = new Dictionary<IndexSlot, IndexAccessor>( typeof( IndexSlot ) );
			  fill( _accessors, Org.Neo4j.Kernel.Api.Index.IndexAccessor_Fields.Empty );
			  _aliveAccessors = new IndexAccessor[activeSlots.Length];
			  for ( int i = 0; i < activeSlots.Length; i++ )
			  {
					IndexAccessor mock = mock( typeof( IndexAccessor ) );
					_aliveAccessors[i] = mock;
					switch ( activeSlots[i] )
					{
					case STRING:
						 _accessors[STRING] = mock;
						 break;
					case NUMBER:
						 _accessors[NUMBER] = mock;
						 break;
					case SPATIAL:
						 _accessors[SPATIAL] = mock;
						 break;
					case TEMPORAL:
						 _accessors[TEMPORAL] = mock;
						 break;
					case LUCENE:
						 _accessors[LUCENE] = mock;
						 break;
					default:
						 throw new Exception();
					}
			  }
			  _fusionIndexAccessor = new FusionIndexAccessor( FusionVersion.slotSelector(), new InstanceSelector<IndexAccessor>(_accessors), _indexDescriptor, _dropAction );
		 }

		 private void ResetMocks()
		 {
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					reset( accessor );
			  }
		 }

		 /* drop */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustDropAll()
		 public virtual void DropMustDropAll()
		 {
			  // when
			  // ... all drop successful
			  _fusionIndexAccessor.drop();

			  // then
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					verify( accessor, times( 1 ) ).drop();
			  }
			  verify( _dropAction ).drop( _indexId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustThrowIfDropAnyFail()
		 public virtual void DropMustThrowIfDropAnyFail()
		 {
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					// when
					VerifyFailOnSingleDropFailure( accessor, _fusionIndexAccessor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fusionIndexIsDirtyWhenAnyIsDirty()
		 public virtual void FusionIndexIsDirtyWhenAnyIsDirty()
		 {
			  foreach ( IndexAccessor dirtyAccessor in _aliveAccessors )
			  {
					// when
					foreach ( IndexAccessor aliveAccessor in _aliveAccessors )
					{
						 when( aliveAccessor.Dirty ).thenReturn( aliveAccessor == dirtyAccessor );
					}

					// then
					assertTrue( _fusionIndexAccessor.Dirty );
			  }
		 }

		 private static void VerifyFailOnSingleDropFailure( IndexAccessor failingAccessor, FusionIndexAccessor fusionIndexAccessor )
		 {
			  UncheckedIOException expectedFailure = new UncheckedIOException( new IOException( "fail" ) );
			  doThrow( expectedFailure ).when( failingAccessor ).drop();
			  try
			  {
					fusionIndexAccessor.Drop();
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException e )
			  {
					assertSame( expectedFailure, e );
			  }
			  doAnswer( invocation => null ).when( failingAccessor ).drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustThrowIfAllFail()
		 public virtual void DropMustThrowIfAllFail()
		 {
			  // given
			  IList<UncheckedIOException> exceptions = new List<UncheckedIOException>();
			  foreach ( IndexAccessor indexAccessor in _aliveAccessors )
			  {
					UncheckedIOException exception = new UncheckedIOException( new IOException( indexAccessor.GetType().Name + " fail" ) );
					exceptions.Add( exception );
					doThrow( exception ).when( indexAccessor ).drop();
			  }

			  try
			  {
					// when
					_fusionIndexAccessor.drop();
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException e )
			  {
					// then
					assertThat( exceptions, hasItem( e ) );
			  }
		 }

		 /* close */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseAll()
		 public virtual void CloseMustCloseAll()
		 {
			  // when
			  // ... all close successful
			  _fusionIndexAccessor.Dispose();

			  // then
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					verify( accessor, times( 1 ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfOneThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfOneThrow()
		 {
			  //noinspection ForLoopReplaceableByForEach - aliveAccessors is updated in initiateMocks()
			  for ( int i = 0; i < _aliveAccessors.Length; i++ )
			  {
					IndexAccessor accessor = _aliveAccessors[i];
					verifyFusionCloseThrowOnSingleCloseThrow( accessor, _fusionIndexAccessor );
					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseOthersIfOneThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustCloseOthersIfOneThrow()
		 {
			  //noinspection ForLoopReplaceableByForEach - aliveAccessors is updated in initiateMocks()
			  for ( int i = 0; i < _aliveAccessors.Length; i++ )
			  {
					IndexAccessor accessor = _aliveAccessors[i];
					verifyOtherIsClosedOnSingleThrow( accessor, _fusionIndexAccessor, without( _aliveAccessors, accessor ) );
					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustThrowIfAllFail() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustThrowIfAllFail()
		 {
			  verifyFusionCloseThrowIfAllThrow( _fusionIndexAccessor, _aliveAccessors );
		 }

		 // newAllEntriesReader

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustCombineResultFromAll()
		 public virtual void AllEntriesReaderMustCombineResultFromAll()
		 {
			  // given
			  IList<long>[] ids = new System.Collections.IList[_aliveAccessors.Length];
			  long lastId = 0;
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					ids[i] = Arrays.asList( lastId++, lastId++ );
			  }
			  MockAllEntriesReaders( ids );

			  // when
			  ISet<long> result = Iterables.asSet( _fusionIndexAccessor.newAllEntriesReader() );

			  // then
			  foreach ( IList<long> part in ids )
			  {
					AssertResultContainsAll( result, part );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustCombineResultFromAllEmpty()
		 public virtual void AllEntriesReaderMustCombineResultFromAllEmpty()
		 {
			  // given
			  IList<long>[] ids = new System.Collections.IList[_aliveAccessors.Length];
			  for ( int j = 0; j < ids.Length; j++ )
			  {
					ids[j] = Collections.emptyList();
			  }
			  MockAllEntriesReaders( ids );

			  // when
			  ISet<long> result = Iterables.asSet( _fusionIndexAccessor.newAllEntriesReader() );

			  // then
			  assertTrue( result.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustCombineResultFromAllAccessors()
		 public virtual void AllEntriesReaderMustCombineResultFromAllAccessors()
		 {
			  // given
			  IList<long>[] parts = new System.Collections.IList[_aliveAccessors.Length];
			  for ( int i = 0; i < parts.Length; i++ )
			  {
					parts[i] = new List<long>();
			  }
			  for ( long i = 0; i < 10; i++ )
			  {
					Random.among( parts ).Add( i );
			  }
			  MockAllEntriesReaders( parts );

			  // when
			  ISet<long> result = Iterables.asSet( _fusionIndexAccessor.newAllEntriesReader() );

			  // then
			  foreach ( IList<long> part in parts )
			  {
					AssertResultContainsAll( result, part );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustCloseAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllEntriesReaderMustCloseAll()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  BoundedIterable<long>[] allEntriesReaders = java.util.aliveAccessors.Select( accessor => MockSingleAllEntriesReader( accessor, Arrays.asList() ) ).ToArray(BoundedIterable[]::new);

			  // when
			  _fusionIndexAccessor.newAllEntriesReader().close();

			  // then
			  foreach ( BoundedIterable<long> allEntriesReader in allEntriesReaders )
			  {
					verify( allEntriesReader, times( 1 ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustCloseOthersIfOneThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllEntriesReaderMustCloseOthersIfOneThrow()
		 {
			  for ( int i = 0; i < _aliveAccessors.Length; i++ )
			  {
					// given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					BoundedIterable<long>[] allEntriesReaders = java.util.aliveAccessors.Select( accessor => MockSingleAllEntriesReader( accessor, Arrays.asList() ) ).ToArray(BoundedIterable[]::new);

					// then
					BoundedIterable<long> fusionAllEntriesReader = _fusionIndexAccessor.newAllEntriesReader();
					verifyOtherIsClosedOnSingleThrow( allEntriesReaders[i], fusionAllEntriesReader, without( allEntriesReaders, allEntriesReaders[i] ) );

					ResetMocks();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustThrowIfOneThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllEntriesReaderMustThrowIfOneThrow()
		 {
			  foreach ( IndexAccessor failingAccessor in _aliveAccessors )
			  {
					BoundedIterable<long> failingReader = null;
					foreach ( IndexAccessor aliveAccessor in _aliveAccessors )
					{
						 BoundedIterable<long> reader = MockSingleAllEntriesReader( aliveAccessor, Collections.emptyList() );
						 if ( aliveAccessor == failingAccessor )
						 {
							  failingReader = reader;
						 }
					}

					// then
					BoundedIterable<long> fusionAllEntriesReader = _fusionIndexAccessor.newAllEntriesReader();
					FusionIndexTestHelp.VerifyFusionCloseThrowOnSingleCloseThrow( failingReader, fusionAllEntriesReader );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustReportUnknownMaxCountIfAnyReportUnknownMaxCount()
		 public virtual void AllEntriesReaderMustReportUnknownMaxCountIfAnyReportUnknownMaxCount()
		 {
			  for ( int i = 0; i < _aliveAccessors.Length; i++ )
			  {
					for ( int j = 0; j < _aliveAccessors.Length; j++ )
					{
						 // given
						 if ( j == i )
						 {
							  MockSingleAllEntriesReaderWithUnknownMaxCount( _aliveAccessors[j], Collections.emptyList() );
						 }
						 else
						 {
							  MockSingleAllEntriesReader( _aliveAccessors[j], Collections.emptyList() );
						 }
					}

					// then
					BoundedIterable<long> fusionAllEntriesReader = _fusionIndexAccessor.newAllEntriesReader();
					assertThat( fusionAllEntriesReader.MaxCount(), @is(Org.Neo4j.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allEntriesReaderMustReportFusionMaxCountOfAll()
		 public virtual void AllEntriesReaderMustReportFusionMaxCountOfAll()
		 {
			  long lastId = 0;
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					MockSingleAllEntriesReader( accessor, Arrays.asList( lastId++, lastId++ ) );
			  }

			  // then
			  BoundedIterable<long> fusionAllEntriesReader = _fusionIndexAccessor.newAllEntriesReader();
			  assertThat( fusionAllEntriesReader.MaxCount(), @is(lastId) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailValueValidationIfAnyPartFail()
		 public virtual void ShouldFailValueValidationIfAnyPartFail()
		 {
			  // given
			  System.ArgumentException failure = new System.ArgumentException( "failing" );
			  for ( int i = 0; i < _aliveAccessors.Length; i++ )
			  {
					for ( int j = 0; j < _aliveAccessors.Length; j++ )
					{
						 if ( i == j )
						 {
							  doThrow( failure ).when( _aliveAccessors[i] ).validateBeforeCommit( ArgumentMatchers.any( typeof( Value[] ) ) );
						 }
						 else
						 {
							  doAnswer( invocation => null ).when( _aliveAccessors[i] ).validateBeforeCommit( any( typeof( Value[] ) ) );
						 }
					}

					// when
					try
					{
						 _fusionIndexAccessor.validateBeforeCommit( new Value[] { stringValue( "something" ) } );
					}
					catch ( System.ArgumentException e )
					{
						 // then
						 assertSame( failure, e );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedValueValidationIfAllSucceed()
		 public virtual void ShouldSucceedValueValidationIfAllSucceed()
		 {
			  // when
			  _fusionIndexAccessor.validateBeforeCommit( new Value[] { stringValue( "test value" ) } );

			  // then no exception was thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstantiateReadersLazily()
		 public virtual void ShouldInstantiateReadersLazily()
		 {
			  // when getting a new reader, no part-reader should be instantiated
			  IndexReader fusionReader = _fusionIndexAccessor.newReader();
			  for ( int j = 0; j < _aliveAccessors.Length; j++ )
			  {
					// then
					verifyNoMoreInteractions( _aliveAccessors[j] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstantiateUpdatersLazily()
		 public virtual void ShouldInstantiateUpdatersLazily()
		 {
			  // when getting a new reader, no part-reader should be instantiated
			  IndexUpdater updater = _fusionIndexAccessor.newUpdater( IndexUpdateMode.ONLINE );
			  for ( int j = 0; j < _aliveAccessors.Length; j++ )
			  {
					// then
					verifyNoMoreInteractions( _aliveAccessors[j] );
			  }
		 }

		 /* Consistency check */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCheckConsistencyOnAllAliveAccessors()
		 public virtual void MustCheckConsistencyOnAllAliveAccessors()
		 {
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					when( accessor.ConsistencyCheck( any( typeof( ReporterFactory ) ) ) ).thenReturn( true );
			  }
			  assertTrue( _fusionIndexAccessor.consistencyCheck( ReporterFactories.noopReporterFactory() ) );
			  foreach ( IndexAccessor accessor in _aliveAccessors )
			  {
					verify( accessor, times( 1 ) ).consistencyCheck( any( typeof( ReporterFactory ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFailConsistencyCheckIfOneAliveAccessorFails()
		 public virtual void MustFailConsistencyCheckIfOneAliveAccessorFails()
		 {
			  foreach ( IndexAccessor failingAccessor in _aliveAccessors )
			  {
					foreach ( IndexAccessor accessor in _aliveAccessors )
					{
						 if ( accessor == failingAccessor )
						 {
							  when( failingAccessor.ConsistencyCheck( any( typeof( ReporterFactory ) ) ) ).thenReturn( false );
						 }
						 else
						 {
							  when( failingAccessor.ConsistencyCheck( any( typeof( ReporterFactory ) ) ) ).thenReturn( true );
						 }
					}
					assertFalse( _fusionIndexAccessor.consistencyCheck( ReporterFactories.noopReporterFactory() ) );
					ResetMocks();
			  }
		 }

		 private static void AssertResultContainsAll( ISet<long> result, IList<long> expectedEntries )
		 {
			  foreach ( long expectedEntry in expectedEntries )
			  {
					assertTrue( "Expected to contain " + expectedEntry + ", but was " + result, result.Contains( expectedEntry ) );
			  }
		 }

		 private static BoundedIterable<long> MockSingleAllEntriesReader( IndexAccessor targetAccessor, IList<long> entries )
		 {
			  BoundedIterable<long> allEntriesReader = MockedAllEntriesReader( entries );
			  when( targetAccessor.NewAllEntriesReader() ).thenReturn(allEntriesReader);
			  return allEntriesReader;
		 }

		 private static BoundedIterable<long> MockedAllEntriesReader( IList<long> entries )
		 {
			  return MockedAllEntriesReader( true, entries );
		 }

		 private static void MockSingleAllEntriesReaderWithUnknownMaxCount( IndexAccessor targetAccessor, IList<long> entries )
		 {
			  BoundedIterable<long> allEntriesReader = MockedAllEntriesReaderUnknownMaxCount( entries );
			  when( targetAccessor.NewAllEntriesReader() ).thenReturn(allEntriesReader);
		 }

		 private static BoundedIterable<long> MockedAllEntriesReaderUnknownMaxCount( IList<long> entries )
		 {
			  return MockedAllEntriesReader( false, entries );
		 }

		 private static BoundedIterable<long> MockedAllEntriesReader( bool knownMaxCount, IList<long> entries )
		 {
			  BoundedIterable<long> mockedAllEntriesReader = mock( typeof( BoundedIterable ) );
			  when( mockedAllEntriesReader.MaxCount() ).thenReturn(knownMaxCount ? entries.Count : Org.Neo4j.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT);
			  when( mockedAllEntriesReader.GetEnumerator() ).thenReturn(entries.GetEnumerator());
			  return mockedAllEntriesReader;
		 }

		 private void MockAllEntriesReaders( params IList<long>[] entries )
		 {
			  for ( int i = 0; i < entries.Length; i++ )
			  {
					MockSingleAllEntriesReader( _aliveAccessors[i], entries[i] );
			  }
		 }
	}

}