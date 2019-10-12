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
namespace Org.Neo4j.Server.web
{
	using Request = org.eclipse.jetty.server.Request;
	using RequestLog = org.eclipse.jetty.server.RequestLog;
	using Response = org.eclipse.jetty.server.Response;
	using AbstractLifeCycle = org.eclipse.jetty.util.component.AbstractLifeCycle;


	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using RotatingFileOutputStreamSupplier = Org.Neo4j.Logging.RotatingFileOutputStreamSupplier;
	using AsyncLogEvent = Org.Neo4j.Logging.async.AsyncLogEvent;
	using AsyncLogProvider = Org.Neo4j.Logging.async.AsyncLogProvider;
	using Org.Neo4j.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang.StringUtils.defaultString;

	public class AsyncRequestLog : AbstractLifeCycle, RequestLog, System.Action<AsyncLogEvent>, AsyncEvents.Monitor
	{
		 private readonly Log _log;
		 private readonly ExecutorService _asyncLogProcessingExecutor;
		 private readonly AsyncEvents<AsyncLogEvent> _asyncEventProcessor;
		 private readonly RotatingFileOutputStreamSupplier _outputSupplier;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public AsyncRequestLog(org.neo4j.io.fs.FileSystemAbstraction fs, java.time.ZoneId logTimeZone, String logFile, long rotationSize, int rotationKeepNumber) throws java.io.IOException
		 public AsyncRequestLog( FileSystemAbstraction fs, ZoneId logTimeZone, string logFile, long rotationSize, int rotationKeepNumber )
		 {
			  NamedThreadFactory threadFactory = new NamedThreadFactory( "HTTP-Log-Rotator", true );
			  ExecutorService rotationExecutor = Executors.newCachedThreadPool( threadFactory );
			  _outputSupplier = new RotatingFileOutputStreamSupplier( fs, new File( logFile ), rotationSize, 0, rotationKeepNumber, rotationExecutor );
			  FormattedLogProvider logProvider = FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( _outputSupplier );
			  _asyncLogProcessingExecutor = Executors.newSingleThreadExecutor( new NamedThreadFactory( "HTTP-Log-Writer" ) );
			  _asyncEventProcessor = new AsyncEvents<AsyncLogEvent>( this, this );
			  AsyncLogProvider asyncLogProvider = new AsyncLogProvider( _asyncEventProcessor, logProvider );
			  _log = asyncLogProvider.GetLog( "REQUEST" );
		 }

		 public override void Log( Request request, Response response )
		 {
			  // Trying to replicate this logback pattern:
			  // %h %l %user [%t{dd/MMM/yyyy:HH:mm:ss Z}] "%r" %s %b "%i{Referer}" "%i{User-Agent}" %D
			  string remoteHost = SwallowExceptions( request, HttpServletRequest.getRemoteHost );
			  string user = SwallowExceptions( request, HttpServletRequest.getRemoteUser );
			  string requestURL = SwallowExceptions( request, HttpServletRequest.getRequestURI ) + "?" + SwallowExceptions( request, HttpServletRequest.getQueryString );
			  int statusCode = response.Status;
			  long length = response.ContentLength;
			  string referer = SwallowExceptions( request, r => r.getHeader( "Referer" ) );
			  string userAgent = SwallowExceptions( request, r => r.getHeader( "User-Agent" ) );
			  long requestTimeStamp = request != null ? request.TimeStamp : -1;
			  long now = DateTimeHelper.CurrentUnixTimeMillis();
			  long serviceTime = requestTimeStamp < 0 ? -1 : now - requestTimeStamp;

			  _log.info( "%s - %s [%tc] \"%s\" %s %s \"%s\" \"%s\" %s", defaultString( remoteHost ), defaultString( user ), now, defaultString( requestURL ), statusCode, length, defaultString( referer ), defaultString( userAgent ), serviceTime );
		 }

		 private T SwallowExceptions<T>( HttpServletRequest outerRequest, System.Func<HttpServletRequest, T> function )
		 {
			  try
			  {
					return outerRequest == null ? default( T ) : function( outerRequest );
			  }
			  catch ( Exception )
			  {
					return default( T );
			  }
		 }

		 protected internal override void DoStart()
		 {
			 lock ( this )
			 {
				  _asyncLogProcessingExecutor.submit( _asyncEventProcessor );
				  _asyncEventProcessor.awaitStartup();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected synchronized void doStop() throws java.io.IOException
		 protected internal override void DoStop()
		 {
			 lock ( this )
			 {
				  _asyncEventProcessor.shutdown();
				  _asyncEventProcessor.awaitTermination();
				  _outputSupplier.Dispose();
			 }
		 }

		 public override void Accept( AsyncLogEvent @event )
		 {
			  @event.Process();
		 }

		 public override void EventCount( long count )
		 {
		 }
	}

}