using System;
using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking.index
{

	using Neo4Net.Helpers.Collections;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;

	public class IndexIterator : BoundedIterable<long>
	{
		 private readonly IndexAccessor _indexAccessor;
		 private BoundedIterable<long> _indexReader;

		 public IndexIterator( IndexAccessor indexAccessor )
		 {
			  this._indexAccessor = indexAccessor;
		 }

		 public override long MaxCount()
		 {
			  try
			  {
					  using ( BoundedIterable<long> reader = _indexAccessor.newAllEntriesReader() )
					  {
						return reader.MaxCount();
					  }
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  if ( _indexReader != null )
			  {
					_indexReader.close();
			  }
		 }

		 public override IEnumerator<long> Iterator()
		 {
			  if ( _indexReader == null )
			  {
					_indexReader = _indexAccessor.newAllEntriesReader();
			  }

			  return _indexReader.GetEnumerator();
		 }
	}

}