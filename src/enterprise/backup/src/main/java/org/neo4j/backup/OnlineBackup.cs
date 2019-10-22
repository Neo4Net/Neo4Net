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
namespace Neo4Net.backup
{

	using BackupClient = Neo4Net.backup.impl.BackupClient;
	using BackupOutcome = Neo4Net.backup.impl.BackupOutcome;
	using BackupProtocolService = Neo4Net.backup.impl.BackupProtocolService;
	using BackupServer = Neo4Net.backup.impl.BackupServer;
	using ConsistencyCheck = Neo4Net.backup.impl.ConsistencyCheck;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.backup.impl.BackupProtocolServiceFactory.backupProtocolService;

	/// <summary>
	/// This class encapsulates the information needed to perform an online backup against a running Neo4Net instance
	/// configured to act as a backup server. This class is not instantiable, instead factory methods are used to get
	/// instances configured to contact a specific backup server against which all possible backup operations can be
	/// performed.
	/// 
	/// All backup methods return the same instance, allowing for chaining calls.
	/// </summary>
	public class OnlineBackup
	{
		 private readonly string _hostNameOrIp;
		 private readonly int _port;
		 private bool _forensics;
		 private BackupOutcome _outcome;
		 private long _timeoutMillis = BackupClient.BIG_READ_TIMEOUT;
		 private Stream @out = System.out;

		 /// <summary>
		 /// Factory method for this class. The OnlineBackup instance returned will perform backup operations against the
		 /// hostname and port passed in as parameters.
		 /// </summary>
		 /// <param name="hostNameOrIp"> The hostname or the IP address of the backup server </param>
		 /// <param name="port"> The port at which the remote backup server is listening </param>
		 /// <returns> An OnlineBackup instance ready to perform backup operations from the given remote server </returns>
		 public static OnlineBackup From( string hostNameOrIp, int port )
		 {
			  return new OnlineBackup( hostNameOrIp, port );
		 }

		 /// <summary>
		 /// Factory method for this class. The OnlineBackup instance returned will perform backup operations against the
		 /// hostname passed in as parameter, using the default backup port.
		 /// </summary>
		 /// <param name="hostNameOrIp"> The hostname or IP address of the backup server </param>
		 /// <returns> An OnlineBackup instance ready to perform backup operations from the given remote server </returns>
		 public static OnlineBackup From( string hostNameOrIp )
		 {
			  return new OnlineBackup( hostNameOrIp, BackupServer.DEFAULT_PORT );
		 }

		 private OnlineBackup( string hostNameOrIp, int port )
		 {
			  this._hostNameOrIp = hostNameOrIp;
			  this._port = port;
		 }

		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 /// @deprecated use <seealso cref="backup(File)"/> instead 
		 [Obsolete("use <seealso cref=\"backup(File)\"/> instead")]
		 public virtual OnlineBackup Backup( string targetDirectory )
		 {
			  return Backup( new File( targetDirectory ) );
		 }

