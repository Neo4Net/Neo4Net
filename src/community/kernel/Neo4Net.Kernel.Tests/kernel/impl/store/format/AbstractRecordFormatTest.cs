using System;

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
namespace Neo4Net.Kernel.impl.store.format
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestName = org.junit.rules.TestName;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using Neo4Net.Kernel.impl.store.format;
	using BatchingIdSequence = Neo4Net.Kernel.impl.store.id.BatchingIdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;

	public abstract class AbstractRecordFormatTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _pageCacheRule ).around( _fsRule ).around( _random );
		}

		 private static readonly int _pageSize = ( int ) kibiBytes( 1 );

		 // Whoever is hit first
		 private const long TEST_ITERATIONS = 100_000;
		 private const long TEST_TIME = 1000;
		 private const int DATA_SIZE = 100;
		 protected internal static readonly long Null = Record.NULL_REFERENCE.intValue();

		 private readonly RandomRule _random = new RandomRule();
		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName name = new org.junit.rules.TestName();
		 public readonly TestName Name = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(pageCacheRule).around(fsRule).around(random);
		 public RuleChain RuleChain;
		 private PageCache _pageCache;

		 public RecordKeys Keys = FullyCoveringRecordKeys.Instance;

		 private readonly RecordFormats _formats;
		 private readonly int _entityBits;
		 private readonly int _propertyBits;
		 private RecordGenerators _generators;

		 protected internal AbstractRecordFormatTest( RecordFormats formats, int IEntityBits, int propertyBits )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._formats = formats;
			  this._entityBits = IEntityBits;
			  this._propertyBits = propertyBits;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupPageCache()
		 public virtual void SetupPageCache()
		 {
			  _pageCache = _pageCacheRule.getPageCache( _fsRule.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _generators = new LimitedRecordGenerators( _random.randomValues(), _entityBits, _propertyBits, 40, 16, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void node() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Node()
		 {
			  VerifyWriteAndRead( _formats.node, _generators.node, Keys.node, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Relationship()
		 {
			  VerifyWriteAndRead( _formats.relationship, _generators.relationship, Keys.relationship, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void property() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Property()
		 {
			  VerifyWriteAndRead( _formats.property, _generators.property, Keys.property, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RelationshipGroup()
		 {
			  VerifyWriteAndRead( _formats.relationshipGroup, _generators.relationshipGroup, Keys.relationshipGroup, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipTypeToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RelationshipTypeToken()
		 {
			  VerifyWriteAndRead( _formats.relationshipTypeToken, _generators.relationshipTypeToken, Keys.relationshipTypeToken, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertyKeyToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PropertyKeyToken()
		 {
			  VerifyWriteAndRead( _formats.propertyKeyToken, _generators.propertyKeyToken, Keys.propertyKeyToken, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelToken()
		 {
			  VerifyWriteAndRead( _formats.labelToken, _generators.labelToken, Keys.labelToken, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Dynamic()
		 {
			  VerifyWriteAndRead( _formats.dynamic, _generators.dynamic, Keys.dynamic, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <R extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void verifyWriteAndRead(System.Func<RecordFormat<R>> formatSupplier, System.Func<Neo4Net.kernel.impl.store.format.RecordGenerators_Generator<R>> generatorSupplier, System.Func<RecordKey<R>> keySupplier, boolean assertPostReadOffset) throws java.io.IOException
		 private void VerifyWriteAndRead<R>( System.Func<RecordFormat<R>> formatSupplier, System.Func<RecordGenerators_Generator<R>> generatorSupplier, System.Func<RecordKey<R>> keySupplier, bool assertPostReadOffset ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  // GIVEN
			  using ( PagedFile storeFile = _pageCache.map( new File( "store-" + Name.MethodName ), _pageSize, CREATE ) )
			  {
					RecordFormat<R> format = formatSupplier();
					RecordKey<R> key = keySupplier();
					RecordGenerators_Generator<R> generator = generatorSupplier();
					int recordSize = format.GetRecordSize( new IntStoreHeader( DATA_SIZE ) );
					BatchingIdSequence idSequence = new BatchingIdSequence( _random.nextBoolean() ? IdSureToBeOnTheNextPage(_pageSize, recordSize) : 10 );

					// WHEN
					long time = currentTimeMillis();
					long endTime = time + TEST_TIME;
					long i = 0;
					for ( ; i < TEST_ITERATIONS && currentTimeMillis() < endTime; i++ )
					{
						 R written = generator.Get( recordSize, format, i % 5 );
						 R read = format.NewRecord();
						 try
						 {
							  WriteRecord( written, format, storeFile, recordSize, idSequence );
							  ReadAndVerifyRecord( written, read, format, key, storeFile, recordSize, assertPostReadOffset );
							  idSequence.Reset();
						 }
						 catch ( Exception t )
						 {
							  Exceptions.setMessage( t, t.Message + " : written:" + written + ", read:" + read + ", seed:" + _random.seed() + ", iteration:" + i );
							  throw t;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <R extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void readAndVerifyRecord(R written, R read, RecordFormat<R> format, RecordKey<R> key, Neo4Net.io.pagecache.PagedFile storeFile, int recordSize, boolean assertPostReadOffset) throws java.io.IOException
		 private void ReadAndVerifyRecord<R>( R written, R read, RecordFormat<R> format, RecordKey<R> key, PagedFile storeFile, int recordSize, bool assertPostReadOffset ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  using ( PageCursor cursor = storeFile.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					AssertedNext( cursor );
					read.Id = written.Id;

					/*
					 Retry loop is needed here because format does not handle retries on the primary cursor.
					 Same retry is done on the store level in {@link Neo4Net.kernel.impl.store.CommonAbstractStore}
					 */
					int offset = Math.toIntExact( written.Id * recordSize );
					do
					{
						 cursor.Offset = offset;
						 format.Read( read, cursor, NORMAL, recordSize );
					} while ( cursor.ShouldRetry() );
					AssertWithinBounds( written, cursor, "reading" );
					if ( assertPostReadOffset )
					{
						 assertEquals( "Cursor is positioned on first byte of next record after a read", offset + recordSize, cursor.Offset );
					}
					cursor.CheckAndClearCursorException();

					// THEN
					if ( written.inUse() )
					{
						 assertEquals( written.inUse(), read.inUse() );
						 assertEquals( written.Id, read.Id );
						 assertEquals( written.SecondaryUnitId, read.SecondaryUnitId );
						 key.AssertRecordsEquals( written, read );
					}
					else
					{
						 assertEquals( written.inUse(), read.inUse() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <R extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void writeRecord(R record, RecordFormat<R> format, Neo4Net.io.pagecache.PagedFile storeFile, int recordSize, Neo4Net.kernel.impl.store.id.BatchingIdSequence idSequence) throws java.io.IOException
		 private void WriteRecord<R>( R record, RecordFormat<R> format, PagedFile storeFile, int recordSize, BatchingIdSequence idSequence ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  using ( PageCursor cursor = storeFile.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					AssertedNext( cursor );
					if ( record.inUse() )
					{
						 format.Prepare( record, recordSize, idSequence );
					}

					int offset = Math.toIntExact( record.Id * recordSize );
					cursor.Offset = offset;
					format.Write( record, cursor, recordSize );
					AssertWithinBounds( record, cursor, "writing" );
			  }
		 }

		 private void AssertWithinBounds<R>( R record, PageCursor cursor, string operation ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					fail( "Out-of-bounds when " + operation + " record " + record );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertedNext(Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void AssertedNext( PageCursor cursor )
		 {
			  assertTrue( cursor.Next() );
		 }

		 private long IdSureToBeOnTheNextPage( int pageSize, int recordSize )
		 {
			  return ( pageSize + 100 ) / recordSize;
		 }
	}

}