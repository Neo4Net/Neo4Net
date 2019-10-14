using System;

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
namespace Neo4Net.Kernel.impl.transaction.log.files
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_log_rotation_threshold;

	/// <summary>
	/// Transactional log files facade class builder.
	/// Depending from required abilities user can choose what kind of facade instance is required: from fully functional
	/// to simplified that can operate only based on available log files without accessing stores and other external
	/// components.
	/// <br/>
	/// Builder allow to configure any dependency explicitly and will use default value if that exist otherwise.
	/// More specific dependencies always take precedence over more generic.
	/// <br/>
	/// For example: provided rotation threshold will
	/// be used in precedence of value that can be specified in provided config.
	/// </summary>
	public class LogFilesBuilder
	{
		 private bool _readOnly;
		 private PageCache _pageCache;
		 private DatabaseLayout _databaseLayout;
		 private File _logsDirectory;
		 private Config _config;
		 private long? _rotationThreshold;
		 private LogEntryReader _logEntryReader;
		 private LogFileCreationMonitor _logFileCreationMonitor;
		 private Dependencies _dependencies;
		 private FileSystemAbstraction _fileSystem;
		 private LogVersionRepository _logVersionRepository;
		 private TransactionIdStore _transactionIdStore;
		 private System.Func<long> _lastCommittedTransactionIdSupplier;
		 private string _logFileName = TransactionLogFiles.DEFAULT_NAME;
		 private bool _fileBasedOperationsOnly;

		 private LogFilesBuilder()
		 {
		 }

		 /// <summary>
		 /// Builder for fully functional transactional log files.
		 /// Log files will be able to access store and external components information, perform rotations, etc. </summary>
		 /// <param name="databaseLayout"> database directory </param>
		 /// <param name="fileSystem"> log files filesystem </param>
		 public static LogFilesBuilder Builder( DatabaseLayout databaseLayout, FileSystemAbstraction fileSystem )
		 {
			  LogFilesBuilder filesBuilder = new LogFilesBuilder();
			  filesBuilder._databaseLayout = databaseLayout;
			  filesBuilder._fileSystem = fileSystem;
			  return filesBuilder;
		 }

		 /// <summary>
		 /// Build log files that can access and operate only on active set of log files without ability to
		 /// rotate and create any new one. Appending to current log file still possible.
		 /// Store and external components access available in read only mode. </summary>
		 /// <param name="databaseLayout"> store directory </param>
		 /// <param name="fileSystem"> log file system </param>
		 /// <param name="pageCache"> page cache for read only store info access </param>
		 public static LogFilesBuilder ActiveFilesBuilder( DatabaseLayout databaseLayout, FileSystemAbstraction fileSystem, PageCache pageCache )
		 {
			  LogFilesBuilder builder = builder( databaseLayout, fileSystem );
			  builder._pageCache = pageCache;
			  builder._readOnly = true;
			  return builder;
		 }

		 /// <summary>
		 /// Build log files that will be able to perform only operations on a log files directly.
		 /// Any operation that will require access to a store or other parts of runtime will fail.
		 /// Should be mainly used only for testing purposes or when only file based operations will be performed </summary>
		 /// <param name="logsDirectory"> log files directory </param>
		 /// <param name="fileSystem"> file system </param>
		 public static LogFilesBuilder LogFilesBasedOnlyBuilder( File logsDirectory, FileSystemAbstraction fileSystem )
		 {
			  LogFilesBuilder builder = new LogFilesBuilder();
			  builder._logsDirectory = logsDirectory;
			  builder._fileSystem = fileSystem;
			  builder._fileBasedOperationsOnly = true;
			  return builder;
		 }

		 internal virtual LogFilesBuilder WithLogFileName( string name )
		 {
			  this._logFileName = name;
			  return this;
		 }

		 public virtual LogFilesBuilder WithLogVersionRepository( LogVersionRepository logVersionRepository )
		 {
			  this._logVersionRepository = logVersionRepository;
			  return this;
		 }

		 public virtual LogFilesBuilder WithTransactionIdStore( TransactionIdStore transactionIdStore )
		 {
			  this._transactionIdStore = transactionIdStore;
			  return this;
		 }

		 public virtual LogFilesBuilder WithLastCommittedTransactionIdSupplier( System.Func<long> transactionIdSupplier )
		 {
			  this._lastCommittedTransactionIdSupplier = transactionIdSupplier;
			  return this;
		 }

		 public virtual LogFilesBuilder WithLogEntryReader( LogEntryReader logEntryReader )
		 {
			  this._logEntryReader = logEntryReader;
			  return this;
		 }

		 public virtual LogFilesBuilder WithLogFileMonitor( LogFileCreationMonitor logFileCreationMonitor )
		 {
			  this._logFileCreationMonitor = logFileCreationMonitor;
			  return this;
		 }

		 public virtual LogFilesBuilder WithConfig( Config config )
		 {
			  this._config = config;
			  return this;
		 }

		 public virtual LogFilesBuilder WithRotationThreshold( long rotationThreshold )
		 {
			  this._rotationThreshold = rotationThreshold;
			  return this;
		 }

		 public virtual LogFilesBuilder WithDependencies( Dependencies dependencies )
		 {
			  this._dependencies = dependencies;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogFiles build() throws java.io.IOException
		 public virtual LogFiles Build()
		 {
			  TransactionLogFilesContext filesContext = BuildContext();
			  File logsDirectory = LogsDirectory;
			  filesContext.FileSystem.mkdirs( logsDirectory );
			  return new TransactionLogFiles( logsDirectory, _logFileName, filesContext );
		 }

		 private File LogsDirectory
		 {
			 get
			 {
				  if ( _logsDirectory != null )
				  {
						return _logsDirectory;
				  }
				  // try to use absolute position only for default database. For other databases use database directory
				  if ( TryConfigureDefaultDatabaseLogsDirectory() )
				  {
						File neo4jHome = _config.get( GraphDatabaseSettings.neo4j_home );
						File databasePath = _config.get( database_path );
						File logicalLogsLocation = _config.get( GraphDatabaseSettings.logical_logs_location );
						if ( _databaseLayout.StoreLayout.storeDirectory().Equals(neo4jHome) && databasePath.Equals(logicalLogsLocation) )
						{
							 return _databaseLayout.databaseDirectory();
						}
						if ( logicalLogsLocation.Absolute )
						{
							 return logicalLogsLocation;
						}
						if ( neo4jHome == null || !_databaseLayout.databaseDirectory().Equals(databasePath) )
						{
							 Path relativeLogicalLogPath = databasePath.toPath().relativize(logicalLogsLocation.toPath());
							 return _databaseLayout.file( relativeLogicalLogPath.ToString() );
						}
						return logicalLogsLocation;
				  }
				  return _databaseLayout.databaseDirectory();
			 }
		 }

		 private bool TryConfigureDefaultDatabaseLogsDirectory()
		 {
			  return _config != null && _config.get( GraphDatabaseSettings.active_database ).Equals( _databaseLayout.DatabaseName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionLogFilesContext buildContext() throws java.io.IOException
		 internal virtual TransactionLogFilesContext BuildContext()
		 {
			  if ( _logEntryReader == null )
			  {
					_logEntryReader = new VersionAwareLogEntryReader();
			  }
			  if ( _logFileCreationMonitor == null )
			  {
					_logFileCreationMonitor = LogFileCreationMonitor_Fields.NoMonitor;
			  }
			  requireNonNull( _fileSystem );
			  System.Func<LogVersionRepository> logVersionRepositorySupplier = LogVersionRepositorySupplier;
			  System.Func<long> lastCommittedIdSupplier = lastCommittedIdSupplier();
			  System.Func<long> committingTransactionIdSupplier = CommittingIdSupplier();

			  // Register listener for rotation threshold
			  AtomicLong rotationThreshold = RotationThresholdAndRegisterForUpdates;

			  return new TransactionLogFilesContext( rotationThreshold, _logEntryReader, lastCommittedIdSupplier, committingTransactionIdSupplier, _logFileCreationMonitor, logVersionRepositorySupplier, _fileSystem );
		 }

		 private AtomicLong RotationThresholdAndRegisterForUpdates
		 {
			 get
			 {
				  if ( _rotationThreshold != null )
				  {
						return new AtomicLong( _rotationThreshold );
				  }
				  if ( _readOnly )
				  {
						return new AtomicLong( long.MaxValue );
				  }
				  if ( _config == null )
				  {
						_config = Config.defaults();
				  }
				  AtomicLong configThreshold = new AtomicLong( _config.get( logical_log_rotation_threshold ) );
				  _config.registerDynamicUpdateListener( logical_log_rotation_threshold, ( prev, update ) => configThreshold.set( update ) );
				  return configThreshold;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Func<org.neo4j.kernel.impl.transaction.log.LogVersionRepository> getLogVersionRepositorySupplier() throws java.io.IOException
		 private System.Func<LogVersionRepository> LogVersionRepositorySupplier
		 {
			 get
			 {
				  if ( _logVersionRepository != null )
				  {
						return () => _logVersionRepository;
				  }
				  if ( _fileBasedOperationsOnly )
				  {
						return () =>
						{
						 throw new System.NotSupportedException( "Current version of log files can't perform any " + "operation that require availability of log version repository. Please build full version of log " + "files. Please build full version of log files to be able to use them." );
						};
				  }
				  if ( _readOnly )
				  {
						requireNonNull( _pageCache, "Read only log files require page cache to be able to read current log version." );
						requireNonNull( _databaseLayout,"Store directory is required." );
						ReadOnlyLogVersionRepository logVersionRepository = new ReadOnlyLogVersionRepository( _pageCache, _databaseLayout );
						return () => logVersionRepository;
				  }
				  else
				  {
						requireNonNull( _dependencies, typeof( LogVersionRepository ).Name + " is required. " + "Please provide an instance or a dependencies where it can be found." );
						return GetSupplier( typeof( LogVersionRepository ) );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Func<long> lastCommittedIdSupplier() throws java.io.IOException
		 private System.Func<long> LastCommittedIdSupplier()
		 {
			  if ( _lastCommittedTransactionIdSupplier != null )
			  {
					return _lastCommittedTransactionIdSupplier;
			  }
			  if ( _transactionIdStore != null )
			  {
					return _transactionIdStore.getLastCommittedTransactionId;
			  }
			  if ( _fileBasedOperationsOnly )
			  {
					return () =>
					{
					 throw new System.NotSupportedException( "Current version of log files can't perform any " + "operation that require availability of transaction id store. Please build full version of log files " + "to be able to use them." );
					};
			  }
			  if ( _readOnly )
			  {
					requireNonNull( _pageCache, "Read only log files require page cache to be able to read commited " + "transaction info from store store." );
					requireNonNull( _databaseLayout, "Store directory is required." );
					ReadOnlyTransactionIdStore transactionIdStore = new ReadOnlyTransactionIdStore( _pageCache, _databaseLayout );
					return transactionIdStore.getLastCommittedTransactionId;
			  }
			  else
			  {
					requireNonNull( _dependencies, typeof( TransactionIdStore ).Name + " is required. " + "Please provide an instance or a dependencies where it can be found." );
					return () => ResolveDependency(typeof(TransactionIdStore)).LastCommittedTransactionId;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Func<long> committingIdSupplier() throws java.io.IOException
		 private System.Func<long> CommittingIdSupplier()
		 {
			  if ( _transactionIdStore != null )
			  {
					return _transactionIdStore.committingTransactionId;
			  }
			  if ( _fileBasedOperationsOnly )
			  {
					return () =>
					{
					 throw new System.NotSupportedException( "Current version of log files can't perform any " + "operation that require availability of transaction id store. Please build full version of log files " + "to be able to use them." );
					};
			  }
			  if ( _readOnly )
			  {
					requireNonNull( _pageCache, "Read only log files require page cache to be able to read commited " + "transaction info from store store." );
					requireNonNull( _databaseLayout, "Store directory is required." );
					ReadOnlyTransactionIdStore transactionIdStore = new ReadOnlyTransactionIdStore( _pageCache, _databaseLayout );
					return transactionIdStore.committingTransactionId;
			  }
			  else
			  {
					requireNonNull( _dependencies, typeof( TransactionIdStore ).Name + " is required. " + "Please provide an instance or a dependencies where it can be found." );
					return () => ResolveDependency(typeof(TransactionIdStore)).committingTransactionId();
			  }
		 }

		 private System.Func<T> GetSupplier<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return () => ResolveDependency(clazz);
		 }

		 private T ResolveDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return _dependencies.resolveDependency( clazz );
		 }
	}

}