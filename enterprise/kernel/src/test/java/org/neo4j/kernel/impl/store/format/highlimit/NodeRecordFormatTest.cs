/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Kernel.impl.store.format.highlimit
{

	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using NodeRecordFormatV3_0_0 = Org.Neo4j.Kernel.impl.store.format.highlimit.v300.NodeRecordFormatV3_0_0;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class NodeRecordFormatTest
	{

		 private NodeRecordFormat _recordFormat;
		 private FixedLinkedStubPageCursor _pageCursor;
		 private ConstantIdSequence _idSequence;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _recordFormat = new NodeRecordFormat();
			  _pageCursor = new FixedLinkedStubPageCursor( 0, ( int ) ByteUnit.kibiBytes( 8 ) );
			  _idSequence = new ConstantIdSequence();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _pageCursor.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readWriteFixedReferencesRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadWriteFixedReferencesRecord()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), true, RandomFixedReference(), 0L );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferencesFormatWhenRelationshipIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferencesFormatWhenRelationshipIsMissing()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), true, Record.NULL_REFERENCE.byteValue(), 0L );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferencesFormatWhenPropertyIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferencesFormatWhenPropertyIsMissing()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, Record.NULL_REFERENCE.intValue(), true, RandomFixedReference(), 0L );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenRelationshipReferenceTooBig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenRelationshipReferenceTooBig()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, 1L << 37, true, RandomFixedReference(), 0L );

			  WriteReadRecord( source, target );

			  assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenPropertyReferenceTooBig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenPropertyReferenceTooBig()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), true, 1L << 37, 0L );

			  WriteReadRecord( source, target );

			  assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenRecordSizeIsTooSmall() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenRecordSizeIsTooSmall()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), true, RandomFixedReference(), 0L );

			  WriteReadRecord( source, target, NodeRecordFormat.FixedFormatRecordSize - 1 );

			  assertFalse( "Record should use variable length reference if format record is too small.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord()
		 {
			  NodeRecord source = new NodeRecord( 1 );
			  NodeRecord target = new NodeRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), true, RandomFixedReference(), 0L );

			  WriteReadRecord( source, target, NodeRecordFormat.FixedFormatRecordSize );

			  assertTrue( "Record should use fixed reference if can fit in format record.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readSingleUnitRecordStoredNotInFixedReferenceFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadSingleUnitRecordStoredNotInFixedReferenceFormat()
		 {
			  NodeRecord oldFormatRecord = new NodeRecord( 1 );
			  NodeRecord newFormatRecord = new NodeRecord( 1 );
			  oldFormatRecord.Initialize( true, RandomFixedReference(), true, RandomFixedReference(), 1L );

			  WriteRecordWithOldFormat( oldFormatRecord );

			  assertFalse( "This should be single unit record.", oldFormatRecord.HasSecondaryUnitId() );
			  assertFalse( "Old format is not aware about fixed references.", oldFormatRecord.UseFixedReferences );

			  _recordFormat.read( newFormatRecord, _pageCursor, RecordLoad.NORMAL, NodeRecordFormat.RECORD_SIZE );
			  VerifySameReferences( oldFormatRecord, newFormatRecord );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readDoubleUnitRecordStoredNotInFixedReferenceFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadDoubleUnitRecordStoredNotInFixedReferenceFormat()
		 {
			  NodeRecord oldFormatRecord = new NodeRecord( 1 );
			  NodeRecord newFormatRecord = new NodeRecord( 1 );
			  oldFormatRecord.Initialize( true, BigReference(), true, BigReference(), 1L );

			  WriteRecordWithOldFormat( oldFormatRecord );

			  assertTrue( "This should be double unit record.", oldFormatRecord.HasSecondaryUnitId() );
			  assertFalse( "Old format is not aware about fixed references.", oldFormatRecord.UseFixedReferences );

			  _recordFormat.read( newFormatRecord, _pageCursor, RecordLoad.NORMAL, NodeRecordFormat.RECORD_SIZE );
			  VerifySameReferences( oldFormatRecord, newFormatRecord );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRecordWithOldFormat(org.neo4j.kernel.impl.store.record.NodeRecord oldFormatRecord) throws java.io.IOException
		 private void WriteRecordWithOldFormat( NodeRecord oldFormatRecord )
		 {
			  int oldRecordSize = NodeRecordFormatV3_0_0.RECORD_SIZE;
			  NodeRecordFormatV3_0_0 recordFormatV30 = new NodeRecordFormatV3_0_0();
			  recordFormatV30.Prepare( oldFormatRecord, oldRecordSize, _idSequence );
			  recordFormatV30.Write( oldFormatRecord, _pageCursor, oldRecordSize );
			  _pageCursor.Offset = 0;
		 }

		 private void VerifySameReferences( NodeRecord recordA, NodeRecord recordB )
		 {
			  assertEquals( "Next property field should be the same", recordA.NextProp, recordB.NextProp );
			  assertEquals( "Next relationship field should be the same.", recordA.NextRel, recordB.NextRel );
			  assertEquals( "Label field should be the same", recordA.LabelField, recordB.LabelField );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(org.neo4j.kernel.impl.store.record.NodeRecord source, org.neo4j.kernel.impl.store.record.NodeRecord target) throws java.io.IOException
		 private void WriteReadRecord( NodeRecord source, NodeRecord target )
		 {
			  WriteReadRecord( source, target, NodeRecordFormat.RECORD_SIZE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(org.neo4j.kernel.impl.store.record.NodeRecord source, org.neo4j.kernel.impl.store.record.NodeRecord target, int recordSize) throws java.io.IOException
		 private void WriteReadRecord( NodeRecord source, NodeRecord target, int recordSize )
		 {
			  _recordFormat.prepare( source, recordSize, _idSequence );
			  _recordFormat.write( source, _pageCursor, recordSize );
			  _pageCursor.Offset = 0;
			  _recordFormat.read( target, _pageCursor, RecordLoad.NORMAL, recordSize );
		 }

		 private long RandomFixedReference()
		 {
			  return RandomReference( 1L << ( ( sizeof( int ) * 8 ) + ( ( sizeof( sbyte ) * 8 ) / 2 ) ) );
		 }

		 private long RandomReference( long maxValue )
		 {
			  return ThreadLocalRandom.current().nextLong(maxValue);
		 }

		 private long BigReference()
		 {
			  return 1L << 57;
		 }
	}

}