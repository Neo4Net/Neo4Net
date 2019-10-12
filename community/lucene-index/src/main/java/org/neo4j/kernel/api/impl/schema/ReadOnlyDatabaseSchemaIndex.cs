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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{

	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using ReadOnlyIndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.ReadOnlyIndexPartitionFactory;
	using PartitionedIndexStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using Value = Org.Neo4j.Values.Storable.Value;

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
//ORIGINAL LINE: public void verifyUniqueness(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  luceneIndex.verifyUniqueness( accessor, propertyKeyIds );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUniqueness(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds, java.util.List<org.neo4j.values.storable.Value[]> updatedValueTuples) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds, IList<Value[]> updatedValueTuples )
		 {
			  luceneIndex.verifyUniqueness( accessor, propertyKeyIds, updatedValueTuples );
		 }
	}

}