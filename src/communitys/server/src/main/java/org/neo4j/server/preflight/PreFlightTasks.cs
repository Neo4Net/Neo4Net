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
namespace Neo4Net.Server.preflight
{
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// These are tasks that are run on server startup that may take a long time
	/// to execute, such as recovery, upgrades and so on.
	/// 
	/// This implementation still needs some work, because of some of the refactoring
	/// done regarding the NeoServer. Specifically, some of these tasks verify that
	/// properties files exist and are valid. Other preflight tasks we might want to
	/// add could be auto-generating config files if they don't exist and creating required
	/// directories.
	/// 
	/// All of these except generating neo4j.conf depend on having
	/// the configuration available. Eg. we can't both ensure that file exists within these
	/// tests, while at the same time depending on that file existing.
	/// 
	/// Ensuring the config file exists (and potentially auto-generating it) is a problem.
	/// Either this need to be split into tasks that have dependencies, and tasks that don't.
	/// 
	/// Although, it seems it is only this one edge case, so perhaps accepting that and adding
	/// code to the bootstrapper to ensure the config file exists is acceptable.
	/// </summary>
	public class PreFlightTasks
	{
		 private readonly PreflightTask[] _tasks;
		 private readonly Log _log;

		 private PreflightTask _failedTask;

		 public PreFlightTasks( LogProvider logProvider, params PreflightTask[] tasks )
		 {
			  this._tasks = tasks;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public virtual bool Run()
		 {
			  if ( _tasks == null || _tasks.Length < 1 )
			  {
					return true;
			  }

			  foreach ( PreflightTask r in _tasks )
			  {
					if ( !r.Run() )
					{
						 _log.error( r.FailureMessage );
						 _failedTask = r;
						 return false;
					}
			  }

			  return true;
		 }

		 public virtual PreflightTask FailedTask()
		 {
			  return _failedTask;
		 }
	}

}