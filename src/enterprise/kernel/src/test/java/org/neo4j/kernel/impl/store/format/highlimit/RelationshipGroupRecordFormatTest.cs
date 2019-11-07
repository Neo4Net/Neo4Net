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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ByteUnit = Neo4Net.Io.ByteUnit;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.highlimit.BaseHighLimitRecordFormat.NULL;

	public class RelationshipGroupRecordFormatTest
	{

		 private RelationshipGroupRecordFormat _recordFormat;
		 private FixedLinkedStubPageCursor _pageCursor;
		 private ConstantIdSequence _idSequence;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _recordFormat = new RelationshipGroupRecordFormat();
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
			  RelationshipGroupRecord source = new RelationshipGroupRecord( 1 );
			  RelationshipGroupRecord target = new RelationshipGroupRecord( 1 );
			  source.Initialize( true, RandomType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target );

			  assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
			  VerifySame( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenOneOfTheReferencesIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferenceFormatWhenOneOfTheReferencesIsMissing()
		 {
			  RelationshipGroupRecord source = new RelationshipGroupRecord( 1 );
			  RelationshipGroupRecord target = new RelationshipGroupRecord( 1 );

			  VerifyRecordsWithPoisonedReference( source, target, NULL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenOneOfTheReferencesReferenceTooBig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenOneOfTheReferencesReferenceTooBig()
		 {
			  RelationshipGroupRecord source = new RelationshipGroupRecord( 1 );
			  RelationshipGroupRecord target = new RelationshipGroupRecord( 1 );

			  VerifyRecordsWithPoisonedReference( source, target, 1L << ( ( sizeof( int ) * 8 ) + 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useVariableLengthFormatWhenRecordSizeIsTooSmall() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseVariableLengthFormatWhenRecordSizeIsTooSmall()
		 {
			  RelationshipGroupRecord source = new RelationshipGroupRecord( 1 );
			  RelationshipGroupRecord target = new RelationshipGroupRecord( 1 );
			  source.Initialize( true, RandomType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target, RelationshipGroupRecordFormat.FixedFormatRecordSize - 1 );

			  assertFalse( "Record should use variable length reference if format record is too small.", target.UseFixedReferences );
			  VerifySame( source, target );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseFixedReferenceFormatWhenRecordCanFitInRecordSizeRecord()
		 {
			  RelationshipGroupRecord source = new RelationshipGroupRecord( 1 );
			  RelationshipGroupRecord target = new RelationshipGroupRecord( 1 );
			  source.Initialize( true, RandomType(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference(), RandomFixedReference() );

			  WriteReadRecord( source, target, RelationshipGroupRecordFormat.FixedFormatRecordSize );

			  assertTrue( "Record should use fixed reference if can fit in format record.", target.UseFixedReferences );
			  VerifySame( source, target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRecordsWithPoisonedReference(Neo4Net.kernel.impl.store.record.RelationshipGroupRecord source, Neo4Net.kernel.impl.store.record.RelationshipGroupRecord target, long poisonedReference) throws java.io.IOException
		 private void VerifyRecordsWithPoisonedReference( RelationshipGroupRecord source, RelationshipGroupRecord target, long poisonedReference )
		 {
			  bool nullPoisoned = poisonedReference == BaseHighLimitRecordFormat.Null;
			  int differentReferences = 5;
			  IList<long> references = BuildReferenceList( differentReferences, poisonedReference );
			  for ( int i = 0; i < differentReferences; i++ )
			  {
					_pageCursor.Offset = 0;
					IEnumerator<long> iterator = references.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					source.Initialize( true, 0, iterator.next(), iterator.next(), iterator.next(), iterator.next(), iterator.next() );

					WriteReadRecord( source, target );

					if ( nullPoisoned )
					{
						 assertTrue( "Record should use fixed reference format.", target.UseFixedReferences );
					}
					else
					{
						 assertFalse( "Record should use variable length reference format.", target.UseFixedReferences );
					}
					VerifySame( source, target );
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

		 private void VerifySame( RelationshipGroupRecord recordA, RelationshipGroupRecord recordB )
		 {
			  assertEquals( "Types should be equal.", recordA.Type, recordB.Type );
			  assertEquals( "First In references should be equal.", recordA.FirstIn, recordB.FirstIn );
			  assertEquals( "First Loop references should be equal.", recordA.FirstLoop, recordB.FirstLoop );
			  assertEquals( "First Out references should be equal.", recordA.FirstOut, recordB.FirstOut );
			  assertEquals( "Next references should be equal.", recordA.Next, recordB.Next );
			  assertEquals( "Prev references should be equal.", recordA.Prev, recordB.Prev );
			  assertEquals( "Owning node references should be equal.", recordA.OwningNode, recordB.OwningNode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(Neo4Net.kernel.impl.store.record.RelationshipGroupRecord source, Neo4Net.kernel.impl.store.record.RelationshipGroupRecord target) throws java.io.IOException
		 private void WriteReadRecord( RelationshipGroupRecord source, RelationshipGroupRecord target )
		 {
			  WriteReadRecord( source, target, RelationshipGroupRecordFormat.RECORD_SIZE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeReadRecord(Neo4Net.kernel.impl.store.record.RelationshipGroupRecord source, Neo4Net.kernel.impl.store.record.RelationshipGroupRecord target, int recordSize) throws java.io.IOException
		 private void WriteReadRecord( RelationshipGroupRecord source, RelationshipGroupRecord target, int recordSize )
		 {
			  _recordFormat.prepare( source, recordSize, _idSequence );
			  _recordFormat.write( source, _pageCursor, recordSize );
			  _pageCursor.Offset = 0;
			  _recordFormat.read( target, _pageCursor, RecordLoad.NORMAL, recordSize );
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