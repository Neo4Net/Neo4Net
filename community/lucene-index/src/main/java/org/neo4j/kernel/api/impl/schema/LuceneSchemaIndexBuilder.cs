﻿/*
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
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;

	using Org.Neo4j.Function;
	using IndexWriterConfigs = Org.Neo4j.Kernel.Api.Impl.Index.IndexWriterConfigs;
	using Org.Neo4j.Kernel.Api.Impl.Index.builder;
	using ReadOnlyIndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.ReadOnlyIndexPartitionFactory;
	using WritableIndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.WritableIndexPartitionFactory;
	using PartitionedIndexStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;

	/// <summary>
	/// Helper builder class to simplify construction and instantiation of lucene schema indexes.
	/// Most of the values already have most useful default value, that still can be overridden by corresponding
	/// builder methods.
	/// </summary>
	/// <seealso cref= LuceneSchemaIndex </seealso>
	/// <seealso cref= AbstractLuceneIndexBuilder </seealso>
	public class LuceneSchemaIndexBuilder : AbstractLuceneIndexBuilder<LuceneSchemaIndexBuilder>
	{
		 private readonly IndexDescriptor _descriptor;
		 private IndexSamplingConfig _samplingConfig;
		 private Factory<IndexWriterConfig> _writerConfigFactory = IndexWriterConfigs.standard;

		 private LuceneSchemaIndexBuilder( IndexDescriptor descriptor, Config config ) : base( config )
		 {
			  this._descriptor = descriptor;
			  this._samplingConfig = new IndexSamplingConfig( config );
		 }

		 /// <summary>
		 /// Create new lucene schema index builder.
		 /// </summary>
		 /// <returns> new LuceneSchemaIndexBuilder </returns>
		 /// <param name="descriptor"> The descriptor for this index </param>
		 public static LuceneSchemaIndexBuilder Create( IndexDescriptor descriptor, Config config )
		 {
			  return new LuceneSchemaIndexBuilder( descriptor, config );
		 }

		 /// <summary>
		 /// Specify lucene schema index sampling config
		 /// </summary>
		 /// <param name="samplingConfig"> sampling config </param>
		 /// <returns> index builder </returns>
		 public virtual LuceneSchemaIndexBuilder WithSamplingConfig( IndexSamplingConfig samplingConfig )
		 {
			  this._samplingConfig = samplingConfig;
			  return this;
		 }

		 /// <summary>
		 /// Specify <seealso cref="Factory"/> of lucene <seealso cref="IndexWriterConfig"/> to create <seealso cref="IndexWriter"/>s.
		 /// </summary>
		 /// <param name="writerConfigFactory"> the supplier of writer configs </param>
		 /// <returns> index builder </returns>
		 public virtual LuceneSchemaIndexBuilder WithWriterConfig( Factory<IndexWriterConfig> writerConfigFactory )
		 {
			  this._writerConfigFactory = writerConfigFactory;
			  return this;
		 }

		 /// <summary>
		 /// Build lucene schema index with specified configuration
		 /// </summary>
		 /// <returns> lucene schema index </returns>
		 public virtual SchemaIndex Build()
		 {
			  if ( ReadOnly )
			  {
					return new ReadOnlyDatabaseSchemaIndex( StorageBuilder.build(), _descriptor, _samplingConfig, new ReadOnlyIndexPartitionFactory() );
			  }
			  else
			  {
					PartitionedIndexStorage storage = StorageBuilder.build();
					return new WritableDatabaseSchemaIndex( storage, _descriptor, _samplingConfig, new WritableIndexPartitionFactory( _writerConfigFactory ) );
			  }
		 }
	}

}