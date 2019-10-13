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
namespace Neo4Net.Server.rest.repr.formats
{


	public class ListWrappingWriter : ListWriter
	{
		 internal readonly IList<object> Data;
		 private readonly bool _interactive;

		 public ListWrappingWriter( IList<object> data ) : this( data, false )
		 {
		 }

		 internal ListWrappingWriter( IList<object> data, bool interactive )
		 {
			  this.Data = data;
			  this._interactive = interactive;
		 }

		 protected internal override ListWriter NewList( string type )
		 {
			  IList<object> list = new List<object>();
			  Data.Add( list );
			  return new ListWrappingWriter( list, _interactive );
		 }

		 protected internal override MappingWriter NewMapping( string type )
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  Data.Add( map );
			  return new MapWrappingWriter( map, _interactive );
		 }

		 protected internal override void WriteValue( string type, object value )
		 {
			  Data.Add( value );
		 }

		 protected internal override void Done()
		 {
		 }
	}

}