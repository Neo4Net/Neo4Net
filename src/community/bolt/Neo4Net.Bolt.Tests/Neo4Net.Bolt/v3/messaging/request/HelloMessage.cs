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
namespace Neo4Net.Bolt.v3.messaging.request
{

	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;

	public class HelloMessage : InitMessage
	{
		 public new const sbyte SIGNATURE = InitMessage.SIGNATURE;
		 private const string USER_AGENT = "user_agent";
		 private readonly IDictionary<string, object> _meta;

		 public HelloMessage( IDictionary<string, object> meta ) : base( ( string ) meta[USER_AGENT], meta )
		 {
			  this._meta = requireNonNull( meta );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  HelloMessage that = ( HelloMessage ) o;
			  return Objects.Equals( _meta, that._meta );
		 }

		 public virtual IDictionary<string, object> Meta()
		 {
			  return _meta;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _meta );
		 }

		 public override string ToString()
		 {
			  return "HELLO " + _meta;
		 }
	}

}