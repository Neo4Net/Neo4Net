﻿using System;
using System.Collections.Generic;
using System.Threading;

/*
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
namespace Org.Neo4j.causalclustering.core.replication
{

	using DummyRequest = Org.Neo4j.causalclustering.core.state.machines.dummy.DummyRequest;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using Log = Org.Neo4j.Logging.Log;
	using Admin = Org.Neo4j.Procedure.Admin;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class ReplicationBenchmarkProcedure
	public class ReplicationBenchmarkProcedure
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Replicator replicator;
		 public Replicator Replicator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.internal.kernel.api.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
		 public Log Log;

		 private static long _startTime;
		 private static IList<Worker> _workers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Start the benchmark.") @Procedure(name = "dbms.cluster.benchmark.start", mode = DBMS) public synchronized void start(@Name("nThreads") System.Nullable<long> nThreads, @Name("blockSize") System.Nullable<long> blockSize)
		 [Description("Start the benchmark."), Procedure(name : "dbms.cluster.benchmark.start", mode : DBMS)]
		 public virtual void Start( long? nThreads, long? blockSize )
		 {
			 lock ( this )
			 {
				  if ( _workers != null )
				  {
						throw new System.InvalidOperationException( "Already running." );
				  }
      
				  Log.info( "Starting replication benchmark procedure" );
      
				  _startTime = DateTimeHelper.CurrentUnixTimeMillis();
				  _workers = new List<Worker>( toIntExact( nThreads ) );
      
				  for ( int i = 0; i < nThreads; i++ )
				  {
						Worker worker = new Worker( this, toIntExact( blockSize ) );
						_workers.Add( worker );
						worker.Start();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Stop a running benchmark.") @Procedure(name = "dbms.cluster.benchmark.stop", mode = DBMS) public synchronized java.util.stream.Stream<BenchmarkResult> stop() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Stop a running benchmark."), Procedure(name : "dbms.cluster.benchmark.stop", mode : DBMS)]
		 public virtual Stream<BenchmarkResult> Stop()
		 {
			 lock ( this )
			 {
				  if ( _workers == null )
				  {
						throw new System.InvalidOperationException( "Not running." );
				  }
      
				  Log.info( "Stopping replication benchmark procedure" );
      
				  foreach ( Worker worker in _workers )
				  {
						worker.Stop();
				  }
      
				  foreach ( Worker worker in _workers )
				  {
						worker.Join();
				  }
      
				  long runTime = DateTimeHelper.CurrentUnixTimeMillis() - _startTime;
      
				  long totalRequests = 0;
				  long totalBytes = 0;
      
				  foreach ( Worker worker in _workers )
				  {
						totalRequests += worker.TotalRequests;
						totalBytes += worker.TotalBytes;
				  }
      
				  _workers = null;
      
				  return Stream.of( new BenchmarkResult( totalRequests, totalBytes, runTime ) );
			 }
		 }

		 private class Worker : ThreadStart
		 {
			 private readonly ReplicationBenchmarkProcedure _outerInstance;

			  internal readonly int BlockSize;

			  internal long TotalRequests;
			  internal long TotalBytes;

			  internal Thread T;
			  internal volatile bool Stopped;

			  internal Worker( ReplicationBenchmarkProcedure outerInstance, int blockSize )
			  {
				  this._outerInstance = outerInstance;
					this.BlockSize = blockSize;
			  }

			  internal virtual void Start()
			  {
					T = new Thread( this );
					T.Start();
			  }

			  public override void Run()
			  {
					try
					{
						 while ( !Stopped )
						 {
							  Future<object> future = outerInstance.Replicator.replicate( new DummyRequest( new sbyte[BlockSize] ), true );
							  DummyRequest request = ( DummyRequest ) future.get();
							  TotalRequests++;
							  TotalBytes += request.ByteCount();
						 }
					}
					catch ( Exception e )
					{
						 outerInstance.Log.error( "Worker exception", e );
					}
			  }

			  internal virtual void Stop()
			  {
					Stopped = true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void join() throws InterruptedException
			  internal virtual void Join()
			  {
					T.Join();
			  }
		 }
	}

}