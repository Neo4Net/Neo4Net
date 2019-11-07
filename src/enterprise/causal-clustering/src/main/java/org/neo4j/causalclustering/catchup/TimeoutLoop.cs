using System;
using System.Threading;

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
namespace Neo4Net.causalclustering.catchup
{

	using Log = Neo4Net.Logging.Log;

	internal class TimeoutLoop
	{
		 private TimeoutLoop()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static <T> T waitForCompletion(java.util.concurrent.Future<T> future, String operation, System.Func<java.util.Optional<long>> millisSinceLastResponseSupplier, long inactivityTimeoutMillis, Neo4Net.logging.Log log) throws CatchUpClientException
		 internal static T WaitForCompletion<T>( Future<T> future, string operation, System.Func<long?> millisSinceLastResponseSupplier, long inactivityTimeoutMillis, Log log )
		 {
			  long remainingTimeoutMillis = inactivityTimeoutMillis;
			  while ( true )
			  {
					try
					{
						 return future.get( remainingTimeoutMillis, TimeUnit.MILLISECONDS );
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw Exception( future, operation, e );
					}
					catch ( ExecutionException e )
					{
						 throw Exception( future, operation, e );
					}
					catch ( TimeoutException e )
					{
						 if ( !millisSinceLastResponseSupplier().Present )
						 {
							  log.Info( "Request timed out with no responses after " + inactivityTimeoutMillis + " ms." );
							  throw Exception( future, operation, e );
						 }
						 else
						 {
							  long millisSinceLastResponse = millisSinceLastResponseSupplier().get();
							  if ( millisSinceLastResponse < inactivityTimeoutMillis )
							  {
									remainingTimeoutMillis = inactivityTimeoutMillis - millisSinceLastResponse;
							  }
							  else
							  {
									log.Info( "Request timed out after period of inactivity. Time since last response: " + millisSinceLastResponse + " ms." );
									throw Exception( future, operation, e );
							  }
						 }
					}
			  }
		 }

		 private static CatchUpClientException Exception<T1>( Future<T1> future, string operation, Exception e )
		 {
			  future.cancel( true );
			  return new CatchUpClientException( operation, e );
		 }
	}

}