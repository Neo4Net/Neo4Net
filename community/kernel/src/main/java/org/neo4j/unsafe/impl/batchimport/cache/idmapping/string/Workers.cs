using System;
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;

	/// <summary>
	/// Utility for running a handful of <seealso cref="System.Threading.ThreadStart"/> in parallel, each in its own thread.
	/// <seealso cref="System.Threading.ThreadStart"/> instances are <seealso cref="start(System.Threading.ThreadStart) added and started"/> and the caller can
	/// <seealso cref="await()"/> them all to finish, returning a <seealso cref="System.Exception error"/> if any thread encountered one so
	/// that the caller can decide how to handle that error. Or caller can use <seealso cref="awaitAndThrowOnError()"/>
	/// where error from any worker would be thrown from that method.
	/// 
	/// It's basically like using an <seealso cref="ExecutorService"/>, but without that "baggage" and an easier usage
	/// and less code in the scenario described above.
	/// </summary>
	/// @param <R> type of workers </param>
	public class Workers<R> : IEnumerable<R> where R : ThreadStart
	{
		 private readonly IList<Worker> _workers = new List<Worker>();
		 private readonly string _names;

		 public Workers( string names )
		 {
			  this._names = names;
		 }

		 /// <summary>
		 /// Starts a thread to run {@code toRun}. Returns immediately.
		 /// </summary>
		 /// <param name="toRun"> worker to start and run among potentially other workers. </param>
		 public virtual void Start( R toRun )
		 {
			  Worker worker = new Worker( this, _names + "-" + _workers.Count, toRun );
			  worker.Start();
			  _workers.Add( worker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Throwable await() throws InterruptedException
		 public virtual Exception Await()
		 {
			  Exception error = null;
			  foreach ( Worker worker in _workers )
			  {
					Exception anError = worker.Await();
					if ( error == null )
					{
						 error = anError;
					}
			  }
			  return error;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitAndThrowOnError() throws InterruptedException
		 public virtual void AwaitAndThrowOnError()
		 {
			  Exception error = Await();
			  if ( error != null )
			  {
					throw new Exception( error );
			  }
		 }

		 public virtual void AwaitAndThrowOnErrorStrict()
		 {
			  try
			  {
					AwaitAndThrowOnError();
			  }
			  catch ( InterruptedException e )
			  {
					throw HandleInterrupted( e );
			  }
		 }

		 private Exception HandleInterrupted( InterruptedException e )
		 {
			  Thread.interrupted();
			  return new Exception( "Got interrupted while awaiting workers (" + _names + ") to complete", e );
		 }

		 public override IEnumerator<R> Iterator()
		 {
			  return Iterators.map( worker => worker.toRun, _workers.GetEnumerator() );
		 }

		 private class Worker : Thread
		 {
			 private readonly Workers<R> _outerInstance;

			  internal volatile Exception Error;
			  internal readonly R ToRun;

			  internal Worker( Workers<R> outerInstance, string name, R toRun ) : base( name )
			  {
				  this._outerInstance = outerInstance;
					this.ToRun = toRun;
			  }

			  public override void Run()
			  {
					try
					{
						 ToRun.run();
					}
					catch ( Exception t )
					{
						 Error = t;
						 throw new Exception( t );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected synchronized Throwable await() throws InterruptedException
			  protected internal virtual Exception Await()
			  {
				  lock ( this )
				  {
						join();
						return Error;
				  }
			  }
		 }
	}

}