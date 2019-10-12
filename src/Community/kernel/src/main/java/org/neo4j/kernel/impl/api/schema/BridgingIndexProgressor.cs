using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.schema
{

	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Combine multiple progressor to act like one single logical progressor seen from client's perspective.
	/// </summary>
	public class BridgingIndexProgressor : Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient, IndexProgressor
	{
		 private readonly Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient _client;
		 private readonly int[] _keys;
		 // This is a thread-safe queue because it can be used in parallel scenarios.
		 // The overhead of a concurrent queue in this case is negligible since typically there will be two or a very few number
		 // of progressors and each progressor has many results each
		 private readonly LinkedList<IndexProgressor> _progressors = new ConcurrentLinkedQueue<IndexProgressor>();
		 private IndexProgressor _current;

		 public BridgingIndexProgressor( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, int[] keys )
		 {
			  this._client = client;
			  this._keys = keys;
		 }

		 public override bool Next()
		 {
			  if ( _current == null )
			  {
					_current = _progressors.RemoveFirst();
			  }
			  while ( _current != null )
			  {
					if ( _current.next() )
					{
						 return true;
					}
					else
					{
						 _current.close();
						 _current = _progressors.RemoveFirst();
					}
			  }
			  return false;
		 }

		 public override bool NeedsValues()
		 {
			  return _client.needsValues();
		 }

		 public override void Close()
		 {
			  _progressors.forEach( IndexProgressor.close );
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] queries, IndexOrder indexOrder, bool needsValues )
		 {
			  AssertKeysAlign( descriptor.Schema().PropertyIds );
			  _progressors.AddLast( progressor );
		 }

		 private void AssertKeysAlign( int[] keys )
		 {
			  for ( int i = 0; i < this._keys.Length; i++ )
			  {
					if ( this._keys[i] != keys[i] )
					{
						 throw new System.NotSupportedException( "Can not chain multiple progressors with different key set." );
					}
			  }
		 }

		 public override bool AcceptNode( long reference, Value[] values )
		 {
			  return _client.acceptNode( reference, values );
		 }
	}

}