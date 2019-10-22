using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{
	using Test = org.junit.Test;


	using ByteUnit = Neo4Net.Io.ByteUnit;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.RecordPageLocationCalculator.offsetForId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.BaseRecordFormat.IN_USE_BIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.highlimit.BaseHighLimitRecordFormat.NULL;

	public class RelationshipRecordFormatTest
	{
		private bool InstanceFieldsInitialized = false;

		public RelationshipRecordFormatTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_recordSize = _format.getRecordSize( NO_STORE_HEADER );
		}

		 private readonly RelationshipRecordFormat _format = new RelationshipRecordFormat();
		 private int _recordSize;
		 private ConstantIdSequence _idSequence = new ConstantIdSequence();
		 private readonly FixedLinkedStubPageCursor cursor = new FixedLinkedStubPageCursorAnonymousInnerClass();

		 private class FixedLinkedStubPageCursorAnonymousInnerClass : FixedLinkedStubPageCursor
		 {
			 public FixedLinkedStubPageCursorAnonymousInnerClass() : base(0, (int) ByteUnit.kibiBytes(4))
			 {
			 }

			 public override bool next( long pageId )
			 {
				  // We're going to use this cursor in an environment where in all genericness this cursor
				  // is one that can be moved around to other pages. That's not possible with this stub cursor,
				  // however we know that in this test we'll stay on page 0 even if there are calls to next(pageId)
				  // which are part of the format code.
				  assertEquals( 0, pageId );
				  return true;
			 }

			 public override bool next()
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndReadRecordWithRelativeReferences() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriteAndReadRecordWithRelativeReferences()
		 {
			  long recordId = 0xF1F1F1F1F1F1L;
			  int recordOffset = cursor.Offset;

			  RelationshipRecord record = CreateCompactRecord( _format, recordId, false, false );
			  RelationshipRecord firstInSecondChain = CreateCompactRecord( _format, recordId, false, true );
			  RelationshipRecord firstInFirstChain = CreateCompactRecord( _format, recordId, true, false );
			  RelationshipRecord firstInBothChains = CreateCompactRecord( _format, recordId, true, true );

			  CheckRecord( _format, _recordSize, cursor, recordId, recordOffset, record );
			  CheckRecord( _format, _recordSize, cursor, recordId, recordOffset, firstInSecondChain );
			  CheckRecord( _format, _recordSize, cursor, recordId, recordOffset, firstInFirstChain );
			  CheckRecord( _format, _recordSize, cursor, recordId, recordOffset, firstInBothChains );
		 }

		 /*
		  * This test acts as a test group for whoever uses BaseHighLimitRecordFormat base class,
		  * the logic for marking both units as unused when deleting exists there.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarkBothUnitsAsUnusedWhenDeletingRecordWhichHasSecondaryUnit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarkBothUnitsAsUnusedWhenDeletingRecordWhichHasSecondaryUnit()
		 {
			  // GIVEN a record which requires two units
			  PagedFile storeFile = mock( typeof( PagedFile ) );
			  when( storeFile.PageSize() ).thenReturn(cursor.CurrentPageSize);
			  long hugeValue = 1L << 48;
			  RelationshipRecord record = ( new RelationshipRecord( 5 ) ).initialize( true, hugeValue + 1, hugeValue + 2, hugeValue + 3, 4, hugeValue + 5, hugeValue + 6, hugeValue + 7, hugeValue + 8, true, true );
			  record.SecondaryUnitId = 17;
			  record.RequiresSecondaryUnit = true;
			  cursor.Offset = offsetForId( record.Id, cursor.CurrentPageSize, _recordSize );
			  _format.write( record, cursor, _recordSize );

			  // WHEN deleting that record
			  record.InUse = false;
			  cursor.Offset = offsetForId( record.Id, cursor.CurrentPageSize, _recordSize );
			  _format.write( record, cursor, _recordSize );

			  // THEN both units should have been marked as unused
			  cursor.Offset = offsetForId( record.Id, cursor.CurrentPageSize, _recordSize );
			  assertFalse( RecordInUse( cursor ) );
			  cursor.Offset = offsetForId( record.SecondaryUnitId, cursor.CurrentPageSize, _recordSize );
			  assertFalse( RecordInUse( cursor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readWriteFixedReferencesRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadWriteFixedReferencesRecord()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomShortType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), true, true );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenTypeIsTooBig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenTypeIsTooBig()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), 1 << 16, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), true, true );

			  WriteReadRecord( source, target );

			  assertFalse( "Record should use variable length format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenTypeIsSmallEnough() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferenceFormatWhenTypeIsSmallEnough()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), (1 << 16) - 1, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), true, true );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedRecordFormatWhenAtLeastOneOfTheReferencesIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedRecordFormatWhenAtLeastOneOfTheReferencesIsMissing()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );

			  VerifyRecordsWithPoisonedReference( source, target, NULL, RandomShortType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenAtLeastOneOfTheReferencesIsTooBig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenAtLeastOneOfTheReferencesIsTooBig()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  VerifyRecordsWithPoisonedReference( source, target, 1L << ( sizeof( int ) * 8 ) + 5, RandomType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenRecordSizeIsTooSmall() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenRecordSizeIsTooSmall()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), true, true );

			  WriteReadRecord( source, target, RelationshipRecordFormat.FixedFormatRecordSize - 1 );

			  assertFalse( "Record should use variable length reference if format record is too small.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord()
		 {
			  RelationshipRecord source = new RelationshipRecord( 1 );
			  RelationshipRecord target = new RelationshipRecord( 1 );
			  source.Initialize( true, RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomShortType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), true, true );

			  WriteReadRecord( source, target, RelationshipRecordFormat.FixedFormatRecordSize );

			  assertTrue( "Record should use fixed reference if can fit in format record.", target.UseFixedReferences );
			  VerifySameReferences( source, target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRecordsWithPoisonedReference(org.Neo4Net.kernel.impl.store.record.RelationshipRecord source, org.Neo4Net.kernel.impl.store.record.RelationshipRecord target, long poisonedReference, int type) throws java.io.IOException
		 private void VerifyRecordsWithPoisonedReference( RelationshipRecord source, RelationshipRecord target, long poisonedReference, int type )
		 {
			  bool nullPoison = poisonedReference == NULL;
			  // first and second node can't be empty references so excluding them in case if poisoned reference is null
			  int differentReferences = nullPoison ? 5 : 7;
			  IList<long> references = BuildReferenceList( differentReferences, poisonedReference );
			  for ( int i = 0; i < differentReferences; i++ )
			  {
					cursor.Offset = 0;
					IEnumerator<long> iterator = references.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					source.Initialize( true, iterator.next(), nullPoison ? RandomFixedReference() : iterator.next(), nullPoison ? RandomFixedReference() : iterator.next(), type, iterator.next(), iterator.next(), iterator.next(), iterator.next(), true, true );

					WriteReadRecord( source, target );

					if ( nullPoison )
					{
						 assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
					}
					else
					{
						 assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
					}
					VerifySameReferences( source, target );
					Collections.rotate( references, 1 );
			  }
		 }

		 private IList<long> BuildReferenceList( int differentReferences, long poison )
		 {
			  IList<long> references = new List<long>( differentReferences );
			  references.Add( poison );
			  for ( int i = 1; i < differentReferences; i++ )
			  {
					references.Add( RandomFixedReference() );
			  }
			  return references;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(org.Neo4Net.kernel.impl.store.record.RelationshipRecord source, org.Neo4Net.kernel.impl.store.record.RelationshipRecord target) throws java.io.IOException
		 private void WriteReadRecord( RelationshipRecord source, RelationshipRecord target )
		 {
			  WriteReadRecord( source, target, _recordSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(org.Neo4Net.kernel.impl.store.record.RelationshipRecord source, org.Neo4Net.kernel.impl.store.record.RelationshipRecord target, int recordSize) throws java.io.IOException
		 private void WriteReadRecord( RelationshipRecord source, RelationshipRecord target, int recordSize )
		 {
			  _format.prepare( source, recordSize, _idSequence );
			  _format.write( source, cursor, recordSize );
			  cursor.Offset = 0;
			  _format.read( target, cursor, RecordLoad.NORMAL, recordSize );
		 }

		 private bool RecordInUse( StubPageCursor cursor )
		 {
			  sbyte header = cursor.Byte;
			  return ( header & IN_USE_BIT ) != 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkRecord(RelationshipRecordFormat format, int recordSize, org.Neo4Net.io.pagecache.StubPageCursor cursor, long recordId, int recordOffset, org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) throws java.io.IOException
		 private void CheckRecord( RelationshipRecordFormat format, int recordSize, StubPageCursor cursor, long recordId, int recordOffset, RelationshipRecord record )
		 {
			  format.Write( record, cursor, recordSize );

			  RelationshipRecord recordFromStore = format.NewRecord();
			  recordFromStore.Id = recordId;
			  ResetCursor( cursor, recordOffset );
			  format.Read( recordFromStore, cursor, RecordLoad.NORMAL, recordSize );

			  // records should be the same
			  VerifySameReferences( record, recordFromStore );

			  // now lets try to read same data into a record with different id - we should get different absolute references
			  ResetCursor( cursor, recordOffset );
			  RelationshipRecord recordWithOtherId = format.NewRecord();
			  recordWithOtherId.Id = 1L;
			  format.Read( recordWithOtherId, cursor, RecordLoad.NORMAL, recordSize );

			  assertNotEquals( record.FirstNextRel, recordWithOtherId.FirstNextRel );
			  assertNotEquals( record.FirstPrevRel, recordWithOtherId.FirstPrevRel );
			  assertNotEquals( record.SecondNextRel, recordWithOtherId.SecondNextRel );
			  assertNotEquals( record.SecondPrevRel, recordWithOtherId.SecondPrevRel );
		 }

		 private void VerifySameReferences( RelationshipRecord record, RelationshipRecord recordFromStore )
		 {
			  assertEquals( "Types should be equal.", record.Type, recordFromStore.Type );
			  assertEquals( "First Next references should be equal.", record.FirstNextRel, recordFromStore.FirstNextRel );
			  assertEquals( "First Node references should be equal.", record.FirstNode, recordFromStore.FirstNode );
			  assertEquals( "First Prev Rel references should be equal.", record.FirstPrevRel, recordFromStore.FirstPrevRel );
			  assertEquals( "Second Next Rel references should be equal.", record.SecondNextRel, recordFromStore.SecondNextRel );
			  assertEquals( "Second Node references should be equal.", record.SecondNode, recordFromStore.SecondNode );
			  assertEquals( "Second Prev Rel references should be equal.", record.SecondPrevRel, recordFromStore.SecondPrevRel );
			  assertEquals( "Next Prop references should be equal.", record.NextProp, recordFromStore.NextProp );
		 }

		 private void ResetCursor( StubPageCursor cursor, int recordOffset )
		 {
			  cursor.Offset = recordOffset;
		 }

		 // Create high-limit record which fits in one record
		 private RelationshipRecord CreateCompactRecord( RelationshipRecordFormat format, long recordId, bool firstInFirstChain, bool firstInSecondChain )
		 {
			  RelationshipRecord record = format.NewRecord();
			  record.InUse = true;
			  record.FirstInFirstChain = firstInFirstChain;
			  record.FirstInSecondChain = firstInSecondChain;
			  record.Id = recordId;
			  record.FirstNextRel = recordId + 1L;
			  record.FirstNode = recordId + 2L;
			  record.FirstPrevRel = recordId + 3L;
			  record.SecondNextRel = recordId + 4L;
			  record.SecondNode = recordId + 5L;
			  record.SecondPrevRel = recordId + 6L;
			  record.Type = 7;
			  return record;
		 }

		 private int RandomShortType()
		 {
			  return ( int ) RandomReference( 1L << ( sizeof( short ) * 8 ) );
		 }

		 private int RandomType()
		 {
			  return ( int ) RandomReference( 1L << 24 );
		 }

		 private long RandomFixedReference()
		 {
			  return RandomReference( 1L << ( ( sizeof( int ) * 8 ) + 1 ) );
		 }

		 private long RandomReference( long maxValue )
		 {
			  return ThreadLocalRandom.current().nextLong(maxValue);
		 }
	}

}