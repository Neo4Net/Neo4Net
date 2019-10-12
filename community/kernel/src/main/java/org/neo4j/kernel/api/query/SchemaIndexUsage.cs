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
namespace Org.Neo4j.Kernel.api.query
{

	public class SchemaIndexUsage : IndexUsage
	{
		 private readonly string _label;
		 private readonly string[] _propertyKeys;
		 private readonly int _labelId;

		 public SchemaIndexUsage( string identifier, int labelId, string label, params string[] propertyKeys ) : base( identifier )
		 {
			  this._label = label;
			  this._labelId = labelId;
			  this._propertyKeys = propertyKeys;
		 }

		 public virtual int LabelId
		 {
			 get
			 {
				  return _labelId;
			 }
		 }

		 public override IDictionary<string, string> AsMap()
		 {
			  IDictionary<string, string> map = new Dictionary<string, string>();
			  map["indexType"] = "SCHEMA INDEX";
			  map["entityType"] = "NODE";
			  map["identifier"] = Identifier;
			  map["label"] = _label;
			  map["labelId"] = _labelId.ToString();
			  for ( int i = 0; i < _propertyKeys.Length; i++ )
			  {
					string key = ( _propertyKeys.Length > 1 ) ? "propertyKey_" + i : "propertyKey";
					map[key] = _propertyKeys[i];
			  }
			  return map;
		 }
	}

}