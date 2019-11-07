﻿using System.Collections.Generic;

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
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;

	public class AbstractDynamicStoreTest
	{
		 private const int BLOCK_SIZE = 60;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.fs.EphemeralFileSystemRule fsr = new Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fsr = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.PageCacheRule pageCacheRule = new Neo4Net.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private readonly File _storeFile = new File( "store" );
		 private readonly File _idFile = new File( "idStore" );
		 private readonly RecordFormats _formats = Standard.LATEST_RECORD_FORMATS;
		 private PageCache _pageCache;
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _fs = Fsr.get();
			  _pageCache = PageCacheRule.getPageCache( Fsr.get() );
			  using ( StoreChannel channel = _fs.create( _storeFile ) )
			  {
					ByteBuffer buffer = ByteBuffer.allocate( 4 );
					buffer.putInt( BLOCK_SIZE );
					buffer.flip();
					channel.write( buffer );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamicRecordCursorReadsInUseRecords()
		 public virtual void DynamicRecordCursorReadsInUseRecords()
		 {
			  using ( AbstractDynamicStore store = NewTestableDynamicStore() )
			  {
					DynamicRecord first = CreateDynamicRecord( 1, store, 0 );
					DynamicRecord second = CreateDynamicRecord( 2, store, 0 );
					DynamicRecord third = CreateDynamicRecord( 3, store, 10 );
					store.HighId = 3;

					first.NextBlock = second.Id;
					store.UpdateRecord( first );
					second.NextBlock = third.Id;
					store.UpdateRecord( second );

					IEnumerator<DynamicRecord> records = store.GetRecords( 1, NORMAL ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( first, records.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( second, records.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( third, records.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( records.hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamicRecordCursorReadsNotInUseRecords()
		 public virtual void DynamicRecordCursorReadsNotInUseRecords()
		 {
			  using ( AbstractDynamicStore store = NewTestableDynamicStore() )
			  {
					DynamicRecord first = CreateDynamicRecord( 1, store, 0 );
					DynamicRecord second = CreateDynamicRecord( 2, store, 0 );
					DynamicRecord third = CreateDynamicRecord( 3, store, 10 );
					store.HighId = 3;

					first.NextBlock = second.Id;
					store.UpdateRecord( first );
					second.NextBlock = third.Id;
					store.UpdateRecord( second );
					second.InUse = false;
					store.UpdateRecord( second );

					IEnumerator<DynamicRecord> records = store.GetRecords( 1, FORCE ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( first, records.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					DynamicRecord secondReadRecord = records.next();
					assertEquals( second, secondReadRecord );
					assertFalse( secondReadRecord.InUse() );
					// because mode == FORCE we can still move through the chain
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( records.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( third, records.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( records.hasNext() );
			  }
		 }

		 private DynamicRecord CreateDynamicRecord( long id, AbstractDynamicStore store, int dataSize )
		 {
			  DynamicRecord first = new DynamicRecord( id );
			  first.InUse = true;
			  first.Data = RandomUtils.NextBytes( dataSize == 0 ? BLOCK_SIZE - _formats.dynamic().RecordHeaderSize : 10 );
			  store.UpdateRecord( first );
			  return first;
		 }

		 private AbstractDynamicStore NewTestableDynamicStore()
		 {
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( _fs );
			  AbstractDynamicStore store = new AbstractDynamicStoreAnonymousInnerClass( this, _storeFile, _idFile, Config.defaults(), idGeneratorFactory, _pageCache, NullLogProvider.Instance, _formats.dynamic(), _formats.storeVersion() );
			  store.Initialize( true );
			  return store;
		 }

		 private class AbstractDynamicStoreAnonymousInnerClass : AbstractDynamicStore
		 {
			 private readonly AbstractDynamicStoreTest _outerInstance;

			 public AbstractDynamicStoreAnonymousInnerClass( AbstractDynamicStoreTest outerInstance, File storeFile, File idFile, Config defaults, DefaultIdGeneratorFactory idGeneratorFactory, PageCache pageCache, NullLogProvider getInstance, Neo4Net.Kernel.impl.store.format.RecordFormat<DynamicRecord> dynamic, string storeVersion ) : base( storeFile, idFile, defaults, IdType.ARRAY_BLOCK, idGeneratorFactory, pageCache, getInstance, "test", BLOCK_SIZE, dynamic, storeVersion )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void accept( RecordStore_Processor processor, DynamicRecord record )
			 { // Ignore
			 }

			 public override string TypeDescriptor
			 {
				 get
				 {
					  return "TestDynamicStore";
				 }
			 }
		 }
	}

}