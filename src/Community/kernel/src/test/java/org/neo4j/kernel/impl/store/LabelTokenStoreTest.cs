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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;


	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class LabelTokenStoreTest
	{
		 private readonly File _file = mock( typeof( File ) );
		 private readonly File _idFile = mock( typeof( File ) );
		 private readonly IdGeneratorFactory _generatorFactory = mock( typeof( IdGeneratorFactory ) );
		 private readonly PageCache _cache = mock( typeof( PageCache ) );
		 private readonly LogProvider _logProvider = mock( typeof( LogProvider ) );
		 private readonly DynamicStringStore _dynamicStringStore = mock( typeof( DynamicStringStore ) );
		 private readonly PageCursor _pageCursor = mock( typeof( PageCursor ) );
		 private readonly Config _config = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceGetRecordSkipInUsecheck() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ForceGetRecordSkipInUsecheck()
		 {
			  LabelTokenStore store = new UnusedLabelTokenStore( this );
			  LabelTokenRecord record = store.getRecord( 7, store.newRecord(), FORCE );
			  assertFalse( "Record should not be in use", record.InUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = InvalidRecordException.class) public void getRecord() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getRecord()
		 {
			  when( _pageCursor.Byte ).thenReturn( Record.NOT_IN_USE.byteValue() );

			  LabelTokenStore store = new UnusedLabelTokenStore( this );
			  store.getRecord( 7, store.newRecord(), NORMAL );
		 }

		 internal class UnusedLabelTokenStore : LabelTokenStore
		 {
			 private readonly LabelTokenStoreTest _outerInstance;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: UnusedLabelTokenStore() throws java.io.IOException
			  internal UnusedLabelTokenStore( LabelTokenStoreTest outerInstance ) : base( outerInstance.file, outerInstance.idFile, outerInstance.config, outerInstance.generatorFactory, outerInstance.cache, outerInstance.logProvider, outerInstance.dynamicStringStore, RecordFormatSelector.defaultFormat() )
			  {
				  this._outerInstance = outerInstance;
					pagedFile = mock( typeof( PagedFile ) );

					when( pagedFile.io( any( typeof( Long ) ), any( typeof( Integer ) ) ) ).thenReturn( outerInstance.pageCursor );
					when( pagedFile.pageSize() ).thenReturn(1);
					when( outerInstance.pageCursor.Next() ).thenReturn(true);
			  }
		 }
	}

}