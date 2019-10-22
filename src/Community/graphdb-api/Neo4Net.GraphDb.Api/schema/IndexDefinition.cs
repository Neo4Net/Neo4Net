using System;
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
namespace Neo4Net.GraphDb.schema
{

	using IndexManager = Neo4Net.GraphDb.index.IndexManager;

	/// <summary>
	/// Definition for an index.
	/// <para>
	/// NOTE: This is part of the index API introduced in Neo4Net 2.0.
	/// The explicit index API lives in <seealso cref="IndexManager"/>.
	/// </para>
	/// </summary>
	public interface IndexDefinition
	{
		 /// <summary>
		 /// Return the node label that this index applies to. Nodes with this label are indexed by this index.
		 /// <para>
		 /// Note that this assumes that this is a node index (that <seealso cref="isNodeIndex()"/> returns {@code true}) and not a multi-token index
		 /// (that <seealso cref="isMultiTokenIndex()"/> returns {@code false}). If this is not the case, then an <seealso cref="System.InvalidOperationException"/> is thrown.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the <seealso cref="Label label"/> this index definition is associated with. </returns>
		 /// @deprecated This method is deprecated and will be removed in next major release. Please consider using <seealso cref="getLabels()"/> instead. 
		 [Obsolete("This method is deprecated and will be removed in next major release. Please consider using <seealso cref=\"getLabels()\"/> instead.")]
		 ILabel Label { get; }

		 /// <summary>
		 /// Return the set of node labels (in no particular order) that this index applies to. This method works for both <seealso cref="isMultiTokenIndex() multi-token"/>
		 /// indexes, and "single-token" indexes.
		 /// <para>
		 /// Note that this assumes that this is a node index (that <seealso cref="isNodeIndex()"/> returns {@code true}). If this is not the case, then an
		 /// <seealso cref="System.InvalidOperationException"/> is thrown.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the set of <seealso cref="Label labels"/> this index definition is associated with. </returns>
		 IEnumerable<ILabel> Labels { get; }

		 /// <summary>
		 /// Return the relationship type that this index applies to. Relationships with this type are indexed by this index.
		 /// <para>
		 /// Note that this assumes that this is a relationship index (that <seealso cref="isRelationshipIndex()"/> returns {@code true}) and not a multi-token index
		 /// (that <seealso cref="isMultiTokenIndex()"/> returns {@code false}). If this is not the case, then an <seealso cref="System.InvalidOperationException"/> is thrown.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the <seealso cref="RelationshipType relationship type"/> this index definition is associated with. </returns>
		 /// @deprecated This method is deprecated and will be removed in next major release. Please consider using <seealso cref="getRelationshipTypes()"/> instead. 
		 [Obsolete("This method is deprecated and will be removed in next major release. Please consider using <seealso cref=\"getRelationshipTypes()\"/> instead.")]
		 IRelationshipType RelationshipType { get; }

		 /// <summary>
		 /// Return the set of relationship types (in no particular order) that this index applies to. This method works for both
		 /// <seealso cref="isMultiTokenIndex() mult-token"/> indexes, and "single-token" indexes.
		 /// <para>
		 /// Note that this assumes that this is a relationship index (that <seealso cref="isRelationshipIndex()"/> returns {@code true}). If thisk is not the case, then an
		 /// <seealso cref="System.InvalidOperationException"/> is thrown.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the set of <seealso cref="RelationshipType relationship types"/> this index definition is associated with. </returns>
		 IEnumerable<IRelationshipType> RelationshipTypes { get; }

		 /// <summary>
		 /// Return the set of properties that are indexed by this index.
		 /// <para>
		 /// Most indexes will only have a single property, but <seealso cref="isCompositeIndex() composite indexes"/> will have multiple properties.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the property keys this index was created on. </returns>
		 /// <seealso cref= #isCompositeIndex() </seealso>
		 IEnumerable<string> PropertyKeys { get; }

		 /// <summary>
		 /// Drops this index. <seealso cref="Schema.getIndexes(ILabel)"/> will no longer include this index
		 /// and any related background jobs and files will be stopped and removed.
		 /// </summary>
		 void Drop();

		 /// <returns> {@code true} if this index is created as a side effect of the creation of a uniqueness constraint. </returns>
		 bool ConstraintIndex { get; }

		 /// <returns> {@code true} if this index is indexing nodes, otherwise {@code false}. </returns>
		 bool NodeIndex { get; }

		 /// <returns> {@code true} if this index is indexing relationships, otherwise {@code false}. </returns>
		 bool RelationshipIndex { get; }

		 /// <summary>
		 /// A multi-token index is an index that indexes nodes or relationships that have any or all of a given set of labels or relationship types, respectively.
		 /// <para>
		 /// For instance, a multi-token index could apply to all {@code Movie} and {@code Book} nodes that have a {@code description} property. A node or
		 /// relationship do not need to have all of the labels or relationship types for it to be indexed. A node that has any of the given labels, or a relationship
		 /// that has any of the given relationship types, will be a candidate for indexing, depending on their properties.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if this is a multi-token index. </returns>
		 bool MultiTokenIndex { get; }

		 /// <summary>
		 /// A composite index is an index that indexes nodes or relationships by more than one property.
		 /// <para>
		 /// For instance, a composite index for {@code PhoneNumber} nodes could be indexing the {@code country_code}, {@code area_code}, {@code prefix},
		 /// and {@code line_number}.
		 /// 
		 /// <strong>Note:</strong> it is index-implementation specific if a node or relationship must have all of the properties in order to be indexable,
		 /// or if having any of the properties is enough for the given node or relationship to be indexed. For instance, {@code NODE KEY} constraint indexes
		 /// require that all of the properties be present on a node before it will be included in the index, while a full-text index will index nodes or
		 /// relationships that have any of the given properties.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if this is a composite index. </returns>
		 bool CompositeIndex { get; }

		 /// <summary>
		 /// Get the name given to this index when it was created, if any.
		 /// If the index was not given any name, then the string {@code "Unnamed index"} is returned instead.
		 /// </summary>
		 string Name { get; }
	}

}