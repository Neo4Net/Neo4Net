using System;
using System.Text;

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
namespace Neo4Net.Server.web
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	public class SimpleUriBuilder
	{

		 public virtual URI BuildURI( AdvertisedSocketAddress address, bool isSsl )
		 {
			  StringBuilder sb = new StringBuilder();
			  sb.Append( "http" );

			  if ( isSsl )
			  {
					sb.Append( "s" );

			  }
			  sb.Append( "://" );

			  sb.Append( address.Hostname );

			  int port = address.Port;
			  if ( port != 80 && port != 443 )
			  {
					sb.Append( ":" );
					sb.Append( port );
			  }
			  sb.Append( "/" );

			  try
			  {
					return new URI( sb.ToString() );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
		 }

	}

}