//////////////////// OBSOLETE $!!$ tac
////////////////////using System;
////////////////////using System.Collections.Generic;

/////////////////////*
//////////////////// * Copyright © 2018-2020 "Neo4Net,"
//////////////////// * Team NeoN [http://neo4net.com]. All Rights Reserved.
//////////////////// *
//////////////////// * This file is part of Neo4Net.
//////////////////// *
//////////////////// * Neo4Net is free software: you can redistribute it and/or modify
//////////////////// * it under the terms of the GNU General Public License as published by
//////////////////// * the Free Software Foundation, either version 3 of the License, or
//////////////////// * (at your option) any later version.
//////////////////// *
//////////////////// * This program is distributed in the hope that it will be useful,
//////////////////// * but WITHOUT ANY WARRANTY; without even the implied warranty of
//////////////////// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//////////////////// * GNU General Public License for more details.
//////////////////// *
//////////////////// * You should have received a copy of the GNU General Public License
//////////////////// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
//////////////////// */
////////////////////namespace Neo4Net.GraphDb.index
////////////////////{


////////////////////	/// <summary>
////////////////////	/// A one stop shop for accessing <seealso cref="Index"/>s for <seealso cref="INode"/>s
////////////////////	/// and <seealso cref="IRelationship"/>s. An <seealso cref="IndexManager"/> is paired with a
////////////////////	/// <seealso cref="GraphDatabaseService"/> via <seealso cref="GraphDatabaseService.index()"/> so that
////////////////////	/// indexes can be accessed directly from the graph database. </summary>
////////////////////	/// @deprecated The <seealso cref="IndexManager"/> based indexes will be removed in next major release. Please consider using schema indexes instead. 
////////////////////	[Obsolete("The <seealso cref=\"IndexManager\"/> based indexes will be removed in next major release. Please consider using schema indexes instead.")]
////////////////////	public interface IndexManager
////////////////////	{
////////////////////		 /// <summary>
////////////////////		 /// The configuration key to use for specifying which provider an index
////////////////////		 /// will have, i.e. which implementation will be used to back that index.
////////////////////		 /// </summary>

////////////////////		 /// <summary>
////////////////////		 /// Returns whether or not there exists a node index with the name
////////////////////		 /// {@code indexName}. Indexes are created when needed in calls to
////////////////////		 /// <seealso cref="forNodes(string)"/> and <seealso cref="forNodes(string, System.Collections.IDictionary)"/>. </summary>
////////////////////		 /// <param name="indexName"> the name of the index to check. </param>
////////////////////		 /// <returns> whether or not there exists a node index with the name
////////////////////		 /// {@code indexName}. </returns>
////////////////////		 [Obsolete]
////////////////////		 bool ExistsForNodes( string indexName );

////////////////////		 /// <summary>
////////////////////		 /// Returns an <seealso cref="Index"/> for <seealso cref="INode"/>s with the name {@code indexName}.
////////////////////		 /// If such an index doesn't exist it will be created with default configuration.
////////////////////		 /// Indexes created with <seealso cref="forNodes(string, System.Collections.IDictionary)"/> can be returned by this
////////////////////		 /// method also, so that you don't have to supply and match its configuration
////////////////////		 /// for consecutive accesses.
////////////////////		 /// 
////////////////////		 /// This is the prefered way of accessing indexes, whether they were created with
////////////////////		 /// {#forNodes(String)} or <seealso cref="forNodes(string, System.Collections.IDictionary)"/>.
////////////////////		 /// </summary>
////////////////////		 /// <param name="indexName"> the name of the node index. </param>
////////////////////		 /// <returns> the <seealso cref="Index"/> corresponding to the {@code indexName}. </returns>
////////////////////		 [Obsolete]
////////////////////		 Index<INode> ForNodes( string indexName );

