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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using UniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.UniqueIndexSampler;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

	/// <summary>
	/// A <seealso cref="LuceneIndexPopulator"/> used for unique Lucene schema indexes.
	/// Performs sampling using <seealso cref="UniqueIndexSampler"/>.
	/// Verifies uniqueness of added and changed values using
	/// <seealso cref="SchemaIndex.verifyUniqueness(NodePropertyAccessor, int[])"/> method.
	/// </summary>
	public class UniqueLuceneIndexPopulator : LuceneIndexPopulator<SchemaIndex>
	{
		 private readonly int[] _propertyKeyIds;
		 private readonly UniqueIndexSampler _sampler;

		 public UniqueLuceneIndexPopulator( SchemaIndex index, IndexDescriptor descriptor ) : base( index )
		 {
			  this._propertyKeyIds = descriptor.Schema().PropertyIds;
			  this._sampler = new UniqueIndexSampler();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor accessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  try
			  {
					LuceneIndex.verifyUniqueness( accessor, _propertyKeyIds );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newPopulatingUpdater(final org.neo4j.storageengine.api.NodePropertyAccessor accessor)
		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return new UniqueLuceneIndexPopulatingUpdater( Writer, _propertyKeyIds, LuceneIndex, accessor, _sampler );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  _sampler.increment( 1 );
		 }

		 public override IndexSample SampleResult()
		 {
			  return _sampler.result();
		 }
	}

}