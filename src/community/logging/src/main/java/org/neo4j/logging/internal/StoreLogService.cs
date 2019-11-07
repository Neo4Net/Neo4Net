using System;
using System.Collections.Generic;

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
namespace Neo4Net.Logging.Internal
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.file.Files.createOrOpenAsOutputStream;

	public class StoreLogService : AbstractLogService, Lifecycle
	{
		 public class Builder
		 {
			  internal LogProvider UserLogProvider = NullLogProvider.Instance;
			  internal Executor RotationExecutor;
			  internal long InternalLogRotationThreshold;
			  internal long InternalLogRotationDelay;
			  internal int MaxInternalLogArchives;
			  internal System.Action<LogProvider> RotationListener = logProvider =>
			  {
			  };
			  internal IDictionary<string, Level> LogLevels = new Dictionary<string, Level>();
			  internal Level DefaultLevel = Level.INFO;
			  internal ZoneId TimeZoneId = ZoneOffset.UTC;
			  internal File DebugLog;

			  internal Builder()
			  {
			  }

			  public virtual Builder WithUserLogProvider( LogProvider userLogProvider )
			  {
					this.UserLogProvider = userLogProvider;
					return this;
			  }

			  public virtual Builder WithRotation( long internalLogRotationThreshold, long internalLogRotationDelay, int maxInternalLogArchives, IJobScheduler jobScheduler )
			  {
					return WithRotation( internalLogRotationThreshold, internalLogRotationDelay, maxInternalLogArchives, jobScheduler.Executor( Group.LOG_ROTATION ) );
			  }

			  public virtual Builder WithRotation( long internalLogRotationThreshold, long internalLogRotationDelay, int maxInternalLogArchives, Executor rotationExecutor )
			  {
					this.InternalLogRotationThreshold = internalLogRotationThreshold;
					this.InternalLogRotationDelay = internalLogRotationDelay;
					this.MaxInternalLogArchives = maxInternalLogArchives;
					this.RotationExecutor = rotationExecutor;
					return this;
			  }

			  public virtual Builder WithRotationListener( System.Action<LogProvider> rotationListener )
			  {
					this.RotationListener = rotationListener;
					return this;
			  }

			  public virtual Builder WithLevel( string context, Level level )
			  {
					this.LogLevels[context] = level;
					return this;
			  }

			  public virtual Builder WithTimeZone( ZoneId timeZoneId )
			  {
					this.TimeZoneId = timeZoneId;
					return this;
			  }

			  public virtual Builder WithDefaultLevel( Level defaultLevel )
			  {
					this.DefaultLevel = defaultLevel;
					return this;
			  }

			  public virtual Builder WithInternalLog( File logFile )
			  {
					this.DebugLog = logFile;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreLogService build(Neo4Net.io.fs.FileSystemAbstraction fileSystem) throws java.io.IOException
			  public virtual StoreLogService Build( FileSystemAbstraction fileSystem )
			  {
					if ( DebugLog == null )
					{
						 throw new System.ArgumentException( "Debug log can't be null; set its value using `withInternalLog`" );
					}
					return new StoreLogService( UserLogProvider, fileSystem, DebugLog, LogLevels, DefaultLevel, TimeZoneId, InternalLogRotationThreshold, InternalLogRotationDelay, MaxInternalLogArchives, RotationExecutor, RotationListener );
			  }
		 }

		 public static Builder WithUserLogProvider( LogProvider userLogProvider )
		 {
			  return ( new Builder() ).WithUserLogProvider(userLogProvider);
		 }

		 public static Builder WithRotation( long internalLogRotationThreshold, long internalLogRotationDelay, int maxInternalLogArchives, IJobScheduler jobScheduler )
		 {
			  return ( new Builder() ).WithRotation(internalLogRotationThreshold, internalLogRotationDelay, maxInternalLogArchives, jobScheduler);
		 }

		 public static Builder WithInternalLog( File logFile )
		 {
			  return ( new Builder() ).WithInternalLog(logFile);
		 }

		 private readonly System.IDisposable _closeable;
		 private readonly SimpleLogService _logService;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private StoreLogService(Neo4Net.logging.LogProvider userLogProvider, Neo4Net.io.fs.FileSystemAbstraction fileSystem, java.io.File internalLog, java.util.Map<String, Neo4Net.logging.Level> logLevels, Neo4Net.logging.Level defaultLevel, java.time.ZoneId logTimeZone, long internalLogRotationThreshold, long internalLogRotationDelay, int maxInternalLogArchives, java.util.concurrent.Executor rotationExecutor, final System.Action<Neo4Net.logging.LogProvider> rotationListener) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private StoreLogService( LogProvider userLogProvider, FileSystemAbstraction fileSystem, File internalLog, IDictionary<string, Level> logLevels, Level defaultLevel, ZoneId logTimeZone, long internalLogRotationThreshold, long internalLogRotationDelay, int maxInternalLogArchives, Executor rotationExecutor, System.Action<LogProvider> rotationListener )
		 {
			  if ( !internalLog.ParentFile.exists() )
			  {
					fileSystem.Mkdirs( internalLog.ParentFile );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.FormattedLogProvider.Builder internalLogBuilder = Neo4Net.logging.FormattedLogProvider.withZoneId(logTimeZone).withDefaultLogLevel(defaultLevel).withLogLevels(logLevels);
			  FormattedLogProvider.Builder internalLogBuilder = FormattedLogProvider.withZoneId( logTimeZone ).withDefaultLogLevel( defaultLevel ).withLogLevels( logLevels );

			  FormattedLogProvider internalLogProvider;
			  if ( internalLogRotationThreshold == 0 )
			  {
					Stream outputStream = createOrOpenAsOutputStream( fileSystem, internalLog, true );
					internalLogProvider = internalLogBuilder.ToOutputStream( outputStream );
					rotationListener( internalLogProvider );
					this._closeable = outputStream;
			  }
			  else
			  {
					RotatingFileOutputStreamSupplier rotatingSupplier = new RotatingFileOutputStreamSupplier( fileSystem, internalLog, internalLogRotationThreshold, internalLogRotationDelay, maxInternalLogArchives, rotationExecutor, new RotationListenerAnonymousInnerClass( this, rotationListener, internalLogBuilder ) );
					internalLogProvider = internalLogBuilder.ToOutputStream( rotatingSupplier );
					this._closeable = rotatingSupplier;
			  }
			  this._logService = new SimpleLogService( userLogProvider, internalLogProvider );
		 }

		 private class RotationListenerAnonymousInnerClass : RotatingFileOutputStreamSupplier.RotationListener
		 {
			 private readonly StoreLogService _outerInstance;

			 private System.Action<LogProvider> _rotationListener;
			 private FormattedLogProvider.Builder _internalLogBuilder;

			 public RotationListenerAnonymousInnerClass( StoreLogService outerInstance, System.Action<LogProvider> rotationListener, FormattedLogProvider.Builder internalLogBuilder )
			 {
				 this.outerInstance = outerInstance;
				 this._rotationListener = rotationListener;
				 this._internalLogBuilder = internalLogBuilder;
			 }

			 public override void outputFileCreated( Stream newStream )
			 {
				  FormattedLogProvider logProvider = _internalLogBuilder.toOutputStream( newStream );
				  logProvider.GetLog( typeof( StoreLogService ) ).info( "Opened new internal log file" );
				  _rotationListener( logProvider );
			 }

			 public override void rotationCompleted( Stream newStream )
			 {
				  FormattedLogProvider logProvider = _internalLogBuilder.toOutputStream( newStream );
				  logProvider.GetLog( typeof( StoreLogService ) ).info( "Rotated internal log file" );
			 }

			 public override void rotationError( Exception e, Stream outStream )
			 {
				  FormattedLogProvider logProvider = _internalLogBuilder.toOutputStream( outStream );
				  logProvider.GetLog( typeof( StoreLogService ) ).info( "Rotation of internal log file failed:", e );
			 }
		 }

		 public override void Init()
		 {
		 }

		 public override void Start()
		 {
		 }

		 public override void Stop()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _closeable.Dispose();
		 }

		 public override LogProvider UserLogProvider
		 {
			 get
			 {
				  return _logService.UserLogProvider;
			 }
		 }

		 public override LogProvider InternalLogProvider
		 {
			 get
			 {
				  return _logService.InternalLogProvider;
			 }
		 }
	}

}