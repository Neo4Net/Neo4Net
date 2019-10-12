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
namespace Org.Neo4j.backup.impl
{
	using Test = org.junit.Test;

	using BackupRequestType = Org.Neo4j.backup.impl.BackupClient.BackupRequestType;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ResponseUnpacker = Org.Neo4j.com.storecopy.ResponseUnpacker;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.jboss.netty.buffer.ChannelBuffers.EMPTY_BUFFER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class BackupProtocolIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGatherForensicsInFullBackupRequest()
		 public virtual void ShouldGatherForensicsInFullBackupRequest()
		 {
			  ShouldGatherForensicsInFullBackupRequest( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipGatheringForensicsInFullBackupRequest()
		 public virtual void ShouldSkipGatheringForensicsInFullBackupRequest()
		 {
			  ShouldGatherForensicsInFullBackupRequest( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNoForensicsSpecifiedInFullBackupRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNoForensicsSpecifiedInFullBackupRequest()
		 {
			  TheBackupInterface backup = mock( typeof( TheBackupInterface ) );
			  RequestContext ctx = new RequestContext( 0, 1, 0, -1, 12 );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.com.TargetCaller<org.neo4j.backup.TheBackupInterface, Void> targetCaller = (org.neo4j.com.TargetCaller<org.neo4j.backup.TheBackupInterface,Void>) org.neo4j.backup.impl.BackupClient.BackupRequestType.FULL_BACKUP.getTargetCaller();
			  TargetCaller<TheBackupInterface, Void> targetCaller = ( TargetCaller<TheBackupInterface, Void> ) BackupRequestType.FULL_BACKUP.TargetCaller;
			  targetCaller.Call( backup, ctx, EMPTY_BUFFER, null );
			  verify( backup ).fullBackup( any( typeof( StoreWriter ) ), eq( false ) );
		 }

		 private void ShouldGatherForensicsInFullBackupRequest( bool forensics )
		 {
			  // GIVEN
			  Response<Void> response = Response.empty();
			  StoreId storeId = response.StoreId;
			  string host = "localhost";
			  int port = PortAuthority.allocatePort();
			  LifeSupport life = new LifeSupport();

			  LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  ResponseUnpacker responseUnpacker = mock( typeof( ResponseUnpacker ) );
			  ByteCounterMonitor byteCounterMonitor = mock( typeof( ByteCounterMonitor ) );
			  RequestMonitor requestMonitor = mock( typeof( RequestMonitor ) );
			  BackupClient client = new BackupClient( host, port, null, logProvider, storeId, 10_000, responseUnpacker, byteCounterMonitor, requestMonitor, reader );
			  life.Add( client );
			  ControlledBackupInterface backup = new ControlledBackupInterface();
			  HostnamePort hostnamePort = new HostnamePort( host, port );
			  life.Add( new BackupServer( backup, hostnamePort, logProvider, byteCounterMonitor, requestMonitor ) );
			  life.Start();

			  try
			  {
					// WHEN
					StoreWriter writer = mock( typeof( StoreWriter ) );
					client.FullBackup( writer, forensics );

					// THEN
					assertEquals( forensics, backup.ReceivedForensics );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private class ControlledBackupInterface : TheBackupInterface
		 {
			  internal bool? ReceivedForensics;

			  public override Response<Void> FullBackup( StoreWriter writer, bool forensics )
			  {
					this.ReceivedForensics = forensics;
					writer.Dispose();
					return Response.empty();
			  }

			  public override Response<Void> IncrementalBackup( RequestContext context )
			  {
					throw new System.NotSupportedException( "Should be required" );
			  }
		 }
	}

}