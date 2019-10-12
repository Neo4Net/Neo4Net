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
namespace Neo4Net.Io.fs
{
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Life cycle adapter that will perform file system abstraction shutdown on life shutdown.
	/// </summary>
	/// <seealso cref= Lifecycle </seealso>
	/// <seealso cref= FileSystemAbstraction </seealso>
	public class FileSystemLifecycleAdapter : LifecycleAdapter
	{
		 private FileSystemAbstraction _fileSystemAbstraction;

		 public FileSystemLifecycleAdapter( FileSystemAbstraction fileSystemAbstraction )
		 {
			  this._fileSystemAbstraction = fileSystemAbstraction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _fileSystemAbstraction.Dispose();
		 }
	}

}