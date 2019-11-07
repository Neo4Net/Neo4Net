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
namespace Neo4Net.Server.security.enterprise.log
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AuthSubject = Neo4Net.Kernel.Api.Internal.security.AuthSubject;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using Log = Neo4Net.Logging.Log;
	using Logger = Neo4Net.Logging.Logger;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Strings.escape;

	public class SecurityLog : LifecycleAdapter, Log
	{
		 private RotatingFileOutputStreamSupplier _rotatingSupplier;
		 private readonly Log _inner;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SecurityLog(Neo4Net.kernel.configuration.Config config, Neo4Net.io.fs.FileSystemAbstraction fileSystem, java.util.concurrent.Executor executor) throws java.io.IOException
		 public SecurityLog( Config config, FileSystemAbstraction fileSystem, Executor executor )
		 {
			  ZoneId logTimeZoneId = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
			  File logFile = config.Get( SecuritySettings.security_log_filename );

			  FormattedLog.Builder builder = FormattedLog.withZoneId( logTimeZoneId );

			  _rotatingSupplier = new RotatingFileOutputStreamSupplier( fileSystem, logFile, config.Get( SecuritySettings.store_security_log_rotation_threshold ), config.Get( SecuritySettings.store_security_log_rotation_delay ).toMillis(), config.Get(SecuritySettings.store_security_log_max_archives), executor );

			  FormattedLog formattedLog = builder.ToOutputStream( _rotatingSupplier );
			  formattedLog.Level = config.Get( SecuritySettings.security_log_level );

			  this._inner = formattedLog;
		 }

		 /* Only used for tests */
		 public SecurityLog( Log log )
		 {
			  _inner = log;
		 }

		 private static string WithSubject( AuthSubject subject, string msg )
		 {
			  return "[" + escape( subject.Username() ) + "]: " + msg;
		 }

		 public virtual bool DebugEnabled
		 {
			 get
			 {
				  return _inner.DebugEnabled;
			 }
		 }

		 public override Logger DebugLogger()
		 {
			  return _inner.debugLogger();
		 }

		 public override void Debug( string message )
		 {
			  _inner.debug( message );
		 }

		 public override void Debug( string message, Exception throwable )
		 {
			  _inner.debug( message, throwable );
		 }

		 public override void Debug( string format, params object[] arguments )
		 {
			  _inner.debug( format, arguments );
		 }

		 public virtual void Debug( AuthSubject subject, string format, params object[] arguments )
		 {
			  _inner.debug( WithSubject( subject, format ), arguments );
		 }

		 public override Logger InfoLogger()
		 {
			  return _inner.infoLogger();
		 }

		 public override void Info( string message )
		 {
			  _inner.info( message );
		 }

		 public override void Info( string message, Exception throwable )
		 {
			  _inner.info( message, throwable );
		 }

		 public override void Info( string format, params object[] arguments )
		 {
			  _inner.info( format, arguments );
		 }

		 public virtual void Info( AuthSubject subject, string format, params object[] arguments )
		 {
			  _inner.info( WithSubject( subject, format ), arguments );
		 }

		 public virtual void Info( AuthSubject subject, string format )
		 {
			  _inner.info( WithSubject( subject, format ) );
		 }

		 public override Logger WarnLogger()
		 {
			  return _inner.warnLogger();
		 }

		 public override void Warn( string message )
		 {
			  _inner.warn( message );
		 }

		 public override void Warn( string message, Exception throwable )
		 {
			  _inner.warn( message, throwable );
		 }

		 public override void Warn( string format, params object[] arguments )
		 {
			  _inner.warn( format, arguments );
		 }

		 public virtual void Warn( AuthSubject subject, string format, params object[] arguments )
		 {
			  _inner.warn( WithSubject( subject, format ), arguments );
		 }

		 public override Logger ErrorLogger()
		 {
			  return _inner.errorLogger();
		 }

		 public override void Error( string message )
		 {
			  _inner.error( message );
		 }

		 public override void Error( string message, Exception throwable )
		 {
			  _inner.error( message, throwable );
		 }

		 public override void Error( string format, params object[] arguments )
		 {
			  _inner.error( format, arguments );
		 }

		 public virtual void Error( AuthSubject subject, string format, params object[] arguments )
		 {
			  _inner.error( WithSubject( subject, format ), arguments );
		 }

		 public override void Bulk( System.Action<Log> consumer )
		 {
			  _inner.bulk( consumer );
		 }

		 public static SecurityLog Create( Config config, Log log, FileSystemAbstraction fileSystem, IJobScheduler jobScheduler )
		 {
			  try
			  {
					return new SecurityLog( config, fileSystem, jobScheduler.Executor( Group.LOG_ROTATION ) );
			  }
			  catch ( IOException )
			  {
					log.Warn( "Unable to create log for auth-manager. Auth logging turned off." );
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  if ( this._rotatingSupplier != null )
			  {
					this._rotatingSupplier.Dispose();
					this._rotatingSupplier = null;
			  }
		 }
	}

}