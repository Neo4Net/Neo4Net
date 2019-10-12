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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;

	using Org.Neo4j.Function;
	using IndexWriterConfigs = Org.Neo4j.Kernel.Api.Impl.Index.IndexWriterConfigs;
	using Org.Neo4j.Kernel.Api.Impl.Index.builder;
	using ReadOnlyIndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.ReadOnlyIndexPartitionFactory;
	using WritableIndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.WritableIndexPartitionFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;

	public class FulltextIndexBuilder : AbstractLuceneIndexBuilder<FulltextIndexBuilder>
	{
		 private readonly FulltextIndexDescriptor _descriptor;
		 private readonly TokenHolder _propertyKeyTokenHolder;
		 private bool _populating;
		 private IndexUpdateSink _indexUpdateSink = NullIndexUpdateSink.Instance;

		 private FulltextIndexBuilder( FulltextIndexDescriptor descriptor, Config config, TokenHolder propertyKeyTokenHolder ) : base( config )
		 {
			  this._descriptor = descriptor;
			  this._propertyKeyTokenHolder = propertyKeyTokenHolder;
		 }

		 /// <summary>
		 /// Create new lucene fulltext index builder.
		 /// </summary>
		 /// <param name="descriptor"> The descriptor for this index </param>
		 /// <param name="propertyKeyTokenHolder"> A token holder used to look up property key token names by id. </param>
		 /// <returns> new FulltextIndexBuilder </returns>
		 public static FulltextIndexBuilder Create( FulltextIndexDescriptor descriptor, Config config, TokenHolder propertyKeyTokenHolder )
		 {
			  return new FulltextIndexBuilder( descriptor, config, propertyKeyTokenHolder );
		 }

		 /// <summary>
		 /// Whether to create the index in a <seealso cref="IndexWriterConfigs.population() populating"/> mode, if {@code true}, or
		 /// in a <seealso cref="IndexWriterConfigs.standard() standard"/> mode, if {@code false}.
		 /// </summary>
		 /// <param name="isPopulating"> {@code true} if the index should be created in a populating mode. </param>
		 /// <returns> this index builder. </returns>
		 internal virtual FulltextIndexBuilder WithPopulatingMode( bool isPopulating )
		 {
			  this._populating = isPopulating;
			  return this;
		 }

		 internal virtual FulltextIndexBuilder WithIndexUpdateSink( IndexUpdateSink indexUpdateSink )
		 {
			  this._indexUpdateSink = indexUpdateSink;
			  return this;
		 }

		 /// <summary>
		 /// Build lucene schema index with specified configuration
		 /// </summary>
		 /// <returns> lucene schema index </returns>
		 public virtual DatabaseFulltextIndex Build()
		 {
			  if ( ReadOnly )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.impl.index.partition.ReadOnlyIndexPartitionFactory partitionFactory = new org.neo4j.kernel.api.impl.index.partition.ReadOnlyIndexPartitionFactory();
					ReadOnlyIndexPartitionFactory partitionFactory = new ReadOnlyIndexPartitionFactory();
					LuceneFulltextIndex fulltextIndex = new LuceneFulltextIndex( StorageBuilder.build(), partitionFactory, _descriptor, _propertyKeyTokenHolder );
					return new ReadOnlyFulltextIndex( fulltextIndex );
			  }
			  else
			  {
					Factory<IndexWriterConfig> writerConfigFactory;
					if ( _populating )
					{
						 writerConfigFactory = () => IndexWriterConfigs.population(_descriptor.analyzer());
					}
					else
					{
						 writerConfigFactory = () => IndexWriterConfigs.standard(_descriptor.analyzer());
					}
					WritableIndexPartitionFactory partitionFactory = new WritableIndexPartitionFactory( writerConfigFactory );
					LuceneFulltextIndex fulltextIndex = new LuceneFulltextIndex( StorageBuilder.build(), partitionFactory, _descriptor, _propertyKeyTokenHolder );
					return new WritableFulltextIndex( _indexUpdateSink, fulltextIndex );
			  }
		 }
	}

}