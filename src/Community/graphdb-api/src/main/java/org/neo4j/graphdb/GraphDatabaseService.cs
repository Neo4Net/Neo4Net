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
namespace Neo4Net.Graphdb
{

	using KernelEventHandler = Neo4Net.Graphdb.@event.KernelEventHandler;
	using Neo4Net.Graphdb.@event;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using Schema = Neo4Net.Graphdb.schema.Schema;
	using BidirectionalTraversalDescription = Neo4Net.Graphdb.traversal.BidirectionalTraversalDescription;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;

	/// <summary>
	/// The main access point to a running Neo4j instance. The most common way to instantiate a <seealso cref="GraphDatabaseService"/>
	/// is as follows:
	/// <pre>
	/// <code>GraphDatabaseService graphDb = new GraphDatabaseFactory().newEmbeddedDatabase( new File("var/graphDb") );
	/// // ... use Neo4j
	/// graphDb.<seealso cref="shutdown() shutdown()"/>;</code>
	/// </pre>
	/// <para>
	/// GraphDatabaseService provides operations to {@link #createNode() create
	/// nodes}, <seealso cref="getNodeById(long) get nodes given an id"/> and ultimately {@link #shutdown()
	/// shutdown Neo4j}.
	/// </para>
	/// <para>
	/// Please note that all operations on the graph must be invoked in a
	/// <seealso cref="Transaction transactional context"/>. Failure to do so will result in a
	/// <seealso cref="NotInTransactionException"/> being thrown.
	/// </para>
	/// </summary>
	public interface GraphDatabaseService
	{
		 /// <summary>
		 /// Creates a new node.
		 /// </summary>
		 /// <returns> the created node. </returns>
		 Node CreateNode();

		 /// <summary>
		 /// Creates a new node and returns it id.
		 /// Please note: Neo4j reuses its internal ids when
		 /// nodes and relationships are deleted, which means it's bad practice to
		 /// refer to them this way. Instead, use application generated ids.
		 /// </summary>
		 /// <returns> the created nodes id. </returns>
		 /// @deprecated This method will be removed in a future major release. 
		 [Obsolete("This method will be removed in a future major release.")]
		 long? CreateNodeId();

		 /// <summary>
		 /// Creates a new node and adds the provided labels to it.
		 /// </summary>
		 /// <param name="labels"> <seealso cref="Label labels"/> to add to the created node. </param>
		 /// <returns> the created node. </returns>
		 Node CreateNode( params Label[] labels );

		 /// <summary>
		 /// Looks up a node by id. Please note: Neo4j reuses its internal ids when
		 /// nodes and relationships are deleted, which means it's bad practice to
		 /// refer to them this way. Instead, use application generated ids.
		 /// </summary>
		 /// <param name="id"> the id of the node </param>
		 /// <returns> the node with id <code>id</code> if found </returns>
		 /// <exception cref="NotFoundException"> if not found </exception>
		 Node GetNodeById( long id );

		 /// <summary>
		 /// Looks up a relationship by id. Please note: Neo4j reuses its internal ids
		 /// when nodes and relationships are deleted, which means it's bad practice
		 /// to refer to them this way. Instead, use application generated ids.
		 /// </summary>
		 /// <param name="id"> the id of the relationship </param>
		 /// <returns> the relationship with id <code>id</code> if found </returns>
		 /// <exception cref="NotFoundException"> if not found </exception>
		 Relationship GetRelationshipById( long id );

		 /// <summary>
		 /// Returns all nodes in the graph.
		 /// </summary>
		 /// <returns> all nodes in the graph. </returns>
		 ResourceIterable<Node> AllNodes { get; }

		 /// <summary>
		 /// Returns all relationships in the graph.
		 /// </summary>
		 /// <returns> all relationships in the graph. </returns>
		 ResourceIterable<Relationship> AllRelationships { get; }

