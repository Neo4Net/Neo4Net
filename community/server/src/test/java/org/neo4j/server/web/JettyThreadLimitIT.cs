using System;

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
namespace Org.Neo4j.Server.web
{
	using QueuedThreadPool = org.eclipse.jetty.util.thread.QueuedThreadPool;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class JettyThreadLimitIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public readonly SuppressOutput SuppressOutput = suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigurableJettyThreadPoolSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveConfigurableJettyThreadPoolSize()
		 {
			  Jetty9WebServer server = new Jetty9WebServer( NullLogProvider.Instance, Config.defaults(), NetworkConnectionTracker.NO_OP );
			  int numCores = 1;
			  int configuredMaxThreads = 12; // 12 is the new min max Threads value, for one core
			  int acceptorThreads = 1; // In this configuration, 1 thread will become an acceptor...
			  int selectorThreads = 1; // ... and 1 thread will become a selector...
			  int jobThreads = configuredMaxThreads - acceptorThreads - selectorThreads; // ... and the rest are job threads
			  server.MaxThreads = numCores;
			  server.HttpAddress = new ListenSocketAddress( "localhost", 0 );
			  try
			  {
					server.Start();
					QueuedThreadPool threadPool = ( QueuedThreadPool ) server.Jetty.ThreadPool;
					threadPool.start();
					System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( jobThreads );
					System.Threading.CountdownEvent endLatch = LoadThreadPool( threadPool, configuredMaxThreads + 1, startLatch );
					startLatch.await(); // Wait for threadPool to create threads
					int threads = threadPool.Threads;
					assertEquals( "Wrong number of threads in pool", configuredMaxThreads, threads );
					endLatch.Signal();
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static java.util.concurrent.CountDownLatch loadThreadPool(org.eclipse.jetty.util.thread.QueuedThreadPool threadPool, int tasksToSubmit, final java.util.concurrent.CountDownLatch startLatch)
		 private static System.Threading.CountdownEvent LoadThreadPool( QueuedThreadPool threadPool, int tasksToSubmit, System.Threading.CountdownEvent startLatch )
		 {
			  System.Threading.CountdownEvent endLatch = new System.Threading.CountdownEvent( 1 );
			  for ( int i = 0; i < tasksToSubmit; i++ )
			  {
					threadPool.execute(() =>
					{
					 startLatch.Signal();
					 try
					 {
						  endLatch.await();
					 }
					 catch ( InterruptedException e )
					 {
						  Console.WriteLine( e.ToString() );
						  Console.Write( e.StackTrace );
					 }
					});
			  }
			  return endLatch;
		 }
	}

}