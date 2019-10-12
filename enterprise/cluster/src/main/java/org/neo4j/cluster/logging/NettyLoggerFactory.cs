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
namespace Org.Neo4j.cluster.logging
{
	using AbstractInternalLogger = org.jboss.netty.logging.AbstractInternalLogger;
	using InternalLogLevel = org.jboss.netty.logging.InternalLogLevel;
	using InternalLogger = org.jboss.netty.logging.InternalLogger;
	using InternalLoggerFactory = org.jboss.netty.logging.InternalLoggerFactory;

	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Adapter which send Netty logging messages to our internal log.
	/// </summary>
	public class NettyLoggerFactory : InternalLoggerFactory
	{
		 private LogProvider _logProvider;

		 public NettyLoggerFactory( LogProvider logProvider )
		 {
			  this._logProvider = logProvider;
		 }

		 public override InternalLogger NewInstance( string name )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.Log log = logProvider.getLog(name);
			  Log log = _logProvider.getLog( name );
			  return new AbstractInternalLoggerAnonymousInnerClass( this, log );
		 }

		 private class AbstractInternalLoggerAnonymousInnerClass : AbstractInternalLogger
		 {
			 private readonly NettyLoggerFactory _outerInstance;

			 private Log _log;

			 public AbstractInternalLoggerAnonymousInnerClass( NettyLoggerFactory outerInstance, Log log )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
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
					  return true;
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

			 public override bool isEnabled( InternalLogLevel level )
			 {
				  return level != InternalLogLevel.DEBUG || DebugEnabled;
			 }

			 public override void debug( string msg )
			 {
				  _log.debug( msg );
			 }

			 public override void debug( string msg, Exception cause )
			 {
				  _log.debug( msg, cause );
			 }

			 public override void info( string msg )
			 {
				  _log.info( msg );
			 }

			 public override void info( string msg, Exception cause )
			 {
				  _log.info( msg, cause );
			 }

			 public override void warn( string msg )
			 {
				  _log.warn( msg );
			 }

			 public override void warn( string msg, Exception cause )
			 {
				  _log.warn( msg, cause );
			 }

			 public override void error( string msg )
			 {
				  _log.error( msg );
			 }

			 public override void error( string msg, Exception cause )
			 {
				  _log.error( msg, cause );
			 }
		 }
	}

}