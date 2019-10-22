/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LogVersionUpgradeCheckerTest
	{
		 private LogTailScanner _tailScanner = mock( typeof( LogTailScanner ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noThrowWhenLatestVersionAndUpgradeIsNotAllowed()
		 public virtual void NoThrowWhenLatestVersionAndUpgradeIsNotAllowed()
		 {
			  when( _tailScanner.TailInformation ).thenReturn( new OnlyVersionTailInformation( LogEntryVersion.CURRENT ) );

			  LogVersionUpgradeChecker.Check( _tailScanner, Config.defaults( GraphDatabaseSettings.allow_upgrade, "false" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwWhenVersionIsOlderAndUpgradeIsNotAllowed()
		 public virtual void ThrowWhenVersionIsOlderAndUpgradeIsNotAllowed()
		 {
			  when( _tailScanner.TailInformation ).thenReturn( new OnlyVersionTailInformation( LogEntryVersion.V2_3 ) );

			  Expect.expect( typeof( UpgradeNotAllowedByConfigurationException ) );

			  LogVersionUpgradeChecker.Check( _tailScanner, Config.defaults( GraphDatabaseSettings.allow_upgrade, "false" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stillAcceptLatestVersionWhenUpgradeIsAllowed()
		 public virtual void StillAcceptLatestVersionWhenUpgradeIsAllowed()
		 {
			  when( _tailScanner.TailInformation ).thenReturn( new OnlyVersionTailInformation( LogEntryVersion.CURRENT ) );

			  LogVersionUpgradeChecker.Check( _tailScanner, Config.defaults( GraphDatabaseSettings.allow_upgrade, "true" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acceptOlderLogsWhenUpgradeIsAllowed()
		 public virtual void AcceptOlderLogsWhenUpgradeIsAllowed()
		 {
			  when( _tailScanner.TailInformation ).thenReturn( new OnlyVersionTailInformation( LogEntryVersion.V2_3 ) );

			  LogVersionUpgradeChecker.Check( _tailScanner, Config.defaults( GraphDatabaseSettings.allow_upgrade, "true" ) );
		 }

		 private class OnlyVersionTailInformation : LogTailScanner.LogTailInformation
		 {
			  internal OnlyVersionTailInformation( LogEntryVersion logEntryVersion ) : base( false, 0, 0, 0, logEntryVersion )
			  {
			  }
		 }
	}

}