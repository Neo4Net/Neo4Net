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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Index.Internal.gbptree;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.epochDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class GenericIndexKeyValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBotherSerializingToRealBytesIfFarFromThreshold()
		 public virtual void ShouldNotBotherSerializingToRealBytesIfFarFromThreshold()
		 {
			  // given
			  Layout<GenericKey, NativeIndexValue> layout = mock( typeof( Layout ) );
			  doThrow( typeof( Exception ) ).when( layout ).newKey();
			  GenericIndexKeyValidator validator = new GenericIndexKeyValidator( 120, layout );

			  // when
			  validator.Validate( new Value[]{ intValue( 10 ), epochDate( 100 ), stringValue( "abc" ) } );

			  // then no exception should have been thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvolveSerializingToRealBytesIfMayCrossThreshold()
		 public virtual void ShouldInvolveSerializingToRealBytesIfMayCrossThreshold()
		 {
			  // given
			  Layout<GenericKey, NativeIndexValue> layout = mock( typeof( Layout ) );
			  when( layout.NewKey() ).thenReturn(new CompositeGenericKey(3, SpatialSettings()));
			  GenericIndexKeyValidator validator = new GenericIndexKeyValidator( 48, layout );

			  // when
			  try
			  {
					validator.Validate( new Value[]{ intValue( 10 ), epochDate( 100 ), stringValue( "abcdefghijklmnopqrstuvw" ) } );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// then good
					assertThat( e.Message, containsString( "abcdefghijklmnopqrstuvw" ) );
					verify( layout, times( 1 ) ).newKey();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectValidationErrorsOnRandomlyGeneratedValues()
		 public virtual void ShouldReportCorrectValidationErrorsOnRandomlyGeneratedValues()
		 {
			  // given
			  int slots = Random.Next( 1, 6 );
			  int maxLength = Random.Next( 15, 30 ) * slots;
			  GenericLayout layout = new GenericLayout( slots, SpatialSettings() );
			  GenericIndexKeyValidator validator = new GenericIndexKeyValidator( maxLength, layout );
			  GenericKey key = layout.NewKey();

			  int countOk = 0;
			  int countNotOk = 0;
			  for ( int i = 0; i < 100; i++ )
			  {
					// when
					Value[] tuple = GenerateValueTuple( slots );
					bool isOk;
					try
					{
						 validator.Validate( tuple );
						 isOk = true;
						 countOk++;
					}
					catch ( System.ArgumentException )
					{
						 isOk = false;
						 countNotOk++;
					}
					int actualSize = actualSize( tuple, key );
					bool manualIsOk = actualSize <= maxLength;

					// then
					if ( manualIsOk != isOk )
					{
						 fail( format( "Validator not validating %s correctly. Manual validation on actual key resulted in %b whereas validator said %b", Arrays.ToString( tuple ), manualIsOk, isOk ) );
					}
			  }
		 }

		 private IndexSpecificSpaceFillingCurveSettingsCache SpatialSettings()
		 {
			  return new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<Neo4Net.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
		 }

		 private static int ActualSize( Value[] tuple, GenericKey key )
		 {
			  key.Initialize( 0 );
			  for ( int i = 0; i < tuple.Length; i++ )
			  {
					key.InitFromValue( i, tuple[i], NativeIndexKey.Inclusion.Neutral );
			  }
			  return key.Size();
		 }

		 private Value[] GenerateValueTuple( int slots )
		 {
			  Value[] tuple = new Value[slots];
			  for ( int j = 0; j < slots; j++ )
			  {
					tuple[j] = Random.nextValue();
			  }
			  return tuple;
		 }
	}

}