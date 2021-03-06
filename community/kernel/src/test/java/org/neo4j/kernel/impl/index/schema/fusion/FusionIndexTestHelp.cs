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
	using Matcher = org.hamcrest.Matcher;
	using Mockito = org.mockito.Mockito;


	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using Org.Neo4j.Kernel.Api.Index;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AnyOf.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
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

	internal class FusionIndexTestHelp
	{
		 private static LabelSchemaDescriptor _indexKey = SchemaDescriptorFactory.forLabel( 0, 0 );
		 private static LabelSchemaDescriptor _compositeIndexKey = SchemaDescriptorFactory.forLabel( 0, 0, 1 );

		 private static readonly Value[] _stringValues = new Value[] { Values.stringValue( "abc" ), Values.stringValue( "abcdefghijklmnopqrstuvwxyzåäö" ), Values.charValue( 'S' ) };
		 private static readonly Value[] _numberValues = new Value[] { Values.byteValue( ( sbyte ) 1 ), Values.shortValue( ( short ) 2 ), Values.intValue( 3 ), Values.longValue( 4 ), Values.floatValue( 5.6f ), Values.doubleValue( 7.8 ) };
		 private static readonly Value[] _pointValues = new Value[] { Values.pointValue( CoordinateReferenceSystem.Cartesian, 123.0, 456.0 ), Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 123.0, 456.0, 789.0 ), Values.pointValue( CoordinateReferenceSystem.WGS84, 13.2, 56.8 ) };
		 private static readonly Value[] _temporalValues = new Value[] { DateValue.epochDate( 1 ), DateValue.epochDate( 10000 ) };
		 private static readonly Value[] _otherValues = new Value[] { Values.booleanValue( true ), Values.booleanArray( new bool[2] ), Values.byteArray( new sbyte[]{ 1, 2 } ), Values.shortArray( new short[]{ 3, 4 } ), Values.intArray( new int[]{ 5, 6 } ), Values.longArray( new long[]{ 7, 8 } ), Values.floatArray( new float[]{ 9.10f, 11.12f } ), Values.doubleArray( new double[]{ 13.14, 15.16 } ), Values.charArray( new char[2] ), Values.stringArray( "a", "b" ), Values.pointArray( _pointValues ), Values.NO_VALUE };

		 internal static Value[] ValuesSupportedByString()
		 {
			  return _stringValues;
		 }

		 internal static Value[] ValuesSupportedByNumber()
		 {
			  return _numberValues;
		 }

		 internal static Value[] ValuesSupportedBySpatial()
		 {
			  return _pointValues;
		 }

		 internal static Value[] ValuesSupportedByTemporal()
		 {
			  return _temporalValues;
		 }

		 internal static Value[] ValuesNotSupportedBySpecificIndex()
		 {
			  return _otherValues;
		 }

		 internal static Value[] AllValues()
		 {
			  IList<Value> values = new List<Value>();
			  foreach ( Value[] group in ValuesByGroup().Values )
			  {
					( ( IList<Value> )values ).AddRange( Arrays.asList( group ) );
			  }
			  return values.ToArray();
		 }

		 internal static Dictionary<IndexSlot, Value[]> ValuesByGroup()
		 {
			  Dictionary<IndexSlot, Value[]> values = new Dictionary<IndexSlot, Value[]>( typeof( IndexSlot ) );
			  values[STRING] = FusionIndexTestHelp.ValuesSupportedByString();
			  values[NUMBER] = FusionIndexTestHelp.ValuesSupportedByNumber();
			  values[SPATIAL] = FusionIndexTestHelp.ValuesSupportedBySpatial();
			  values[TEMPORAL] = FusionIndexTestHelp.ValuesSupportedByTemporal();
			  values[LUCENE] = FusionIndexTestHelp.ValuesNotSupportedBySpecificIndex();
			  return values;
		 }

		 internal static void VerifyCallFail( Exception expectedFailure, Callable failingCall )
		 {
			  try
			  {
					failingCall.call();
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertSame( expectedFailure, e );
			  }
		 }

		 internal static IndexEntryUpdate<LabelSchemaDescriptor> Add( params Value[] value )
		 {
			  switch ( value.Length )
			  {
			  case 1:
					return IndexEntryUpdate.add( 0, _indexKey, value );
			  case 2:
					return IndexEntryUpdate.add( 0, _compositeIndexKey, value );
			  default:
					return null;
			  }
		 }

		 internal static IndexEntryUpdate<LabelSchemaDescriptor> Remove( params Value[] value )
		 {
			  switch ( value.Length )
			  {
			  case 1:
					return IndexEntryUpdate.remove( 0, _indexKey, value );
			  case 2:
					return IndexEntryUpdate.remove( 0, _compositeIndexKey, value );
			  default:
					return null;
			  }
		 }

		 internal static IndexEntryUpdate<LabelSchemaDescriptor> Change( Value[] before, Value[] after )
		 {
			  return IndexEntryUpdate.change( 0, _compositeIndexKey, before, after );
		 }

		 internal static IndexEntryUpdate<LabelSchemaDescriptor> Change( Value before, Value after )
		 {
			  return IndexEntryUpdate.change( 0, _indexKey, before, after );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void verifyOtherIsClosedOnSingleThrow(AutoCloseable failingCloseable, AutoCloseable fusionCloseable, AutoCloseable... successfulCloseables) throws Exception
		 internal static void VerifyOtherIsClosedOnSingleThrow( AutoCloseable failingCloseable, AutoCloseable fusionCloseable, params AutoCloseable[] successfulCloseables )
		 {
			  UncheckedIOException failure = new UncheckedIOException( new IOException( "fail" ) );
			  doThrow( failure ).when( failingCloseable ).close();

			  // when
			  try
			  {
					fusionCloseable.close();
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException )
			  {
			  }

			  // then
			  foreach ( AutoCloseable successfulCloseable in successfulCloseables )
			  {
					verify( successfulCloseable, Mockito.times( 1 ) ).close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void verifyFusionCloseThrowOnSingleCloseThrow(AutoCloseable failingCloseable, AutoCloseable fusionCloseable) throws Exception
		 internal static void VerifyFusionCloseThrowOnSingleCloseThrow( AutoCloseable failingCloseable, AutoCloseable fusionCloseable )
		 {
			  UncheckedIOException expectedFailure = new UncheckedIOException( new IOException( "fail" ) );
			  doThrow( expectedFailure ).when( failingCloseable ).close();
			  try
			  {
					fusionCloseable.close();
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException e )
			  {
					assertSame( expectedFailure, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void verifyFusionCloseThrowIfAllThrow(AutoCloseable fusionCloseable, AutoCloseable... autoCloseables) throws Exception
		 internal static void VerifyFusionCloseThrowIfAllThrow( AutoCloseable fusionCloseable, params AutoCloseable[] autoCloseables )
		 {
			  // given
			  UncheckedIOException[] failures = new UncheckedIOException[autoCloseables.Length];
			  for ( int i = 0; i < autoCloseables.Length; i++ )
			  {
					failures[i] = new UncheckedIOException( new IOException( "unknown" ) );
					doThrow( failures[i] ).when( autoCloseables[i] ).close();
			  }

			  try
			  {
					// when
					fusionCloseable.close();
					fail( "Should have failed" );
			  }
			  catch ( UncheckedIOException e )
			  {
					// then
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: java.util.List<org.hamcrest.Matcher<? super java.io.UncheckedIOException>> matchers = new java.util.ArrayList<>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					IList<Matcher> matchers = new List<Matcher>();
					foreach ( UncheckedIOException failure in failures )
					{
						 matchers.Add( sameInstance( failure ) );
					}
					assertThat( e, anyOf( matchers ) );
			  }
		 }

		 internal static void Fill<T>( Dictionary<IndexSlot, T> map, T instance )
		 {
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					map[slot] = instance;
			  }
		 }
	}

}