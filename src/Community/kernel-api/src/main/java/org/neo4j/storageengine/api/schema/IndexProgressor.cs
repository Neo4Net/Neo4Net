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
namespace Neo4Net.Storageengine.Api.schema
{
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using LabelSet = Neo4Net.@internal.Kernel.Api.LabelSet;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// The index progressor is a cursor like class, which allows controlled progression through the entries of an index.
	/// In contrast to a cursor, the progressor does not hold value state, but rather attempts to write the next entry to a
	/// Client. The client can them accept the entry, in which case next() returns, or reject it, in which case the
	/// progression continues until an acceptable entry is found or the progression is done.
	/// 
	/// A Progressor is expected to feed a single client, which is setup for example in the constructor. The typical
	/// interaction goes something like
	/// 
	///   -- query(client) -> INDEX
	///                       progressor = new Progressor( client )
	///                       client.initialize( progressor, ... )
	/// 
	///   -- next() --> client
	///                 client ---- next() --> progressor
	///                        <-- accept() --
	///                                 :false
	///                        <-- accept() --
	///                                 :false
	///                        <-- accept() --
	///                                  :true
	///                 client <--------------
	///   <-----------
	/// </summary>
	public interface IndexProgressor : AutoCloseable
	{
		 /// <summary>
		 /// Progress through the index until the next accepted entry. Entries are feed to a Client, which
		 /// is setup in an implementation specific way.
		 /// </summary>
		 /// <returns> true if an accepted entry was found, false otherwise </returns>
		 bool Next();

		 /// <summary>
		 /// Close the progressor and all attached resources. Idempotent.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Client which accepts nodes and some of their property values.
		 /// </summary>

		 /// <summary>
		 /// Client which accepts nodes and some of their labels.
		 /// </summary>

		 /// <summary>
		 /// Client which accepts graph entities (nodes and relationships) and a fuzzy score.
		 /// </summary>

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 IndexProgressor EMPTY = new IndexProgressor()
	//	 {
	//		  @@Override public boolean next()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void close()
	//		  { // no-op
	//		  }
	//	 };
	}

	 public interface IndexProgressor_NodeValueClient
	 {
		  /// <summary>
		  /// Setup the client for progressing using the supplied progressor. The values feed in accept map to the
		  /// propertyIds provided here. Called by index implementation. </summary>
		  /// <param name="descriptor"> The descriptor </param>
		  /// <param name="progressor"> The progressor </param>
		  /// <param name="query"> The query of this progression </param>
		  /// <param name="indexOrder"> The required order the index should return nodeids in </param>
		  /// <param name="needsValues"> if the index should fetch property values together with node ids for index queries </param>
		  void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues );

		  /// <summary>
		  /// Accept the node id and values of a candidate index entry. Return true if the entry is
		  /// accepted, false otherwise. </summary>
		  /// <param name="reference"> the node id of the candidate index entry </param>
		  /// <param name="values"> the values of the candidate index entry </param>
		  /// <returns> true if the entry is accepted, false otherwise </returns>
		  bool AcceptNode( long reference, params Value[] values );

		  bool NeedsValues();
	 }

	 public interface IndexProgressor_NodeLabelClient
	 {
		  /// <summary>
		  /// Setup the client for progressing using the supplied progressor. Called by index implementation. </summary>
		  /// <param name="progressor"> the progressor </param>
		  /// <param name="providesLabels"> true if the progression can provide label information </param>
		  /// <param name="label"> the label to scan for </param>
		  void Scan( IndexProgressor progressor, bool providesLabels, int label );

		  void UnionScan( IndexProgressor progressor, bool providesLabels, params int[] labels );

		  void IntersectionScan( IndexProgressor progressor, bool providesLabels, params int[] labels );

		  /// <summary>
		  /// Accept the node id and (some) labels of a candidate index entry. Return true if the entry
		  /// is accepted, false otherwise. </summary>
		  /// <param name="reference"> the node id of the candidate index entry </param>
		  /// <param name="labels"> some labels of the candidate index entry </param>
		  /// <returns> true if the entry is accepted, false otherwise </returns>
		  bool AcceptNode( long reference, LabelSet labels );
	 }

	 public interface IndexProgressor_ExplicitClient
	 {
		  /// <summary>
		  /// Setup the client for progressing using the supplied progressor. Called by index implementation. </summary>
		  /// <param name="progressor"> the progressor </param>
		  /// <param name="expectedSize"> expected number of entries this progressor will feed the client. </param>
		  void Initialize( IndexProgressor progressor, int expectedSize );

		  /// <summary>
		  /// Accept the entity id and a score. Return true if the entry is accepted, false otherwise </summary>
		  /// <param name="reference"> the node id of the candidate index entry </param>
		  /// <param name="score"> score of the candidate index entry </param>
		  /// <returns> true if the entry is accepted, false otherwise </returns>
		  bool AcceptEntity( long reference, float score );
	 }

}