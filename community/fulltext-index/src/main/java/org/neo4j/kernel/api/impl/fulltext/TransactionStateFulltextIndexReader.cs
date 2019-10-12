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
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;

	using IOUtils = Org.Neo4j.Io.IOUtils;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.ScoreEntityIterator.mergeIterators;

	internal class TransactionStateFulltextIndexReader : FulltextIndexReader
	{
		 private readonly FulltextIndexReader _baseReader;
		 private readonly FulltextIndexReader _nearRealTimeReader;
		 private readonly MutableLongSet _modifiedEntityIdsInThisTransaction;

		 internal TransactionStateFulltextIndexReader( FulltextIndexReader baseReader, FulltextIndexReader nearRealTimeReader, MutableLongSet modifiedEntityIdsInThisTransaction )
		 {
			  this._baseReader = baseReader;
			  this._nearRealTimeReader = nearRealTimeReader;
			  this._modifiedEntityIdsInThisTransaction = modifiedEntityIdsInThisTransaction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ScoreEntityIterator query(String query) throws org.apache.lucene.queryparser.classic.ParseException
		 public override ScoreEntityIterator Query( string query )
		 {
			  ScoreEntityIterator iterator = _baseReader.query( query );
			  iterator = iterator.Filter( entry => !_modifiedEntityIdsInThisTransaction.contains( entry.entityId() ) );
			  iterator = mergeIterators( asList( iterator, _nearRealTimeReader.query( query ) ) );
			  return iterator;
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  // This is only used in the Consistency Checker. We don't need to worry about this here.
			  return 0;
		 }

		 public override void Close()
		 {
			  // The 'baseReader' is managed by the kernel, so we don't need to close it here.
			  IOUtils.closeAllUnchecked( _nearRealTimeReader );
		 }
	}

}