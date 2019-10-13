using System.Collections.Concurrent;

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
namespace Neo4Net.Test.extension
{

	public class ExecutionSharedContext
	{
		 internal const string FAILED_TEST_FILE_KEY = "failedFileName";
		 internal const string LOCKED_TEST_FILE_KEY = "lockedFileName";
		 internal const string SUCCESSFUL_TEST_FILE_KEY = "successfulFileName";
		 private static readonly ConcurrentDictionary<string, object> _context = new ConcurrentDictionary<string, object>();

		 public static readonly ExecutionSharedContext Context = new ExecutionSharedContext();

		 private ExecutionSharedContext()
		 {
		 }

		 public virtual void Clear()
		 {
			  _context.Clear();
		 }

		 public virtual T GetValue<T>( string key )
		 {
			  return ( T ) _context[key];
		 }

		 public virtual void SetValue( string key, object value )
		 {
			  _context[key] = value;
		 }
	}

}