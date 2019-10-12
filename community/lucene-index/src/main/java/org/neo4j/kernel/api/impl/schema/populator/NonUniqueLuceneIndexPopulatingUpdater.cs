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
	using LuceneIndexWriter = Org.Neo4j.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using Org.Neo4j.Kernel.Api.Index;
	using NonUniqueIndexSampler = Org.Neo4j.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler;

	/// <summary>
	/// A <seealso cref="LuceneIndexPopulatingUpdater"/> used for non-unique Lucene schema indexes.
	/// </summary>
	public class NonUniqueLuceneIndexPopulatingUpdater : LuceneIndexPopulatingUpdater
	{
		 private readonly NonUniqueIndexSampler _sampler;

		 public NonUniqueLuceneIndexPopulatingUpdater( LuceneIndexWriter writer, NonUniqueIndexSampler sampler ) : base( writer )
		 {
			  this._sampler = sampler;
		 }

		 protected internal override void Added<T1>( IndexEntryUpdate<T1> update )
		 {
			  string encodedValue = LuceneDocumentStructure.encodedStringValuesForSampling( update.Values() );
			  _sampler.include( encodedValue );
		 }

		 protected internal override void Changed<T1>( IndexEntryUpdate<T1> update )
		 {
			  string encodedValueBefore = LuceneDocumentStructure.encodedStringValuesForSampling( update.BeforeValues() );
			  _sampler.exclude( encodedValueBefore );

			  string encodedValueAfter = LuceneDocumentStructure.encodedStringValuesForSampling( update.Values() );
			  _sampler.include( encodedValueAfter );
		 }

		 protected internal override void Removed<T1>( IndexEntryUpdate<T1> update )
		 {
			  string removedValue = LuceneDocumentStructure.encodedStringValuesForSampling( update.Values() );
			  _sampler.exclude( removedValue );
		 }

		 public override void Close()
		 {
		 }
	}

}