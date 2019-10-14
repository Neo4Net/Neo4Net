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
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;

	internal class NodeLabelIndexProgressor : IndexProgressor
	{
		 private readonly PrimitiveLongResourceIterator _iterator;
		 private readonly Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeLabelClient _client;

		 internal NodeLabelIndexProgressor( PrimitiveLongResourceIterator iterator, Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeLabelClient client )
		 {
			  this._iterator = iterator;
			  this._client = client;
		 }

		 public override bool Next()
		 {
			  while ( _iterator.hasNext() )
			  {
					if ( _client.acceptNode( _iterator.next(), null ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  _iterator.close();
		 }
	}

}