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
namespace Neo4Net.Kernel.impl.store
{
	using Pair = org.eclipse.collections.api.tuple.Pair;
	using Assume = org.junit.Assume;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.tuple.Tuples.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class TestPropertyBlocks : AbstractNeo4jTestCase
	{
		 protected internal override bool RestartGraphDbBetweenTests()
		 {
			  return true;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void simpleAddIntegers()
		 public virtual void SimpleAddIntegers()
		 {
			  long inUseBefore = PropertyRecordsInUse();
			  Node node = GraphDb.createNode();

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs; i++ )
			  {
					node.SetProperty( "prop" + i, i );
					assertEquals( i, node.GetProperty( "prop" + i ) );
			  }

			  NewTransaction();
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs; i++ )
			  {
					assertEquals( i, node.GetProperty( "prop" + i ) );
			  }

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs; i++ )
			  {
					assertEquals( i, node.RemoveProperty( "prop" + i ) );
					assertFalse( node.HasProperty( "prop" + i ) );
			  }
			  Commit();
			  assertEquals( inUseBefore, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void simpleAddDoubles()
		 public virtual void SimpleAddDoubles()
		 {
			  long inUseBefore = PropertyRecordsInUse();
			  Node node = GraphDb.createNode();

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs / 2; i++ )
			  {
					node.SetProperty( "prop" + i, i * -1.0 );
					assertEquals( i * -1.0, node.GetProperty( "prop" + i ) );
			  }

			  NewTransaction();
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs / 2; i++ )
			  {
					assertEquals( i * -1.0, node.GetProperty( "prop" + i ) );
			  }

