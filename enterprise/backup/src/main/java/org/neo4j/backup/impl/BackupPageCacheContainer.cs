﻿/*
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
namespace Org.Neo4j.backup.impl
{
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	internal class BackupPageCacheContainer : AutoCloseable
	{
		 private readonly PageCache _pageCache;
		 private readonly JobScheduler _jobScheduler;

		 public static BackupPageCacheContainer Of( PageCache pageCache )
		 {
			  return Of( pageCache, null );
		 }

		 public static BackupPageCacheContainer Of( PageCache pageCache, JobScheduler jobScheduler )
		 {
			  return new BackupPageCacheContainer( pageCache, jobScheduler );
		 }

		 private BackupPageCacheContainer( PageCache pageCache, JobScheduler jobScheduler )
		 {
			  this._pageCache = pageCache;
			  this._jobScheduler = jobScheduler;
		 }

		 public virtual PageCache PageCache
		 {
			 get
			 {
				  return _pageCache;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _pageCache.close();
			  if ( _jobScheduler != null )
			  {
					_jobScheduler.close();
			  }
		 }
	}

}