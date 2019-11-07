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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Unifies all data input given to a <seealso cref="BatchImporter"/> to allow for more coherent implementations.
	/// </summary>
	public interface IInput
	{

		 /// <summary>
		 /// Provides all node data for an import.
		 /// </summary>
		 /// <returns> an <seealso cref="InputIterator"/> which will provide all node data for the whole import. </returns>
		 InputIterable Nodes();

		 /// <summary>
		 /// Provides all relationship data for an import.
		 /// </summary>
		 /// <returns> an <seealso cref="InputIterator"/> which will provide all relationship data for the whole import. </returns>
		 InputIterable Relationships();

		 /// <returns> <seealso cref="IdMapper"/> which will get populated by <seealso cref="NodeImporter"/>
		 /// and later queried by <seealso cref="RelationshipImporter"/>
		 /// to resolve potentially temporary input node ids to actual node ids in the database. </returns>
		 /// <param name="numberArrayFactory"> The factory for creating data-structures to use for caching internally in the IdMapper. </param>
		 IdMapper IdMapper( NumberArrayFactory numberArrayFactory );

		 /// <returns> a <seealso cref="Collector"/> capable of writing bad relationships
		 /// and duplicate nodes to an output stream for later handling. </returns>
		 Collector BadCollector();

		 /// <param name="valueSizeCalculator"> for calculating property sizes on disk. </param>
		 /// <returns> <seealso cref="Estimates"/> for this input w/o reading through it entirely. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Input_Estimates calculateEstimates(System.Func<Neo4Net.values.storable.Value[], int> valueSizeCalculator) throws java.io.IOException;
		 Input_Estimates CalculateEstimates( System.Func<Value[], int> valueSizeCalculator );
	}

	 public interface IInput_Estimates
	 {
		  /// <returns> estimated number of nodes for the entire input. </returns>
		  long NumberOfNodes();

		  /// <returns> estimated number of relationships for the entire input. </returns>
		  long NumberOfRelationships();

		  /// <returns> estimated number of node properties. </returns>
		  long NumberOfNodeProperties();

		  /// <returns> estimated number of relationship properties. </returns>
		  long NumberOfRelationshipProperties();

		  /// <returns> estimated size that the estimated number of node properties will require on disk.
		  /// This is a separate estimate since it depends on the type and size of the actual properties. </returns>
		  long SizeOfNodeProperties();

		  /// <returns> estimated size that the estimated number of relationship properties will require on disk.
		  /// This is a separate estimate since it depends on the type and size of the actual properties. </returns>
		  long SizeOfRelationshipProperties();

		  /// <returns> estimated number of node labels. Examples:
		  /// <ul>
		  /// <li>2 nodes, 1 label each ==> 2</li>
		  /// <li>1 node, 2 labels each ==> 2</li>
		  /// <li>2 nodes, 2 labels each ==> 4</li>
		  /// </ul> </returns>
		  long NumberOfNodeLabels();
	 }

}