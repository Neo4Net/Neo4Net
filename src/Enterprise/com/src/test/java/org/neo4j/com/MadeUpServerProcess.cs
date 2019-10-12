using System;
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
namespace Neo4Net.com
{

	using Neo4Net.Test.subprocess;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;

	[Serializable]
	public class MadeUpServerProcess : SubProcess<ServerInterface, StartupData>, ServerInterface
	{
		 private const long SERIAL_VERSION_UID = 1L;

		 [NonSerialized]
		 private volatile MadeUpServer _server;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void startup(StartupData data) throws Throwable
		 protected internal override void Startup( StartupData data )
		 {
			  MadeUpCommunicationInterface implementation = new MadeUpServerImplementation( newStoreIdForCurrentVersion( data.CreationTime, data.StoreId, data.CreationTime, data.StoreId ) );
			  MadeUpServer localServer = new MadeUpServer( implementation, data.Port, data.InternalProtocolVersion, data.ApplicationProtocolVersion, TxChecksumVerifier_Fields.AlwaysMatch, data.ChunkSize );
			  localServer.Init();
			  localServer.Start();
			  // The field being non null is an indication of startup, so assign last
			  _server = localServer;
		 }

		 public override void AwaitStarted()
		 {
			  try
			  {
					long endTime = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.SECONDS.toMillis(60);
					while ( _server == null && DateTimeHelper.CurrentUnixTimeMillis() < endTime )
					{
						 Thread.Sleep( 10 );
					}
					if ( _server == null )
					{
						 throw new Exception( "Couldn't start server, wait timeout" );
					}
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( e );
			  }
		 }

		 protected internal override void Shutdown( bool normal )
		 {
			  if ( _server != null )
			  {
					try
					{
						 _server.stop();
						 _server.shutdown();
					}
					catch ( Exception throwable )
					{
						 throw new Exception( throwable );
					}
			  }
			  (new Thread(() =>
			  {
			  try
			  {
				  Thread.Sleep( 100 );
			  }
			  catch ( InterruptedException )
			  {
				  Thread.interrupted();
			  }
			  ShutdownProcess();
			  })).Start();
		 }

		 protected internal virtual void ShutdownProcess()
		 {
			  base.Shutdown();
		 }
	}

}