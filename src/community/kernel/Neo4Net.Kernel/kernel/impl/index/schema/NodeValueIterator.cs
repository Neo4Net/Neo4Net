﻿/*
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// A <seealso cref="IndexProgressor"/> + <seealso cref="IndexProgressor.NodeValueClient"/> combo presented as a <seealso cref="LongIterator"/>.
	/// </summary>
	public class NodeValueIterator : PrimitiveLongCollections.PrimitiveLongBaseIterator, Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient, PrimitiveLongResourceIterator
	{
		 private bool _closed;
		 private IndexProgressor _progressor;

		 protected internal override bool FetchNext()
		 {
			  // progressor.next() will progress underlying SeekCursor
			  // and feed result into this with node( long reference, Value... values )
			  if ( _closed || !_progressor.next() )
			  {
					Close();
					return false;
			  }
			  return true;
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
		 {
			  this._progressor = progressor;
		 }

		 public override bool AcceptNode( long reference, params Value[] values )
		 {
			  return Next( reference );
		 }

		 public override bool NeedsValues()
		 {
			  return false;
		 }

		 public override void Close()
		 {
			  if ( !_closed )
			  {
					_closed = true;
					_progressor.close();
					_progressor = null;
			  }
		 }
	}

}