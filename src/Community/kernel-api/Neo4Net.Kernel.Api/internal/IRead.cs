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

namespace Neo4Net.Kernel.Api.Internal
{
   using Value = Neo4Net.Values.Storable.Value;
   using Values = Neo4Net.Values.Storable.Values;

   /// <summary>
   /// Defines the graph read operations of the Kernel.
   /// </summary>
   public interface IRead
   {
      /// <summary>
      /// Seek all nodes matching the provided index query in an index. </summary>
      ///  <param name="index"> <seealso cref="IIndexReference"/> referencing index to query. </param>
      /// <param name="cursor"> the ICursor to use for consuming the results. </param>
      /// <param name="indexOrder"> requested <seealso cref="IndexOrder"/> of result. Must be among the capabilities of
      /// <seealso cref="IIndexReference referenced index"/>, or <seealso cref="IndexOrder.NONE"/>. </param>
      /// <param name="needsValues"> if the index should fetch property values together with node ids for index queries </param>
      /// <param name="query"> Combination of <seealso cref="IndexQuery index queries"/> to run against referenced index. </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void nodeIndexSeek(IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, boolean needsValues, IndexQuery... query) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      void NodeIndexSeek(IIndexReference index, INodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query);

      /// <summary>
      /// Access all distinct counts in an index. Entries fed to the {@code cursor} will be (count,Value[]),
      /// where the count (number of nodes having the particular value) will be accessed using <seealso cref="INodeValueIndexCursor.nodeReference()"/>
      /// and the value (if the index can provide it) using <seealso cref="INodeValueIndexCursor.propertyValue(int)"/>.
      /// Before accessing a property value the caller should check <seealso cref="INodeValueIndexCursor.hasValue()"/> to see
      /// whether or not the index could yield values.
      ///
      /// For merely counting distinct values in an index, loop over and sum iterations.
      /// For counting number of indexed nodes in an index, loop over and sum all counts.
      ///
      /// NOTE distinct values may not be 100% accurate for point values that are very close to each other. In those cases they can be
      /// reported as a single distinct values with a higher count instead of several separate values. </summary>
      /// <param name="index"> <seealso cref="IIndexReference"/> referencing index. </param>
      /// <param name="cursor"> <seealso cref="INodeValueIndexCursor"/> receiving distinct count data. </param>
      /// <param name="needsValues"> whether or not values should be loaded and given to the cursor. </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void nodeIndexDistinctValues(IndexReference index, NodeValueIndexCursor cursor, boolean needsValues) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
      void NodeIndexDistinctValues(IIndexReference index, INodeValueIndexCursor cursor, bool needsValues);

      /// <summary>
      /// Returns node id of node found in unique index or -1 if no node was found.
      ///
      /// Note that this is a very special method and should be use with caution. It has special locking semantics in
      /// order to facilitate unique creation of nodes. If a node is found; a shared lock for the index entry will be
      /// held whereas if no node is found we will hold onto an exclusive lock until the close of the transaction.
      /// </summary>
      /// <param name="index"> <seealso cref="IIndexReference"/> referencing index to query.
      ///              <seealso cref="IIndexReference referenced index"/>, or <seealso cref="IndexOrder.NONE"/>. </param>
      /// <param name="predicates"> Combination of <seealso cref="IndexQuery.ExactPredicate index queries"/> to run against referenced index. </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: long lockingNodeUniqueIndexSeek(IndexReference index, IndexQuery.ExactPredicate... predicates) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      long LockingNodeUniqueIndexSeek(IIndexReference index, params IndexQuery.ExactPredicate[] predicates);

      /// <summary>
      /// Scan all values in an index.
      /// </summary>
      /// <param name="index"> <seealso cref="IIndexReference"/> referencing index to query. </param>
      /// <param name="cursor"> the ICursor to use for consuming the results. </param>
      /// <param name="indexOrder"> requested <seealso cref="IndexOrder"/> of result. Must be among the capabilities of
      /// <seealso cref="IIndexReference referenced index"/>, or <seealso cref="IndexOrder.NONE"/>. </param>
      /// <param name="needsValues"> if the index should fetch property values together with node ids for index queries </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void nodeIndexScan(IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, boolean needsValues) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      void NodeIndexScan(IIndexReference index, INodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues);

