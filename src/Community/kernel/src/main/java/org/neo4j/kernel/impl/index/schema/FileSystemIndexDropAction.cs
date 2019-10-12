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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexes.deleteIndex;

	public class FileSystemIndexDropAction : IndexDropAction
	{
		 private readonly FileSystemAbstraction _fs;
		 private readonly IndexDirectoryStructure _directoryStructure;

		 public FileSystemIndexDropAction( FileSystemAbstraction fs, IndexDirectoryStructure directoryStructure )
		 {
			  this._fs = fs;
			  this._directoryStructure = directoryStructure;
		 }

		 public override void Drop( long indexId, bool archiveExistentIndex )
		 {
			  try
			  {
					deleteIndex( _fs, _directoryStructure, indexId, archiveExistentIndex );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}