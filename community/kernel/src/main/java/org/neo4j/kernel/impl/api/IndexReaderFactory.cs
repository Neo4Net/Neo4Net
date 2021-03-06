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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;

	public interface IndexReaderFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.IndexReader newReader(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReader NewReader( IndexDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.IndexReader newUnCachedReader(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReader NewUnCachedReader( IndexDescriptor descriptor );

		 void Close();
	}

	 public class IndexReaderFactory_Caching : IndexReaderFactory
	 {
		  internal IDictionary<IndexDescriptor, IndexReader> IndexReaders;
		  internal readonly IndexingService IndexingService;

		  public IndexReaderFactory_Caching( IndexingService indexingService )
		  {
				this.IndexingService = indexingService;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newReader(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		  public override IndexReader NewReader( IndexDescriptor descriptor )
		  {
				if ( IndexReaders == null )
				{
					 IndexReaders = new Dictionary<IndexDescriptor, IndexReader>();
				}

				IndexReader reader = IndexReaders[descriptor];
				if ( reader == null )
				{
					 reader = NewUnCachedReader( descriptor );
					 IndexReaders[descriptor] = reader;
				}
				return reader;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newUnCachedReader(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		  public override IndexReader NewUnCachedReader( IndexDescriptor descriptor )
		  {
				IndexProxy index = IndexingService.getIndexProxy( descriptor.Schema() );
				return index.NewReader();
		  }

		  public override void Close()
		  {
				if ( IndexReaders != null )
				{
					 foreach ( IndexReader indexReader in IndexReaders.Values )
					 {
						  indexReader.Close();
					 }
					 IndexReaders.Clear();
				}
		  }
	 }

}