////////////////////		 /// <summary>
////////////////////		 /// Returns an <seealso cref="Index"/> for <seealso cref="INode"/>s with the name {@code indexName}.
////////////////////		 /// If the index exists it will be returned if the provider and customConfiguration
////////////////////		 /// matches, else an <seealso cref="System.ArgumentException"/> will be thrown.
////////////////////		 /// If the index doesn't exist it will be created with the given
////////////////////		 /// provider (given in the configuration map).
////////////////////		 /// </summary>
////////////////////		 /// <param name="indexName"> the name of the index to create. </param>
////////////////////		 /// <param name="customConfiguration"> configuration for the index being created.
////////////////////		 /// Use the <b>provider</b> key to control which index implementation to use for this index if it's created.
////////////////////		 /// The value represents the service name corresponding to the index implementation.
////////////////////		 /// Other options can f.ex. say that the index will be a fulltext index and that it
////////////////////		 /// should be case insensitive. The parameters given here (except "provider") are
////////////////////		 /// only interpreted by the implementation represented by the provider. </param>
////////////////////		 /// <returns> a named <seealso cref="Index"/> for <seealso cref="INode"/>s </returns>
////////////////////		 [Obsolete]
////////////////////		 Index<INode> ForNodes( string indexName, IDictionary<string, string> customConfiguration );

////////////////////		 /// <summary>
////////////////////		 /// Returns the names of all existing <seealso cref="INode"/> indexes.
////////////////////		 /// Those names can then be used to get to the actual <seealso cref="Index"/>
////////////////////		 /// instances.
////////////////////		 /// </summary>
////////////////////		 /// <returns> the names of all existing <seealso cref="INode"/> indexes. </returns>
////////////////////		 [Obsolete]
////////////////////		 string[] NodeIndexNames();

////////////////////		 /// <summary>
////////////////////		 /// Returns whether or not there exists a relationship index with the name
////////////////////		 /// {@code indexName}. Indexes are created when needed in calls to
////////////////////		 /// <seealso cref="forRelationships(string)"/> and <seealso cref="forRelationships(string, System.Collections.IDictionary)"/>. </summary>
////////////////////		 /// <param name="indexName"> the name of the index to check. </param>
////////////////////		 /// <returns> whether or not there exists a relationship index with the name
////////////////////		 /// {@code indexName}. </returns>
////////////////////		 [Obsolete]
////////////////////		 bool ExistsForRelationships( string indexName );

////////////////////		 /// <summary>
////////////////////		 /// Returns an <seealso cref="Index"/> for <seealso cref="IRelationship"/>s with the name {@code indexName}.
////////////////////		 /// If such an index doesn't exist it will be created with default configuration.
////////////////////		 /// Indexes created with <seealso cref="forRelationships(string, System.Collections.IDictionary)"/> can be returned by this
////////////////////		 /// method also, so that you don't have to supply and match its configuration
////////////////////		 /// for consecutive accesses.
////////////////////		 /// 
////////////////////		 /// This is the prefered way of accessing indexes, whether they were created with
////////////////////		 /// <seealso cref="forRelationships(string)"/> or <seealso cref="forRelationships(string, System.Collections.IDictionary)"/>.
////////////////////		 /// </summary>
////////////////////		 /// <param name="indexName"> the name of the node index. </param>
////////////////////		 /// <returns> the <seealso cref="Index"/> corresponding to the {@code indexName}. </returns>
////////////////////		 [Obsolete]
////////////////////		 RelationshipIndex ForRelationships( string indexName );

////////////////////		 /// <summary>
////////////////////		 /// Returns an <seealso cref="Index"/> for <seealso cref="IRelationship"/>s with the name {@code indexName}.
////////////////////		 /// If the index exists it will be returned if the provider and customConfiguration
////////////////////		 /// matches, else an <seealso cref="System.ArgumentException"/> will be thrown.
////////////////////		 /// If the index doesn't exist it will be created with the given
////////////////////		 /// provider (given in the configuration map).
////////////////////		 /// </summary>
////////////////////		 /// <param name="indexName"> the name of the index to create. </param>
////////////////////		 /// <param name="customConfiguration"> configuration for the index being created.
////////////////////		 /// Use the <b>provider</b> key to control which index implementation. The
////////////////////		 /// value represents the service name corresponding to the index provider implementation.
////////////////////		 /// Other options can f.ex. say that the index will be a fulltext index and that it
////////////////////		 /// should be case insensitive. The parameters given here (except "provider") are
////////////////////		 /// only interpreted by the implementation represented by the provider. </param>
////////////////////		 /// <returns> a named <seealso cref="Index"/> for <seealso cref="IRelationship"/>s </returns>
////////////////////		 [Obsolete]
////////////////////		 RelationshipIndex ForRelationships( string indexName, IDictionary<string, string> customConfiguration );

////////////////////		 /// <summary>
////////////////////		 /// Returns the names of all existing <seealso cref="IRelationship"/> indexes.
////////////////////		 /// Those names can then be used to get to the actual <seealso cref="Index"/>
////////////////////		 /// instances.
////////////////////		 /// </summary>
////////////////////		 /// <returns> the names of all existing <seealso cref="IRelationship"/> indexes. </returns>
////////////////////		 [Obsolete]
////////////////////		 string[] RelationshipIndexNames();

