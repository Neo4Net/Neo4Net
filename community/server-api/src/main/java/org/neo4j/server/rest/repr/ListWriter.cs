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
namespace Org.Neo4j.Server.rest.repr
{
	public abstract class ListWriter
	{
		 internal virtual MappingWriter NewMapping( RepresentationType type )
		 {
			  return NewMapping( type.ValueName );
		 }

		 protected internal abstract MappingWriter NewMapping( string type );

		 internal virtual ListWriter NewList( RepresentationType type )
		 {
			  if ( string.ReferenceEquals( type.ListName, null ) )
			  {
					throw new System.InvalidOperationException( "Invalid list type: " + type );
			  }
			  return NewList( type.ListName );
		 }

		 protected internal abstract ListWriter NewList( string type );

		 protected internal virtual void WriteString( string value )
		 {
			  writeValue( RepresentationType.String, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeBoolean(boolean value)
		 protected internal virtual void WriteBoolean( bool value )
		 {
			  writeValue( RepresentationType.Boolean, value );
		 }

		 internal virtual void WriteInteger( RepresentationType type, long value )
		 {
			  WriteInteger( type.ValueName, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeInteger(String type, long value)
		 protected internal virtual void WriteInteger( string type, long value )
		 {
			  writeValue( type, value );
		 }

		 internal virtual void WriteFloatingPointNumber( RepresentationType type, double value )
		 {
			  WriteFloatingPointNumber( type.ValueName, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected void writeFloatingPointNumber(String type, double value)
		 protected internal virtual void WriteFloatingPointNumber( string type, double value )
		 {
			  writeValue( type, value );
		 }

		 internal virtual void WriteValue( RepresentationType type, object value )
		 {
			  WriteValue( type.ValueName, value );
		 }

		 protected internal abstract void WriteValue( string type, object value );

		 protected internal abstract void Done();
	}

}