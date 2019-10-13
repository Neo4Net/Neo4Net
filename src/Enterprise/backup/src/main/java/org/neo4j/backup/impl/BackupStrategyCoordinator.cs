using System;
using System.Collections.Generic;

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

	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Controls the outcome of the backup tool.
	/// Iterates over multiple backup strategies and stops when a backup was successful, there was a critical failure or
	/// when none of the backups worked.
	/// Also handles the consistency check
	/// </summary>
	internal class BackupStrategyCoordinator
	{
		 private const int STATUS_CC_ERROR = 2;
		 private const int STATUS_CC_INCONSISTENT = 3;

		 private ConsistencyCheckService _consistencyCheckService;
		 private readonly OutsideWorld _outsideWorld;
		 private readonly LogProvider _logProvider;
		 private readonly ProgressMonitorFactory _progressMonitorFactory;
		 private readonly IList<BackupStrategyWrapper> _strategies;

		 internal BackupStrategyCoordinator( ConsistencyCheckService consistencyCheckService, OutsideWorld outsideWorld, LogProvider logProvider, ProgressMonitorFactory progressMonitorFactory, IList<BackupStrategyWrapper> strategies )
		 {
			  this._consistencyCheckService = consistencyCheckService;
			  this._outsideWorld = outsideWorld;
			  this._logProvider = logProvider;
			  this._progressMonitorFactory = progressMonitorFactory;
			  this._strategies = strategies;
		 }

		 /// <summary>
		 /// Iterate over all the provided strategies trying to perform a successful backup.
		 /// Will also do consistency checks if specified in <seealso cref="OnlineBackupContext"/>
		 /// </summary>
		 /// <param name="onlineBackupContext"> filesystem, command arguments and configuration </param>
		 /// <exception cref="CommandFailed"> when backup failed or there were issues with consistency checks </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void performBackup(OnlineBackupContext onlineBackupContext) throws org.neo4j.commandline.admin.CommandFailed
		 public virtual void PerformBackup( OnlineBackupContext onlineBackupContext )
		 {
			  // Convenience
			  OnlineBackupRequiredArguments requiredArgs = onlineBackupContext.RequiredArguments;
			  Path destination = onlineBackupContext.ResolvedLocationFromName;
			  ConsistencyFlags consistencyFlags = onlineBackupContext.ConsistencyFlags;

			  Fallible<BackupStrategyOutcome> throwableWithState = null;
			  IList<Exception> causesOfFailure = new List<Exception>();
			  foreach ( BackupStrategyWrapper backupStrategy in _strategies )
			  {
					throwableWithState = backupStrategy.DoBackup( onlineBackupContext );
					if ( throwableWithState.State == BackupStrategyOutcome.Success )
					{
						 break;
					}
					if ( throwableWithState.State == BackupStrategyOutcome.CorrectStrategyFailed )
					{
						 throw CommandFailedWithCause( throwableWithState ).get();
					}
					throwableWithState.Cause.ifPresent( causesOfFailure.add );
			  }
			  if ( throwableWithState == null || !BackupStrategyOutcome.Success.Equals( throwableWithState.State ) )
			  {
					CommandFailed commandFailed = new CommandFailed( "Failed to run a backup using the available strategies." );
					causesOfFailure.ForEach( commandFailed.addSuppressed );
					throw commandFailed;
			  }
			  if ( requiredArgs.DoConsistencyCheck )
			  {
					PerformConsistencyCheck( onlineBackupContext.Config, requiredArgs, consistencyFlags, DatabaseLayout.of( destination.toFile() ) );
			  }
		 }

		 private static System.Func<CommandFailed> CommandFailedWithCause( Fallible<BackupStrategyOutcome> cause )
		 {
			  if ( cause.Cause.Present )
			  {
					return () => new CommandFailed("Execution of backup failed", cause.Cause.get());
			  }
			  return () => new CommandFailed("Execution of backup failed");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void performConsistencyCheck(org.neo4j.kernel.configuration.Config config, OnlineBackupRequiredArguments requiredArgs, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags, org.neo4j.io.layout.DatabaseLayout layout) throws org.neo4j.commandline.admin.CommandFailed
		 private void PerformConsistencyCheck( Config config, OnlineBackupRequiredArguments requiredArgs, ConsistencyFlags consistencyFlags, DatabaseLayout layout )
		 {
			  try
			  {
					bool verbose = false;
					File reportDir = requiredArgs.ReportDir.toFile();
					ConsistencyCheckService.Result ccResult = _consistencyCheckService.runFullConsistencyCheck( layout, config, _progressMonitorFactory, _logProvider, _outsideWorld.fileSystem(), verbose, reportDir, consistencyFlags );

					if ( !ccResult.Successful )
					{
						 throw new CommandFailed( format( "Inconsistencies found. See '%s' for details.", ccResult.ReportFile() ), STATUS_CC_INCONSISTENT );
					}
			  }
			  catch ( Exception e )
			  {
					if ( e is CommandFailed )
					{
						 throw ( CommandFailed ) e;
					}
					throw new CommandFailed( "Failed to do consistency check on backup: " + e.Message, e, STATUS_CC_ERROR );
			  }
		 }
	}

}