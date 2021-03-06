﻿using System;

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
namespace Org.Neo4j.Index.@internal.gbptree
{

	internal class GBPTreeCleanupJob : CleanupJob
	{
		 private readonly CrashGenerationCleaner _crashGenerationCleaner;
		 private readonly GBPTreeLock _gbpTreeLock;
		 private readonly GBPTree.Monitor _monitor;
		 private readonly File _indexFile;
		 private volatile bool _needed;
		 private volatile Exception _failure;

		 /// <param name="crashGenerationCleaner"> <seealso cref="CrashGenerationCleaner"/> to use for cleaning. </param>
		 /// <param name="gbpTreeLock"> <seealso cref="GBPTreeLock"/> to be released when job has either successfully finished or failed. </param>
		 /// <param name="monitor"> <seealso cref="GBPTree.Monitor"/> to report to </param>
		 /// <param name="indexFile"> Target file </param>
		 internal GBPTreeCleanupJob( CrashGenerationCleaner crashGenerationCleaner, GBPTreeLock gbpTreeLock, GBPTree.Monitor monitor, File indexFile )
		 {
			  this._crashGenerationCleaner = crashGenerationCleaner;
			  this._gbpTreeLock = gbpTreeLock;
			  this._monitor = monitor;
			  this._indexFile = indexFile;
			  this._needed = true;

		 }

		 public override bool Needed()
		 {
			  return _needed;
		 }

		 public override bool HasFailed()
		 {
			  return _failure != null;
		 }

		 public virtual Exception Cause
		 {
			 get
			 {
				  return _failure;
			 }
		 }

		 public override void Close()
		 {
			  _gbpTreeLock.cleanerUnlock();
			  _monitor.cleanupClosed();
		 }

		 public override void Run( ExecutorService executor )
		 {
			  try
			  {
					_crashGenerationCleaner.clean( executor );
					_needed = false;
			  }
			  catch ( Exception e )
			  {
					_monitor.cleanupFailed( e );
					_failure = e;
			  }
		 }

		 public override string ToString()
		 {
			  StringJoiner joiner = new StringJoiner( ", ", "CleanupJob(", ")" );
			  joiner.add( "file=" + _indexFile.AbsolutePath );
			  joiner.add( "needed=" + _needed );
			  joiner.add( "failure=" + ( _failure == null ? null : _failure.ToString() ) );
			  return joiner.ToString();
		 }
	}

}