		 /// <summary>
		 /// Returns all nodes having the label, and the wanted property value.
		 /// If an online index is found, it will be used to look up the requested
		 /// nodes.
		 /// <para>
		 /// If no indexes exist for the label/property combination, the database will
		 /// scan all labeled nodes looking for the property value.
		 /// </para>
		 /// <para>
		 /// Note that equality for values do not follow the rules of Java. This means that the number 42 is equals to all
		 /// other 42 numbers, regardless of whether they are encoded as Integer, Long, Float, Short, Byte or Double.
		 /// </para>
		 /// <para>
		 /// Same rules follow Character and String - the Character 'A' is equal to the String 'A'.
		 /// </para>
		 /// <para>
		 /// Finally - arrays also follow these rules. An int[] {1,2,3} is equal to a double[] {1.0, 2.0, 3.0}
		 /// </para>
		 /// <para>
		 /// Please ensure that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="label"> consider nodes with this label </param>
		 /// <param name="key">   required property key </param>
		 /// <param name="value"> required property value </param>
		 /// <returns> an iterator containing all matching nodes. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
		 ResourceIterator<Node> FindNodes( Label label, string key, object value );

		 /// <summary>
		 /// Returns all nodes having the label, and the wanted property values.
		 /// If an online index is found, it will be used to look up the requested
		 /// nodes.
		 /// <para>
		 /// If no indexes exist for the label with all provided properties, the database will
		 /// scan all labeled nodes looking for matching nodes.
		 /// </para>
		 /// <para>
		 /// Note that equality for values do not follow the rules of Java. This means that the number 42 is equals to all
		 /// other 42 numbers, regardless of whether they are encoded as Integer, Long, Float, Short, Byte or Double.
		 /// </para>
		 /// <para>
		 /// Same rules follow Character and String - the Character 'A' is equal to the String 'A'.
		 /// </para>
		 /// <para>
		 /// Finally - arrays also follow these rules. An int[] {1,2,3} is equal to a double[] {1.0, 2.0, 3.0}
		 /// </para>
		 /// <para>
		 /// Please ensure that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="label">  consider nodes with this label </param>
		 /// <param name="key1">   required property key1 </param>
		 /// <param name="value1"> required property value of key1 </param>
		 /// <param name="key2">   required property key2 </param>
		 /// <param name="value2"> required property value of key2 </param>
		 /// <returns> an iterator containing all matching nodes. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default ResourceIterator<Node> findNodes(Label label, String key1, Object value1, String key2, Object value2)
	//	 {
	//		  throw new UnsupportedOperationException("findNodes by multiple property names and values is not supported.");
	//	 }

		 /// <summary>
		 /// Returns all nodes having the label, and the wanted property values.
		 /// If an online index is found, it will be used to look up the requested
		 /// nodes.
		 /// <para>
		 /// If no indexes exist for the label with all provided properties, the database will
		 /// scan all labeled nodes looking for matching nodes.
		 /// </para>
		 /// <para>
		 /// Note that equality for values do not follow the rules of Java. This means that the number 42 is equals to all
		 /// other 42 numbers, regardless of whether they are encoded as Integer, Long, Float, Short, Byte or Double.
		 /// </para>
		 /// <para>
		 /// Same rules follow Character and String - the Character 'A' is equal to the String 'A'.
		 /// </para>
		 /// <para>
		 /// Finally - arrays also follow these rules. An int[] {1,2,3} is equal to a double[] {1.0, 2.0, 3.0}
		 /// </para>
		 /// <para>
		 /// Please ensure that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="label">  consider nodes with this label </param>
		 /// <param name="key1">   required property key1 </param>
		 /// <param name="value1"> required property value of key1 </param>
		 /// <param name="key2">   required property key2 </param>
		 /// <param name="value2"> required property value of key2 </param>
		 /// <param name="key3">   required property key3 </param>
		 /// <param name="value3"> required property value of key3 </param>
		 /// <returns> an iterator containing all matching nodes. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default ResourceIterator<Node> findNodes(Label label, String key1, Object value1, String key2, Object value2, String key3, Object value3)
	//	 {
	//		  throw new UnsupportedOperationException("findNodes by multiple property names and values is not supported.");
	//	 }

