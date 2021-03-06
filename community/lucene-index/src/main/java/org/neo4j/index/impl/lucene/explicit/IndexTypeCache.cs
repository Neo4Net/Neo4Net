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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{

	using Org.Neo4j.Helpers.Collection;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;

	internal class IndexTypeCache
	{
		 private readonly IDictionary<IndexIdentifier, Pair<int, IndexType>> _cache = Collections.synchronizedMap( new Dictionary<IndexIdentifier, Pair<int, IndexType>>() );
		 private readonly IndexConfigStore _indexStore;

		 internal IndexTypeCache( IndexConfigStore indexStore )
		 {
			  this._indexStore = indexStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexType getIndexType(IndexIdentifier identifier, boolean recovery) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual IndexType GetIndexType( IndexIdentifier identifier, bool recovery )
		 {
			  Pair<int, IndexType> type = _cache[identifier];
			  IDictionary<string, string> config = _indexStore.get( identifier.EntityType.entityClass(), identifier.IndexName );
			  if ( config == null )
			  {
					if ( recovery )
					{
						 return null;
					}
					throw new ExplicitIndexNotFoundKernelException( "Index '%s' doesn't exist.", identifier );
			  }
			  if ( type != null && config.GetHashCode() == type.First() )
			  {
					return type.Other();
			  }
			  type = Pair.of( config.GetHashCode(), IndexType.GetIndexType(config) );
			  _cache[identifier] = type;
			  return type.Other();
		 }

		 internal virtual void Invalidate( IndexIdentifier identifier )
		 {
			  _cache.Remove( identifier );
		 }
	}

}