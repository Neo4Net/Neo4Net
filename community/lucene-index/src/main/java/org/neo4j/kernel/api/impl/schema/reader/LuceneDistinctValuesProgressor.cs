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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.reader
{
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using BytesRef = org.apache.lucene.util.BytesRef;


	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal class LuceneDistinctValuesProgressor : IndexProgressor
	{
		 private readonly TermsEnum _terms;
		 private readonly Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient _client;
		 private readonly System.Func<BytesRef, Value> _valueMaterializer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: LuceneDistinctValuesProgressor(org.apache.lucene.index.TermsEnum terms, org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, System.Func<org.apache.lucene.util.BytesRef,org.neo4j.values.storable.Value> valueMaterializer) throws java.io.IOException
		 internal LuceneDistinctValuesProgressor( TermsEnum terms, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, System.Func<BytesRef, Value> valueMaterializer )
		 {
			  this._terms = terms;
			  this._client = client;
			  this._valueMaterializer = valueMaterializer;
		 }

		 public override bool Next()
		 {
			  try
			  {
					while ( ( _terms.next() ) != null )
					{
						 if ( _client.acceptNode( _terms.docFreq(), _valueMaterializer.apply(_terms.term()) ) )
						 {
							  return true;
						 }
					}
					return false;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Close()
		 {
		 }
	}

}