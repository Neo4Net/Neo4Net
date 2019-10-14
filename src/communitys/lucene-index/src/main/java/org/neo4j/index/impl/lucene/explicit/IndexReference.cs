using System;

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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;


	internal abstract class IndexReference
	{
		 private readonly IndexIdentifier _identifier;
		 private readonly IndexSearcher _searcher;
		 private readonly AtomicInteger _refCount = new AtomicInteger( 0 );
		 private bool _searcherIsClosed;

		 /// <summary>
		 /// We need this because we only want to close the reader/searcher if
		 /// it has been detached... i.e. the <seealso cref="LuceneDataSource"/> no longer
		 /// has any reference to it, only an iterator out in the client has a ref.
		 /// And when that client calls close() it should be closed.
		 /// </summary>
		 private volatile bool _detached;

		 internal IndexReference( IndexIdentifier identifier, IndexSearcher searcher )
		 {
			  this._identifier = identifier;
			  this._searcher = searcher;
		 }

		 internal abstract IndexWriter Writer { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void dispose() throws java.io.IOException;
		 internal abstract void Dispose();

		 internal abstract bool CheckAndClearStale();

		 internal abstract void SetStale();

		 public virtual IndexSearcher Searcher
		 {
			 get
			 {
				  return _searcher;
			 }
		 }

		 public virtual IndexIdentifier Identifier
		 {
			 get
			 {
				  return _identifier;
			 }
		 }

		 internal virtual void IncRef()
		 {
			  this._refCount.incrementAndGet();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void disposeSearcher() throws java.io.IOException
		 internal virtual void DisposeSearcher()
		 {
			  if ( !_searcherIsClosed )
			  {
					_searcher.IndexReader.close();
					_searcherIsClosed = true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void detachOrClose() throws java.io.IOException
		 internal virtual void DetachOrClose()
		 {
			  if ( this._refCount.get() == 0 )
			  {
					DisposeSearcher();
			  }
			  else
			  {
					this._detached = true;
			  }
		 }

		 public virtual bool Close()
		 {
			 lock ( this )
			 {
				  try
				  {
						if ( this._searcherIsClosed || this._refCount.get() == 0 )
						{
							 return true;
						}
      
						bool reallyClosed = false;
						if ( this._refCount.decrementAndGet() <= 0 && this._detached )
						{
							 DisposeSearcher();
							 reallyClosed = true;
						}
						return reallyClosed;
				  }
				  catch ( IOException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _searcherIsClosed;
			 }
		 }

		 internal virtual bool Detached
		 {
			 get
			 {
				  return _detached;
			 }
		 }
	}

}