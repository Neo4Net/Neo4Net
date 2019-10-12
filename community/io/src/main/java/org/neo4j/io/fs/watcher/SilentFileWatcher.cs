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
namespace Org.Neo4j.Io.fs.watcher
{

	using WatchedResource = Org.Neo4j.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// Silent file watcher implementation that do not perform any monitoring and can't observe any directories status or
	/// content update.
	/// </summary>
	public class SilentFileWatcher : FileWatcher
	{

		 public override WatchedResource Watch( File file )
		 {
			  return Org.Neo4j.Io.fs.watcher.resource.WatchedResource_Fields.Empty;
		 }

		 public override void AddFileWatchEventListener( FileWatchEventListener listener )
		 {
		 }

		 public override void RemoveFileWatchEventListener( FileWatchEventListener listener )
		 {
		 }

		 public override void StopWatching()
		 {
		 }

		 public override void StartWatching()
		 {
		 }

		 public override void Close()
		 {
		 }
	}

}