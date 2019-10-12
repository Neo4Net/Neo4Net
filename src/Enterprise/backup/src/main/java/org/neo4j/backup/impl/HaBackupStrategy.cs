using System;

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
namespace Neo4Net.backup.impl
{
	using ComException = Neo4Net.com.ComException;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	internal class HaBackupStrategy : LifecycleAdapter, BackupStrategy
	{
		 private readonly BackupProtocolService _backupProtocolService;
		 private readonly AddressResolver _addressResolver;
		 private readonly long _timeout;
		 private readonly Log _log;

		 internal HaBackupStrategy( BackupProtocolService backupProtocolService, AddressResolver addressResolver, LogProvider logProvider, long timeout )
		 {
			  this._backupProtocolService = backupProtocolService;
			  this._addressResolver = addressResolver;
			  this._timeout = timeout;
			  this._log = logProvider.GetLog( typeof( HaBackupStrategy ) );
		 }

		 public override Fallible<BackupStageOutcome> PerformIncrementalBackup( DatabaseLayout targetDatabaseLayout, Config config, OptionalHostnamePort fromAddress )
		 {
			  HostnamePort resolvedAddress = _addressResolver.resolveCorrectHAAddress( config, fromAddress );
			  _log.info( "Resolved address for backup protocol is " + resolvedAddress );
			  try
			  {
					string host = resolvedAddress.Host;
					int port = resolvedAddress.Port;
					_backupProtocolService.doIncrementalBackup( host, port, targetDatabaseLayout, ConsistencyCheck.NONE, _timeout, config );
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Success, null );
			  }
			  catch ( MismatchingStoreIdException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.UnrecoverableFailure, e );
			  }
			  catch ( Exception e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, e );
			  }
		 }

		 public override Fallible<BackupStageOutcome> PerformFullBackup( DatabaseLayout targetDatabaseLayout, Config config, OptionalHostnamePort userProvidedAddress )
		 {
			  HostnamePort fromAddress = _addressResolver.resolveCorrectHAAddress( config, userProvidedAddress );
			  _log.info( "Resolved address for backup protocol is " + fromAddress );
			  ConsistencyCheck consistencyCheck = ConsistencyCheck.NONE;
			  bool forensics = false;
			  try
			  {
					string host = fromAddress.Host;
					int port = fromAddress.Port;
					_backupProtocolService.doFullBackup( host, port, targetDatabaseLayout, consistencyCheck, config, _timeout, forensics );
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Success, null );
			  }
			  catch ( ComException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.WrongProtocol, e );
			  }
		 }
	}

}