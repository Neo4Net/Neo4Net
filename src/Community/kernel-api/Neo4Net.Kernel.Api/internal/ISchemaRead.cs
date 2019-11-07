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

namespace Neo4Net.Kernel.Api.Internal
{
   using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.IConstraintDescriptor;
   using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
   using Register = Neo4Net.Register.Register;
   using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.ISchemaDescriptor;
   using Value = Neo4Net.Values.Storable.Value;

   /// <summary>
   /// Surface for getting schema information, such as fetching specific indexes or constraints.
   /// </summary>
   public interface ISchemaRead : SchemaReadCore
   {
      /// <summary>
      /// Acquire a reference to the index mapping the given {@code label} and {@code properties}.
      /// </summary>
      /// <param name="label"> the index label </param>
      /// <param name="properties"> the index properties </param>
      /// <returns> the IndexReference, or <seealso cref="IIndexReference.NO_INDEX"/> if such an index does not exist. </returns>
      IIndexReference Index(int label, params int[] properties);

      /// <summary>
      /// Acquire an index reference of the given {@code label} and {@code properties}. This method does not assert
      /// that the created reference points to a valid online index.
      /// </summary>
      /// <param name="label"> the index label </param>
      /// <param name="properties"> the index properties </param>
      /// <returns> a IndexReference for the given label and properties </returns>
      IIndexReference IndexReferenceUnchecked(int label, params int[] properties);

      /// <summary>
      /// Acquire an index reference of the given <seealso cref="SchemaDescriptor"/>. This method does not assert
      /// that the created reference points to a valid online index.
      /// </summary>
      /// <param name="schema"> <seealso cref="SchemaDescriptor"/> for the index. </param>
      /// <returns> a IndexReference for the given schema. </returns>
      IIndexReference IndexReferenceUnchecked(SchemaDescriptor schema);

      /// <summary>
      /// Returns the index with the given name
      /// </summary>
      /// <param name="name"> The name of the index you are looking for </param>
      /// <returns> The index associated with the given name </returns>
      IIndexReference IndexGetForName(string name);

      /// <summary>
      /// Get the index id (the id or the schema rule record) for a committed index
      /// - throws exception for indexes that aren't committed.
      /// </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: long indexGetCommittedId(IndexReference index) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
      long IndexGetCommittedId(IIndexReference index);

      /// <summary>
      /// Computes the selectivity of the unique values.
      /// </summary>
      /// <param name="index"> The index of interest </param>
      /// <returns> The selectivity of the given index </returns>
      /// <exception cref="IndexNotFoundKernelException"> if the index is not there </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: double indexUniqueValuesSelectivity(IndexReference index) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
      double IndexUniqueValuesSelectivity(IIndexReference index);

      /// <summary>
      /// Returns the size of the index.
      /// </summary>
      /// <param name="index"> The index of interest </param>
      /// <returns> The size of the current index </returns>
      /// <exception cref="IndexNotFoundKernelException"> if the index is not there </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: long indexSize(IndexReference index) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
      long IndexSize(IIndexReference index);

      /// <summary>
      /// Count the number of index entries for the given nodeId and value.
      /// </summary>
      /// <param name="index"> The index of interest </param>
      /// <param name="nodeId"> node id to match. </param>
      /// <param name="propertyKeyId"> the indexed property to look at (composite indexes apply to more than one property, so we need to specify this) </param>
      /// <param name="value"> the property value </param>
      /// <returns> number of index entries for the given {@code nodeId} and {@code value}. </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: long nodesCountIndexed(IndexReference index, long nodeId, int propertyKeyId, Neo4Net.values.storable.Value value) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      long NodesCountIndexed(IIndexReference index, long nodeId, int propertyKeyId, Value value);

      /// <summary>
      /// Returns how many updates that have been applied to the index since the last sampling, and total index size at the last sampling.
      ///
      /// Results are written to a <seealso cref="Register.DoubleLongRegister"/>, writing the update count into the first long, and
      /// the size into the second.
      /// </summary>
      /// <param name="index"> The index of interest </param>
      /// <param name="target"> A <seealso cref="Register.DoubleLongRegister"/> to which to write the update count and size. </param>
      /// <returns> {@code target} </returns>
      /// <exception cref="IndexNotFoundKernelException"> if the index does not exist. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.register.Register_DoubleLongRegister indexUpdatesAndSize(IndexReference index, Neo4Net.register.Register_DoubleLongRegister target) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
      Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize(IIndexReference index, Neo4Net.Register.Register_DoubleLongRegister target);

      /// <summary>
      /// Returns the number of unique entries and the total number of entries in an index.
      ///
      /// Results are written to a <seealso cref="Register.DoubleLongRegister"/>, writing the number of unique entries into
      /// the first long, and the total number of entries into the second.
      /// </summary>
      /// <param name="index"> The index of interest </param>
      /// <param name="target"> A <seealso cref="Register.DoubleLongRegister"/> to which to write the entry counts. </param>
      /// <returns> {@code target} </returns>
      /// <exception cref="IndexNotFoundKernelException"> if the index does not exist. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.register.Register_DoubleLongRegister indexSample(IndexReference index, Neo4Net.register.Register_DoubleLongRegister target) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
      Neo4Net.Register.Register_DoubleLongRegister IndexSample(IIndexReference index, Neo4Net.Register.Register_DoubleLongRegister target);

      /// <summary>
      /// Finds all constraints for the given schema
      /// </summary>
      /// <param name="descriptor"> The descriptor of the schema </param>
      /// <returns> All constraints for the given schema </returns>
      IEnumerator<ConstraintDescriptor> ConstraintsGetForSchema(SchemaDescriptor descriptor);

      /// <summary>
      /// Checks if a constraint exists
      /// </summary>
      /// <param name="descriptor"> The descriptor of the constraint to check. </param>
      /// <returns> {@code true} if the constraint exists, otherwise {@code false} </returns>
      bool ConstraintExists(ConstraintDescriptor descriptor);

      /// <summary>
      /// Produce a snapshot of the current schema, which can be accessed without acquiring any schema locks.
      /// <para>
      /// This is useful for inspecting schema elements when you have no intention of updating the schema,
      /// and where waiting on schema locks from, for instance, constraint creating transactions,
      /// would be inconvenient.
      /// </para>
      /// <para>
      /// The snapshot observes transaction state of the current transaction.
      /// </para>
      /// </summary>
      ISchemaReadCore Snapshot();

      /// <summary>
      /// Get the owning constraint for a constraint index or <tt>null</tt> if the index does not have an owning
      /// constraint.
      /// </summary>
      long? IndexGetOwningUniquenessConstraintId(IIndexReference index);

      /// <summary>
      /// Returns schema state for the given key or create a new state if not there </summary>
      /// <param name="key"> The key to access </param>
      /// <param name="creator"> function creating schema state </param>
      /// @param <K> type of the key </param>
      /// @param <V> type of the schema state value </param>
      /// <returns> the state associated with the key or a new value if non-existing </returns>
      V schemaStateGetOrCreate<K, V>(K key, System.Func<K, V> creator);

      /// <summary>
      /// Flush the schema state
      /// </summary>
      void SchemaStateFlush();
   }
}