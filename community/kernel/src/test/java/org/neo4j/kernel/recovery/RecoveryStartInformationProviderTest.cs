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
namespace Org.Neo4j.Kernel.recovery
{
	using Test = org.junit.Test;

	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using CheckPoint = Org.Neo4j.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntryVersion = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogTailInformation = Org.Neo4j.Kernel.recovery.LogTailScanner.LogTailInformation;
	using Monitor = Org.Neo4j.Kernel.recovery.RecoveryStartInformationProvider.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionRepository_Fields.INITIAL_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.recovery.LogTailScanner.NO_TRANSACTION_ID;

	public class RecoveryStartInformationProviderTest
	{
		 private readonly long _currentLogVersion = 2L;
		 private readonly long _logVersion = 2L;
		 private readonly LogTailScanner _tailScanner = mock( typeof( LogTailScanner ) );
		 private readonly Monitor _monitor = mock( typeof( Monitor ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUnspecifiedIfThereIsNoNeedForRecovery()
		 public virtual void ShouldReturnUnspecifiedIfThereIsNoNeedForRecovery()
		 {
			  // given
			  when( _tailScanner.TailInformation ).thenReturn( new LogTailScanner.LogTailInformation( false, NO_TRANSACTION_ID, _logVersion, _currentLogVersion, LogEntryVersion.CURRENT ) );

			  // when
			  RecoveryStartInformation recoveryStartInformation = ( new RecoveryStartInformationProvider( _tailScanner, _monitor ) ).get();

			  // then
			  verify( _monitor ).noCommitsAfterLastCheckPoint( null );
			  assertEquals( LogPosition.UNSPECIFIED, recoveryStartInformation.RecoveryPosition );
			  assertEquals( NO_TRANSACTION_ID, recoveryStartInformation.FirstTxIdAfterLastCheckPoint );
			  assertFalse( recoveryStartInformation.RecoveryRequired );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnLogPositionToRecoverFromIfNeeded()
		 public virtual void ShouldReturnLogPositionToRecoverFromIfNeeded()
		 {
			  // given
			  LogPosition checkPointLogPosition = new LogPosition( 1L, 4242 );
			  when( _tailScanner.TailInformation ).thenReturn( new LogTailInformation( new CheckPoint( checkPointLogPosition ), true, 10L, _logVersion, _currentLogVersion, LogEntryVersion.CURRENT ) );

			  // when
			  RecoveryStartInformation recoveryStartInformation = ( new RecoveryStartInformationProvider( _tailScanner, _monitor ) ).get();

			  // then
			  verify( _monitor ).commitsAfterLastCheckPoint( checkPointLogPosition, 10L );
			  assertEquals( checkPointLogPosition, recoveryStartInformation.RecoveryPosition );
			  assertEquals( 10L, recoveryStartInformation.FirstTxIdAfterLastCheckPoint );
			  assertTrue( recoveryStartInformation.RecoveryRequired );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromStartOfLogZeroIfThereAreNoCheckPointAndOldestLogIsVersionZero()
		 public virtual void ShouldRecoverFromStartOfLogZeroIfThereAreNoCheckPointAndOldestLogIsVersionZero()
		 {
			  // given
			  when( _tailScanner.TailInformation ).thenReturn( new LogTailInformation( true, 10L, INITIAL_LOG_VERSION, _currentLogVersion, LogEntryVersion.CURRENT ) );

			  // when
			  RecoveryStartInformation recoveryStartInformation = ( new RecoveryStartInformationProvider( _tailScanner, _monitor ) ).get();

			  // then
			  verify( _monitor ).noCheckPointFound();
			  assertEquals( LogPosition.start( INITIAL_LOG_VERSION ), recoveryStartInformation.RecoveryPosition );
			  assertEquals( 10L, recoveryStartInformation.FirstTxIdAfterLastCheckPoint );
			  assertTrue( recoveryStartInformation.RecoveryRequired );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfThereAreNoCheckPointsAndOldestLogVersionInNotZero()
		 public virtual void ShouldFailIfThereAreNoCheckPointsAndOldestLogVersionInNotZero()
		 {
			  // given
			  long oldestLogVersionFound = 1L;
			  when( _tailScanner.TailInformation ).thenReturn( new LogTailScanner.LogTailInformation( true, 10L, oldestLogVersionFound, _currentLogVersion, LogEntryVersion.CURRENT ) );

			  // when
			  try
			  {
					( new RecoveryStartInformationProvider( _tailScanner, _monitor ) ).get();
			  }
			  catch ( UnderlyingStorageException ex )
			  {
					// then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String expectedMessage = "No check point found in any log file from version " + oldestLogVersionFound + " to " + logVersion;
					string expectedMessage = "No check point found in any log file from version " + oldestLogVersionFound + " to " + _logVersion;
					assertEquals( expectedMessage, ex.Message );
			  }
		 }
	}

}