using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

	/// <summary>
	/// A place to access <seealso cref="BatchInserterIndex"/>s from a certain index provider.
	/// Use together with <seealso cref="BatchInserter"/> to create indexes which later can be
	/// accessed through <seealso cref="GraphDatabaseService.index()"/>.
	/// 
	/// @author Mattias Persson
	/// </summary>
	/// @deprecated This will be removed in 4.0. 
	[Obsolete("This will be removed in 4.0.")]
	public interface BatchInserterIndexProvider
	{
		 /// <summary>
		 /// Returns a <seealso cref="BatchInserterIndex"/> for <seealso cref="Node"/>s for the name
		 /// {@code indexName} with the given {@code config}. The {@code config}
		 /// <seealso cref="System.Collections.IDictionary"/> can contain any provider-implementation-specific data that
		 /// can control how an index behaves.
		 /// </summary>
		 /// <param name="indexName"> the name of the index. It will be created if it doesn't
		 ///            exist. </param>
		 /// <param name="config"> a <seealso cref="System.Collections.IDictionary"/> of configuration parameters to use with the
		 ///            index if it doesn't exist. Parameters can be anything and are
		 ///            implementation-specific. </param>
		 /// <returns> the <seealso cref="BatchInserterIndex"/> corresponding to the
		 ///         {@code indexName}. </returns>
		 [Obsolete]
		 BatchInserterIndex NodeIndex( string indexName, IDictionary<string, string> config );

		 /// <summary>
		 /// Returns a <seealso cref="BatchInserterIndex"/> for <seealso cref="Relationship"/>s for the
		 /// name {@code indexName} with the given {@code config}. The {@code config}
		 /// <seealso cref="System.Collections.IDictionary"/> can contain any provider-implementation-specific data that
		 /// can control how an index behaves.
		 /// </summary>
		 /// <param name="indexName"> the name of the index. It will be created if it doesn't
		 ///            exist. </param>
		 /// <param name="config"> a <seealso cref="System.Collections.IDictionary"/> of configuration parameters to use with the
		 ///            index if it doesn't exist. Parameters can be anything and are
		 ///            implementation-specific. </param>
		 /// <returns> the <seealso cref="BatchInserterIndex"/> corresponding to the
		 ///         {@code indexName}. </returns>
		 [Obsolete]
		 BatchInserterIndex RelationshipIndex( string indexName, IDictionary<string, string> config );

		 /// <summary>
		 /// Shuts down this index provider and ensures that all indexes are fully
		 /// written to disk. If this method isn't called before shutting down the
		 /// <seealso cref="BatchInserter"/> there's no guaranteed that data added to indexes
		 /// will be persisted.
		 /// </summary>
		 [Obsolete]
		 void Shutdown();
	}

}