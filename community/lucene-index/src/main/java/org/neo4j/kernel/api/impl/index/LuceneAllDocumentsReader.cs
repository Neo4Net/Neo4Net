﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Index
{
	using Document = org.apache.lucene.document.Document;


	using Org.Neo4j.Helpers.Collection;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using IOUtils = Org.Neo4j.Io.IOUtils;

	public class LuceneAllDocumentsReader : BoundedIterable<Document>
	{
		 private readonly IList<LucenePartitionAllDocumentsReader> _partitionReaders;

		 public LuceneAllDocumentsReader( IList<LucenePartitionAllDocumentsReader> partitionReaders )
		 {
			  this._partitionReaders = partitionReaders;
		 }

		 public override long MaxCount()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _partitionReaders.Select( LucenePartitionAllDocumentsReader::maxCount ).Sum();
		 }

		 public override IEnumerator<Document> Iterator()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IEnumerator<IEnumerator<Document>> iterators = _partitionReaders.Select( LucenePartitionAllDocumentsReader::iterator ).ToList().GetEnumerator();

			  return Iterators.concat( iterators );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _partitionReaders );
		 }
	}

}