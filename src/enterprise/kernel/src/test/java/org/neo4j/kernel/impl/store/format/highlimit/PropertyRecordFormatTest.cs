/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using ByteUnit = Neo4Net.Io.ByteUnit;
	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;
	using PropertyRecordFormatV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.PropertyRecordFormatV3_0_0;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PropertyRecordFormatTest
	{
		 private const int DATA_SIZE = 100;
		 private static readonly long _tooBigReference = 1L << ( ( sizeof( int ) * 8 ) + ( ( sizeof( sbyte ) * 8 ) * 3 ) );

		 private PropertyRecordFormat _recordFormat;
		 private StubPageCursor _pageCursor;
		 private ConstantIdSequence _idSequence;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _recordFormat = new PropertyRecordFormat();
			  _pageCursor = new StubPageCursor( 0, ( int ) ByteUnit.kibiBytes( 8 ) );
			  _idSequence = new ConstantIdSequence();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _pageCursor.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndReadRecordWithRelativeReferences()
		 public virtual void WriteAndReadRecordWithRelativeReferences()
		 {
			  int recordSize = _recordFormat.getRecordSize( new IntStoreHeader( DATA_SIZE ) );
			  long recordId = 0xF1F1F1F1F1F1L;
			  int recordOffset = _pageCursor.Offset;

			  PropertyRecord record = CreateRecord( _recordFormat, recordId );
			  _recordFormat.write( record, _pageCursor, recordSize );

			  PropertyRecord recordFromStore = _recordFormat.newRecord();
			  recordFromStore.Id = recordId;
			  ResetCursor( _pageCursor, recordOffset );
			  _recordFormat.read( recordFromStore, _pageCursor, RecordLoad.NORMAL, recordSize );

			  // records should be the same
			  assertEquals( record.NextProp, recordFromStore.NextProp );
			  assertEquals( record.PrevProp, recordFromStore.PrevProp );

			  // now lets try to read same data into a record with different id - we should get different absolute references
			  ResetCursor( _pageCursor, recordOffset );
			  PropertyRecord recordWithOtherId = _recordFormat.newRecord();
			  recordWithOtherId.Id = 1L;
			  _recordFormat.read( recordWithOtherId, _pageCursor, RecordLoad.NORMAL, recordSize );

			  VerifyDifferentReferences( record, recordWithOtherId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readWriteFixedReferencesRecord()
		 public virtual void ReadWriteFixedReferencesRecord()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenNextPropertyIsMissing()
		 public virtual void UseFixedReferenceFormatWhenNextPropertyIsMissing()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), Record.NULL_REFERENCE.byteValue() );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenPreviousPropertyIsMissing()
		 public virtual void UseFixedReferenceFormatWhenPreviousPropertyIsMissing()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, Record.NULL_REFERENCE.intValue(), RandomFixedReference() );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenPreviousPropertyReferenceTooBig()
		 public virtual void UseVariableLengthFormatWhenPreviousPropertyReferenceTooBig()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, _tooBigReference, RandomFixedReference() );

			  WriteReadRecord( source, target );

			  assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenNextPropertyReferenceTooBig()
		 public virtual void UseVariableLengthFormatWhenNextPropertyReferenceTooBig()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), _tooBigReference );

			  WriteReadRecord( source, target );

			  assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenRecordSizeIsTooSmall()
		 public virtual void UseVariableLengthFormatWhenRecordSizeIsTooSmall()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target, PropertyRecordFormat.FixedFormatRecordSize - 1 );

			  assertFalse( "Record should use variable length reference if format record is too small.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord()
		 public virtual void UseFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord()
		 {
			  PropertyRecord source = new PropertyRecord( 1 );
			  PropertyRecord target = new PropertyRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target, PropertyRecordFormat.FixedFormatRecordSize );

			  assertTrue( "Record should use fixed reference if can fit in format record.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readSingleUnitRecordStoredNotInFixedReferenceFormat()
		 public virtual void ReadSingleUnitRecordStoredNotInFixedReferenceFormat()
		 {
			  PropertyRecord oldFormatRecord = new PropertyRecord( 1 );
			  PropertyRecord newFormatRecord = new PropertyRecord( 1 );
			  oldFormatRecord.Initialize( true, RandomFixedReference(), RandomFixedReference() );

			  WriteRecordWithOldFormat( oldFormatRecord );

			  assertFalse( "This should be single unit record.", oldFormatRecord.HasSecondaryUnitId() );
			  assertFalse( "Old format is not aware about fixed references.", oldFormatRecord.UseFixedReferences );

			  _recordFormat.read( newFormatRecord, _pageCursor, RecordLoad.NORMAL, PropertyRecordFormat.RECORD_SIZE );
			  VerifySameReferences( oldFormatRecord, newFormatRecord );
		 }

		 private void WriteRecordWithOldFormat( PropertyRecord oldFormatRecord )
		 {
			  int oldRecordSize = PropertyRecordFormatV3_0_0.RECORD_SIZE;
			  PropertyRecordFormatV3_0_0 recordFormatV30 = new PropertyRecordFormatV3_0_0();
			  recordFormatV30.prepare( oldFormatRecord, oldRecordSize, _idSequence );
			  recordFormatV30.Write( oldFormatRecord, _pageCursor, oldRecordSize );
			  _pageCursor.Offset = 0;
		 }

		 private void VerifySameReferences( PropertyRecord recordA, PropertyRecord recordB )
		 {
			  assertEquals( recordA.NextProp, recordB.NextProp );
			  assertEquals( recordA.PrevProp, recordB.PrevProp );
		 }

		 private void VerifyDifferentReferences( PropertyRecord recordA, PropertyRecord recordB )
		 {
			  assertNotEquals( recordA.NextProp, recordB.NextProp );
			  assertNotEquals( recordA.PrevProp, recordB.PrevProp );
		 }

		 private void WriteReadRecord( PropertyRecord source, PropertyRecord target )
		 {
			  WriteReadRecord( source, target, PropertyRecordFormat.RECORD_SIZE );
		 }

		 private void WriteReadRecord( PropertyRecord source, PropertyRecord target, int recordSize )
		 {
			  _recordFormat.prepare( source, recordSize, _idSequence );
			  _recordFormat.write( source, _pageCursor, recordSize );
			  _pageCursor.Offset = 0;
			  _recordFormat.read( target, _pageCursor, RecordLoad.NORMAL, recordSize );
		 }

		 private long RandomFixedReference()
		 {
			  return RandomReference( 1L << ( ( sizeof( int ) * 8 ) + ( ( sizeof( sbyte ) * 8 ) * 2 ) ) );
		 }

		 private long RandomReference( long maxValue )
		 {
			  return ThreadLocalRandom.current().nextLong(maxValue);
		 }

		 private void ResetCursor( StubPageCursor cursor, int recordOffset )
		 {
			  cursor.Offset = recordOffset;
		 }

		 private PropertyRecord CreateRecord( PropertyRecordFormat format, long recordId )
		 {
			  PropertyRecord record = format.NewRecord();
			  record.InUse = true;
			  record.Id = recordId;
			  record.NextProp = 1L;
			  record.PrevProp = ( int.MaxValue + 1L ) << ( sizeof( sbyte ) * 8 ) * 3;
			  return record;
		 }
	}

}