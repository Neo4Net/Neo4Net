using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.Logging
{

	/// <summary>
	/// An abstract <seealso cref="LogProvider"/> implementation, which ensures <seealso cref="Log"/>s are cached and reused.
	/// </summary>
	public abstract class AbstractLogProvider<T> : LogProvider where T : Log
	{
		 private readonly ConcurrentDictionary<string, T> _logCache = new ConcurrentDictionary<string, T>();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public T getLog(final Class loggingClass)
		 public override T GetLog( Type loggingClass )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return GetLog( loggingClass.FullName, () => BuildLog(loggingClass) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public T getLog(final String name)
		 public override T GetLog( string name )
		 {
			  return GetLog( name, () => BuildLog(name) );
		 }

		 private T GetLog( string name, System.Func<T> logSupplier )
		 {
			  return _logCache.computeIfAbsent( name, s => logSupplier() );
		 }

		 /// <returns> a <seealso cref="System.Collections.ICollection"/> of the <seealso cref="Log"/> mappings that are currently held in the cache </returns>
		 protected internal virtual ICollection<T> CachedLogs()
		 {
			  return _logCache.Values;
		 }

		 /// <param name="loggingClass"> the context for the returned <seealso cref="Log"/> </param>
		 /// <returns> a <seealso cref="Log"/> that logs messages with the {@code loggingClass} as the context </returns>
		 protected internal abstract T BuildLog( Type loggingClass );

		 /// <param name="name"> the context for the returned <seealso cref="Log"/> </param>
		 /// <returns> a <seealso cref="Log"/> that logs messages with the specified name as the context </returns>
		 protected internal abstract T BuildLog( string name );
	}

}