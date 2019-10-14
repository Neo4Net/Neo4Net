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
namespace Neo4Net.Storageengine.Api.schema
{
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using Value = Neo4Net.Values.Storable.Value;

	internal class NodeValueIndexProgressor : IndexProgressor
	{
		 private readonly PrimitiveLongResourceIterator _ids;
		 private readonly IndexProgressor_NodeValueClient _client;

		 internal NodeValueIndexProgressor( PrimitiveLongResourceIterator ids, IndexProgressor_NodeValueClient client )
		 {
			  this._ids = ids;
			  this._client = client;
		 }

		 public override bool Next()
		 {
			  while ( _ids.hasNext() )
			  {
					if ( _client.acceptNode( _ids.next(), (Value[]) null ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  if ( _ids != null )
			  {
					_ids.close();
			  }
		 }
	}

}