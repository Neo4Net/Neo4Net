using System;
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
namespace Org.Neo4j.Server
{
	using Test = org.junit.Test;


	using PageEvictionCallback = Org.Neo4j.Io.pagecache.PageEvictionCallback;
	using PageSwapper = Org.Neo4j.Io.pagecache.PageSwapper;
	using SingleFilePageSwapperFactory = Org.Neo4j.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using ThreadTestUtils = Org.Neo4j.Test.ThreadTestUtils;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class NeoServerRestartTestIT : ExclusiveServerTestBase
	{
		 public const string CUSTOM_SWAPPER = "CustomSwapper";
		 private static Semaphore _semaphore;

		 static NeoServerRestartTestIT()
		 {
			  _semaphore = new Semaphore( 0 );
		 }

		 /// <summary>
		 /// This test makes sure that the database is able to start after having been stopped during initialization.
		 /// 
		 /// In order to make sure that the server is stopped during startup we create a separate thread that calls stop.
		 /// In order to make sure that this thread does not call stop before the startup procedure has started we use a
		 /// custom implementation of a PageSwapperFactory, which communicates with the thread that calls stop. We do this
		 /// via a static semaphore. </summary>
		 /// <exception cref="IOException"> </exception>
		 /// <exception cref="InterruptedException"> </exception>

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRestartWhenStoppedDuringStartup() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRestartWhenStoppedDuringStartup()
		 {
			  // Make sure that the semaphore is in a clean state.
			  _semaphore.drainPermits();
			  // Get a server that uses our custom swapper.
			  NeoServer server = GetNeoServer( CUSTOM_SWAPPER );

			  try
			  {
					AtomicBoolean failure = new AtomicBoolean();
					Thread serverStoppingThread = ThreadTestUtils.fork( StopServerAfterStartingHasStarted( server, failure ) );
					server.Start();
					// Wait for the server to stop.
					serverStoppingThread.Join();
					// Check if the server stopped successfully.
					if ( failure.get() )
					{
						 fail( "Server failed to stop." );
					}
					// Verify that we can start the server again.
					server = GetNeoServer( CUSTOM_SWAPPER );
					server.Start();
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract NeoServer getNeoServer(String customPageSwapperName) throws java.io.IOException;
		 protected internal abstract NeoServer GetNeoServer( string customPageSwapperName );

		 private ThreadStart StopServerAfterStartingHasStarted( NeoServer server, AtomicBoolean failure )
		 {
			  return () =>
			  {
				try
				{
					 // Make sure that we have started the startup procedure before calling stop.
					 _semaphore.acquire();
					 server.Stop();
				}
				catch ( Exception )
				{
					 failure.set( true );
				}
			  };
		 }

		 // This class is used to notify the test that the server has started its startup procedure.
		 public class CustomSwapper : SingleFilePageSwapperFactory
		 {
			  public override string ImplementationName()
			  {
					return CUSTOM_SWAPPER;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageSwapper createPageSwapper(java.io.File file, int filePageSize, org.neo4j.io.pagecache.PageEvictionCallback onEviction, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException
			  public override PageSwapper CreatePageSwapper( File file, int filePageSize, PageEvictionCallback onEviction, bool createIfNotExist, bool noChannelStriping )
			  {
					// This will be called early in the startup sequence. Notifies that we can call stop on the server.
					_semaphore.release();
					return base.CreatePageSwapper( file, filePageSize, onEviction, createIfNotExist, noChannelStriping );
			  }
		 }
	}

}