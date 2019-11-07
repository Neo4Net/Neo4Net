﻿/*
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
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;

	/// <summary>
	/// Here we check the latest entry in the transaction log and make sure it matches the current version, if this check
	/// fails, it means that we will write entries with a version not compatible with the previous version responsible for
	/// creating the transaction logs.
	/// <para>
	/// This can be considered an upgrade since the user is not able to revert back to the previous version of Neo4Net. This
	/// will effectively guard the users from accidental upgrades.
	/// </para>
	/// </summary>
	public class LogVersionUpgradeChecker
	{
		 private LogVersionUpgradeChecker()
		 {
			  throw new AssertionError( "No instances allowed" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void check(Neo4Net.kernel.recovery.LogTailScanner tailScanner, Neo4Net.kernel.configuration.Config config) throws Neo4Net.kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException
		 public static void Check( LogTailScanner tailScanner, Config config )
		 {
			  if ( !config.Get( GraphDatabaseSettings.allow_upgrade ) )
			  {
					// The user doesn't want us to upgrade the store.
					LogEntryVersion latestLogEntryVersion = tailScanner.TailInformation.latestLogEntryVersion;
					if ( latestLogEntryVersion != null && LogEntryVersion.moreRecentVersionExists( latestLogEntryVersion ) )
					{
						 string message = "The version you're upgrading to is using a new transaction log format. This is a " +
									"non-reversible upgrade and you wont be able to downgrade after starting";

						 throw new UpgradeNotAllowedByConfigurationException( message );
					}
			  }
		 }
	}

}