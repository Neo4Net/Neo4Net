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
namespace Org.Neo4j.Io.pagecache.randomharness
{

	internal class Plan
	{
		 private readonly Action[] _plan;
		 private readonly IDictionary<File, PagedFile> _fileMap;
		 private readonly IList<File> _mappedFiles;
		 private readonly ISet<File> _filesTouched;
		 private readonly long[] _executedByThread;
		 private readonly AtomicInteger _actionCounter;
		 private readonly System.Threading.CountdownEvent _startLatch;

		 internal Plan( Action[] plan, IDictionary<File, PagedFile> fileMap, IList<File> mappedFiles, ISet<File> filesTouched )
		 {
			  this._plan = plan;
			  this._fileMap = fileMap;
			  this._mappedFiles = mappedFiles;
			  this._filesTouched = filesTouched;
			  _executedByThread = new long[plan.Length];
			  Arrays.fill( _executedByThread, -1 );
			  _actionCounter = new AtomicInteger();
			  _startLatch = new System.Threading.CountdownEvent( 1 );
		 }

		 public virtual void Start()
		 {
			  _startLatch.Signal();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Action next() throws InterruptedException
		 public virtual Action Next()
		 {
			  _startLatch.await();
			  int index = _actionCounter.AndIncrement;
			  if ( index < _plan.Length )
			  {
					_executedByThread[index] = Thread.CurrentThread.Id;
					return _plan[index];
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public virtual void Close()
		 {
			  foreach ( File mappedFile in _mappedFiles )
			  {
					PagedFile pagedFile = _fileMap[mappedFile];
					if ( pagedFile != null )
					{
						 pagedFile.Close();
					}
			  }
		 }

		 public virtual void Print( PrintStream @out )
		 {
			  @out.println( "Plan: [thread; action]" );
			  for ( int i = 0; i < _plan.Length; i++ )
			  {
					long threadId = _executedByThread[i];
					@out.printf( "  % 3d : %s%n", threadId, _plan[i] );
					if ( threadId == -1 )
					{
						 break;
					}
			  }
		 }

		 public virtual ISet<File> FilesTouched
		 {
			 get
			 {
				  return _filesTouched;
			 }
		 }
	}

}