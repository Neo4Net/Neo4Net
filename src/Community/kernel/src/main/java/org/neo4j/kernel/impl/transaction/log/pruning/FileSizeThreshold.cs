﻿/*
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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	public sealed class FileSizeThreshold : Threshold
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly long _maxSize;

		 private long _currentSize;

		 internal FileSizeThreshold( FileSystemAbstraction fileSystem, long maxSize )
		 {
			  this._fileSystem = fileSystem;
			  this._maxSize = maxSize;
		 }

		 public override void Init()
		 {
			  _currentSize = 0;
		 }

		 public override bool Reached( File file, long version, LogFileInformation source )
		 {
			  _currentSize += _fileSystem.getFileSize( file );
			  return _currentSize >= _maxSize;
		 }
	}

}