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
namespace Neo4Net.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using PropertyRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyRecordFormat;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PropertyValueRecordSizeCalculatorTest
	{
		 private static readonly int _propertyRecordSize = PropertyRecordFormat.RECORD_SIZE;
		 private const int DYNAMIC_RECORD_SIZE = 120;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludePropertyRecordSize()
		 public virtual void ShouldIncludePropertyRecordSize()
		 {
			  // given
			  PropertyValueRecordSizeCalculator calculator = NewCalculator();

			  // when
			  int size = calculator.ApplyAsInt( new Value[] { Values.of( 10 ) } );

			  // then
			  assertEquals( PropertyRecordFormat.RECORD_SIZE, size );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeDynamicRecordSizes()
		 public virtual void ShouldIncludeDynamicRecordSizes()
		 {
			  // given
			  PropertyValueRecordSizeCalculator calculator = NewCalculator();

			  // when
			  int size = calculator.ApplyAsInt( new Value[] { Values.of( String( 80 ) ), Values.of( new string[] { String( 150 ) } ) } );

			  // then
			  assertEquals( _propertyRecordSize + DYNAMIC_RECORD_SIZE + DYNAMIC_RECORD_SIZE * 2, size );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSpanMultiplePropertyRecords()
		 public virtual void ShouldSpanMultiplePropertyRecords()
		 {
			  // given
			  PropertyValueRecordSizeCalculator calculator = NewCalculator();

			  // when
			  int size = calculator.ApplyAsInt( new Value[] { Values.of( 10 ), Values.of( "test" ), Values.of( ( sbyte ) 5 ), Values.of( String( 80 ) ), Values.of( "a bit longer short string" ), Values.of( 1234567890123456789L ), Values.of( 5 ), Values.of( "value" ) } );

			  // then
			  assertEquals( _propertyRecordSize * 3 + DYNAMIC_RECORD_SIZE, size );
		 }

		 private string String( int length )
		 {
			  return Random.nextAlphaNumericString( length, length );
		 }

		 private PropertyValueRecordSizeCalculator NewCalculator()
		 {
			  return new PropertyValueRecordSizeCalculator( _propertyRecordSize, DYNAMIC_RECORD_SIZE, DYNAMIC_RECORD_SIZE - 10, DYNAMIC_RECORD_SIZE, DYNAMIC_RECORD_SIZE - 10 );
		 }
	}

}