		 /// <summary>
		 /// Returns all nodes having the label, and the wanted property values.
		 /// If an online index is found, it will be used to look up the requested
		 /// nodes.
		 /// <para>
		 /// If no indexes exist for the label with all provided properties, the database will
		 /// scan all labeled nodes looking for matching nodes.
		 /// </para>
		 /// <para>
		 /// Note that equality for values do not follow the rules of Java. This means that the number 42 is equals to all
		 /// other 42 numbers, regardless of whether they are encoded as Integer, Long, Float, Short, Byte or Double.
		 /// </para>
		 /// <para>
		 /// Same rules follow Character and String - the Character 'A' is equal to the String 'A'.
		 /// </para>
		 /// <para>
		 /// Finally - arrays also follow these rules. An int[] {1,2,3} is equal to a double[] {1.0, 2.0, 3.0}
		 /// </para>
		 /// <para>
		 /// Please ensure that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="label">          consider nodes with this label </param>
		 /// <param name="propertyValues"> required property key-value combinations </param>
		 /// <returns> an iterator containing all matching nodes. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default ResourceIterator<Node> findNodes(Label label, java.util.Map<String, Object> propertyValues)
	//	 {
	//		  throw new UnsupportedOperationException("findNodes by multiple property names and values is not supported.");
	//	 }

		 /// <summary>
		 /// Returns all nodes having a given label, and a property value of type String or Character matching the
		 /// given value template and search mode.
		 /// <para>
		 /// If an online index is found, it will be used to look up the requested nodes.
		 /// If no indexes exist for the label/property combination, the database will
		 /// scan all labeled nodes looking for matching property values.
		 /// </para>
		 /// <para>
		 /// The search mode and value template are used to select nodes of interest. The search mode can
		 /// be one of
		 /// <ul>
		 ///   <li>EXACT: The value has to match the template exactly. This is the same behavior as <seealso cref="GraphDatabaseService.findNode(Label, string, object)"/>.</li>
		 ///   <li>PREFIX: The value must have a prefix matching the template.</li>
		 ///   <li>SUFFIX: The value must have a suffix matching the template.</li>
		 ///   <li>CONTAINS: The value must contain the template. Only exact matches are supported.</li>
		 /// </ul>
		 /// Note that in Neo4j the Character 'A' will be treated the same way as the String 'A'.
		 /// </para>
		 /// <para>
		 /// Please ensure that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="label">      consider nodes with this label </param>
		 /// <param name="key">        required property key </param>
		 /// <param name="template">   required property value template </param>
		 /// <param name="searchMode"> required property value template </param>
		 /// <returns> an iterator containing all matching nodes. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default ResourceIterator<Node> findNodes(Label label, String key, String template, StringSearchMode searchMode)
	//	 {
	//		  throw new UnsupportedOperationException("Specialized string queries are not supported");
	//	 }

		 /// <summary>
		 /// Equivalent to <seealso cref="findNodes(Label, string, object)"/>, however it must find no more than one
		 /// <seealso cref="Node node"/> or it will throw an exception.
		 /// </summary>
		 /// <param name="label"> consider nodes with this label </param>
		 /// <param name="key">   required property key </param>
		 /// <param name="value"> required property value </param>
		 /// <returns> the matching node or <code>null</code> if none could be found </returns>
		 /// <exception cref="MultipleFoundException"> if more than one matching <seealso cref="Node node"/> is found </exception>
		 Node FindNode( Label label, string key, object value );

		 /// <summary>
		 /// Returns all <seealso cref="Node nodes"/> with a specific <seealso cref="Label label"/>.
		 /// 
		 /// Please take care that the returned <seealso cref="ResourceIterator"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// </summary>
		 /// <param name="label"> the <seealso cref="Label"/> to return nodes for. </param>
		 /// <returns> an iterator containing all nodes matching the label. See <seealso cref="ResourceIterator"/> for responsibilities. </returns>
		 ResourceIterator<Node> FindNodes( Label label );