      void NodeLabelScan(int label, INodeLabelIndexCursor ICursor);

      /// <summary>
      /// Scan for nodes that have a <i>disjunction</i> of the specified labels.
      /// i.e. MATCH (n) WHERE n:Label1 OR n:Label2 OR ...
      /// </summary>
      void NodeLabelUnionScan(INodeLabelIndexCursor cursor, params int[] labels);

      /// <summary>
      /// Scan for nodes that have a <i>conjunction</i> of the specified labels.
      /// i.e. MATCH (n) WHERE n:Label1 AND n:Label2 AND ...
      /// </summary>
      void NodeLabelIntersectionScan(INodeLabelIndexCursor cursor, params int[] labels);

      Scan<INodeLabelIndexCursor> NodeLabelScan(int label);

      /// <summary>
      /// Return all nodes in the graph.
      /// </summary>
      /// <param name="cursor"> ICursor to initialize for scanning. </param>
      void AllNodesScan(INodeCursor ICursor);

      Scan<INodeCursor> AllNodesScan();

      /// <param name="reference"> a reference from <seealso cref="INodeCursor.nodeReference()"/>, {@link
      /// RelationshipDataAccessor#sourceNodeReference()},
      /// <seealso cref="IRelationshipDataAccessor.targetNodeReference()"/>, <seealso cref="INodeIndexCursor.nodeReference()"/>,
      /// <seealso cref="IRelationshipIndexCursor.sourceNodeReference()"/>, or <seealso cref="IRelationshipIndexCursor.targetNodeReference()"/>. </param>
      /// <param name="cursor"> the ICursor to use for consuming the results. </param>
      void SingleNode(long reference, INodeCursor ICursor);

      /// <summary>
      /// Checks if a node exists in the database
      /// </summary>
      /// <param name="reference"> The reference of the node to check </param>
      /// <returns> {@code true} if the node exists, otherwise {@code false} </returns>
      bool NodeExists(long reference);

      /// <summary>
      /// The number of nodes in the graph, including anything changed in the transaction state.
      ///
      /// If the label parameter is <seealso cref="ANY_LABEL"/>, this method returns the total number of nodes in the graph, i.e.
      /// {@code MATCH (n) RETURN count(n)}.
      ///
      /// If the label parameter is set to any other value, this method returns the number of nodes that has that label,
      /// i.e. {@code MATCH (n:LBL) RETURN count(n)}.
      /// </summary>
      /// <param name="labelId"> the label to get the count for, or <seealso cref="ANY_LABEL"/> to get the total number of nodes. </param>
      /// <returns> the number of matching nodes in the graph. </returns>
      long CountsForNode(int labelId);

      /// <summary>
      /// The number of nodes in the graph, without taking into account anything in the transaction state.
      ///
      /// If the label parameter is <seealso cref="ANY_LABEL"/>, this method returns the total number of nodes in the graph, i.e.
      /// {@code MATCH (n) RETURN count(n)}.
      ///
      /// If the label parameter is set to any other value, this method returns the number of nodes that has that label,
      /// i.e. {@code MATCH (n:LBL) RETURN count(n)}.
      /// </summary>
      /// <param name="labelId"> the label to get the count for, or <seealso cref="ANY_LABEL"/> to get the total number of nodes. </param>
      /// <returns> the number of matching nodes in the graph. </returns>
      long CountsForNodeWithoutTxState(int labelId);

