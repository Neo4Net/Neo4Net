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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.populator
{

	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using LuceneIndexWriter = Org.Neo4j.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using Org.Neo4j.Kernel.Api.Index;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using UniqueIndexSampler = Org.Neo4j.Kernel.Impl.Api.index.sampling.UniqueIndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A <seealso cref="LuceneIndexPopulatingUpdater"/> used for unique Lucene schema indexes.
	/// Verifies uniqueness of added and changed values when closed using
	/// <seealso cref="SchemaIndex.verifyUniqueness(NodePropertyAccessor, int[], System.Collections.IList)"/> method.
	/// </summary>
	public class UniqueLuceneIndexPopulatingUpdater : LuceneIndexPopulatingUpdater
	{
		 private readonly int[] _propertyKeyIds;
		 private readonly SchemaIndex _luceneIndex;
		 private readonly NodePropertyAccessor _nodePropertyAccessor;
		 private readonly UniqueIndexSampler _sampler;

		 private readonly IList<Value[]> _updatedValueTuples = new List<Value[]>();

		 public UniqueLuceneIndexPopulatingUpdater( LuceneIndexWriter writer, int[] propertyKeyIds, SchemaIndex luceneIndex, NodePropertyAccessor nodePropertyAccessor, UniqueIndexSampler sampler ) : base( writer )
		 {
			  this._propertyKeyIds = propertyKeyIds;
			  this._luceneIndex = luceneIndex;
			  this._nodePropertyAccessor = nodePropertyAccessor;
			  this._sampler = sampler;
		 }

		 protected internal override void Added<T1>( IndexEntryUpdate<T1> update )
		 {
			  _sampler.increment( 1 );
			  _updatedValueTuples.Add( update.Values() );
		 }

		 protected internal override void Changed<T1>( IndexEntryUpdate<T1> update )
		 {
			  _updatedValueTuples.Add( update.Values() );
		 }

		 protected internal override void Removed<T1>( IndexEntryUpdate<T1> update )
		 {
			  _sampler.increment( -1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  try
			  {
					_luceneIndex.verifyUniqueness( _nodePropertyAccessor, _propertyKeyIds, _updatedValueTuples );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}