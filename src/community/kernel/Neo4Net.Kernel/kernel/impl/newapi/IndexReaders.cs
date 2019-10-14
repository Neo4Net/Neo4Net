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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAllUnchecked;

	internal class IndexReaders : System.IDisposable
	{
		 private readonly IList<IndexReader> _indexReaders = new List<IndexReader>();
		 private readonly IndexReference _indexReference;
		 private readonly Read _read;

		 internal IndexReaders( IndexReference indexReference, Read read )
		 {
			  this._indexReference = indexReference;
			  this._read = read;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.IndexReader createReader() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal virtual IndexReader CreateReader()
		 {
			  IndexReader indexReader = _read.indexReader( _indexReference, true );
			  _indexReaders.Add( indexReader );
			  return indexReader;
		 }

		 public override void Close()
		 {
			  closeAllUnchecked( _indexReaders );
		 }
	}

}