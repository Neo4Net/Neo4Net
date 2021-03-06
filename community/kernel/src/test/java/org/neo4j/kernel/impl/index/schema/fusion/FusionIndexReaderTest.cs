﻿using System;
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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using PrimitiveLongResourceCollections = Org.Neo4j.Collection.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using StringContainsPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.StringContainsPredicate;
	using StringPrefixPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.StringPrefixPredicate;
	using StringSuffixPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.StringSuffixPredicate;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexTestHelp.fill;
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
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FusionIndexReaderTest
	public class FusionIndexReaderTest
	{
		 private IndexReader[] _aliveReaders;
		 private Dictionary<IndexSlot, IndexReader> _readers;
		 private FusionIndexReader _fusionIndexReader;
		 private const int PROP_KEY = 1;
		 private const int LABEL_KEY = 11;

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
			  _readers = new Dictionary<IndexSlot, IndexReader>( typeof( IndexSlot ) );
			  fill( _readers, IndexReader.EMPTY );
			  _aliveReaders = new IndexReader[activeSlots.Length];
			  for ( int i = 0; i < activeSlots.Length; i++ )
			  {
					IndexReader mock = mock( typeof( IndexReader ) );
					_aliveReaders[i] = mock;
					switch ( activeSlots[i] )
					{
					case STRING:
						 _readers[STRING] = mock;
						 break;
					case NUMBER:
						 _readers[NUMBER] = mock;
						 break;
					case SPATIAL:
						 _readers[SPATIAL] = mock;
						 break;
					case TEMPORAL:
						 _readers[TEMPORAL] = mock;
						 break;
					case LUCENE:
						 _readers[LUCENE] = mock;
						 break;
					default:
						 throw new Exception();
					}
			  }
			  _fusionIndexReader = new FusionIndexReader( FusionVersion.slotSelector(), new LazyInstanceSelector<IndexReader>(_readers, ThrowingFactory()), TestIndexDescriptorFactory.forLabel(LABEL_KEY, PROP_KEY) );
		 }

		 private System.Func<IndexSlot, IndexReader> ThrowingFactory()
		 {
			  return i =>
			  {
				throw new System.InvalidOperationException( "All readers should exist already" );
			  };
		 }

		 /* close */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseBothNativeAndLucene()
		 public virtual void CloseMustCloseBothNativeAndLucene()
		 {
			  // when
			  _fusionIndexReader.close();

			  // then
			  foreach ( IndexReader reader in _aliveReaders )
			  {
					verify( reader, times( 1 ) ).close();
			  }
		 }

		 // close iterator

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeIteratorMustCloseAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseIteratorMustCloseAll()
		 {
			  // given
			  PrimitiveLongResourceIterator[] iterators = new PrimitiveLongResourceIterator[_aliveReaders.Length];
			  for ( int i = 0; i < _aliveReaders.Length; i++ )
			  {
					PrimitiveLongResourceIterator iterator = mock( typeof( PrimitiveLongResourceIterator ) );
					when( _aliveReaders[i].query( any( typeof( IndexQuery ) ) ) ).thenReturn( iterator );
					iterators[i] = iterator;
			  }

			  // when
			  _fusionIndexReader.query( IndexQuery.exists( PROP_KEY ) ).close();

			  // then
			  foreach ( PrimitiveLongResourceIterator iterator in iterators )
			  {
					verify( iterator, times( 1 ) ).close();
			  }
		 }

		 /* countIndexedNodes */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void countIndexedNodesMustSelectCorrectReader()
		 public virtual void CountIndexedNodesMustSelectCorrectReader()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  Value[] allValues = FusionIndexTestHelp.AllValues();

			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					foreach ( Value value in values[slot] )
					{
						 VerifyCountIndexedNodesWithCorrectReader( OrLucene( _readers[slot] ), value );
					}
			  }

			  // When passing composite keys, they are only handled by lucene
			  foreach ( Value firstValue in allValues )
			  {
					foreach ( Value secondValue in allValues )
					{
						 VerifyCountIndexedNodesWithCorrectReader( _readers[LUCENE], firstValue, secondValue );
					}
			  }
		 }

		 private void VerifyCountIndexedNodesWithCorrectReader( IndexReader correct, params Value[] nativeValue )
		 {
			  _fusionIndexReader.countIndexedNodes( 0, new int[] { PROP_KEY }, nativeValue );
			  verify( correct, times( 1 ) ).countIndexedNodes( 0, new int[] { PROP_KEY }, nativeValue );
			  foreach ( IndexReader reader in _aliveReaders )
			  {
					if ( reader != correct )
					{
						 verify( reader, never() ).countIndexedNodes(0, new int[] { PROP_KEY }, nativeValue);
					}
			  }
		 }

		 /* query */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectLuceneForCompositePredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectLuceneForCompositePredicate()
		 {
			  // then
			  VerifyQueryWithCorrectReader( _readers[LUCENE], any( typeof( IndexQuery ) ), any( typeof( IndexQuery ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectStringForExactPredicateWithNumberValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectStringForExactPredicateWithNumberValue()
		 {
			  // given
			  foreach ( object value in FusionIndexTestHelp.ValuesSupportedByString() )
			  {
					IndexQuery indexQuery = IndexQuery.exact( PROP_KEY, value );

					// then
					VerifyQueryWithCorrectReader( ExpectedForStrings(), indexQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectNumberForExactPredicateWithNumberValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectNumberForExactPredicateWithNumberValue()
		 {
			  // given
			  foreach ( object value in FusionIndexTestHelp.ValuesSupportedByNumber() )
			  {
					IndexQuery indexQuery = IndexQuery.exact( PROP_KEY, value );

					// then
					VerifyQueryWithCorrectReader( ExpectedForNumbers(), indexQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectSpatialForExactPredicateWithSpatialValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectSpatialForExactPredicateWithSpatialValue()
		 {
			  // given
			  assumeTrue( HasSpatialSupport() );
			  foreach ( object value in FusionIndexTestHelp.ValuesSupportedBySpatial() )
			  {
					IndexQuery indexQuery = IndexQuery.exact( PROP_KEY, value );

					// then
					VerifyQueryWithCorrectReader( _readers[SPATIAL], indexQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectTemporalForExactPredicateWithTemporalValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectTemporalForExactPredicateWithTemporalValue()
		 {
			  // given
			  assumeTrue( HasTemporalSupport() );
			  foreach ( object temporalValue in FusionIndexTestHelp.ValuesSupportedByTemporal() )
			  {
					IndexQuery indexQuery = IndexQuery.exact( PROP_KEY, temporalValue );

					// then
					VerifyQueryWithCorrectReader( _readers[TEMPORAL], indexQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectLuceneForExactPredicateWithOtherValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectLuceneForExactPredicateWithOtherValue()
		 {
			  // given
			  foreach ( object value in FusionIndexTestHelp.ValuesNotSupportedBySpecificIndex() )
			  {
					IndexQuery indexQuery = IndexQuery.exact( PROP_KEY, value );

					// then
					VerifyQueryWithCorrectReader( _readers[LUCENE], indexQuery );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectStringForRangeStringPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectStringForRangeStringPredicate()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> stringRange = org.neo4j.internal.kernel.api.IndexQuery.range(PROP_KEY, "abc", true, "def", false);
			  IndexQuery.RangePredicate<object> stringRange = IndexQuery.range( PROP_KEY, "abc", true, "def", false );

			  // then
			  VerifyQueryWithCorrectReader( ExpectedForStrings(), stringRange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectNumberForRangeNumericPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectNumberForRangeNumericPredicate()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> numberRange = org.neo4j.internal.kernel.api.IndexQuery.range(PROP_KEY, 0, true, 1, false);
			  IndexQuery.RangePredicate<object> numberRange = IndexQuery.range( PROP_KEY, 0, true, 1, false );

			  // then
			  VerifyQueryWithCorrectReader( ExpectedForNumbers(), numberRange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectSpatialForRangeGeometricPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectSpatialForRangeGeometricPredicate()
		 {
			  // given
			  assumeTrue( HasSpatialSupport() );
			  PointValue from = Values.pointValue( CoordinateReferenceSystem.Cartesian, 1.0, 1.0 );
			  PointValue to = Values.pointValue( CoordinateReferenceSystem.Cartesian, 2.0, 2.0 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> geometryRange = org.neo4j.internal.kernel.api.IndexQuery.range(PROP_KEY, from, true, to, false);
			  IndexQuery.RangePredicate<object> geometryRange = IndexQuery.range( PROP_KEY, from, true, to, false );

			  // then
			  VerifyQueryWithCorrectReader( _readers[SPATIAL], geometryRange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectStringForStringPrefixPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectStringForStringPrefixPredicate()
		 {
			  // given
			  IndexQuery.StringPrefixPredicate stringPrefix = IndexQuery.stringPrefix( PROP_KEY, stringValue( "abc" ) );

			  // then
			  VerifyQueryWithCorrectReader( ExpectedForStrings(), stringPrefix );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectStringForStringSuffixPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectStringForStringSuffixPredicate()
		 {
			  // given
			  IndexQuery.StringSuffixPredicate stringPrefix = IndexQuery.stringSuffix( PROP_KEY, stringValue( "abc" ) );

			  // then
			  VerifyQueryWithCorrectReader( ExpectedForStrings(), stringPrefix );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSelectStringForStringContainsPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSelectStringForStringContainsPredicate()
		 {
			  // given
			  IndexQuery.StringContainsPredicate stringContains = IndexQuery.stringContains( PROP_KEY, stringValue( "abc" ) );

			  // then
			  VerifyQueryWithCorrectReader( ExpectedForStrings(), stringContains );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCombineResultFromExistsPredicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustCombineResultFromExistsPredicate()
		 {
			  // given
			  IndexQuery.ExistsPredicate exists = IndexQuery.exists( PROP_KEY );
			  long lastId = 0;
			  foreach ( IndexReader aliveReader in _aliveReaders )
			  {
					when( aliveReader.Query( exists ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, lastId++, lastId++ ) );
			  }

			  // when
			  LongIterator result = _fusionIndexReader.query( exists );

			  // then

			  LongSet resultSet = PrimitiveLongCollections.asSet( result );
			  for ( long i = 0L; i < lastId; i++ )
			  {
					assertTrue( "Expected to contain " + i + ", but was " + resultSet, resultSet.contains( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstantiatePartLazilyForSpecificValueGroupQuery() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstantiatePartLazilyForSpecificValueGroupQuery()
		 {
			  // given
			  Dictionary<IndexSlot, Value[]> values = FusionIndexTestHelp.ValuesByGroup();
			  foreach ( IndexSlot i in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					if ( _readers[i] != IndexReader.EMPTY )
					{
						 // when
						 Value value = values[i][0];
						 _fusionIndexReader.query( IndexQuery.exact( 0, value ) );
						 foreach ( IndexSlot j in Enum.GetValues( typeof( IndexSlot ) ) )
						 {
							  // then
							  if ( _readers[j] != IndexReader.EMPTY )
							  {
									if ( i == j )
									{
										 verify( _readers[i] ).query( any( typeof( IndexQuery ) ) );
									}
									else
									{
										 verifyNoMoreInteractions( _readers[j] );
									}
							  }
						 }
					}

					InitiateMocks();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyQueryWithCorrectReader(org.neo4j.storageengine.api.schema.IndexReader expectedReader, org.neo4j.internal.kernel.api.IndexQuery... indexQuery) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 private void VerifyQueryWithCorrectReader( IndexReader expectedReader, params IndexQuery[] indexQuery )
		 {
			  // when
			  _fusionIndexReader.query( indexQuery );

			  // then
			  verify( expectedReader, times( 1 ) ).query( indexQuery );
			  foreach ( IndexReader reader in _aliveReaders )
			  {
					if ( reader != expectedReader )
					{
						 verifyNoMoreInteractions( reader );
					}
			  }
		 }

		 private IndexReader ExpectedForStrings()
		 {
			  return OrLucene( _readers[STRING] );
		 }

		 private IndexReader ExpectedForNumbers()
		 {
			  return OrLucene( _readers[NUMBER] );
		 }

		 private bool HasSpatialSupport()
		 {
			  return _readers[SPATIAL] != IndexReader.EMPTY;
		 }

		 private bool HasTemporalSupport()
		 {
			  return _readers[TEMPORAL] != IndexReader.EMPTY;
		 }

		 private IndexReader OrLucene( IndexReader reader )
		 {
			  return reader != IndexReader.EMPTY ? reader : _readers[LUCENE];
		 }
	}

}