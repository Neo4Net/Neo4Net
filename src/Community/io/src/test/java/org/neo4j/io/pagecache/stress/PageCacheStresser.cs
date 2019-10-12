using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.pagecache.stress
{


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	/// <summary>
	/// It works like this: We have N threads, and a number of records with N long fields plus a sum field. So each record
	/// can verify their consistency by summing up their N fields and comparing the result to their sum field. Further, each
	/// thread can also verify their consistency, by taking the sum of their respective N field across all records, and
	/// comparing it to the number of increments they've done. The records are protected by entity locks, since page write
	/// locks are not exclusive, so in the end we should see no lost updates. That is, both consistency checks should pass.
	/// We will also have many more file pages and cache pages, so we'll have lots of concurrent eviction and page faulting
	/// as well.
	/// </summary>
	public class PageCacheStresser
	{
		 private readonly int _maxPages;
		 private readonly int _numberOfThreads;

		 private readonly File _workingDirectory;

		 public PageCacheStresser( int maxPages, int numberOfThreads, File workingDirectory )
		 {
			  this._maxPages = maxPages;
			  this._numberOfThreads = numberOfThreads;
			  this._workingDirectory = workingDirectory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stress(org.neo4j.io.pagecache.PageCache pageCache, Condition condition) throws Exception
		 public virtual void Stress( PageCache pageCache, Condition condition )
		 {
			  File file = Files.createTempFile( _workingDirectory.toPath(), "pagecacheundertest", ".bin" ).toFile();
			  file.deleteOnExit();

			  int cachePageSize = pageCache.PageSize();
			  RecordFormat format = new RecordFormat( _numberOfThreads, cachePageSize );
			  int filePageSize = format.FilePageSize;

			  using ( PagedFile pagedFile = pageCache.Map( file, filePageSize ) )
			  {
					IList<RecordStresser> recordStressers = Prepare( condition, pagedFile, format );
					VerifyResults( format, pagedFile, recordStressers );
					Execute( recordStressers );
					VerifyResults( format, pagedFile, recordStressers );
			  }
		 }

		 private IList<RecordStresser> Prepare( Condition condition, PagedFile pagedFile, RecordFormat format )
		 {
			  int maxRecords = Math.multiplyExact( _maxPages, format.RecordsPerPage );
			  TinyLockManager locks = new TinyLockManager();

			  IList<RecordStresser> recordStressers = new LinkedList<RecordStresser>();
			  for ( int threadId = 0; threadId < _numberOfThreads; threadId++ )
			  {
					recordStressers.Add( new RecordStresser( pagedFile, condition, maxRecords, format, threadId, locks ) );
			  }
			  return recordStressers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute(java.util.List<RecordStresser> recordStressers) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void Execute( IList<RecordStresser> recordStressers )
		 {
			  ExecutorService executorService = Executors.newFixedThreadPool(_numberOfThreads, r =>
			  {
				Thread thread = Executors.defaultThreadFactory().newThread(r);
				thread.Daemon = true;
				return thread;
			  });
			  IList<Future<Void>> futures = executorService.invokeAll( recordStressers );
			  foreach ( Future<Void> future in futures )
			  {
					future.get();
			  }
			  executorService.shutdown();
			  assertTrue( executorService.awaitTermination( 10, TimeUnit.SECONDS ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyResults(RecordFormat format, org.neo4j.io.pagecache.PagedFile pagedFile, java.util.List<RecordStresser> recordStressers) throws java.io.IOException
		 private void VerifyResults( RecordFormat format, PagedFile pagedFile, IList<RecordStresser> recordStressers )
		 {
			  foreach ( RecordStresser stresser in recordStressers )
			  {
					stresser.VerifyCounts();
			  }
			  using ( PageCursor cursor = pagedFile.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					while ( cursor.Next() )
					{
						 format.VerifyCheckSums( cursor );
					}
			  }
		 }
	}

}