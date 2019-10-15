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
namespace Neo4Net.Kernel.Api.Impl.Index.partition
{
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using Directory = org.apache.lucene.store.Directory;


	using Neo4Net.Functions;

	/// <summary>
	/// Factory to create writable partitions for partitioned index.
	/// </summary>
	public class WritableIndexPartitionFactory : IndexPartitionFactory
	{
		 private Factory<IndexWriterConfig> _writerConfigFactory;

		 public WritableIndexPartitionFactory( Factory<IndexWriterConfig> writerConfigFactory )
		 {
			  this._writerConfigFactory = writerConfigFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public AbstractIndexPartition createPartition(java.io.File partitionFolder, org.apache.lucene.store.Directory directory) throws java.io.IOException
		 public override AbstractIndexPartition CreatePartition( File partitionFolder, Directory directory )
		 {
			  return new WritableIndexPartition( partitionFolder, directory, _writerConfigFactory.newInstance() );
		 }
	}

}