			  for ( int i = 0; i < PropertyType.PayloadSizeLongs / 2; i++ )
			  {
					assertEquals( i * -1.0, node.RemoveProperty( "prop" + i ) );
					assertFalse( node.HasProperty( "prop" + i ) );
			  }
			  Commit();
			  assertEquals( inUseBefore, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteEverythingInMiddleRecord()
		 public virtual void DeleteEverythingInMiddleRecord()
		 {
			  long inUseBefore = PropertyRecordsInUse();
			  Node node = GraphDb.createNode();

			  for ( int i = 0; i < 3 * PropertyType.PayloadSizeLongs; i++ )
			  {
					node.SetProperty( "shortString" + i, i.ToString() );
			  }
			  NewTransaction();
			  assertEquals( inUseBefore + 3, PropertyRecordsInUse() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.Pair<String, Object>> middleRecordProps = getPropertiesFromRecord(1);
			  IList<Pair<string, object>> middleRecordProps = GetPropertiesFromRecord( 1 );
			  middleRecordProps.ForEach(nameAndValue =>
			  {
				string name = nameAndValue.One;
				object value = nameAndValue.Two;
				assertEquals( value, node.RemoveProperty( name ) );
			  });

			  NewTransaction();

			  assertEquals( inUseBefore + 2, PropertyRecordsInUse() );
			  middleRecordProps.ForEach( nameAndValue => assertFalse( node.HasProperty( nameAndValue.One ) ) );
			  GetPropertiesFromRecord( 0 ).ForEach(nameAndValue =>
			  {
				string name = nameAndValue.One;
				object value = nameAndValue.Two;
				assertEquals( value, node.RemoveProperty( name ) );
			  });
			  GetPropertiesFromRecord( 2 ).ForEach(nameAndValue =>
			  {
				string name = nameAndValue.One;
				object value = nameAndValue.Two;
				assertEquals( value, node.RemoveProperty( name ) );
			  });
		 }

		 private IList<Pair<string, object>> GetPropertiesFromRecord( long recordId )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.record.PropertyRecord record = propertyStore().getRecord(recordId, propertyStore().newRecord(), org.neo4j.kernel.impl.store.record.RecordLoad.FORCE);
			  PropertyRecord record = PropertyStore().getRecord(recordId, PropertyStore().newRecord(), RecordLoad.FORCE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.Pair<String, Object>> props = new java.util.ArrayList<>();
			  IList<Pair<string, object>> props = new List<Pair<string, object>>();
			  record.forEach(block =>
			  {
				object value = PropertyStore().getValue(block).asObject();
				string name = PropertyStore().PropertyKeyTokenStore.getToken(block.KeyIndexId).name();
				props.Add( pair( name, value ) );
			  });
			  return props;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void largeTx()
		 public virtual void LargeTx()
		 {
			  Node node = GraphDb.createNode();

			  node.SetProperty( "anchor", "hi" );
			  for ( int i = 0; i < 255; i++ )
			  {
					node.SetProperty( "foo", 1 );
					node.RemoveProperty( "foo" );
			  }
			  Commit();
		 }

		 /*
		  * Creates a PropertyRecord, fills it up, removes something and
		  * adds something that should fit.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteAndAddToFullPropertyRecord()
		 public virtual void DeleteAndAddToFullPropertyRecord()
		 {
			  // Fill it up, each integer is one block
			  Node node = GraphDb.createNode();
			  for ( int i = 0; i < PropertyType.PayloadSizeLongs; i++ )
			  {
					node.SetProperty( "prop" + i, i );
			  }

			  NewTransaction();

			  // Remove all but one and add one
			  for ( int i = 0; i < PropertyType.PayloadSizeLongs - 1; i++ )
			  {
					assertEquals( i, node.RemoveProperty( "prop" + i ) );
			  }
			  node.SetProperty( "profit", 5 );

			  NewTransaction();

			  // Verify
			  int remainingProperty = PropertyType.PayloadSizeLongs - 1;
			  assertEquals( remainingProperty, node.GetProperty( "prop" + remainingProperty ) );
			  assertEquals( 5, node.GetProperty( "profit" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPacking()
		 public virtual void CheckPacking()
		 {
			  long inUseBefore = PropertyRecordsInUse();

			  // Fill it up, each integer is one block
			  Node node = GraphDb.createNode();
			  node.SetProperty( "prop0", 0 );
			  NewTransaction();

			  // One record must have been added
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Since integers take up one block, adding the remaining should not
			  // create a new record.
			  for ( int i = 1; i < PropertyType.PayloadSizeLongs; i++ )
			  {
					node.SetProperty( "prop" + i, i );
			  }
			  NewTransaction();

			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Removing one and adding one of the same size should not create a new
			  // record.
			  assertEquals( 0, node.RemoveProperty( "prop0" ) );
			  node.SetProperty( "prop-1", -1 );
			  NewTransaction();

			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Removing two that take up 1 block and adding one that takes up 2
			  // should not create a new record.
			  assertEquals( -1, node.RemoveProperty( "prop-1" ) );
			  // Hopefully prop1 exists, meaning payload is at least 16
			  assertEquals( 1, node.RemoveProperty( "prop1" ) );
			  // A double value should do the trick
			  node.SetProperty( "propDouble", 1.0 );
			  NewTransaction();

			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Adding just one now should create a new property record.
			  node.SetProperty( "prop-2", -2 );
			  NewTransaction();
			  assertEquals( inUseBefore + 2, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void substituteOneLargeWithManySmallPropBlocks()
		 public virtual void SubstituteOneLargeWithManySmallPropBlocks()
		 {
			  Node node = GraphDb.createNode();
			  long inUseBefore = PropertyRecordsInUse();
			  /*
			   * Fill up with doubles and the rest with ints - we assume
			   * the former take up two blocks, the latter 1.
			   */
			  for ( int i = 0; i < PropertyType.PayloadSizeLongs / 2; i++ )
			  {
					node.SetProperty( "double" + i, i * 1.0 );
			  }
			  /*
			   * I know this is stupid in that it is executed 0 or 1 times but it
			   * is easier to maintain and change for different payload sizes.
			   */
			  for ( int i = 0; i < PropertyType.PayloadSizeLongs % 2; i++ )
			  {
					node.SetProperty( "int" + i, i );
			  }
			  NewTransaction();

			  // Just checking that the assumptions above is correct
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // We assume at least one double has been added
			  node.RemoveProperty( "double0" );
			  NewTransaction();
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Do the actual substitution, check that no record is created
			  node.SetProperty( "int-1", -1 );
			  node.SetProperty( "int-2", -2 );
			  NewTransaction();
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Finally, make sure we actually are with a full prop record
			  node.SetProperty( "int-3", -3 );
			  NewTransaction();
			  assertEquals( inUseBefore + 2, PropertyRecordsInUse() );
		 }

		 /*
		  * Adds at least 3 1-block properties and removes the first and third.
		  * Adds a 2-block property and checks if it is added in the same record.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBlockDefragmentationWithTwoSpaces()
		 public virtual void TestBlockDefragmentationWithTwoSpaces()
		 {
			  Assume.assumeTrue( PropertyType.PayloadSizeLongs > 2 );
			  Node node = GraphDb.createNode();
			  long inUseBefore = PropertyRecordsInUse();

			  int stuffedIntegers = 0;
			  for ( ; stuffedIntegers < PropertyType.PayloadSizeLongs; stuffedIntegers++ )
			  {
					node.SetProperty( "int" + stuffedIntegers, stuffedIntegers );
			  }

			  // Basic check that integers take up one (8 byte) block.
			  assertEquals( stuffedIntegers, PropertyType.PayloadSizeLongs );
			  NewTransaction();

			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  // Remove first and third
			  node.RemoveProperty( "int0" );
			  node.RemoveProperty( "int2" );
			  NewTransaction();
			  // Add the two block thing.
			  node.SetProperty( "theDouble", 1.0 );
			  NewTransaction();

			  // Let's make sure everything is in one record and with proper values.
			  assertEquals( inUseBefore + 1, PropertyRecordsInUse() );

			  assertNull( node.GetProperty( "int0", null ) );
			  assertEquals( 1, node.GetProperty( "int1" ) );
			  assertNull( node.GetProperty( "int2", null ) );
			  for ( int i = 3; i < stuffedIntegers; i++ )
			  {
					assertEquals( i, node.GetProperty( "int" + i ) );
			  }
			  assertEquals( 1.0, node.GetProperty( "theDouble" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkDeletesRemoveRecordsWhenProper()
		 public virtual void CheckDeletesRemoveRecordsWhenProper()
		 {
			  Node node = GraphDb.createNode();
			  long recordsInUseAtStart = PropertyRecordsInUse();

			  int stuffedBooleans = 0;
			  for ( ; stuffedBooleans < PropertyType.PayloadSizeLongs; stuffedBooleans++ )
			  {
					node.SetProperty( "boolean" + stuffedBooleans, stuffedBooleans % 2 == 0 );
			  }
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.SetProperty( "theExraOne", true );
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );

			  for ( int i = 0; i < stuffedBooleans; i++ )
			  {
					assertEquals( Convert.ToBoolean( i % 2 == 0 ), node.RemoveProperty( "boolean" + i ) );
			  }
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  for ( int i = 0; i < stuffedBooleans; i++ )
			  {
					assertFalse( node.HasProperty( "boolean" + i ) );
			  }
			  assertEquals( true, node.GetProperty( "theExraOne" ) );
		 }

		 /*
		  * Creates 3 records and deletes stuff from the middle one. Assumes that a 2 character
		  * string that is a number fits in one block.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessWithMiddleRecordDeletes()
		 public virtual void TestMessWithMiddleRecordDeletes()
		 {
			  Node node = GraphDb.createNode();
			  long recordsInUseAtStart = PropertyRecordsInUse();

			  int stuffedShortStrings = 0;
			  for ( ; stuffedShortStrings < 3 * PropertyType.PayloadSizeLongs; stuffedShortStrings++ )
			  {
					node.SetProperty( "shortString" + stuffedShortStrings, stuffedShortStrings.ToString() );
			  }
			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 3, PropertyRecordsInUse() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.Pair<String, Object>> middleRecordProps = getPropertiesFromRecord(1);
			  IList<Pair<string, object>> middleRecordProps = GetPropertiesFromRecord( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.tuple.Pair<String, Object> secondBlockInMiddleRecord = middleRecordProps.get(1);
			  Pair<string, object> secondBlockInMiddleRecord = middleRecordProps[1];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.tuple.Pair<String, Object> thirdBlockInMiddleRecord = middleRecordProps.get(2);
			  Pair<string, object> thirdBlockInMiddleRecord = middleRecordProps[2];

			  assertEquals( secondBlockInMiddleRecord.Two, node.RemoveProperty( secondBlockInMiddleRecord.One ) );
			  assertEquals( thirdBlockInMiddleRecord.Two, node.RemoveProperty( thirdBlockInMiddleRecord.One ) );

			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 3, PropertyRecordsInUse() );

			  for ( int i = 0; i < stuffedShortStrings; i++ )
			  {
					if ( secondBlockInMiddleRecord.Two.Equals( i.ToString() ) || thirdBlockInMiddleRecord.Two.Equals(i.ToString()) )
					{
						 assertFalse( node.HasProperty( "shortString" + i ) );
					}
					else
					{
						 assertEquals( i.ToString(), node.GetProperty("shortString" + i) );
					}
			  }
			  // Start deleting stuff. First, all the middle property blocks
			  int deletedProps = 0;

			  foreach ( Pair<string, object> prop in middleRecordProps )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String name = prop.getOne();
					string name = prop.One;
					if ( node.HasProperty( name ) )
					{
						 deletedProps++;
						 node.RemoveProperty( name );
					}
			  }

			  assertEquals( PropertyType.PayloadSizeLongs - 2, deletedProps );

			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );

			  middleRecordProps.ForEach( nameAndValue => assertFalse( node.HasProperty( nameAndValue.One ) ) );
			  GetPropertiesFromRecord( 0 ).ForEach(nameAndValue =>
			  {
				string name = nameAndValue.One;
				object value = nameAndValue.Two;
				assertEquals( value, node.RemoveProperty( name ) );
			  });
			  GetPropertiesFromRecord( 2 ).ForEach(nameAndValue =>
			  {
				string name = nameAndValue.One;
				object value = nameAndValue.Two;
				assertEquals( value, node.RemoveProperty( name ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixAndPackDifferentTypes()
		 public virtual void MixAndPackDifferentTypes()
		 {
			  Node node = GraphDb.createNode();
			  long recordsInUseAtStart = PropertyRecordsInUse();

			  int stuffedShortStrings = 0;
			  for ( ; stuffedShortStrings < PropertyType.PayloadSizeLongs; stuffedShortStrings++ )
			  {
					node.SetProperty( "shortString" + stuffedShortStrings, stuffedShortStrings.ToString() );
			  }
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.RemoveProperty( "shortString0" );
			  node.RemoveProperty( "shortString2" );
			  node.SetProperty( "theDoubleOne", -1.0 );
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );
			  for ( int i = 0; i < stuffedShortStrings; i++ )
			  {
					if ( i == 0 )
					{
						 assertFalse( node.HasProperty( "shortString" + i ) );
					}
					else if ( i == 2 )
					{
						 assertFalse( node.HasProperty( "shortString" + i ) );
					}
					else
					{
						 assertEquals( i.ToString(), node.GetProperty("shortString" + i) );
					}
			  }
			  assertEquals( -1.0, node.GetProperty( "theDoubleOne" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAdditionsHappenAtTheFirstRecordIfFits1()
		 public virtual void TestAdditionsHappenAtTheFirstRecordIfFits1()
		 {
			  Node node = GraphDb.createNode();
			  long recordsInUseAtStart = PropertyRecordsInUse();

			  node.SetProperty( "int1", 1 );
			  node.SetProperty( "double1", 1.0 );
			  node.SetProperty( "int2", 2 );
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.RemoveProperty( "double1" );
			  NewTransaction();
			  node.SetProperty( "double2", 1.0 );
			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.SetProperty( "paddingBoolean", false );
			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAdditionHappensInTheMiddleIfItFits()
		 public virtual void TestAdditionHappensInTheMiddleIfItFits()
		 {
			  Node node = GraphDb.createNode();

			  long recordsInUseAtStart = PropertyRecordsInUse();

			  node.SetProperty( "int1", 1 );
			  node.SetProperty( "double1", 1.0 );
			  node.SetProperty( "int2", 2 );

			  int stuffedShortStrings = 0;
			  for ( ; stuffedShortStrings < PropertyType.PayloadSizeLongs; stuffedShortStrings++ )
			  {
					node.SetProperty( "shortString" + stuffedShortStrings, stuffedShortStrings.ToString() );
			  }
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );

			  node.RemoveProperty( "shortString" + 1 );
			  node.SetProperty( "int3", 3 );

			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangePropertyType()
		 public virtual void TestChangePropertyType()
		 {
			  Node node = GraphDb.createNode();

			  long recordsInUseAtStart = PropertyRecordsInUse();

			  int stuffedShortStrings = 0;
			  for ( ; stuffedShortStrings < PropertyType.PayloadSizeLongs; stuffedShortStrings++ )
			  {
					node.SetProperty( "shortString" + stuffedShortStrings, stuffedShortStrings.ToString() );
			  }
			  NewTransaction();

			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.SetProperty( "shortString1", 1.0 );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRevertOverflowingChange()
		 public virtual void TestRevertOverflowingChange()
		 {
			  Relationship rel = GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), RelationshipType.withName("INVALIDATES"));

			  long recordsInUseAtStart = PropertyRecordsInUse();
			  long valueRecordsInUseAtStart = DynamicArrayRecordsInUse();

			  rel.SetProperty( "theByte", ( sbyte ) - 8 );
			  rel.SetProperty( "theDoubleThatGrows", Math.PI );
			  rel.SetProperty( "theInteger", -444345 );

			  rel.SetProperty( "theDoubleThatGrows", new long[] { 1L << 63, 1L << 63, 1L << 63 } );

			  rel.SetProperty( "theDoubleThatGrows", Math.E );

			  // When
			  NewTransaction();

			  // Then
			  /*
			   * The following line should pass if we have packing on property block
			   * size shrinking.
			   */
			  // assertEquals( recordsInUseAtStart + 1, propertyRecordsInUse() );
			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );
			  assertEquals( valueRecordsInUseAtStart, DynamicArrayRecordsInUse() );

			  assertEquals( ( sbyte ) - 8, rel.GetProperty( "theByte" ) );
			  assertEquals( -444345, rel.GetProperty( "theInteger" ) );
			  assertEquals( Math.E, rel.GetProperty( "theDoubleThatGrows" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testYoYoArrayPropertyWithinTx()
		 public virtual void TestYoYoArrayPropertyWithinTx()
		 {
			  TestYoyoArrayBase( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testYoYoArrayPropertyOverTxs()
		 public virtual void TestYoYoArrayPropertyOverTxs()
		 {
			  TestYoyoArrayBase( true );
		 }

		 private void TestYoyoArrayBase( bool withNewTx )
		 {
			  Relationship rel = GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), RelationshipType.withName("LOCKS"));

			  long recordsInUseAtStart = PropertyRecordsInUse();
			  long valueRecordsInUseAtStart = DynamicArrayRecordsInUse();

			  IList<long> theYoyoData = new List<long>();
			  for ( int i = 0; i < PropertyType.PayloadSizeLongs - 1; i++ )
			  {
					theYoyoData.Add( 1L << 63 );
					long?[] value = theYoyoData.ToArray();
					rel.SetProperty( "yoyo", value );
					if ( withNewTx )
					{
						 NewTransaction();
						 assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );
						 assertEquals( valueRecordsInUseAtStart, DynamicArrayRecordsInUse() );
					}
			  }

			  theYoyoData.Add( 1L << 63 );
			  long?[] value = theYoyoData.ToArray();
			  rel.SetProperty( "yoyo", value );

			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );
			  assertEquals( valueRecordsInUseAtStart + 1, DynamicArrayRecordsInUse() );
			  rel.SetProperty( "filler", new long[] { 1L << 63, 1L << 63, 1L << 63 } );
			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveZigZag()
		 public virtual void TestRemoveZigZag()
		 {
			  Relationship rel = GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), RelationshipType.withName("LOCKS"));

			  long recordsInUseAtStart = PropertyRecordsInUse();

			  int propRecCount = 1;
			  for ( ; propRecCount <= 3; propRecCount++ )
			  {
					for ( int i = 1; i <= PropertyType.PayloadSizeLongs; i++ )
					{
						 rel.SetProperty( "int" + ( propRecCount * 10 + i ), propRecCount * 10 + i );
					}
			  }

			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 3, PropertyRecordsInUse() );