		 /// <summary>
		 /// Performs a backup into targetDirectory. The server contacted is the one configured in the factory method used to
		 /// obtain this instance. After the backup is complete, a verification phase will take place, checking
		 /// the database for consistency. If any errors are found, they will be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database, a full backup will be performed, otherwise an incremental
		 /// backup mechanism is used.
		 /// 
		 /// If the backup has become too far out of date for an incremental backup to succeed, a full backup is performed.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 public virtual OnlineBackup Backup( File targetDirectory )
		 {
			  PerformBackup( protocolService => protocolService.doIncrementalBackupOrFallbackToFull( _hostNameOrIp, _port, DatabaseLayout.of( targetDirectory ), GetConsistencyCheck( true ), DefaultConfig(), _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="verification"> If true, the verification phase will be run. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 /// @deprecated use <seealso cref="backup(File, bool)"/> instead 
		 [Obsolete("use <seealso cref=\"backup(File, bool)\"/> instead")]
		 public virtual OnlineBackup Backup( string targetDirectory, bool verification )
		 {
			  return Backup( new File( targetDirectory ), verification );
		 }

		 /// <summary>
		 /// Performs a backup into targetDirectory. The server contacted is the one configured in the factory method used to
		 /// obtain this instance. After the backup is complete, and if the verification parameter is set to true,
		 /// a verification phase will take place, checking the database for consistency. If any errors are found, they will
		 /// be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database, a full backup will be performed, otherwise an incremental
		 /// backup mechanism is used.
		 /// 
		 /// If the backup has become too far out of date for an incremental backup to succeed, a full backup is performed.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="verification"> If true, the verification phase will be run. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 public virtual OnlineBackup Backup( File targetDirectory, bool verification )
		 {
			  PerformBackup( protocolService => protocolService.doIncrementalBackupOrFallbackToFull( _hostNameOrIp, _port, DatabaseLayout.of( targetDirectory ), GetConsistencyCheck( verification ), DefaultConfig(), _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="tuningConfiguration"> The <seealso cref="Config"/> to use when running the consistency check </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 /// @deprecated use <seealso cref="backup(File, Config)"/> instead 
		 [Obsolete("use <seealso cref=\"backup(File, Config)\"/> instead")]
		 public virtual OnlineBackup Backup( string targetDirectory, Config tuningConfiguration )
		 {
			  return Backup( new File( targetDirectory ), tuningConfiguration );
		 }

		 /// <summary>
		 /// Performs a backup into targetDirectory. The server contacted is the one configured in the factory method used to
		 /// obtain this instance. After the backup is complete, a verification phase will take place, checking
		 /// the database for consistency. If any errors are found, they will be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database, a full backup will be performed, otherwise an incremental
		 /// backup mechanism is used.
		 /// 
		 /// If the backup has become too far out of date for an incremental backup to succeed, a full backup is performed.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="tuningConfiguration"> The <seealso cref="Config"/> to use when running the consistency check </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 public virtual OnlineBackup Backup( File targetDirectory, Config tuningConfiguration )
		 {
			  PerformBackup( backupProtocolService => backupProtocolService.doIncrementalBackupOrFallbackToFull( _hostNameOrIp, _port, DatabaseLayout.of( targetDirectory ), GetConsistencyCheck( true ), tuningConfiguration, _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="tuningConfiguration"> The <seealso cref="Config"/> to use when running the consistency check </param>
		 /// <param name="verification"> If true, the verification phase will be run. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 /// @deprecated use <seealso cref="backup(File, Config, bool)"/> instead 
		 [Obsolete("use <seealso cref=\"backup(File, Config, bool)\"/> instead")]
		 public virtual OnlineBackup Backup( string targetDirectory, Config tuningConfiguration, bool verification )
		 {
			  return Backup( new File( targetDirectory ), tuningConfiguration, verification );
		 }

		 /// <summary>
		 /// Performs a backup into targetDirectory. The server contacted is the one configured in the factory method used to
		 /// obtain this instance. After the backup is complete, and if the verification parameter is set to true,
		 /// a verification phase will take place, checking the database for consistency. If any errors are found, they will
		 /// be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database, a full backup will be performed, otherwise an incremental
		 /// backup mechanism is used.
		 /// 
		 /// If the backup has become too far out of date for an incremental backup to succeed, a full backup is performed.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="tuningConfiguration"> The <seealso cref="Config"/> to use when running the consistency check </param>
		 /// <param name="verification"> If true, the verification phase will be run. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 public virtual OnlineBackup Backup( File targetDirectory, Config tuningConfiguration, bool verification )
		 {
			  PerformBackup( backupProtocolService => backupProtocolService.doIncrementalBackupOrFallbackToFull( _hostNameOrIp, _port, DatabaseLayout.of( targetDirectory ), GetConsistencyCheck( verification ), tuningConfiguration, _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <summary>
		 /// Use this method to change the default timeout to keep the client waiting for each reply from the server when
		 /// doing online backup. Once the value is changed, then every time when doing online backup, the timeout will be
		 /// reused until this method is called again and a new value is assigned.
		 /// </summary>
		 /// <param name="timeoutMillis"> The time duration in millisecond that keeps the client waiting for each reply from the
		 /// server. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 public virtual OnlineBackup WithTimeout( long timeoutMillis )
		 {
			  this._timeoutMillis = timeoutMillis;
			  return this;
		 }

		 public virtual OnlineBackup WithOutput( Stream @out )
		 {
			  this.@out = @out;
			  return this;
		 }

		 /// <summary>
		 /// Performs a full backup storing the resulting database at the given directory. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. At the end of the backup, a verification phase
		 /// will take place, running over the resulting database ensuring it is consistent. If the check fails, the fact
		 /// will be printed in stderr.
		 /// 
		 /// If the target directory already contains a database, a RuntimeException denoting the fact will be thrown.
		 /// </summary>
		 /// <param name="targetDirectory"> The directory in which to store the database </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 /// @deprecated Use <seealso cref="backup(File)"/> instead. 
		 [Obsolete("Use <seealso cref=\"backup(File)\"/> instead.")]
		 public virtual OnlineBackup Full( string targetDirectory )
		 {
			  PerformBackup( protocolService => protocolService.doFullBackup( _hostNameOrIp, _port, GetTargetDatabaseLayout( targetDirectory ), GetConsistencyCheck( true ), DefaultConfig(), _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <summary>
		 /// Performs a full backup storing the resulting database at the given directory. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. If the verification flag is set, at the end of
		 /// the backup, a verification phase will take place, running over the resulting database ensuring it is consistent.
		 /// If the check fails, the fact will be printed in stderr.
		 /// 
		 /// If the target directory already contains a database, a RuntimeException denoting the fact will be thrown.
		 /// </summary>
		 /// <param name="targetDirectory"> The directory in which to store the database </param>
		 /// <param name="verification"> a boolean indicating whether to perform verification on the created backup </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 /// @deprecated Use <seealso cref="backup(File, bool)"/> instead 
		 [Obsolete("Use <seealso cref=\"backup(File, bool)\"/> instead")]
		 public virtual OnlineBackup Full( string targetDirectory, bool verification )
		 {
			  PerformBackup( protocolService => protocolService.doFullBackup( _hostNameOrIp, _port, GetTargetDatabaseLayout( targetDirectory ), GetConsistencyCheck( verification ), DefaultConfig(), _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <summary>
		 /// Performs a full backup storing the resulting database at the given directory. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. If the verification flag is set, at the end of
		 /// the backup, a verification phase will take place, running over the resulting database ensuring it is consistent.
		 /// If the check fails, the fact will be printed in stderr. The consistency check will run with the provided
		 /// tuning configuration.
		 /// 
		 /// If the target directory already contains a database, a RuntimeException denoting the fact will be thrown.
		 /// </summary>
		 /// <param name="targetDirectory"> The directory in which to store the database </param>
		 /// <param name="verification"> a boolean indicating whether to perform verification on the created backup </param>
		 /// <param name="tuningConfiguration"> The <seealso cref="Config"/> to use when running the consistency check </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 /// @deprecated Use <seealso cref="backup(File, Config, bool)"/> instead. 
		 [Obsolete("Use <seealso cref=\"backup(File, Config, bool)\"/> instead.")]
		 public virtual OnlineBackup Full( string targetDirectory, bool verification, Config tuningConfiguration )
		 {
			  PerformBackup( protocolService => protocolService.doFullBackup( _hostNameOrIp, _port, GetTargetDatabaseLayout( targetDirectory ), GetConsistencyCheck( verification ), tuningConfiguration, _timeoutMillis, _forensics ) );
			  return this;
		 }

		 /// <summary>
		 /// Performs an incremental backup on the database stored in targetDirectory. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. After the incremental backup is complete, a
		 /// verification phase will take place, checking the database for consistency. If any errors are found, they will
		 /// be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database or it is not compatible with the one present in the
		 /// configured backup server a RuntimeException will be thrown denoting the fact.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 /// @deprecated Use <seealso cref="backup(File)"/> instead. 
		 [Obsolete("Use <seealso cref=\"backup(File)\"/> instead.")]
		 public virtual OnlineBackup Incremental( string targetDirectory )
		 {
			  PerformBackup( protocolService => protocolService.doIncrementalBackup( _hostNameOrIp, _port, GetTargetDatabaseLayout( targetDirectory ), GetConsistencyCheck( false ), _timeoutMillis, DefaultConfig() ) );
			  return this;
		 }

		 /// <summary>
		 /// Performs an incremental backup on the database stored in targetDirectory. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. After the incremental backup is complete, and if
		 /// the verification parameter is set to true, a  verification phase will take place, checking the database for
		 /// consistency. If any errors are found, they will be printed in stderr.
		 /// 
		 /// If the target directory does not contain a database or it is not compatible with the one present in the
		 /// configured backup server a RuntimeException will be thrown denoting the fact.
		 /// </summary>
		 /// <param name="targetDirectory"> A directory holding a complete database previously obtained from the backup server. </param>
		 /// <param name="verification"> If true, the verification phase will be run. </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 /// @deprecated Use <seealso cref="backup(File, bool)"/> instead. 
		 [Obsolete("Use <seealso cref=\"backup(File, bool)\"/> instead.")]
		 public virtual OnlineBackup Incremental( string targetDirectory, bool verification )
		 {
			  PerformBackup( protocolService => protocolService.doIncrementalBackup( _hostNameOrIp, _port, GetTargetDatabaseLayout( targetDirectory ), GetConsistencyCheck( verification ), _timeoutMillis, DefaultConfig() ) );
			  return this;
		 }

		 /// <summary>
		 /// Performs an incremental backup on the supplied target database. The server contacted is the one
		 /// configured in the factory method used to obtain this instance. After the incremental backup is complete
		 /// a verification phase will take place, checking the database for consistency. If any errors are found, they will
		 /// be printed in stderr.
		 /// 
		 /// If the target database is not compatible with the one present in the target backup server, a RuntimeException
		 /// will be thrown denoting the fact.
		 /// </summary>
		 /// <param name="targetDb"> The database on which the incremental backup is to be applied </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation. </returns>
		 /// @deprecated Use <seealso cref="backup(string)"/> instead. 
		 [Obsolete("Use <seealso cref=\"backup(string)\"/> instead.")]
		 public virtual OnlineBackup Incremental( GraphDatabaseAPI targetDb )
		 {
			  PerformBackup( protocolService => protocolService.doIncrementalBackup( _hostNameOrIp, _port, targetDb, _timeoutMillis ) );
			  return this;
		 }

		 /// <summary>
		 /// Provides information about the last committed transaction for each data source present in the last backup
		 /// operation performed by this OnlineBackup.
		 /// In particular, it returns a map where the keys are the names of the data sources and the values the longs that
		 /// are the last committed transaction id for that data source.
		 /// </summary>
		 /// <returns> A map from data source name to last committed transaction id. </returns>
		 public virtual long LastCommittedTx
		 {
			 get
			 {
				  return Outcome().LastCommittedTx;
			 }
		 }

		 /// <returns> the consistency outcome of the last made backup. I </returns>
		 public virtual bool Consistent
		 {
			 get
			 {
				  return Outcome().Consistent;
			 }
		 }

		 private BackupOutcome Outcome()
		 {
			  if ( _outcome == null )
			  {
					throw new System.InvalidOperationException( "No outcome yet. Please call full or incremental backup first" );
			  }
			  return _outcome;
		 }

		 private static Config DefaultConfig()
		 {
			  return Config.defaults();
		 }

		 /// <param name="forensics"> whether or not additional information should be backed up, for example transaction </param>
		 /// <returns> The same OnlineBackup instance, possible to use for a new backup operation </returns>
		 public virtual OnlineBackup GatheringForensics( bool forensics )
		 {
			  this._forensics = forensics;
			  return this;
		 }

		 private static DatabaseLayout GetTargetDatabaseLayout( string targetDirectory )
		 {
			  return DatabaseLayout.of( Paths.get( targetDirectory ).toFile() );
		 }

		 private static ConsistencyCheck GetConsistencyCheck( bool verification )
		 {
			  return verification ? ConsistencyCheck.FULL : ConsistencyCheck.NONE;
		 }

		 private void PerformBackup( System.Func<BackupProtocolService, BackupOutcome> backupFunction )
		 {
			  try
			  {
					  using ( BackupProtocolService protocolService = backupProtocolService( @out ) )
					  {
						_outcome = backupFunction( protocolService );
					  }
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}