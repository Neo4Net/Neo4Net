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
namespace Org.Neo4j.Tooling.procedure.procedures.invalid.bad_record_field_type
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

	public class BadRecordGenericFieldType
	{

		 public IDictionary<string, int> WrongType1;
		 public IList<int> WrongType2;
		 public IList<IList<IDictionary<string, int>>> WrongType3;
		 public IList<string> OkType1;
		 public IList<long> OkType2;
		 public IList<double> OkType4;
		 public IList<Number> OkType6;
		 public IList<bool> OkType7;
		 public IList<Path> OkType9;
		 public IList<Node> OkType10;
		 public IList<Relationship> OkType11;
		 public IList<object> OkType12;
		 public IDictionary<string, object> OkType13;
		 public Dictionary<string, object> OkType14;
		 public List<bool> OkType15;
		 public List<object> OkType16;
	}

}