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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using IndexCreator = Neo4Net.Graphdb.schema.IndexCreator;
	using Neo4Net.Index.@internal.gbptree;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_GEOMETRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_LOCAL_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_LOCAL_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_SHORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_STRING_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_ZONED_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_ZONED_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_ONE;

	public class GenericIndexValidationIT
	{
		 private static readonly string[] _propKeys = new string[]{ "prop0", "prop1", "prop2", "prop3", "prop4" };
		 private static readonly int _keySizeLimit = TreeNodeDynamicSize.keyValueSizeCapFromPageSize( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE );
		 private const int ESTIMATED_OVERHEAD_PER_SLOT = 2;
		 private const int WIGGLE_ROOM = 50;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(default_schema_provider, NATIVE_BTREE10.providerName());
		 public DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(default_schema_provider, NATIVE_BTREE10.providerName());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public static RandomRule Random = new RandomRule();

		 /// <summary>
		 /// Key size validation test for single type.
		 /// 
		 /// Validate that we handle index reads and writes correctly for dynamically sized values (arrays and strings)
		 /// of all different types with length close to and over the max limit for given type.
		 /// 
		 /// We do this by inserting arrays of increasing size (doubling each iteration) and when we hit the upper limit
		 /// we do binary search between the established min and max limit.
		 /// We also verify that the largest successful array length for each type is as expected because this value
		 /// is documented and if it changes, documentation also needs to change.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceSizeCapSingleValueSingleType()
		 public virtual void ShouldEnforceSizeCapSingleValueSingleType()
		 {
			  NamedDynamicValueGenerator[] dynamicValueGenerators = NamedDynamicValueGenerator.values();
			  foreach ( NamedDynamicValueGenerator generator in dynamicValueGenerators )
			  {
					string propKey = _propKeys[0] + generator.name();
					CreateIndex( propKey );

					BinarySearch binarySearch = new BinarySearch( this );
					object propValue;

					while ( !binarySearch.Finished() )
					{
						 propValue = generator.dynamicValue( binarySearch.ArrayLength );
						 long expectedNodeId = -1;

						 // Write
						 bool wasAbleToWrite = true;
						 try
						 {
								 using ( Transaction tx = Db.beginTx() )
								 {
								  Node node = Db.createNode( LABEL_ONE );
								  node.SetProperty( propKey, propValue );
								  expectedNodeId = node.Id;
								  tx.Success();
								 }
						 }
						 catch ( Exception )
						 {
							  wasAbleToWrite = false;
						 }

						 // Read
						 VerifyReadExpected( propKey, propValue, expectedNodeId, wasAbleToWrite );

						 // Progress binary search
						 binarySearch.Progress( wasAbleToWrite );
					}
					assertEquals( format( "expected longest successful array length for type %s, to be %d but was %d. " + "This is a strong indication that documentation of max limit needs to be updated.", generator.name(), generator.expectedMax, binarySearch.LongestSuccessful ), generator.expectedMax, binarySearch.LongestSuccessful );
			  }
		 }

		 private class BinarySearch
		 {
			 private readonly GenericIndexValidationIT _outerInstance;

			 public BinarySearch( GenericIndexValidationIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int LongestSuccessful;
			  internal int MinArrayLength;
			  internal int MaxArrayLength = 1;
			  internal int ArrayLength = 1;
			  internal bool FoundMaxLimit;

			  internal virtual bool Finished()
			  {
					// When arrayLength is stable on minArrayLength, our binary search for max limit is finished
					return ArrayLength == MinArrayLength;
			  }

			  internal virtual void Progress( bool wasAbleToWrite )
			  {
					if ( wasAbleToWrite )
					{
						 LongestSuccessful = Math.Max( ArrayLength, LongestSuccessful );
						 if ( !FoundMaxLimit )
						 {
							  // We continue to double the max limit until we find some upper limit
							  MinArrayLength = ArrayLength;
							  MaxArrayLength *= 2;
							  ArrayLength = MaxArrayLength;
						 }
						 else
						 {
							  // We where able to write so we can move min limit up to current array length
							  MinArrayLength = ArrayLength;
							  ArrayLength = ( MinArrayLength + MaxArrayLength ) / 2;
						 }
					}
					else
					{
						 FoundMaxLimit = true;
						 // We where not able to write so we take max limit down to current array length
						 MaxArrayLength = ArrayLength;
						 ArrayLength = ( MinArrayLength + MaxArrayLength ) / 2;
					}
			  }
		 }

		 /// <summary>
		 /// Key size validation test for mixed types in composite index.
		 /// 
		 /// Validate that we handle index reads and writes correctly for
		 /// dynamically sized values (arrays and strings) of all different
		 /// types with length close to and over the max limit for given
		 /// type.
		 /// 
		 /// We do this by trying to insert random dynamically sized values
		 /// with size in range that covers the limit, taking into account
		 /// the number of slots in the index.
		 /// Then we verify that we either
		 ///  - write successfully and are able to read value back
		 ///  - fail to write and no result is found during read
		 /// 
		 /// Even though we don't keep track of all inserted values, the
		 /// probability that we will ever generate two identical values
		 /// is, for single property boolean array which is the most likely,
		 /// (1/2)^3995. As a reference (1/2)^100 = 7.8886091e-31.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceSizeCapMixedTypes()
		 public virtual void ShouldEnforceSizeCapMixedTypes()
		 {
			  for ( int numberOfSlots = 1; numberOfSlots < 5; numberOfSlots++ )
			  {
					string[] propKeys = GeneratePropertyKeys( numberOfSlots );

					CreateIndex( propKeys );
					int keySizeLimitPerSlot = _keySizeLimit / propKeys.Length - ESTIMATED_OVERHEAD_PER_SLOT;
					int wiggleRoomPerSlot = WIGGLE_ROOM / propKeys.Length;
					SuccessAndFail successAndFail = new SuccessAndFail( this );
					for ( int i = 0; i < 1_000; i++ )
					{
						 object[] propValues = GeneratePropertyValues( propKeys, keySizeLimitPerSlot, wiggleRoomPerSlot );
						 long expectedNodeId = -1;

						 // Write
						 bool ableToWrite = true;
						 try
						 {
								 using ( Transaction tx = Db.beginTx() )
								 {
								  Node node = Db.createNode( LABEL_ONE );
								  SetProperties( propKeys, propValues, node );
								  expectedNodeId = node.Id;
								  tx.Success();
								 }
						 }
						 catch ( Exception )
						 {
							  ableToWrite = false;
						 }
						 successAndFail.AbleToWrite( ableToWrite );

						 // Read
						 verifyReadExpected( propKeys, propValues, expectedNodeId, ableToWrite );
					}
					successAndFail.VerifyBothSuccessAndFail();
			  }
		 }

		 private void SetProperties( string[] propKeys, object[] propValues, Node node )
		 {
			  for ( int propKey = 0; propKey < propKeys.Length; propKey++ )
			  {
					node.SetProperty( propKeys[propKey], propValues[propKey] );
			  }
		 }

		 private string[] GeneratePropertyKeys( int numberOfSlots )
		 {
			  string[] propKeys = new string[numberOfSlots];
			  for ( int i = 0; i < numberOfSlots; i++ )
			  {
					// Use different property keys for each iteration
					propKeys[i] = _propKeys[i] + "numberOfSlots" + numberOfSlots;
			  }
			  return propKeys;
		 }

		 private object[] GeneratePropertyValues( string[] propKeys, int keySizeLimitPerSlot, int wiggleRoomPerSlot )
		 {
			  object[] propValues = new object[propKeys.Length];
			  for ( int propKey = 0; propKey < propKeys.Length; propKey++ )
			  {
					NamedDynamicValueGenerator among = Random.among( NamedDynamicValueGenerator.values() );
					propValues[propKey] = among.dynamicValue( keySizeLimitPerSlot, wiggleRoomPerSlot );
			  }
			  return propValues;
		 }

		 private void VerifyReadExpected( string propKey, object propValue, long expectedNodeId, bool ableToWrite )
		 {
			  verifyReadExpected( new string[]{ propKey }, new object[]{ propValue }, expectedNodeId, ableToWrite );
		 }

		 private void VerifyReadExpected( string[] propKeys, object[] propValues, long expectedNodeId, bool ableToWrite )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IDictionary<string, object> values = new Dictionary<string, object>();
					for ( int propKey = 0; propKey < propKeys.Length; propKey++ )
					{
						 values[propKeys[propKey]] = propValues[propKey];
					}
					ResourceIterator<Node> nodes = Db.findNodes( LABEL_ONE, values );
					if ( ableToWrite )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( nodes.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Node node = nodes.next();
						 assertNotNull( node );
						 assertEquals( "node id", expectedNodeId, node.Id );
					}
					else
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertFalse( nodes.hasNext() );
					}
					tx.Success();
			  }
		 }

		 private void CreateIndex( params string[] propKeys )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexCreator indexCreator = Db.schema().indexFor(LABEL_ONE);
					foreach ( string propKey in propKeys )
					{
						 indexCreator = indexCreator.On( propKey );
					}
					indexCreator.Create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private class SuccessAndFail
		 {
			 private readonly GenericIndexValidationIT _outerInstance;

			 public SuccessAndFail( GenericIndexValidationIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal bool AtLeastOneSuccess;
			  internal bool AtLeastOneFail;

			  internal virtual void AbleToWrite( bool ableToWrite )
			  {
					if ( ableToWrite )
					{
						 AtLeastOneSuccess = true;
					}
					else
					{
						 AtLeastOneFail = true;
					}
			  }

			  internal virtual void VerifyBothSuccessAndFail()
			  {
					assertTrue( "not a single successful write, need to adjust parameters", AtLeastOneSuccess );
					assertTrue( "not a single failed write, need to adjust parameters", AtLeastOneFail );
			  }
		 }

		 private sealed class NamedDynamicValueGenerator
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           string(Byte.BYTES, 4036, i -> random.randomValues().nextAlphaNumericTextValue(i, i).stringValue()),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           byteArray(SIZE_NUMBER_BYTE, 4035, i -> random.randomValues().nextByteArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           shortArray(SIZE_NUMBER_SHORT, 2017, i -> random.randomValues().nextShortArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           intArray(SIZE_NUMBER_INT, 1008, i -> random.randomValues().nextIntArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           longArray(SIZE_NUMBER_LONG, 504, i -> random.randomValues().nextLongArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           floatArray(SIZE_NUMBER_FLOAT, 1008, i -> random.randomValues().nextFloatArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           doubleArray(SIZE_NUMBER_DOUBLE, 504, i -> random.randomValues().nextDoubleArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           booleanArray(SIZE_BOOLEAN, 4036, i -> random.randomValues().nextBooleanArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           charArray(Byte.BYTES, 1345, i -> random.randomValues().nextAlphaNumericTextValue(i, i).stringValue().toCharArray()),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           stringArray1(SIZE_STRING_LENGTH + 1, 1345, i -> random.randomValues().nextAlphaNumericStringArrayRaw(i, i, 1, 1)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           stringArray10(SIZE_STRING_LENGTH + 10, 336, i -> random.randomValues().nextAlphaNumericStringArrayRaw(i, i, 10, 10)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           stringArray100(SIZE_STRING_LENGTH + 100, 39, i -> random.randomValues().nextAlphaNumericStringArrayRaw(i, i, 100, 100)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           stringArray1000(SIZE_STRING_LENGTH + 1000, 4, i -> random.randomValues().nextAlphaNumericStringArrayRaw(i, i, 1000, 1000)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           dateArray(SIZE_DATE, 504, i -> random.randomValues().nextDateArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           timeArray(SIZE_ZONED_TIME, 336, i -> random.randomValues().nextTimeArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           localTimeArray(SIZE_LOCAL_TIME, 504, i -> random.randomValues().nextLocalTimeArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           dateTimeArray(SIZE_ZONED_DATE_TIME, 252, i -> random.randomValues().nextDateTimeArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           localDateTimeArray(SIZE_LOCAL_DATE_TIME, 336, i -> random.randomValues().nextLocalDateTimeArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           durationArray(SIZE_DURATION, 144, i -> random.randomValues().nextDurationArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           periodArray(SIZE_DURATION, 144, i -> random.randomValues().nextPeriodArrayRaw(i, i)),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           cartesianPointArray(SIZE_GEOMETRY, 168, i -> random.randomValues().nextCartesianPointArray(i, i).asObjectCopy()),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           cartesian3DPointArray(SIZE_GEOMETRY, 126, i -> random.randomValues().nextCartesian3DPointArray(i, i).asObjectCopy()),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           geographicPointArray(SIZE_GEOMETRY, 168, i -> random.randomValues().nextGeographicPointArray(i, i).asObjectCopy()),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           geographic3DPointArray(SIZE_GEOMETRY, 126, i -> random.randomValues().nextGeographic3DPointArray(i, i).asObjectCopy());

			  private static readonly IList<NamedDynamicValueGenerator> valueList = new List<NamedDynamicValueGenerator>();

			  static NamedDynamicValueGenerator()
			  {
				  valueList.Add( @string );
				  valueList.Add( byteArray );
				  valueList.Add( shortArray );
				  valueList.Add( intArray );
				  valueList.Add( longArray );
				  valueList.Add( floatArray );
				  valueList.Add( doubleArray );
				  valueList.Add( booleanArray );
				  valueList.Add( charArray );
				  valueList.Add( stringArray1 );
				  valueList.Add( stringArray10 );
				  valueList.Add( stringArray100 );
				  valueList.Add( stringArray1000 );
				  valueList.Add( dateArray );
				  valueList.Add( timeArray );
				  valueList.Add( localTimeArray );
				  valueList.Add( dateTimeArray );
				  valueList.Add( localDateTimeArray );
				  valueList.Add( durationArray );
				  valueList.Add( periodArray );
				  valueList.Add( cartesianPointArray );
				  valueList.Add( cartesian3DPointArray );
				  valueList.Add( geographicPointArray );
				  valueList.Add( geographic3DPointArray );
			  }

			  public enum InnerEnum
			  {
				  @string,
				  byteArray,
				  shortArray,
				  intArray,
				  longArray,
				  floatArray,
				  doubleArray,
				  booleanArray,
				  charArray,
				  stringArray1,
				  stringArray10,
				  stringArray100,
				  stringArray1000,
				  dateArray,
				  timeArray,
				  localTimeArray,
				  dateTimeArray,
				  localDateTimeArray,
				  durationArray,
				  periodArray,
				  cartesianPointArray,
				  cartesian3DPointArray,
				  geographicPointArray,
				  geographic3DPointArray
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private NamedDynamicValueGenerator( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal readonly int singleArrayEntrySize;
			  internal readonly DynamicValueGenerator generator;
			  internal readonly int expectedMax;

			  internal NamedDynamicValueGenerator( string name, InnerEnum innerEnum, int singleArrayEntrySize, int expectedLongestArrayLength, DynamicValueGenerator generator )
			  {
					this._singleArrayEntrySize = singleArrayEntrySize;
					this._expectedMax = expectedLongestArrayLength;
					this._generator = generator;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal object DynamicValue( int length )
			  {
					return _generator.dynamicValue( length );
			  }

			  internal object DynamicValue( int keySizeLimit, int wiggleRoom )
			  {
					int lowLimit = lowLimit( keySizeLimit, wiggleRoom, _singleArrayEntrySize );
					int highLimit = highLimit( keySizeLimit, wiggleRoom, _singleArrayEntrySize );
					return DynamicValue( Random.intBetween( lowLimit, highLimit ) );
			  }

			  internal int LowLimit( int keySizeLimit, int wiggleRoom, int singleEntrySize )
			  {
					return ( keySizeLimit - wiggleRoom ) / singleEntrySize;
			  }

			  internal int HighLimit( int keySizeLimit, int wiggleRoom, int singleEntrySize )
			  {
					return ( keySizeLimit + wiggleRoom ) / singleEntrySize;
			  }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//			  private interface DynamicValueGenerator
	//		  {
	//				Object dynamicValue(int arrayLength);
	//		  }

			 public static IList<NamedDynamicValueGenerator> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static NamedDynamicValueGenerator valueOf( string name )
			 {
				 foreach ( NamedDynamicValueGenerator enumInstance in NamedDynamicValueGenerator.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}