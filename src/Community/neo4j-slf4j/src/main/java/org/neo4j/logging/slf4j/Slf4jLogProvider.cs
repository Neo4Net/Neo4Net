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
namespace Neo4Net.Logging.slf4j
{
	using ILoggerFactory = org.slf4j.ILoggerFactory;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// A <seealso cref="LogProvider"/> that forwards log events to SLF4J
	/// </summary>
	public class Slf4jLogProvider : LogProvider
	{
		 private ILoggerFactory _loggerFactory;

		 public Slf4jLogProvider() : this(LoggerFactory.ILoggerFactory)
		 {
		 }

		 public Slf4jLogProvider( ILoggerFactory loggerFactory )
		 {
			  this._loggerFactory = loggerFactory;
		 }

		 public override Log GetLog( Type loggingClass )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return new Slf4jLog( _loggerFactory.getLogger( loggingClass.FullName ) );
		 }

		 public override Log GetLog( string context )
		 {
			  return new Slf4jLog( _loggerFactory.getLogger( context ) );
		 }
	}

}