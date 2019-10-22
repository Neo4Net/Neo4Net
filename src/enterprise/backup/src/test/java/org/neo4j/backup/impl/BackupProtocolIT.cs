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
namespace Neo4Net.backup.impl
{
	using Test = org.junit.Test;

	using BackupRequestType = Neo4Net.backup.impl.BackupClient.BackupRequestType;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using Neo4Net.com;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

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
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.com.TargetCaller<org.Neo4Net.backup.TheBackupInterface, Void> targetCaller = (org.Neo4Net.com.TargetCaller<org.Neo4Net.backup.TheBackupInterface,Void>) org.Neo4Net.backup.impl.BackupClient.BackupRequestType.FULL_BACKUP.getTargetCaller();
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