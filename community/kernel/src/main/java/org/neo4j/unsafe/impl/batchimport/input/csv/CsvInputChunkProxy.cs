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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input.csv
{

	using Chunker = Org.Neo4j.Csv.Reader.Chunker;

	/// <summary>
	/// <seealso cref="CsvInputChunk"/> that adapts new input source groups during the streaming of data.
	/// <seealso cref="InputIterator"/> is fairly straight-forward, but is made a bit more complicated by the fact that
	/// there can be multiple different data streams. The outer iterator, <seealso cref="CsvGroupInputIterator"/>, is still responsible
	/// for handing out chunks, something that generally is good thing since it solves a bunch of other problems.
	/// The problem it has is that it doesn't know exactly which type of <seealso cref="CsvInputChunk"/> it wants to create,
	/// because that's up to each input group. This gap is bridged here in this class.
	/// </summary>
	public class CsvInputChunkProxy : CsvInputChunk
	{
		 private CsvInputChunk _actual;
		 private int _groupId = -1;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void ensureInstantiated(System.Func<CsvInputChunk> newChunk, int groupId) throws java.io.IOException
		 public virtual void EnsureInstantiated( System.Func<CsvInputChunk> newChunk, int groupId )
		 {
			  if ( _actual == null || groupId != this._groupId )
			  {
					CloseCurrent();
					_actual = newChunk();
			  }
			  this._groupId = groupId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  CloseCurrent();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeCurrent() throws java.io.IOException
		 private void CloseCurrent()
		 {
			  if ( _actual != null )
			  {
					_actual.Dispose();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean fillFrom(org.neo4j.csv.reader.Chunker chunker) throws java.io.IOException
		 public override bool FillFrom( Chunker chunker )
		 {
			  return _actual.fillFrom( chunker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor visitor) throws java.io.IOException
		 public override bool Next( InputEntityVisitor visitor )
		 {
			  return _actual.next( visitor );
		 }
	}

}