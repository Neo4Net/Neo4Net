using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;

	public interface IIndexingUpdateService
	{
		 /// <summary>
		 /// Apply updates to the relevant indexes.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void apply(Neo4Net.kernel.impl.transaction.state.IndexUpdates updates) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 void Apply( IndexUpdates updates );

		 /// <summary>
		 /// Convert the updates for a node into index entry updates. This has to happen earlier than the actual
		 /// application of the updates to the indexes, so that the properties reflect the exact state of the
		 /// transaction.
		 /// </summary>
		 IEnumerable<IndexEntryUpdate<SchemaDescriptor>> ConvertToIndexUpdates( IEntityUpdates IEntityUpdates, EntityType type );
	}

}