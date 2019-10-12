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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;


	internal class WritableIndexReference : IndexReference
	{
		 private readonly IndexWriter _writer;
		 private bool _writerIsClosed;
		 private readonly AtomicBoolean _stale = new AtomicBoolean();

		 internal WritableIndexReference( IndexIdentifier identifier, IndexSearcher searcher, IndexWriter writer ) : base( identifier, searcher )
		 {
			  this._writer = writer;
		 }

		 public override IndexWriter Writer
		 {
			 get
			 {
				  return _writer;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void dispose() throws java.io.IOException
		 public override void Dispose()
		 {
			 lock ( this )
			 {
				  DisposeSearcher();
				  DisposeWriter();
			 }
		 }

		 public override bool CheckAndClearStale()
		 {
			  return _stale.compareAndSet( true, false );
		 }

		 public override void SetStale()
		 {
			  _stale.set( true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void disposeWriter() throws java.io.IOException
		 private void DisposeWriter()
		 {
			  if ( !_writerIsClosed )
			  {
					_writer.close();
					_writerIsClosed = true;
			  }
		 }

		 internal virtual bool WriterClosed
		 {
			 get
			 {
				  return _writerIsClosed;
			 }
		 }
	}

}