			  for ( int i = 1; i <= PropertyType.PayloadSizeLongs; i++ )
			  {
					for ( int j = 1; j < propRecCount; j++ )
					{
						 assertEquals( j * 10 + i, rel.RemoveProperty( "int" + ( j * 10 + i ) ) );
						 if ( i == PropertyType.PayloadSize - 1 && j != propRecCount - 1 )
						 {
							  assertEquals( recordsInUseAtStart + ( propRecCount - j ), PropertyRecordsInUse() );
						 }
						 else if ( i == PropertyType.PayloadSize - 1 && j == propRecCount - 1 )
						 {
							  assertEquals( recordsInUseAtStart, PropertyRecordsInUse() );
						 }
						 else
						 {
							  assertEquals( recordsInUseAtStart + 3, PropertyRecordsInUse() );
						 }
					}
			  }
			  for ( int i = 1; i <= PropertyType.PayloadSizeLongs; i++ )
			  {
					for ( int j = 1; j < propRecCount; j++ )
					{
						 assertFalse( rel.HasProperty( "int" + ( j * 10 + i ) ) );
					}
			  }
			  NewTransaction();

			  for ( int i = 1; i <= PropertyType.PayloadSizeLongs; i++ )
			  {
					for ( int j = 1; j < propRecCount; j++ )
					{
						 assertFalse( rel.HasProperty( "int" + ( j * 10 + i ) ) );
					}
			  }
			  assertEquals( recordsInUseAtStart, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetWithSameValue()
		 public virtual void TestSetWithSameValue()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "rev_pos", "40000633e7ad67ff" );
			  assertEquals( "40000633e7ad67ff", node.GetProperty( "rev_pos" ) );
			  NewTransaction();
			  node.SetProperty( "rev_pos", "40000633e7ad67ef" );
			  assertEquals( "40000633e7ad67ef", node.GetProperty( "rev_pos" ) );
		 }

		 private void TestStringYoYoBase( bool withNewTx )
		 {
			  Node node = GraphDb.createNode();

			  long recordsInUseAtStart = PropertyRecordsInUse();
			  long valueRecordsInUseAtStart = DynamicStringRecordsInUse();

			  string data = "0";
			  int counter = 1;
			  while ( DynamicStringRecordsInUse() == valueRecordsInUseAtStart )
			  {
					data += counter++;
					node.SetProperty( "yoyo", data );
					if ( withNewTx )
					{
						 NewTransaction();
						 assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );
					}
			  }

			  data = data.Substring( 0, data.Length - 2 );
			  node.SetProperty( "yoyo", data );

			  NewTransaction();

			  assertEquals( valueRecordsInUseAtStart, DynamicStringRecordsInUse() );
			  assertEquals( recordsInUseAtStart + 1, PropertyRecordsInUse() );

			  node.SetProperty( "fillerBoolean", true );

			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringYoYoWithTx()
		 public virtual void TestStringYoYoWithTx()
		 {
			  TestStringYoYoBase( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveFirstOfTwo()
		 public virtual void TestRemoveFirstOfTwo()
		 {
			  Node node = GraphDb.createNode();

			  long recordsInUseAtStart = PropertyRecordsInUse();

			  node.SetProperty( "Double1", 1.0 );
			  node.SetProperty( "Int1", 1 );
			  node.SetProperty( "Int2", 2 );
			  node.SetProperty( "Int2", 1.2 );
			  node.SetProperty( "Int2", 2 );
			  node.SetProperty( "Double3", 3.0 );
			  NewTransaction();
			  assertEquals( recordsInUseAtStart + 2, PropertyRecordsInUse() );
			  assertEquals( 1.0, node.GetProperty( "Double1" ) );
			  assertEquals( 1, node.GetProperty( "Int1" ) );
			  assertEquals( 2, node.GetProperty( "Int2" ) );
			  assertEquals( 3.0, node.GetProperty( "Double3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteNodeWithNewPropertyRecordShouldFreeTheNewRecord()
		 public virtual void DeleteNodeWithNewPropertyRecordShouldFreeTheNewRecord()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long propcount = getIdGenerator(org.neo4j.kernel.impl.store.id.IdType.PROPERTY).getNumberOfIdsInUse();
			  long propcount = GetIdGenerator( IdType.PROPERTY ).NumberOfIdsInUse;
			  Node node = GraphDb.createNode();
			  node.SetProperty( "one", 1 );
			  node.SetProperty( "two", 2 );
			  node.SetProperty( "three", 3 );
			  node.SetProperty( "four", 4 );
			  NewTransaction();
			  assertEquals( "Invalid assumption: property record count", propcount + 1, PropertyRecordsInUse() );
			  node.SetProperty( "final", 666 );
			  NewTransaction();
			  assertEquals( "Invalid assumption: property record count", propcount + 2, PropertyRecordsInUse() );
			  node.Delete();
			  Commit();
			  assertEquals( "All property records should be freed", propcount, PropertyRecordsInUse() );
		 }
	}

}