		 /// <summary>
		 /// Returns all labels currently in the underlying store. Labels are added to the store the first time
		 /// they are used. This method guarantees that it will return all labels currently in use.
		 /// 
		 /// Please take care that the returned <seealso cref="ResourceIterable"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// </summary>
		 /// <returns> all labels in the underlying store. </returns>
		 ResourceIterable<Label> AllLabelsInUse { get; }

		 /// <summary>
		 /// Returns all relationship types currently in the underlying store.
		 /// Relationship types are added to the underlying store the first time they
		 /// are used in a successfully committed {@link Node#createRelationshipTo
		 /// node.createRelationshipTo(...)}. This method guarantees that it will
		 /// return all relationship types currently in use.
		 /// </summary>
		 /// <returns> all relationship types in the underlying store </returns>
		 ResourceIterable<RelationshipType> AllRelationshipTypesInUse { get; }

		 /// <summary>
		 /// Returns all labels currently in the underlying store. Labels are added to the store the first time
		 /// they are used. This method guarantees that it will return all labels currently in use. However,
		 /// it may also return <i>more</i> than that (e.g. it can return "historic" labels that are no longer used).
		 /// 
		 /// Please take care that the returned <seealso cref="ResourceIterable"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// </summary>
		 /// <returns> all labels in the underlying store. </returns>
		 ResourceIterable<Label> AllLabels { get; }

		 /// <summary>
		 /// Returns all relationship types currently in the underlying store.
		 /// Relationship types are added to the underlying store the first time they
		 /// are used in a successfully committed {@link Node#createRelationshipTo
		 /// node.createRelationshipTo(...)}. Note that this method is guaranteed to
		 /// return all known relationship types, but it does not guarantee that it
		 /// won't return <i>more</i> than that (e.g. it can return "historic"
		 /// relationship types that no longer have any relationships in the node
		 /// space).
		 /// </summary>
		 /// <returns> all relationship types in the underlying store </returns>
		 ResourceIterable<RelationshipType> AllRelationshipTypes { get; }

		 /// <summary>
		 /// Returns all property keys currently in the underlying store. This method guarantees that it will return all
		 /// property keys currently in use. However, it may also return <i>more</i> than that (e.g. it can return "historic"
		 /// labels that are no longer used).
		 /// 
		 /// Please take care that the returned <seealso cref="ResourceIterable"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// </summary>
		 /// <returns> all property keys in the underlying store. </returns>
		 ResourceIterable<string> AllPropertyKeys { get; }

		 /// <summary>
		 /// Use this method to check if the database is currently in a usable state.
		 /// </summary>
		 /// <param name="timeout"> timeout (in milliseconds) to wait for the database to become available.
		 ///   If the database has been shut down {@code false} is returned immediately. </param>
		 /// <returns> the state of the database: {@code true} if it is available, otherwise {@code false} </returns>
		 bool IsAvailable( long timeout );

		 /// <summary>
		 /// Shuts down Neo4j. After this method has been invoked, it's invalid to
		 /// invoke any methods in the Neo4j API and all references to this instance
		 /// of GraphDatabaseService should be discarded.
		 /// </summary>
		 void Shutdown();

		 /// <summary>
		 /// Starts a new <seealso cref="Transaction transaction"/> and associates it with the current thread.
		 /// <para>
		 /// <em>All database operations must be wrapped in a transaction.</em>
		 /// </para>
		 /// <para>
		 /// If you attempt to access the graph outside of a transaction, those operations will throw
		 /// <seealso cref="NotInTransactionException"/>.
		 /// </para>
		 /// <para>
		 /// Please ensure that any returned <seealso cref="ResourceIterable"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a new transaction instance </returns>
		 Transaction BeginTx();

