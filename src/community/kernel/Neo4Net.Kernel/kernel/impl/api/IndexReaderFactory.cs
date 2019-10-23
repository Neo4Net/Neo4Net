﻿using System.Collections.Generic;

/*
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
namespace Neo4Net.Kernel.Impl.Api
{

	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexProxy = Neo4Net.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;

	public interface IndexReaderFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader newReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
		 IndexReader NewReader( IndexDescriptor descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader newUnCachedReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
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
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader newReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader newUnCachedReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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