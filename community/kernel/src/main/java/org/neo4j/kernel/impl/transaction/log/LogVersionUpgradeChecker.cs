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
namespace Org.Neo4j.Kernel.impl.transaction.log
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LogEntryVersion = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;

	/// <summary>
	/// Here we check the latest entry in the transaction log and make sure it matches the current version, if this check
	/// fails, it means that we will write entries with a version not compatible with the previous version responsible for
	/// creating the transaction logs.
	/// <para>
	/// This can be considered an upgrade since the user is not able to revert back to the previous version of neo4j. This
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
//ORIGINAL LINE: public static void check(org.neo4j.kernel.recovery.LogTailScanner tailScanner, org.neo4j.kernel.configuration.Config config) throws org.neo4j.kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException
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