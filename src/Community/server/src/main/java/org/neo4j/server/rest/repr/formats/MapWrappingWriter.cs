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
namespace Neo4Net.Server.rest.repr.formats
{


	public class MapWrappingWriter : MappingWriter
	{
		 internal readonly IDictionary<string, object> Data;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly bool InteractiveConflict;

		 public MapWrappingWriter( IDictionary<string, object> data ) : this( data, false )
		 {
		 }

		 public MapWrappingWriter( IDictionary<string, object> data, bool interactive )
		 {
			  this.Data = data;
			  this.InteractiveConflict = interactive;
		 }

		 protected internal override sealed bool Interactive
		 {
			 get
			 {
				  return InteractiveConflict;
			 }
		 }

		 protected internal override ListWriter NewList( string type, string key )
		 {
			  IList<object> list = new List<object>();
			  Data[key] = list;
			  return new ListWrappingWriter( list, InteractiveConflict );
		 }

		 protected internal override MappingWriter NewMapping( string type, string key )
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  Data[key] = map;
			  return new MapWrappingWriter( map, InteractiveConflict );
		 }

		 protected internal override void WriteValue( string type, string key, object value )
		 {
			  Data[key] = value;
		 }

		 protected internal override void Done()
		 {
		 }
	}

}