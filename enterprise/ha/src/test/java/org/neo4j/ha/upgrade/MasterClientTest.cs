﻿/*
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
namespace Org.Neo4j.ha.upgrade
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;

	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using ResourceReleaser = Org.Neo4j.com.ResourceReleaser;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using StoreIdTestFactory = Org.Neo4j.com.StoreIdTestFactory;
	using Org.Neo4j.com;
	using TxChecksumVerifier = Org.Neo4j.com.TxChecksumVerifier;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ResponseUnpacker = Org.Neo4j.com.storecopy.ResponseUnpacker;
	using TransactionCommittingResponseUnpacker = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker;
	using Dependencies = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker.Dependencies;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MasterClient320 = Org.Neo4j.Kernel.ha.MasterClient320;
	using ConversationManager = Org.Neo4j.Kernel.ha.com.master.ConversationManager;
	using HandshakeResult = Org.Neo4j.Kernel.ha.com.master.HandshakeResult;
	using MasterImpl = Org.Neo4j.Kernel.ha.com.master.MasterImpl;
	using Monitor = Org.Neo4j.Kernel.ha.com.master.MasterImpl.Monitor;
	using MasterImplTest = Org.Neo4j.Kernel.ha.com.master.MasterImplTest;
	using MasterServer = Org.Neo4j.Kernel.ha.com.master.MasterServer;
	using MasterClient = Org.Neo4j.Kernel.ha.com.slave.MasterClient;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using MismatchingStoreIdException = Org.Neo4j.Kernel.impl.store.MismatchingStoreIdException;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Commands = Org.Neo4j.Kernel.impl.transaction.command.Commands;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.ResponseUnpacker_Fields.NO_OP_RESPONSE_UNPACKER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.TransactionCommittingResponseUnpacker.DEFAULT_BATCH_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Commands.createNode;

	public class MasterClientTest
	{
		 private const string MASTER_SERVER_HOST = "localhost";
		 private const int MASTER_SERVER_PORT = 9191;
		 private const int CHUNK_SIZE = 1024;
		 private const int TIMEOUT = 2000;
		 private const int TX_LOG_COUNT = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );
		 private readonly Monitors _monitors = new Monitors();
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.kernel.impl.store.MismatchingStoreIdException.class) public void newClientsShouldNotIgnoreStoreIdDifferences() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewClientsShouldNotIgnoreStoreIdDifferences()
		 {
			  // Given
			  MasterImpl.SPI masterImplSPI = MasterImplTest.mockedSpi( StoreIdTestFactory.newStoreIdForCurrentVersion( 1, 2, 3, 4 ) );
			  when( masterImplSPI.GetTransactionChecksum( anyLong() ) ).thenReturn(5L);

			  NewMasterServer( masterImplSPI );

			  StoreId storeId = StoreIdTestFactory.newStoreIdForCurrentVersion( 5, 6, 7, 8 );
			  MasterClient masterClient = NewMasterClient320( storeId );

			  // When
			  masterClient.Handshake( 1, storeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientShouldReadAndApplyTransactionLogsOnNewLockSessionRequest() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClientShouldReadAndApplyTransactionLogsOnNewLockSessionRequest()
		 {
			  // Given
			  MasterImpl master = spy( NewMasterImpl( MockMasterImplSpiWith( StoreId.DEFAULT ) ) );
			  doReturn( VoidResponseWithTransactionLogs() ).when(master).newLockSession(any(typeof(RequestContext)));

			  NewMasterServer( master );

			  TransactionCommittingResponseUnpacker.Dependencies deps = mock( typeof( TransactionCommittingResponseUnpacker.Dependencies ) );
			  TransactionCommitProcess commitProcess = mock( typeof( TransactionCommitProcess ) );
			  when( deps.CommitProcess() ).thenReturn(commitProcess);
			  when( deps.LogService() ).thenReturn(NullLogService.Instance);
			  when( deps.VersionContextSupplier() ).thenReturn(EmptyVersionContextSupplier.EMPTY);
			  KernelTransactions transactions = mock( typeof( KernelTransactions ) );
			  when( deps.KernelTransactions() ).thenReturn(transactions);

			  ResponseUnpacker unpacker = Life.add( new TransactionCommittingResponseUnpacker( deps, DEFAULT_BATCH_SIZE, 0 ) );

			  MasterClient masterClient = NewMasterClient320( StoreId.DEFAULT, unpacker );

			  // When
			  masterClient.NewLockSession( new RequestContext( 1, 2, 3, 4, 5 ) );

			  // Then
			  verify( commitProcess ).commit( any( typeof( TransactionToApply ) ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void endLockSessionDoesNotUnpackResponse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EndLockSessionDoesNotUnpackResponse()
		 {
			  StoreId storeId = new StoreId( 1, 2, 3, 4, 5 );
			  long txChecksum = 123;
			  long lastAppliedTx = 5;

			  ResponseUnpacker responseUnpacker = mock( typeof( ResponseUnpacker ) );
			  MasterImpl.SPI masterImplSPI = MasterImplTest.mockedSpi( storeId );
			  when( masterImplSPI.PackTransactionObligationResponse( any( typeof( RequestContext ) ), ArgumentMatchers.any() ) ).thenReturn(Response.empty());
			  when( masterImplSPI.GetTransactionChecksum( anyLong() ) ).thenReturn(txChecksum);

			  NewMasterServer( masterImplSPI );

			  MasterClient client = NewMasterClient320( storeId, responseUnpacker );

			  HandshakeResult handshakeResult;
			  using ( Response<HandshakeResult> handshakeResponse = client.Handshake( 1, storeId ) )
			  {
					handshakeResult = handshakeResponse.ResponseConflict();
			  }
			  verify( responseUnpacker ).unpackResponse( any( typeof( Response ) ), any( typeof( TxHandler ) ) );
			  reset( responseUnpacker );

			  RequestContext context = new RequestContext( handshakeResult.Epoch(), 1, 1, lastAppliedTx, txChecksum );

			  client.EndLockSession( context, false );
			  verify( responseUnpacker, never() ).unpackResponse(any(typeof(Response)), any(typeof(TxHandler)));
		 }

		 private static MasterImpl.SPI MockMasterImplSpiWith( StoreId storeId )
		 {
			  return when( mock( typeof( MasterImpl.SPI ) ).storeId() ).thenReturn(storeId).Mock;
		 }

		 private MasterServer NewMasterServer( MasterImpl.SPI masterImplSPI )
		 {
			  MasterImpl masterImpl = new MasterImpl( masterImplSPI, mock( typeof( ConversationManager ) ), mock( typeof( MasterImpl.Monitor ) ), MasterConfig() );

			  return NewMasterServer( masterImpl );
		 }

		 private static MasterImpl NewMasterImpl( MasterImpl.SPI masterImplSPI )
		 {
			  return new MasterImpl( masterImplSPI, mock( typeof( ConversationManager ) ), mock( typeof( MasterImpl.Monitor ) ), MasterConfig() );
		 }

		 private MasterServer NewMasterServer( MasterImpl masterImpl )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return Life.add( new MasterServer( masterImpl, NullLogProvider.Instance, MasterServerConfiguration(), mock(typeof(TxChecksumVerifier)), _monitors.newMonitor(typeof(ByteCounterMonitor), typeof(MasterClient).FullName), _monitors.newMonitor(typeof(RequestMonitor), typeof(MasterClient).FullName), mock(typeof(ConversationManager)), _logEntryReader ) );
		 }

		 private MasterClient NewMasterClient320( StoreId storeId )
		 {
			  return NewMasterClient320( storeId, NO_OP_RESPONSE_UNPACKER );
		 }

		 private MasterClient NewMasterClient320( StoreId storeId, ResponseUnpacker responseUnpacker )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return Life.add( new MasterClient320( MASTER_SERVER_HOST, MASTER_SERVER_PORT, null, NullLogProvider.Instance, storeId, TIMEOUT, TIMEOUT, 1, CHUNK_SIZE, responseUnpacker, _monitors.newMonitor( typeof( ByteCounterMonitor ), typeof( MasterClient320 ).FullName ), _monitors.newMonitor( typeof( RequestMonitor ), typeof( MasterClient320 ).FullName ), _logEntryReader ) );
		 }

		 private static Response<Void> VoidResponseWithTransactionLogs()
		 {
			  return new TransactionStreamResponse<Void>(null, StoreId.DEFAULT, visitor =>
			  {
				for ( int i = 1; i <= TX_LOG_COUNT; i++ )
				{
					 visitor.visit( CommittedTransactionRepresentation( i ) );
				}
			  }, ResourceReleaser.NO_OP);
		 }

		 private static CommittedTransactionRepresentation CommittedTransactionRepresentation( int id )
		 {
			  return new CommittedTransactionRepresentation( new LogEntryStart( id, id, id, id - 1, new sbyte[]{}, LogPosition.UNSPECIFIED ), Commands.transactionRepresentation( createNode( 0 ) ), new LogEntryCommit( id, id ) );
		 }

		 private static Config MasterConfig()
		 {
			  return Config.defaults( ClusterSettings.server_id, "1" );
		 }

		 private static Server.Configuration MasterServerConfiguration()
		 {
			  return new ConfigurationAnonymousInnerClass();
		 }

		 private class ConfigurationAnonymousInnerClass : Server.Configuration
		 {
			 public long OldChannelThreshold
			 {
				 get
				 {
					  return -1;
				 }
			 }

			 public int MaxConcurrentTransactions
			 {
				 get
				 {
					  return 1;
				 }
			 }

			 public int ChunkSize
			 {
				 get
				 {
					  return CHUNK_SIZE;
				 }
			 }

			 public HostnamePort ServerAddress
			 {
				 get
				 {
					  return new HostnamePort( MASTER_SERVER_HOST, MASTER_SERVER_PORT );
				 }
			 }
		 }
	}

}