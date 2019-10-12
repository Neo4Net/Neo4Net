using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// This class listens for rotations and does log pruning.
	/// </summary>
	public class LogPruningImpl : LogPruning
	{
		 private readonly Lock _pruneLock = new ReentrantLock();
		 private readonly FileSystemAbstraction _fs;
		 private readonly LogFiles _logFiles;
		 private readonly Log _msgLog;
		 private readonly LogPruneStrategyFactory _strategyFactory;
		 private readonly Clock _clock;
		 private volatile LogPruneStrategy _pruneStrategy;

		 public LogPruningImpl( FileSystemAbstraction fs, LogFiles logFiles, LogProvider logProvider, LogPruneStrategyFactory strategyFactory, Clock clock, Config config )
		 {
			  this._fs = fs;
			  this._logFiles = logFiles;
			  this._msgLog = logProvider.getLog( this.GetType() );
			  this._strategyFactory = strategyFactory;
			  this._clock = clock;
			  this._pruneStrategy = strategyFactory.StrategyFromConfigValue( fs, logFiles, clock, config.Get( GraphDatabaseSettings.keep_logical_logs ) );

			  // Register listener for updates
			  config.RegisterDynamicUpdateListener( GraphDatabaseSettings.keep_logical_logs, ( prev, update ) => updateConfiguration( update ) );
		 }

		 private class CountingDeleter : System.Action<long>
		 {
			  internal const int NO_VERSION = -1;
			  internal readonly LogFiles LogFiles;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly long UpToVersion;
			  internal long FromVersion;
			  internal long ToVersion;

			  internal CountingDeleter( LogFiles logFiles, FileSystemAbstraction fs, long upToVersion )
			  {
					this.LogFiles = logFiles;
					this.Fs = fs;
					this.UpToVersion = upToVersion;
					FromVersion = NO_VERSION;
					ToVersion = NO_VERSION;
			  }

			  public override void Accept( long version )
			  {
					FromVersion = FromVersion == NO_VERSION ? version : Math.Min( FromVersion, version );
					ToVersion = ToVersion == NO_VERSION ? version : Math.Max( ToVersion, version );
					File logFile = LogFiles.getLogFileForVersion( version );
					Fs.deleteFile( logFile );
			  }

			  public virtual string DescribeResult()
			  {
					if ( FromVersion == NO_VERSION )
					{
						 return "No log version pruned, last checkpoint was made in version " + UpToVersion;
					}
					else
					{
						 return "Pruned log versions " + FromVersion + "-" + ToVersion +
								  ", last checkpoint was made in version " + UpToVersion;
					}
			  }
		 }

		 private void UpdateConfiguration( string pruningConf )
		 {
			  this._pruneStrategy = _strategyFactory.strategyFromConfigValue( _fs, _logFiles, _clock, pruningConf );
			  _msgLog.info( "Retention policy updated, value will take effect during the next evaluation." );
		 }

		 public override void PruneLogs( long upToVersion )
		 {
			  // Only one is allowed to do pruning at any given time,
			  // and it's OK to skip pruning if another one is doing so right now.
			  if ( _pruneLock.tryLock() )
			  {
					try
					{
						 CountingDeleter deleter = new CountingDeleter( _logFiles, _fs, upToVersion );
						 _pruneStrategy( upToVersion ).forEachOrdered( deleter );
						 _msgLog.info( deleter.DescribeResult() );
					}
					finally
					{
						 _pruneLock.unlock();
					}
			  }
		 }

		 public override bool MightHaveLogsToPrune()
		 {
			  return _pruneStrategy( _logFiles.HighestLogVersion ).count() > 0;
		 }
	}

}