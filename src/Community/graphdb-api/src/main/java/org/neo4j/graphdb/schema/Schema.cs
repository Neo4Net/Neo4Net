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
namespace Neo4Net.Graphdb.schema
{

	using IndexPopulationProgress = Neo4Net.Graphdb.index.IndexPopulationProgress;

	/// <summary>
	/// Interface for managing the schema of your graph database. This currently includes
	/// the indexing support added in Neo4j 2.0. Please see the Neo4j manual for details.
	/// 
	/// Compatibility note: New methods may be added to this interface without notice,
	/// backwards compatibility is only guaranteed for clients of this interface, not for
	/// implementors.
	/// </summary>
	public interface Schema
	{
		 /// <summary>
		 /// The states that an index can be in. This mostly relates to tracking the background
		 /// population of an index, to tell when it is done populating and is online serving
		 /// requests.
		 /// </summary>

		 /// <summary>
		 /// Returns an <seealso cref="IndexCreator"/> where details about the index to create can be
		 /// specified. When all details have been entered <seealso cref="IndexCreator.create() create"/>
		 /// must be called for it to actually be created.
		 /// 
		 /// Creating an index enables indexing for nodes with the specified label. The index will
		 /// have the details supplied to the <seealso cref="IndexCreator returned index creator"/>.
		 /// All existing and all future nodes matching the index definition will be indexed,
		 /// speeding up future operations.
		 /// </summary>
		 /// <param name="label"> <seealso cref="Label label"/> on nodes to be indexed
		 /// </param>
		 /// <returns> an <seealso cref="IndexCreator"/> capable of providing details for, as well as creating
		 /// an index for the given <seealso cref="Label label"/>. </returns>
		 IndexCreator IndexFor( Label label );

		 /// <param name="label"> the <seealso cref="Label"/> to get <seealso cref="IndexDefinition indexes"/> for. </param>
		 /// <returns> all <seealso cref="IndexDefinition indexes"/> attached to the given <seealso cref="Label label"/>. </returns>
		 IEnumerable<IndexDefinition> GetIndexes( Label label );

		 /// <returns> all <seealso cref="IndexDefinition indexes"/> in this database. </returns>
		 IEnumerable<IndexDefinition> Indexes { get; }

		 /// <summary>
		 /// Poll the database for the state of a given index. This can be used to track in which
		 /// state the creation of the index is, for example if it's still
		 /// <seealso cref="IndexState.POPULATING populating"/> in the background, or has come
		 /// <seealso cref="IndexState.ONLINE online"/>.
		 /// </summary>
		 /// <param name="index"> the index that we want to poll state for </param>
		 /// <returns> the current <seealso cref="IndexState"/> of the index </returns>
		 Schema_IndexState GetIndexState( IndexDefinition index );

		 /// <summary>
		 /// Poll the database for the population progress. This can be used to track the progress of the
		 /// population job associated to the given index. If the index is
		 /// <seealso cref="IndexState.POPULATING populating"/> or <seealso cref="IndexState.ONLINE online"/>, the state will contain current
		 /// progress. If the index is <seealso cref="IndexState.FAILED failed"/> then the state returned from this method
		 /// should be regarded as invalid.
		 /// </summary>
		 /// <param name="index"> the index that we want to poll state for </param>
		 /// <returns> the current population progress for the index
		 ///  </returns>
		 IndexPopulationProgress GetIndexPopulationProgress( IndexDefinition index );

		 /// <summary>
		 /// If <seealso cref="getIndexState(IndexDefinition)"/> return <seealso cref="IndexState.FAILED"/> this method will
		 /// return the failure description. </summary>
		 /// <param name="index"> the <seealso cref="IndexDefinition"/> to get failure from. </param>
		 /// <returns> the failure description. </returns>
		 /// <exception cref="IllegalStateException"> if the {@code index} isn't in a <seealso cref="IndexState.FAILED"/> state. </exception>
		 string GetIndexFailure( IndexDefinition index );

		 /// <summary>
		 /// Returns a <seealso cref="ConstraintCreator"/> where details about the constraint can be
		 /// specified. When all details have been entered <seealso cref="ConstraintCreator.create()"/>
		 /// must be called for it to actually be created.
		 /// 
		 /// Creating a constraint will block on the <seealso cref="ConstraintCreator.create() create method"/> until
		 /// all existing data has been verified for compliance. If any existing data doesn't comply with the constraint an
		 /// exception will be thrown, and the constraint will not be created.
		 /// </summary>
		 /// <param name="label"> the label this constraint is for. </param>
		 /// <returns> a <seealso cref="ConstraintCreator"/> capable of providing details for, as well as creating
		 /// a constraint for the given <seealso cref="Label label"/>. </returns>
		 ConstraintCreator ConstraintFor( Label label );

		 /// <param name="label"> the <seealso cref="Label label"/> to get constraints for. </param>
		 /// <returns> all constraints for the given label. </returns>
		 IEnumerable<ConstraintDefinition> GetConstraints( Label label );

		 /// <param name="type"> the <seealso cref="RelationshipType relationship type"/> to get constraints for. </param>
		 /// <returns> all constraints for the given relationship type. </returns>
		 IEnumerable<ConstraintDefinition> GetConstraints( RelationshipType type );

		 /// <returns> all constraints </returns>
		 IEnumerable<ConstraintDefinition> Constraints { get; }

		 /// <summary>
		 /// Wait until an index comes online
		 /// </summary>
		 /// <param name="index"> the index that we want to wait for </param>
		 /// <param name="duration"> duration to wait for the index to come online </param>
		 /// <param name="unit"> TimeUnit of duration </param>
		 /// <exception cref="IllegalStateException"> if the index did not enter the ONLINE state
		 ///             within the given duration or if the index entered the FAILED
		 ///             state </exception>
		 void AwaitIndexOnline( IndexDefinition index, long duration, TimeUnit unit );

		 /// <summary>
		 /// Wait until all indices comes online
		 /// </summary>
		 /// <param name="duration"> duration to wait for all indexes to come online </param>
		 /// <param name="unit"> TimeUnit of duration </param>
		 /// <exception cref="IllegalStateException"> if some index did not enter the ONLINE
		 ///             state within the given duration or if the index entered the
		 ///             FAILED state </exception>
		 void AwaitIndexesOnline( long duration, TimeUnit unit );

		 /// <summary>
		 /// Get an <seealso cref="IndexDefinition"/> by the given name of the index. </summary>
		 /// <param name="indexName"> The given name of the index. </param>
		 /// <returns> The index with that given name. </returns>
		 /// <exception cref="IllegalArgumentException"> if there is no index with that given name. </exception>
		 IndexDefinition GetIndexByName( string indexName );
	}

	 public enum Schema_IndexState
	 {
		  Online,
		  Populating,
		  Failed
	 }

}