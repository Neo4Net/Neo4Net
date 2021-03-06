﻿using System;
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.store
{
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using BinaryLatch = Org.Neo4j.Util.concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.throwIfUnchecked;

	/// <summary>
	/// A dedicated thread which constantly call <seealso cref="PageCache.flushAndForce()"/> until a call to <seealso cref="halt()"/> is made.
	/// Must be started manually by calling <seealso cref="start()"/>.
	/// </summary>
	internal class PageCacheFlusher : Thread
	{
		 private readonly PageCache _pageCache;
		 private readonly BinaryLatch _halt = new BinaryLatch();
		 private volatile bool _halted;
		 private volatile Exception _error;

		 internal PageCacheFlusher( PageCache pageCache ) : base( "PageCacheFlusher" )
		 {
			  this._pageCache = pageCache;
		 }

		 public override void Run()
		 {
			  try
			  {
					while ( !_halted )
					{
						 try
						 {
							  _pageCache.flushAndForce();
						 }
						 catch ( Exception e )
						 {
							  _error = e;
							  break;
						 }
					}
			  }
			  finally
			  {
					_halt.release();
			  }
		 }

		 /// <summary>
		 /// Halts this flusher, making it stop flushing. The current call to <seealso cref="PageCache.flushAndForce()"/>
		 /// will complete before exiting this method call. If there was an error in the thread doing the flushes
		 /// that exception will be thrown from this method as a <seealso cref="System.Exception"/>.
		 /// </summary>
		 internal virtual void Halt()
		 {
			  _halted = true;
			  _halt.await();
			  if ( _error != null )
			  {
					throwIfUnchecked( _error );
					throw new Exception( _error );
			  }
		 }
	}

}