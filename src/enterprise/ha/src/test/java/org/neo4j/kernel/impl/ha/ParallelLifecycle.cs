using System;
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
namespace Neo4Net.Kernel.impl.ha
{

	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Acts as a holder of multiple <seealso cref="Lifecycle"/> and executes each transition,
	/// all the individual lifecycles in parallel.
	/// <para>
	/// This is only a test utility and so doesn't support
	/// </para>
	/// </summary>
	internal class ParallelLifecycle : LifecycleAdapter
	{
		 private readonly IList<Lifecycle> _lifecycles = new List<Lifecycle>();
		 private readonly long _timeout;
		 private readonly TimeUnit _unit;

		 internal ParallelLifecycle( long timeout, TimeUnit unit )
		 {
			  this._timeout = timeout;
			  this._unit = unit;
		 }

		 public virtual T Add<T>( T lifecycle ) where T : Neo4Net.Kernel.Lifecycle.Lifecycle
		 {
			  _lifecycles.Add( lifecycle );
			  return lifecycle;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  Perform( Lifecycle.init );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Perform( Lifecycle.start );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  Perform( Lifecycle.stop );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  Perform( Lifecycle.shutdown );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void perform(Action action) throws Exception
		 private void Perform( Action action )
		 {
			  ExecutorService service = Executors.newFixedThreadPool( _lifecycles.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>();
			  IList<Future<object>> futures = new List<Future<object>>();
			  foreach ( Lifecycle lifecycle in _lifecycles )
			  {
					futures.Add(service.submit(() =>
					{
					 try
					 {
						  action.Act( lifecycle );
					 }
					 catch ( Exception e )
					 {
						  throw new Exception( e );
					 }
					}));
			  }

			  service.shutdown();
			  if ( !service.awaitTermination( _timeout, _unit ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
					foreach ( Future<object> future in futures )
					{
						 future.cancel( true );
					}
			  }

			  Exception exception = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					try
					{
						 future.get();
					}
					catch ( Exception e ) when ( e is InterruptedException || e is ExecutionException )
					{
						 if ( exception == null )
						 {
							  exception = new Exception();
						 }
						 exception.addSuppressed( e );
					}
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

		 private interface Action
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void act(Neo4Net.kernel.lifecycle.Lifecycle lifecycle) throws Throwable;
			  void Act( Lifecycle lifecycle );
		 }
	}

}