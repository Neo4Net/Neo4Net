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
namespace Neo4Net.Kernel.impl.recovery
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using RecoveryStartInformationProvider = Neo4Net.Kernel.recovery.RecoveryStartInformationProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.recovery.RecoveryStartInformationProvider.NO_MONITOR;

	/// <summary>
	/// An external tool that can determine if a given store will need recovery.
	/// </summary>
	public class RecoveryRequiredChecker
	{
		 private readonly FileSystemAbstraction _fs;
		 private readonly PageCache _pageCache;
		 private readonly Monitors _monitors;
		 private Config _config;

		 public RecoveryRequiredChecker( FileSystemAbstraction fs, PageCache pageCache, Config config, Monitors monitors )
		 {
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._config = config;
			  this._monitors = monitors;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isRecoveryRequiredAt(Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public virtual bool IsRecoveryRequiredAt( DatabaseLayout databaseLayout )
		 {
			  // We need config to determine where the logical log files are
			  if ( !NeoStores.isStorePresent( _pageCache, databaseLayout ) )
			  {
					return false;
			  }

			  LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( databaseLayout, _fs, _pageCache ).withConfig( _config ).withLogEntryReader( reader ).build();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, reader, _monitors );
			  return ( new RecoveryStartInformationProvider( tailScanner, NO_MONITOR ) ).get().RecoveryRequired;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertRecoveryIsNotRequired(Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.pagecache.PageCache pageCache, Neo4Net.kernel.configuration.Config config, Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.kernel.monitoring.Monitors monitors) throws RecoveryRequiredException, java.io.IOException
		 public static void AssertRecoveryIsNotRequired( FileSystemAbstraction fs, PageCache pageCache, Config config, DatabaseLayout databaseLayout, Monitors monitors )
		 {
			  if ( ( new RecoveryRequiredChecker( fs, pageCache, config, monitors ) ).IsRecoveryRequiredAt( databaseLayout ) )
			  {
					throw new RecoveryRequiredException();
			  }
		 }
	}

}