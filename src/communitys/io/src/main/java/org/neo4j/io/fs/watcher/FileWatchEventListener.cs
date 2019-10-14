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
namespace Neo4Net.Io.fs.watcher
{

	/// <summary>
	/// <seealso cref="FileWatcher"/> listener that allow receive state change updates for registered resources.
	/// </summary>
	public interface FileWatchEventListener : EventListener
	{
		 /// <summary>
		 /// Notification about deletion of file with provided name
		 /// </summary>
		 /// <param name="fileName"> deleted file name </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void fileDeleted(String fileName)
	//	 {
	//		  // no-op
	//	 }

		 /// <summary>
		 /// Notification about update of file with provided name
		 /// </summary>
		 /// <param name="fileName"> updated file name </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void fileModified(String fileName)
	//	 {
	//		  // no-op
	//	 }
	}

}