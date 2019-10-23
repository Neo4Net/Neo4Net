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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using DefaultNonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.DefaultNonUniqueIndexSampler;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using NonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;

	/// <summary>
	/// A <seealso cref="LuceneIndexPopulator"/> used for non-unique Lucene schema indexes.
	/// Performs sampling using <seealso cref="DefaultNonUniqueIndexSampler"/>.
	/// </summary>
	public class NonUniqueLuceneIndexPopulator : LuceneIndexPopulator<SchemaIndex>
	{
		 private readonly IndexSamplingConfig _samplingConfig;
		 private NonUniqueIndexSampler _sampler;

		 public NonUniqueLuceneIndexPopulator( SchemaIndex luceneIndex, IndexSamplingConfig samplingConfig ) : base( luceneIndex )
		 {
			  this._samplingConfig = samplingConfig;
			  this._sampler = CreateDefaultSampler();
		 }

		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  // no constraints to verify so do nothing
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
		 {
			  return new NonUniqueLuceneIndexPopulatingUpdater( Writer, _sampler );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  _sampler.include( LuceneDocumentStructure.encodedStringValuesForSampling( update.Values() ) );
		 }

		 public override IndexSample SampleResult()
		 {
			  return _sampler.result();
		 }

		 private DefaultNonUniqueIndexSampler CreateDefaultSampler()
		 {
			  return new DefaultNonUniqueIndexSampler( _samplingConfig.sampleSizeLimit() );
		 }
	}

}