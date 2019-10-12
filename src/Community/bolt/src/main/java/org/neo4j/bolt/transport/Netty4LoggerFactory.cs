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
namespace Neo4Net.Bolt.transport
{
	using AbstractInternalLogger = io.netty.util.@internal.logging.AbstractInternalLogger;
	using InternalLogger = io.netty.util.@internal.logging.InternalLogger;
	using InternalLoggerFactory = io.netty.util.@internal.logging.InternalLoggerFactory;

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.@internal.LogMessageUtil.slf4jToStringFormatPlaceholders;

	/// <summary>
	/// This class replaces Nettys regular logging system, injecting our own.
	/// </summary>
	public class Netty4LoggerFactory : InternalLoggerFactory
	{
		 private LogProvider _logProvider;

		 public Netty4LoggerFactory( LogProvider logProvider )
		 {
			  this._logProvider = logProvider;
		 }

		 public override InternalLogger NewInstance( string name )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.Log log = logProvider.getLog(name);
			  Log log = _logProvider.getLog( name );
			  return new AbstractInternalLoggerAnonymousInnerClass( this, name, log );
		 }

		 private class AbstractInternalLoggerAnonymousInnerClass : AbstractInternalLogger
		 {
			 private readonly Netty4LoggerFactory _outerInstance;

			 private Log _log;

			 public AbstractInternalLoggerAnonymousInnerClass( Netty4LoggerFactory outerInstance, string name, Log log ) : base( name )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
			 }

			 public override bool TraceEnabled
			 {
				 get
				 {
					  return false;
				 }
			 }

			 public override bool DebugEnabled
			 {
				 get
				 {
					  return _log.DebugEnabled;
				 }
			 }

			 public override bool InfoEnabled
			 {
				 get
				 {
					  // No way to tell log level with better granularity yet, and INFO
					  // logging for Netty component is most likely DEBUG anyway
					  return _log.DebugEnabled;
				 }
			 }

			 public override bool WarnEnabled
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override bool ErrorEnabled
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override void debug( string s )
			 {
				  _log.debug( s );
			 }

			 public override void debug( string s, object o )
			 {
				  _log.debug( slf4jToStringFormatPlaceholders( s ), o );
			 }

			 public override void debug( string s, object o, object o1 )
			 {
				  _log.debug( slf4jToStringFormatPlaceholders( s ), o, o1 );
			 }

			 public override void debug( string s, params object[] objects )
			 {
				  _log.debug( slf4jToStringFormatPlaceholders( s ), objects );
			 }

			 public override void debug( string s, Exception throwable )
			 {
				  _log.debug( s, throwable );
			 }

			 public override void info( string s )
			 {
				  _log.info( s );
			 }

			 public override void info( string s, object o )
			 {
				  _log.info( slf4jToStringFormatPlaceholders( s ), o );
			 }

			 public override void info( string s, object o, object o1 )
			 {
				  _log.info( slf4jToStringFormatPlaceholders( s ), o, o1 );
			 }

			 public override void info( string s, params object[] objects )
			 {
				  _log.info( slf4jToStringFormatPlaceholders( s ), objects );
			 }

			 public override void info( string s, Exception throwable )
			 {
				  _log.info( s, throwable );
			 }

			 public override void warn( string s )
			 {
				  _log.warn( s );
			 }

			 public override void warn( string s, object o )
			 {
				  _log.warn( slf4jToStringFormatPlaceholders( s ), o );
			 }

			 public override void warn( string s, params object[] objects )
			 {
				  _log.warn( slf4jToStringFormatPlaceholders( s ), objects );
			 }

			 public override void warn( string s, object o, object o1 )
			 {
				  _log.warn( slf4jToStringFormatPlaceholders( s ), o, o1 );
			 }

			 public override void warn( string s, Exception throwable )
			 {
				  _log.warn( s, throwable );
			 }

			 public override void error( string s )
			 {
				  _log.error( s );
			 }

			 public override void error( string s, object o )
			 {
				  _log.error( slf4jToStringFormatPlaceholders( s ), o );
			 }

			 public override void error( string s, object o, object o1 )
			 {
				  _log.error( slf4jToStringFormatPlaceholders( s ), o, o1 );
			 }

			 public override void error( string s, params object[] objects )
			 {
				  _log.error( slf4jToStringFormatPlaceholders( s ), objects );
			 }

			 public override void error( string s, Exception throwable )
			 {
				  _log.error( s, throwable );
			 }

			 public override void trace( string s )
			 {

			 }

			 public override void trace( string s, object o )
			 {

			 }

			 public override void trace( string s, object o, object o1 )
			 {

			 }

			 public override void trace( string s, params object[] objects )
			 {

			 }

			 public override void trace( string s, Exception throwable )
			 {

			 }
		 }
	}

}