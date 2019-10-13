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
	/// Default implementation of <seealso cref="FolderLayout"/> for partitioned lucene index.
	/// </summary>
	/// <seealso cref= FolderLayout </seealso>
	public class IndexFolderLayout : FolderLayout
	{
		 private readonly File _indexFolder;

		 public IndexFolderLayout( File rootDirectory )
		 {
			  this._indexFolder = rootDirectory;
		 }

		 public virtual File IndexFolder
		 {
			 get
			 {
				  return _indexFolder;
			 }
		 }

		 public override File GetPartitionFolder( int partition )
		 {
			  return new File( _indexFolder, partition.ToString() );
		 }
	}

}