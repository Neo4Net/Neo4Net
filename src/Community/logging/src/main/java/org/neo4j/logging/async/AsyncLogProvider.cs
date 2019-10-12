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
namespace Neo4Net.Logging.async
{
	using Neo4Net.Util.concurrent;

	public class AsyncLogProvider : LogProvider
	{
		 private readonly LogProvider _provider;
		 private readonly AsyncEventSender<AsyncLogEvent> _events;

		 public AsyncLogProvider( AsyncEventSender<AsyncLogEvent> events, LogProvider provider )
		 {
			  this._provider = provider;
			  this._events = events;
		 }

		 public override Log GetLog( Type loggingClass )
		 {
			  return new AsyncLog( _events, _provider.getLog( loggingClass ) );
		 }

		 public override Log GetLog( string name )
		 {
			  return new AsyncLog( _events, _provider.getLog( name ) );
		 }
	}

}