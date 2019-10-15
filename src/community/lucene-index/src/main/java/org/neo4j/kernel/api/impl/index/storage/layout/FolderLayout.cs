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
namespace Neo4Net.Kernel.Api.Impl.Index.storage.layout
{

	/// <summary>
	/// Component that represent on-disk layout for partitioned lucene index.
	/// <para>
	/// It's aware how files and partitions located on disk and knows how to map particular partition index into folder.
	/// Physically files for index with id <i>indexId</i> will be located in a index root folder indexId with
	/// separate sub-folder for each partition.
	/// Since each partition is separate lucene index all lucene index files will be located in a corresponding partition
	/// folder.
	/// </para>
	/// <para>
	/// As example for index with 3 partitions we will have following directory structure:
	/// <pre>
	/// ...indexId/
	///    |-- 1
	///    |   `-- partition index files
	///    |-- 2
	///    |   `-- partition index files
	///    |-- 3
	///        `-- partition index files
	/// </pre>
	/// </para>
	/// </summary>
	public interface FolderLayout
	{
		 /// <summary>
		 /// Get root folder of partitioned index
		 /// </summary>
		 /// <returns> the file that represent directory where whole index is located </returns>
		 File IndexFolder { get; }

		 /// <summary>
		 /// Get folder that contain particular partition
		 /// </summary>
		 /// <param name="partition"> index of partition to get folder for </param>
		 /// <returns> the file that represents directory where partition with given index is located. </returns>
		 File GetPartitionFolder( int partition );
	}

}