		 /// <summary>
		 /// Starts a new <seealso cref="Transaction transaction"/> with custom timeout and associates it with the current thread.
		 /// Timeout will be taken into account <b>only</b> when execution guard is enabled.
		 /// <para>
		 /// <em>All database operations must be wrapped in a transaction.</em>
		 /// </para>
		 /// <para>
		 /// If you attempt to access the graph outside of a transaction, those operations will throw
		 /// <seealso cref="NotInTransactionException"/>.
		 /// </para>
		 /// <para>
		 /// Please ensure that any returned <seealso cref="ResourceIterable"/> is closed correctly and as soon as possible
		 /// inside your transaction to avoid potential blocking of write operations.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="timeout"> transaction timeout </param>
		 /// <param name="unit"> time unit of timeout argument </param>
		 /// <returns> a new transaction instance </returns>
		 Transaction BeginTx( long timeout, TimeUnit unit );

		 /// <summary>
		 /// Executes a query and returns an iterable that contains the result set.
		 /// 
		 /// This method is the same as <seealso cref="execute(string, System.Collections.IDictionary)"/> with an empty parameters-map.
		 /// </summary>
		 /// <param name="query"> The query to execute </param>
		 /// <returns> A <seealso cref="org.neo4j.graphdb.Result"/> that contains the result set. </returns>
		 /// <exception cref="QueryExecutionException"> If the Query contains errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Result execute(String query) throws QueryExecutionException;
		 Result Execute( string query );

		 /// <summary>
		 /// Executes a query and returns an iterable that contains the result set.
		 /// If query will not gonna be able to complete within specified timeout time interval it will be terminated.
		 /// 
		 /// This method is the same as <seealso cref="execute(string, System.Collections.IDictionary)"/> with an empty parameters-map.
		 /// </summary>
		 /// <param name="query"> The query to execute </param>
		 /// <param name="timeout"> The maximum time interval within which query should be completed. </param>
		 /// <param name="unit"> time unit of timeout argument </param>
		 /// <returns> A <seealso cref="org.neo4j.graphdb.Result"/> that contains the result set. </returns>
		 /// <exception cref="QueryExecutionException"> If the Query contains errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Result execute(String query, long timeout, java.util.concurrent.TimeUnit unit) throws QueryExecutionException;
		 Result Execute( string query, long timeout, TimeUnit unit );

		 /// <summary>
		 /// Executes a query and returns an iterable that contains the result set.
		 /// </summary>
		 /// <param name="query">      The query to execute </param>
		 /// <param name="parameters"> Parameters for the query </param>
		 /// <returns> A <seealso cref="org.neo4j.graphdb.Result"/> that contains the result set </returns>
		 /// <exception cref="QueryExecutionException"> If the Query contains errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Result execute(String query, java.util.Map<String,Object> parameters) throws QueryExecutionException;
		 Result Execute( string query, IDictionary<string, object> parameters );

		 /// <summary>
		 /// Executes a query and returns an iterable that contains the result set.
		 /// If query will not gonna be able to complete within specified timeout time interval it will be terminated.
		 /// </summary>
		 /// <param name="query">      The query to execute </param>
		 /// <param name="parameters"> Parameters for the query </param>
		 /// <param name="timeout"> The maximum time interval within which query should be completed. </param>
		 /// <param name="unit"> time unit of timeout argument </param>
		 /// <returns> A <seealso cref="org.neo4j.graphdb.Result"/> that contains the result set </returns>
		 /// <exception cref="QueryExecutionException"> If the Query contains errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Result execute(String query, java.util.Map<String,Object> parameters, long timeout, java.util.concurrent.TimeUnit unit) throws QueryExecutionException;
		 Result Execute( string query, IDictionary<string, object> parameters, long timeout, TimeUnit unit );

		 /// <summary>
		 /// Registers {@code handler} as a handler for transaction events which
		 /// are generated from different places in the lifecycle of each
		 /// transaction. To guarantee that the handler gets all events properly
		 /// it shouldn't be registered when the application is running (i.e. in the
		 /// middle of one or more transactions). If the specified handler instance
		 /// has already been registered this method will do nothing.
		 /// </summary>
		 /// @param <T>     the type of state object used in the handler, see more
		 ///                documentation about it at <seealso cref="TransactionEventHandler"/>. </param>
		 /// <param name="handler"> the handler to receive events about different states
		 ///                in transaction lifecycles. </param>
		 /// <returns> the handler passed in as the argument. </returns>
		 TransactionEventHandler<T> registerTransactionEventHandler<T>( TransactionEventHandler<T> handler );

