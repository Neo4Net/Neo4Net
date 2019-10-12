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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A <seealso cref="IndexProgressor"/> + <seealso cref="IndexProgressor.NodeValueClient"/> combo presented as a <seealso cref="LongIterator"/>.
	/// </summary>
	public class NodeValueIterator : PrimitiveLongCollections.PrimitiveLongBaseIterator, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient, PrimitiveLongResourceIterator
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