using System;
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
namespace Neo4Net.Server.rest.repr
{

	using URIHelper = Neo4Net.Server.rest.domain.URIHelper;

	public abstract class IndexRepresentation : MappingRepresentation, EntityRepresentation
	{
		 private readonly string _name;
		 private readonly new IDictionary<string, string> _type;

		 public IndexRepresentation( string name, IDictionary<string, string> type ) : base( RepresentationType.Index )
		 {
			  this._name = name;
			  this._type = type;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void serialize(final MappingSerializer serializer)
		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutRelativeUriTemplate( "template", Path() + "{key}/{value}" );
			  foreach ( KeyValuePair<string, string> pair in _type.SetOfKeyValuePairs() )
			  {
					serializer.PutString( pair.Key, pair.Value );
			  }
		 }

		 public virtual string RelativeUriFor( string key, string value, long entityId )
		 {
			  return Path() + URIHelper.encode(key) + "/" + URIHelper.encode(value) + "/" + Convert.ToString(entityId);
		 }

		 public override ValueRepresentation SelfUri()
		 {
			  return ValueRepresentation.Uri( Path() );
		 }

		 protected internal virtual string Path()
		 {
			  return "index/" + PropertyContainerType() + "/" + URIHelper.encode(_name) + "/";
		 }

		 protected internal abstract string PropertyContainerType();

	}

}