		 /// <summary>
		 /// Unregisters {@code handler} from the list of transaction event handlers.
		 /// If {@code handler} hasn't been registered with
		 /// <seealso cref="registerTransactionEventHandler(TransactionEventHandler)"/> prior
		 /// to calling this method an <seealso cref="System.InvalidOperationException"/> will be thrown.
		 /// After a successful call to this method the {@code handler} will no
		 /// longer receive any transaction events.
		 /// </summary>
		 /// @param <T>     the type of state object used in the handler, see more
		 ///                documentation about it at <seealso cref="TransactionEventHandler"/>. </param>
		 /// <param name="handler"> the handler to receive events about different states
		 ///                in transaction lifecycles. </param>
		 /// <returns> the handler passed in as the argument. </returns>
		 /// <exception cref="IllegalStateException"> if {@code handler} wasn't registered prior
		 ///                               to calling this method. </exception>
		 TransactionEventHandler<T> unregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler );

		 /// <summary>
		 /// Registers {@code handler} as a handler for kernel events which
		 /// are generated from different places in the lifecycle of the kernel.
		 /// To guarantee proper behavior the handler should be registered right
		 /// after the graph database has been started. If the specified handler
		 /// instance has already been registered this method will do nothing.
		 /// </summary>
		 /// <param name="handler"> the handler to receive events about different states
		 ///                in the kernel lifecycle. </param>
		 /// <returns> the handler passed in as the argument. </returns>
		 KernelEventHandler RegisterKernelEventHandler( KernelEventHandler handler );

		 /// <summary>
		 /// Unregisters {@code handler} from the list of kernel event handlers.
		 /// If {@code handler} hasn't been registered with
		 /// <seealso cref="registerKernelEventHandler(KernelEventHandler)"/> prior to calling
		 /// this method an <seealso cref="System.InvalidOperationException"/> will be thrown.
		 /// After a successful call to this method the {@code handler} will no
		 /// longer receive any kernel events.
		 /// </summary>
		 /// <param name="handler"> the handler to receive events about different states
		 ///                in the kernel lifecycle. </param>
		 /// <returns> the handler passed in as the argument. </returns>
		 /// <exception cref="IllegalStateException"> if {@code handler} wasn't registered prior
		 ///                               to calling this method. </exception>
		 KernelEventHandler UnregisterKernelEventHandler( KernelEventHandler handler );

		 /// <summary>
		 /// Returns the <seealso cref="Schema schema manager"/> where all things related to schema,
		 /// for example constraints and indexing on <seealso cref="Label labels"/>.
		 /// </summary>
		 /// <returns> the <seealso cref="Schema schema manager"/> for this database. </returns>
		 Schema Schema();

		 /// <summary>
		 /// Returns the <seealso cref="IndexManager"/> paired with this graph database service
		 /// and is the entry point for managing indexes coupled with this database.
		 /// </summary>
		 /// <returns> the <seealso cref="IndexManager"/> for this database. </returns>
		 /// @deprecated The <seealso cref="IndexManager"/> based indexes will be removed in the next major release. Please consider using schema indexes instead. 
		 [Obsolete("The <seealso cref=\"IndexManager\"/> based indexes will be removed in the next major release. Please consider using schema indexes instead.")]
		 IndexManager Index();

		 /// <summary>
		 /// Factory method for unidirectional traversal descriptions.
		 /// </summary>
		 /// <returns> a new <seealso cref="TraversalDescription"/> </returns>
		 TraversalDescription TraversalDescription();

		 /// <summary>
		 /// Factory method for bidirectional traversal descriptions.
		 /// </summary>
		 /// <returns> a new <seealso cref="BidirectionalTraversalDescription"/> </returns>
		 BidirectionalTraversalDescription BidirectionalTraversalDescription();
	}

}