      /// <summary>
      /// The number of relationships in the graph, including anything changed in the transaction state.
      ///
      /// Returns the number of relationships in the graph that matches the specified pattern,
      /// {@code (:startLabelId)-[:typeId]->(:endLabelId)}, like so:
      ///
      /// <table>
      /// <thead>
      /// <tr><th>{@code startLabelId}</th><th>{@code typeId}</th>                  <th>{@code endLabelId}</th>
      /// <td></td>                 <th>Pattern</th>                       <td></td></tr>
      /// </thead>
      /// <tdata>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r]->()}</td>            <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td>{@code REL}</td>                     <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r:REL]->()}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td>{@code LHS}</td>             <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code (:LHS)-[r]->()}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td>{@code RHS}</td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r]->(:RHS)}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td>{@code LHS}</td>             <td>{@code REL}</td>                     <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code (:LHS)-[r:REL]->()}</td>    <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td>{@code REL}</td>                     <td>{@code RHS}</td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r:REL]->(:RHS)}</td>    <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// </tdata>
      /// </table>
      /// </summary>
      /// <param name="startLabelId"> the label of the start node of relationships to get the count for, or <seealso cref="ANY_LABEL"/>. </param>
      /// <param name="typeId">       the type of relationships to get a count for, or <seealso cref="ANY_RELATIONSHIP_TYPE"/>. </param>
      /// <param name="endLabelId">   the label of the end node of relationships to get the count for, or <seealso cref="ANY_LABEL"/>. </param>
      /// <returns> the number of matching relationships in the graph. </returns>
      long CountsForRelationship(int startLabelId, int typeId, int endLabelId);

      /// <summary>
      /// The number of relationships in the graph, without taking into account anything in the transaction state.
      ///
      /// Returns the number of relationships in the graph that matches the specified pattern,
      /// {@code (:startLabelId)-[:typeId]->(:endLabelId)}, like so:
      ///
      /// <table>
      /// <thead>
      /// <tr><th>{@code startLabelId}</th><th>{@code typeId}</th>                  <th>{@code endLabelId}</th>
      /// <td></td>                 <th>Pattern</th>                       <td></td></tr>
      /// </thead>
      /// <tdata>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r]->()}</td>            <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td>{@code REL}</td>                     <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r:REL]->()}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td>{@code LHS}</td>             <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code (:LHS)-[r]->()}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td><seealso cref="ANY_RELATIONSHIP_TYPE"/></td>  <td>{@code RHS}</td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r]->(:RHS)}</td>        <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td>{@code LHS}</td>             <td>{@code REL}</td>                     <td><seealso cref="ANY_LABEL"/></td>
      /// <td>{@code MATCH}</td>    <td>{@code (:LHS)-[r:REL]->()}</td>    <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// <tr>
      /// <td><seealso cref="ANY_LABEL"/></td>      <td>{@code REL}</td>                     <td>{@code RHS}</td>
      /// <td>{@code MATCH}</td>    <td>{@code ()-[r:REL]->(:RHS)}</td>    <td>{@code RETURN count(r)}</td>
      /// </tr>
      /// </tdata>
      /// </table>
      /// </summary>
      /// <param name="startLabelId"> the label of the start node of relationships to get the count for, or <seealso cref="ANY_LABEL"/>. </param>
      /// <param name="typeId">       the type of relationships to get a count for, or <seealso cref="ANY_RELATIONSHIP_TYPE"/>. </param>
      /// <param name="endLabelId">   the label of the end node of relationships to get the count for, or <seealso cref="ANY_LABEL"/>. </param>
      /// <returns> the number of matching relationships in the graph. </returns>
      long CountsForRelationshipWithoutTxState(int startLabelId, int typeId, int endLabelId);

      /// <summary>
      /// Count of the total number of nodes in the database including changes in the current transaction.
      /// </summary>
      /// <returns> the total number of nodes in the database </returns>
      long NodesGetCount();

      /// <summary>
      /// Count of the total number of relationships in the database including changes in the current transaction.
      /// </summary>
      /// <returns> the total number of relationships in the database </returns>
      long RelationshipsGetCount();

      /// <param name="reference">
      ///         a reference from <seealso cref="IRelationshipDataAccessor.relationshipReference()"/>. </param>
      /// <param name="cursor">
      ///         the ICursor to use for consuming the results. </param>
      void SingleRelationship(long reference, IRelationshipScanCursor ICursor);

