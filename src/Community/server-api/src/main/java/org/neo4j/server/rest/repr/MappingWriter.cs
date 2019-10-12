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
namespace Neo4Net.Server.rest.repr
{
	public abstract class MappingWriter
	{
		 internal virtual MappingWriter NewMapping( RepresentationType type, string param )
		 {
			  return NewMapping( type.ValueName, param );
		 }

		 protected internal virtual bool Interactive
		 {
			 get
			 {
				  return false;
			 }
		 }

		 protected internal abstract MappingWriter NewMapping( string type, string key );

		 internal virtual ListWriter NewList( RepresentationType type, string param )
		 {
			  if ( type.ValueName.Equals( "map" ) )
			  {
					return NewList( type.ListName, param );
			  }
			  if ( string.ReferenceEquals( type.ListName, null ) )
			  {
					throw new System.InvalidOperationException( "Invalid list type: " + type );
			  }
			  return NewList( type.ListName, param );
		 }

		 protected internal abstract ListWriter NewList( string type, string key );

		 protected internal virtual void WriteString( string key, string value )
		 {
			  writeValue( RepresentationType.String, key, value );
		 }

		 internal virtual void WriteInteger( RepresentationType type, string param, long property )
		 {
			  WriteInteger( type.ValueName, param, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeInteger(String type, String key, long value)
		 protected internal virtual void WriteInteger( string type, string key, long value )
		 {
			  writeValue( type, key, value );
		 }

		 internal virtual void WriteFloatingPointNumber( RepresentationType type, string key, double value )
		 {
			  WriteFloatingPointNumber( type.ValueName, key, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeFloatingPointNumber(String type, String key, double value)
		 protected internal virtual void WriteFloatingPointNumber( string type, string key, double value )
		 {
			  writeValue( type, key, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeBoolean(String key, boolean value)
		 protected internal virtual void WriteBoolean( string key, bool value )
		 {
			  writeValue( RepresentationType.Boolean, key, value );
		 }

		 internal virtual void WriteValue( RepresentationType type, string key, object value )
		 {
			  WriteValue( type.ValueName, key, value );
		 }

		 protected internal abstract void WriteValue( string type, string key, object value );

		 protected internal abstract void Done();
	}

}