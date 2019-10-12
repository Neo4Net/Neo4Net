using System;
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
namespace Org.Neo4j.Logging
{

	/// <summary>
	/// A <seealso cref="LogProvider"/> implementation that duplicates all messages to other LogProvider instances
	/// </summary>
	public class DuplicatingLogProvider : AbstractLogProvider<DuplicatingLog>
	{
		 private readonly CopyOnWriteArraySet<LogProvider> _logProviders;
		 private readonly IDictionary<DuplicatingLog, IDictionary<LogProvider, Log>> _duplicatingLogCache = Collections.synchronizedMap( new WeakHashMap<DuplicatingLog, IDictionary<LogProvider, Log>>() );

		 /// <param name="logProviders"> A list of <seealso cref="LogProvider"/> instances that messages should be duplicated to </param>
		 public DuplicatingLogProvider( params LogProvider[] logProviders )
		 {
			  this._logProviders = new CopyOnWriteArraySet<LogProvider>( Arrays.asList( logProviders ) );
		 }

		 /// <summary>
		 /// Remove a <seealso cref="LogProvider"/> from the duplicating set. Note that the LogProvider must return
		 /// cached Log instances from its <seealso cref="LogProvider.getLog(string)"/> for this to behave as expected.
		 /// </summary>
		 /// <param name="logProvider"> the LogProvider to be removed </param>
		 /// <returns> true if the log was found and removed </returns>
		 public virtual bool Remove( LogProvider logProvider )
		 {
			  if ( !this._logProviders.remove( logProvider ) )
			  {
					return false;
			  }
			  foreach ( DuplicatingLog duplicatingLog in CachedLogs() )
			  {
					duplicatingLog.Remove( _duplicatingLogCache[duplicatingLog].Remove( logProvider ) );
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected DuplicatingLog buildLog(final Class loggingClass)
		 protected internal override DuplicatingLog BuildLog( Type loggingClass )
		 {
			  return buildLog( logProvider => logProvider.getLog( loggingClass ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected DuplicatingLog buildLog(final String name)
		 protected internal override DuplicatingLog BuildLog( string name )
		 {
			  return buildLog( logProvider => logProvider.getLog( name ) );
		 }

		 private DuplicatingLog BuildLog( System.Func<LogProvider, Log> logConstructor )
		 {
			  List<Log> logs = new List<Log>( _logProviders.size() );
			  Dictionary<LogProvider, Log> providedLogs = new Dictionary<LogProvider, Log>();
			  foreach ( LogProvider logProvider in _logProviders )
			  {
					Log log = logConstructor( logProvider );
					providedLogs[logProvider] = log;
					logs.Add( log );
			  }
			  DuplicatingLog duplicatingLog = new DuplicatingLog( logs );
			  _duplicatingLogCache[duplicatingLog] = providedLogs;
			  return duplicatingLog;
		 }
	}

}