      /// <summary>
      /// Checks if a relationship exists in the database
      /// </summary>
      /// <param name="reference"> The reference of the relationship to check </param>
      /// <returns> <tt>true</tt> if the relationship exists, otherwise <tt>false</tt> </returns>
      bool RelationshipExists(long reference);

      void AllRelationshipsScan(IRelationshipScanCursor ICursor);

      Scan<IRelationshipScanCursor> AllRelationshipsScan();

      void RelationshipTypeScan(int type, IRelationshipScanCursor ICursor);

      Scan<IRelationshipScanCursor> RelationshipTypeScan(int type);

      /// <param name="nodeReference">
      ///         a reference from <seealso cref="INodeCursor.nodeReference()"/>. </param>
      /// <param name="reference">
      ///         a reference from <seealso cref="INodeCursor.relationshipGroupReference()"/>. </param>
      /// <param name="cursor">
      ///         the ICursor to use for consuming the results. </param>
      void RelationshipGroups(long nodeReference, long reference, IRelationshipGroupCursor ICursor);

      /// <param name="nodeReference">
      ///         a reference from <seealso cref="INodeCursor.nodeReference()"/>. </param>
      /// <param name="reference">
      ///         a reference from <seealso cref="IRelationshipGroupCursor.outgoingReference()"/>,
      ///         <seealso cref="IRelationshipGroupCursor.incomingReference()"/>,
      ///         or <seealso cref="IRelationshipGroupCursor.loopsReference()"/>. </param>
      /// <param name="cursor">
      ///         the ICursor to use for consuming the results. </param>
      void Relationships(long nodeReference, long reference, IRelationshipTraversalCursor ICursor);

      /// <param name="nodeReference">
      ///         the owner of the properties. </param>
      /// <param name="reference">
      ///         a reference from <seealso cref="INodeCursor.propertiesReference()"/>. </param>
      /// <param name="cursor">
      ///         the ICursor to use for consuming the results. </param>
      void NodeProperties(long nodeReference, long reference, IPropertyCursor ICursor);

      /// <param name="relationshipReference">
      ///         the owner of the properties. </param>
      /// <param name="reference">
      ///         a reference from <seealso cref="IRelationshipDataAccessor.propertiesReference()"/>. </param>
      /// <param name="cursor">
      ///         the ICursor to use for consuming the results. </param>
      void RelationshipProperties(long relationshipReference, long reference, IPropertyCursor ICursor);

      /// <summary>
      /// Checks if a node was deleted in the current transaction </summary>
      /// <param name="node"> the node to check </param>
      /// <returns> <code>true</code> if the node was deleted otherwise <code>false</code> </returns>
      bool NodeDeletedInTransaction(long node);

      /// <summary>
      /// Checks if a relationship was deleted in the current transaction </summary>
      /// <param name="relationship"> the relationship to check </param>
      /// <returns> <code>true</code> if the relationship was deleted otherwise <code>false</code> </returns>
      bool RelationshipDeletedInTransaction(long relationship);

      /// <summary>
      /// Returns the value of a node property if set in this transaction. </summary>
      /// <param name="node"> the node </param>
      /// <param name="propertyKeyId"> the property key id of interest </param>
      /// <returns> <code>null</code> if the property has not been changed for the node in this transaction. Otherwise returns
      ///         the new property value, or <seealso cref="Values.NO_VALUE"/> if the property has been removed in this transaction. </returns>
      Value NodePropertyChangeInTransactionOrNull(long node, int propertyKeyId);

      void GraphProperties(IPropertyCursor ICursor);

      // hints to the page cache about data we will be accessing in the future:

      void FutureNodeReferenceRead(long reference);

      void FutureRelationshipsReferenceRead(long reference);

      void FutureNodePropertyReferenceRead(long reference);

      void FutureRelationshipPropertyReferenceRead(long reference);
   }

   public static class Read_Fields
   {
      public const int ANY_LABEL = -1;
      public const int ANY_RELATIONSHIP_TYPE = -1;
   }
}