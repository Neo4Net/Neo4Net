﻿/*
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
namespace Neo4Net.Kernel.impl.coreapi
{

	using Relationship = Neo4Net.Graphdb.Relationship;
	using ReadableRelationshipIndex = Neo4Net.Graphdb.index.ReadableRelationshipIndex;
	using RelationshipAutoIndexer = Neo4Net.Graphdb.index.RelationshipAutoIndexer;
	using AutoIndexOperations = Neo4Net.Kernel.api.explicitindex.AutoIndexOperations;

	public class RelationshipAutoIndexerFacade : AutoIndexerFacade<Relationship>, RelationshipAutoIndexer
	{
		 private readonly System.Func<ReadableRelationshipIndex> _idxSupplier;

		 public RelationshipAutoIndexerFacade( System.Func<ReadableRelationshipIndex> idxSupplier, AutoIndexOperations autoIndexing ) : base( idxSupplier.get, autoIndexing )
		 {
			  this._idxSupplier = idxSupplier;
		 }

		 public override ReadableRelationshipIndex AutoIndex
		 {
			 get
			 {
				  return _idxSupplier.get();
			 }
		 }
	}

}