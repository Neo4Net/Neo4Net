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
namespace Org.Neo4j.Kernel.Api.Impl.Index.storage
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;

	public class IndexStorageFactory : AutoCloseable
	{
		 private readonly DirectoryFactory _dirFactory;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly IndexDirectoryStructure _structure;

		 public IndexStorageFactory( DirectoryFactory dirFactory, FileSystemAbstraction fileSystem, IndexDirectoryStructure structure )
		 {
			  this._dirFactory = dirFactory;
			  this._fileSystem = fileSystem;
			  this._structure = structure;
		 }

		 public virtual PartitionedIndexStorage IndexStorageOf( long indexId )
		 {
			  return new PartitionedIndexStorage( _dirFactory, _fileSystem, _structure.directoryForIndex( indexId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _dirFactory.close();
		 }
	}

}