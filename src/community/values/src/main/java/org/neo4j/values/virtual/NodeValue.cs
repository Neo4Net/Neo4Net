using System;
using System.Diagnostics;

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
namespace Neo4Net.Values.@virtual
{

	using Neo4Net.Values;
	using TextArray = Neo4Net.Values.Storable.TextArray;

	public abstract class NodeValue : VirtualNodeValue
	{
		 private readonly long _id;

		 protected internal NodeValue( long id )
		 {
			  this._id = id;
		 }

		 public abstract TextArray Labels();

		 public abstract MapValue Properties();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteNode( _id, Labels(), Properties() );
		 }

		 public override long Id()
		 {
			  return _id;
		 }

		 public override string ToString()
		 {
			  return format( "(%d)", _id );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Node";
			 }
		 }

		 internal class DirectNodeValue : NodeValue
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TextArray LabelsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly MapValue PropertiesConflict;

			  internal DirectNodeValue( long id, TextArray labels, MapValue properties ) : base( id )
			  {
					Debug.Assert( labels != null );
					Debug.Assert( properties != null );
					this.LabelsConflict = labels;
					this.PropertiesConflict = properties;
			  }

			  public override TextArray Labels()
			  {
					return LabelsConflict;
			  }

			  public override MapValue Properties()
			  {
					return PropertiesConflict;
			  }
		 }
	}

}