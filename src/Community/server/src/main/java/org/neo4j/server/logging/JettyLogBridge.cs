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
namespace Neo4Net.Server.logging
{
	using AbstractLogger = org.eclipse.jetty.util.log.AbstractLogger;
	using Logger = org.eclipse.jetty.util.log.Logger;


	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.@internal.LogMessageUtil.slf4jToStringFormatPlaceholders;

	public class JettyLogBridge : AbstractLogger
	{
		 private static readonly Pattern _packagePattern = Pattern.compile( "(\\w)\\w+\\." );
		 private static readonly AtomicReference<LogProvider> _logProvider = new AtomicReference<LogProvider>( NullLogProvider.Instance );
		 private readonly string _fullname;
		 private readonly Log _log;

		 public JettyLogBridge() : this("org.eclipse.jetty.util.log")
		 {
		 }

		 public JettyLogBridge( string fullname )
		 {
			  this._fullname = fullname;
			  this._log = _logProvider.get().getLog(_packagePattern.matcher(fullname).replaceAll("$1."));
		 }

		 public static LogProvider setLogProvider( LogProvider newLogProvider )
		 {
			  return _logProvider.getAndSet( newLogProvider );
		 }

		 protected internal override Logger NewLogger( string fullname )
		 {
			  return new JettyLogBridge( fullname );
		 }

		 public override string Name
		 {
			 get
			 {
				  return _fullname;
			 }
		 }

		 public override void Warn( string msg, params object[] args )
		 {
			  _log.warn( slf4jToStringFormatPlaceholders( msg ), args );
		 }

		 public override void Warn( Exception thrown )
		 {
			  _log.warn( "", thrown );
		 }

		 public override void Warn( string msg, Exception thrown )
		 {
			  _log.warn( msg, thrown );
		 }

		 public override void Info( string msg, params object[] args )
		 {
		 }

		 public override void Info( Exception thrown )
		 {
		 }

		 public override void Info( string msg, Exception thrown )
		 {
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return false;
			 }
			 set
			 {
				  throw new System.NotSupportedException();
			 }
		 }


		 public override void Debug( string msg, params object[] args )
		 {
		 }

		 public override void Debug( Exception thrown )
		 {
		 }

		 public override void Debug( string msg, Exception thrown )
		 {
		 }

		 public override void Ignore( Exception ignored )
		 {
		 }
	}

}