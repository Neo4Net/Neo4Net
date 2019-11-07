using System;

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
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using ComException = Neo4Net.com.ComException;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.backup.impl.BackupStageOutcome.SUCCESS;

	public class BackupProtocolServiceStrategyTest
	{
		 private BackupProtocolService _backupProtocolService = mock( typeof( BackupProtocolService ) );

		 internal HaBackupStrategy Subject;

		 internal Config Config = mock( typeof( Config ) );
		 internal OnlineBackupRequiredArguments RequiredArgs = mock( typeof( OnlineBackupRequiredArguments ) );
		 internal OnlineBackupContext OnlineBackupContext = mock( typeof( OnlineBackupContext ) );
		 internal AddressResolver AddressResolver = mock( typeof( AddressResolver ) );
		 internal HostnamePort HostnamePort = new HostnamePort( "hostname:1234" );
		 internal DatabaseLayout Backuplayout = mock( typeof( DatabaseLayout ) );
		 internal OptionalHostnamePort UserSpecifiedHostname = new OptionalHostnamePort( ( string ) null, null, null );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( OnlineBackupContext.RequiredArguments ).thenReturn( RequiredArgs );
			  when( AddressResolver.resolveCorrectHAAddress( any(), any() ) ).thenReturn(HostnamePort);
			  Subject = new HaBackupStrategy( _backupProtocolService, AddressResolver, NullLogProvider.Instance, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalBackupsAreDoneAgainstResolvedAddress()
		 public virtual void IncrementalBackupsAreDoneAgainstResolvedAddress()
		 {
			  // when
			  Fallible<BackupStageOutcome> state = Subject.performIncrementalBackup( Backuplayout, Config, UserSpecifiedHostname );

			  // then
			  verify( _backupProtocolService ).doIncrementalBackup( eq( HostnamePort.Host ), eq( HostnamePort.Port ), any(), eq(ConsistencyCheck.NONE), anyLong(), any() );
			  assertEquals( SUCCESS, state.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionsDuringIncrementalBackupAreMarkedAsFailedBackups()
		 public virtual void ExceptionsDuringIncrementalBackupAreMarkedAsFailedBackups()
		 {
			  // given incremental backup will fail
			  IncrementalBackupNotPossibleException expectedException = new IncrementalBackupNotPossibleException( "Expected test message", new Exception( "Expected cause" ) );
			  when( _backupProtocolService.doIncrementalBackup( any(), anyInt(), any(), eq(ConsistencyCheck.NONE), anyLong(), any() ) ).thenThrow(expectedException);

			  // when
			  Fallible state = Subject.performIncrementalBackup( Backuplayout, Config, UserSpecifiedHostname );

			  // then
			  assertEquals( BackupStageOutcome.Failure, state.State );
			  assertEquals( expectedException, state.Cause.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupUsesResolvedAddress()
		 public virtual void FullBackupUsesResolvedAddress()
		 {
			  // when
			  Fallible state = Subject.performFullBackup( Backuplayout, Config, UserSpecifiedHostname );

			  // then
			  verify( _backupProtocolService ).doFullBackup( any(), anyInt(), any(), eq(ConsistencyCheck.NONE), any(), anyLong(), anyBoolean() );
			  assertEquals( BackupStageOutcome.Success, state.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupFailsWithCauseOnException()
		 public virtual void FullBackupFailsWithCauseOnException()
		 {
			  // given full backup fails with a protocol/network exception
			  when( _backupProtocolService.doFullBackup( any(), anyInt(), any(), any(), any(), anyLong(), anyBoolean() ) ).thenThrow(typeof(ComException));

			  // when
			  Fallible state = Subject.performFullBackup( Backuplayout, Config, UserSpecifiedHostname );

			  // then
			  assertEquals( BackupStageOutcome.WrongProtocol, state.State );
			  assertEquals( typeof( ComException ), state.Cause.get().GetType() );
		 }
	}

}