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
namespace Neo4Net.Kernel.Api.Impl.Schema
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Impl.Index;
	using ReadOnlyIndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.ReadOnlyIndexPartitionFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Read only schema index
	/// </summary>
	public class ReadOnlyDatabaseSchemaIndex : ReadOnlyAbstractDatabaseIndex<LuceneSchemaIndex, IndexReader>, SchemaIndex
	{
		 public ReadOnlyDatabaseSchemaIndex( PartitionedIndexStorage indexStorage, IndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ReadOnlyIndexPartitionFactory readOnlyIndexPartitionFactory ) : base( new LuceneSchemaIndex( indexStorage, descriptor, samplingConfig, readOnlyIndexPartitionFactory ) )
		 {
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUniqueness(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor accessor, int[] propertyKeyIds) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  luceneIndex.verifyUniqueness( accessor, propertyKeyIds );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUniqueness(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor accessor, int[] propertyKeyIds, java.util.List<org.Neo4Net.values.storable.Value[]> updatedValueTuples) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds, IList<Value[]> updatedValueTuples )
		 {
			  luceneIndex.verifyUniqueness( accessor, propertyKeyIds, updatedValueTuples );
		 }
	}

}