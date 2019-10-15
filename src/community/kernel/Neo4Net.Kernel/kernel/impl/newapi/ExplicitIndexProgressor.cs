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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using ExplicitIndexHits = Neo4Net.Kernel.api.ExplicitIndexHits;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;

	public class ExplicitIndexProgressor : IndexProgressor
	{
		 private readonly Neo4Net.Storageengine.Api.schema.IndexProgressor_ExplicitClient _client;
		 private readonly ExplicitIndexHits _hits;

		 internal ExplicitIndexProgressor( ExplicitIndexHits hits, Neo4Net.Storageengine.Api.schema.IndexProgressor_ExplicitClient client )
		 {
			  this._hits = hits;
			  this._client = client;
		 }

		 public override bool Next()
		 {
			  while ( _hits.hasNext() )
			  {
					if ( _client.acceptEntity( _hits.next(), _hits.currentScore() ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  _hits.close();
		 }
	}

}