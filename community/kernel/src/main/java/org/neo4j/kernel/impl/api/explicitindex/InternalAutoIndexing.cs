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
namespace Org.Neo4j.Kernel.Impl.Api.explicitindex
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using AutoIndexOperations = Org.Neo4j.Kernel.api.explicitindex.AutoIndexOperations;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;

	/// <summary>
	/// This gets notified whenever there are changes to entities and their properties, and given a runtime-configurable set of rules
	/// then automatically triggers writes to two special explicit indexes - eg. it automatically keeps these indexes up to date.
	/// </summary>
	public class InternalAutoIndexing : AutoIndexing
	{
		 public const string NODE_AUTO_INDEX = "node_auto_index";
		 public const string RELATIONSHIP_AUTO_INDEX = "relationship_auto_index";

		 private readonly InternalAutoIndexOperations _nodes;
		 private readonly InternalAutoIndexOperations _relationships;

		 public InternalAutoIndexing( Config config, TokenHolder propertyKeyLookup )
		 {
			  this._nodes = new InternalAutoIndexOperations( propertyKeyLookup, InternalAutoIndexOperations.EntityType.Node );
			  this._relationships = new InternalAutoIndexOperations( propertyKeyLookup, InternalAutoIndexOperations.EntityType.Relationship );

			  this._nodes.enabled( config.Get( GraphDatabaseSettings.node_auto_indexing ) );
			  this._nodes.replacePropertyKeysToInclude( config.Get( GraphDatabaseSettings.node_keys_indexable ) );
			  this._relationships.enabled( config.Get( GraphDatabaseSettings.relationship_auto_indexing ) );
			  this._relationships.replacePropertyKeysToInclude( config.Get( GraphDatabaseSettings.relationship_keys_indexable ) );
		 }

		 public override AutoIndexOperations Nodes()
		 {
			  return _nodes;
		 }

		 public override AutoIndexOperations Relationships()
		 {
			  return _relationships;
		 }
	}

}