////////////////////		 /// <summary>
////////////////////		 /// Returns the configuration for {@code index}. Configuration can be
////////////////////		 /// set when creating an index, with f.ex <seealso cref="forNodes(string, System.Collections.IDictionary)"/>
////////////////////		 /// or with <seealso cref="setConfiguration(Index, string, string)"/> or
////////////////////		 /// <seealso cref="removeConfiguration(Index, string)"/>.
////////////////////		 /// </summary>
////////////////////		 /// <param name="index"> the index to get the configuration for </param>
////////////////////		 /// <returns> configuration for the {@code index}. </returns>
//////////////////////JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//////////////////////ORIGINAL LINE: Map<String, String> getConfiguration(Index<? extends org.Neo4Net.graphdb.PropertyContainer> index);
////////////////////		 [Obsolete]
////////////////////		 IDictionary<string, string> getConfiguration<T1>( Index<T1> index );

////////////////////		 /// <summary>
////////////////////		 /// EXPERT: Sets a configuration parameter for an index. If a configuration
////////////////////		 /// parameter with the given {@code key} it will be overwritten.
////////////////////		 /// 
////////////////////		 /// WARNING: Overwriting parameters which controls the storage format of index
////////////////////		 /// data may lead to existing index data being unusable.
////////////////////		 /// 
////////////////////		 /// The key "provider" is a reserved parameter and cannot be overwritten,
////////////////////		 /// if key is "provider" then an <seealso cref="System.ArgumentException"/> will be thrown.
////////////////////		 /// </summary>
////////////////////		 /// <param name="index"> the index to set a configuration parameter for. </param>
////////////////////		 /// <param name="key"> the configuration parameter key. </param>
////////////////////		 /// <param name="value"> the new value of the configuration parameter. </param>
////////////////////		 /// <returns> the overwritten value if any. </returns>
//////////////////////JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//////////////////////ORIGINAL LINE: String setConfiguration(Index<? extends org.Neo4Net.graphdb.PropertyContainer> index, String key, String value);
////////////////////		 [Obsolete]
////////////////////		 string setConfiguration<T1>( Index<T1> index, string key, string value );

////////////////////		 /// <summary>
////////////////////		 /// EXPERT: Removes a configuration parameter from an index. If there's no
////////////////////		 /// value for the given {@code key} nothing will happen and {@code null}
////////////////////		 /// will be returned.
////////////////////		 /// 
////////////////////		 /// WARNING: Removing parameters which controls the storage format of index
////////////////////		 /// data may lead to existing index data being unusable.
////////////////////		 /// 
////////////////////		 /// The key "provider" is a reserved parameter and cannot be removed,
////////////////////		 /// if key is "provider" then an <seealso cref="System.ArgumentException"/> will be thrown.
////////////////////		 /// </summary>
////////////////////		 /// <param name="index"> the index to remove a configuration parameter from. </param>
////////////////////		 /// <param name="key"> the configuration parameter key. </param>
////////////////////		 /// <returns> the removed value if any. </returns>
//////////////////////JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//////////////////////ORIGINAL LINE: String removeConfiguration(Index<? extends org.Neo4Net.graphdb.PropertyContainer> index, String key);
////////////////////		 [Obsolete]
////////////////////		 string removeConfiguration<T1>( Index<T1> index, string key );

////////////////////		 /// @deprecated this feature will be removed in a future release, please consider using schema indexes instead 
////////////////////		 /// <returns> the auto indexing manager for nodes </returns>
////////////////////		 [Obsolete("this feature will be removed in a future release, please consider using schema indexes instead")]
////////////////////		 AutoIndexer<INode> NodeAutoIndexer { get; }

////////////////////		 /// @deprecated this feature will be removed in a future release, please consider using schema indexes instead 
////////////////////		 /// <returns> the auto indexing manager for relationships </returns>
////////////////////		 [Obsolete("this feature will be removed in a future release, please consider using schema indexes instead")]
////////////////////		 RelationshipAutoIndexer RelationshipAutoIndexer { get; }
////////////////////	}

////////////////////	public static class IndexManager_Fields
////////////////////	{
////////////////////		 public const string PROVIDER = "provider";
////////////////////	}

////////////////////}