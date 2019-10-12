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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using PostingsFormat = org.apache.lucene.codecs.PostingsFormat;
	using BlockTreeOrdsPostingsFormat = org.apache.lucene.codecs.blocktreeords.BlockTreeOrdsPostingsFormat;
	using Lucene54Codec = org.apache.lucene.codecs.lucene54.Lucene54Codec;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using KeepOnlyLastCommitDeletionPolicy = Org.Apache.Lucene.Index.KeepOnlyLastCommitDeletionPolicy;
	using LogByteSizeMergePolicy = Org.Apache.Lucene.Index.LogByteSizeMergePolicy;
	using PooledConcurrentMergeScheduler = Org.Apache.Lucene.Index.PooledConcurrentMergeScheduler;
	using SnapshotDeletionPolicy = Org.Apache.Lucene.Index.SnapshotDeletionPolicy;

	using LuceneDataSource = Neo4Net.Index.impl.lucene.@explicit.LuceneDataSource;
	using FeatureToggles = Neo4Net.Util.FeatureToggles;

	/// <summary>
	/// Helper factory for standard lucene index writer configuration.
	/// </summary>
	public sealed class IndexWriterConfigs
	{
		 private static readonly int _maxBufferedDocs = FeatureToggles.getInteger( typeof( IndexWriterConfigs ), "max_buffered_docs", 100000 );
		 private static readonly int _populationMaxBufferedDocs = FeatureToggles.getInteger( typeof( IndexWriterConfigs ), "population_max_buffered_docs", IndexWriterConfig.DISABLE_AUTO_FLUSH );
		 private static readonly int _maxBufferedDeleteTerms = FeatureToggles.getInteger( typeof( IndexWriterConfigs ), "max_buffered_delete_terms", 15000 );
		 private static readonly int _mergePolicyMergeFactor = FeatureToggles.getInteger( typeof( IndexWriterConfigs ), "merge.factor", 2 );
		 private static readonly double _mergePolicyNoCfsRatio = FeatureToggles.getDouble( typeof( IndexWriterConfigs ), "nocfs.ratio", 1.0 );
		 private static readonly double _mergePolicyMinMergeMb = FeatureToggles.getDouble( typeof( IndexWriterConfigs ), "min.merge", 0.1 );
		 private static readonly bool _codecBlockTreeOrdsPostingFormat = FeatureToggles.flag( typeof( IndexWriterConfigs ), "block.tree.ords.posting.format", true );

		 private static readonly double _standardRamBufferSizeMb = FeatureToggles.getDouble( typeof( IndexWriterConfigs ), "standard.ram.buffer.size", IndexWriterConfig.DEFAULT_RAM_BUFFER_SIZE_MB );
		 private static readonly double _populationRamBufferSizeMb = FeatureToggles.getDouble( typeof( IndexWriterConfigs ), "population.ram.buffer.size", 50 );

		 private static readonly bool _customMergeScheduler = FeatureToggles.flag( typeof( IndexWriterConfigs ), "custom.merge.scheduler", true );

		 /// <summary>
		 /// Default postings format for schema and label scan store indexes.
		 /// </summary>
		 private static readonly BlockTreeOrdsPostingsFormat _blockTreeOrdsPostingsFormat = new BlockTreeOrdsPostingsFormat();

		 private IndexWriterConfigs()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static IndexWriterConfig Standard()
		 {
			  Analyzer analyzer = LuceneDataSource.KEYWORD_ANALYZER;
			  return Standard( analyzer );
		 }

		 public static IndexWriterConfig Standard( Analyzer analyzer )
		 {
			  IndexWriterConfig writerConfig = new IndexWriterConfig( analyzer );

			  writerConfig.MaxBufferedDocs = _maxBufferedDocs;
			  writerConfig.MaxBufferedDeleteTerms = _maxBufferedDeleteTerms;
			  writerConfig.IndexDeletionPolicy = new SnapshotDeletionPolicy( new KeepOnlyLastCommitDeletionPolicy() );
			  writerConfig.UseCompoundFile = true;
			  writerConfig.RAMBufferSizeMB = _standardRamBufferSizeMb;
			  writerConfig.Codec = new Lucene54CodecAnonymousInnerClass();
			  if ( _customMergeScheduler )
			  {
					writerConfig.MergeScheduler = new PooledConcurrentMergeScheduler();
			  }

			  LogByteSizeMergePolicy mergePolicy = new LogByteSizeMergePolicy();
			  mergePolicy.NoCFSRatio = _mergePolicyNoCfsRatio;
			  mergePolicy.MinMergeMB = _mergePolicyMinMergeMb;
			  mergePolicy.MergeFactor = _mergePolicyMergeFactor;
			  writerConfig.MergePolicy = mergePolicy;

			  return writerConfig;
		 }

		 private class Lucene54CodecAnonymousInnerClass : Lucene54Codec
		 {
			 public override PostingsFormat getPostingsFormatForField( string field )
			 {
				  PostingsFormat postingFormat = base.getPostingsFormatForField( field );
				  return _codecBlockTreeOrdsPostingFormat ? _blockTreeOrdsPostingsFormat : postingFormat;
			 }
		 }

		 public static IndexWriterConfig Population()
		 {
			  Analyzer analyzer = LuceneDataSource.KEYWORD_ANALYZER;
			  return Population( analyzer );
		 }

		 public static IndexWriterConfig Population( Analyzer analyzer )
		 {
			  IndexWriterConfig writerConfig = Standard( analyzer );
			  writerConfig.MaxBufferedDocs = _populationMaxBufferedDocs;
			  writerConfig.RAMBufferSizeMB = _populationRamBufferSizeMb;
			  return writerConfig;
		 }

		 public static IndexWriterConfig TransactionState( Analyzer analyzer )
		 {
			  IndexWriterConfig config = Standard( analyzer );
			  // Index transaction state is never directly persisted, so never commit it on close.
			  config.CommitOnClose = false;
			  return config;
		 }
	}

}