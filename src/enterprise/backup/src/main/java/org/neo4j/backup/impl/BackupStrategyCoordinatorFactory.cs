﻿using System.Collections.Generic;

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

	using StoreFiles = Neo4Net.causalclustering.catchup.storecopy.StoreFiles;
	using FileMoveProvider = Neo4Net.com.storecopy.FileMoveProvider;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/*
	 * Backup strategy coordinators iterate through backup strategies and make sure at least one of them can perform a valid backup.
	 * Handles cases when individual backups aren't  possible.
	 */
	internal class BackupStrategyCoordinatorFactory
	{
		 private readonly LogProvider _logProvider;
		 private readonly ConsistencyCheckService _consistencyCheckService;
		 private readonly AddressResolver _addressResolver;
		 private readonly OutsideWorld _outsideWorld;

		 internal BackupStrategyCoordinatorFactory( BackupModule backupModule )
		 {
			  this._logProvider = backupModule.LogProvider;
			  this._outsideWorld = backupModule.OutsideWorld;

			  this._consistencyCheckService = new ConsistencyCheckService();
			  this._addressResolver = new AddressResolver();
		 }

		 /// <summary>
		 /// Construct a wrapper of supported backup strategies
		 /// </summary>
		 /// <param name="onlineBackupContext"> the input of the backup tool, such as CLI arguments, config etc. </param>
		 /// <param name="backupProtocolService"> the underlying backup implementation for HA and single node instances </param>
		 /// <param name="backupDelegator"> the backup implementation used for CC backups </param>
		 /// <param name="pageCache"> the page cache used moving files </param>
		 /// <returns> strategy coordinator that handles the which backup strategies are tried and establishes if a backup was successful or not </returns>
		 internal virtual BackupStrategyCoordinator BackupStrategyCoordinator( OnlineBackupContext onlineBackupContext, BackupProtocolService backupProtocolService, BackupDelegator backupDelegator, PageCache pageCache )
		 {
			  FileSystemAbstraction fs = _outsideWorld.fileSystem();
			  BackupCopyService copyService = new BackupCopyService( fs, new FileMoveProvider( fs ) );
			  ProgressMonitorFactory progressMonitorFactory = ProgressMonitorFactory.textual( _outsideWorld.errorStream() );
			  BackupRecoveryService recoveryService = new BackupRecoveryService();
			  long timeout = onlineBackupContext.RequiredArguments.Timeout;
			  Config config = onlineBackupContext.Config;

			  StoreFiles storeFiles = new StoreFiles( fs, pageCache );
			  BackupStrategy ccStrategy = new CausalClusteringBackupStrategy( backupDelegator, _addressResolver, _logProvider, storeFiles );
			  BackupStrategy haStrategy = new HaBackupStrategy( backupProtocolService, _addressResolver, _logProvider, timeout );

			  BackupStrategyWrapper ccStrategyWrapper = Wrap( ccStrategy, copyService, pageCache, config, recoveryService );
			  BackupStrategyWrapper haStrategyWrapper = Wrap( haStrategy, copyService, pageCache, config, recoveryService );
			  StrategyResolverService strategyResolverService = new StrategyResolverService( haStrategyWrapper, ccStrategyWrapper );
			  IList<BackupStrategyWrapper> strategies = strategyResolverService.GetStrategies( onlineBackupContext.RequiredArguments.SelectedBackupProtocol );

			  return new BackupStrategyCoordinator( _consistencyCheckService, _outsideWorld, _logProvider, progressMonitorFactory, strategies );
		 }

		 private BackupStrategyWrapper Wrap( BackupStrategy strategy, BackupCopyService copyService, PageCache pageCache, Config config, BackupRecoveryService recoveryService )
		 {
			  return new BackupStrategyWrapper( strategy, copyService, pageCache, config, recoveryService, _logProvider );
		 }
	}

}